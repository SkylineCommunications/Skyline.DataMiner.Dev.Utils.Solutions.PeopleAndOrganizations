namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.Categories.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Extensions;

	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Represents a team in People and Organizations.
	/// </summary>
	public class Team : ApiObject
	{
		private StoragePeopleAndOrganizations.TeamsInstance originalInstance;
		private StoragePeopleAndOrganizations.TeamsInstance updatedInstance;

		private HashSet<string> skills = new HashSet<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Team"/> class.
		/// </summary>
		public Team() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Team"/> class with a specific team ID.
		/// </summary>
		/// <param name="teamId">The unique identifier of the team.</param>
		public Team(Guid teamId) : base(teamId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

		internal Team(StoragePeopleAndOrganizations.TeamsInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}

		/// <summary>
		/// Gets or sets the name of the team.
		/// </summary>
		public override string Name { get; set; }

		/// <summary>
		/// Gets or sets the email address of the team.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets a description of the functions and responsibilities of the team.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets a value indicating whether the team is bookable.
		/// </summary>
		public bool IsBookable { get; private set; }

		/// <summary>
		/// Gets the state of the team.
		/// </summary>
		public TeamState State { get; private set; }

		/// <summary>
		/// Gets the collection of skills assigned to the team.
		/// </summary>
		public IReadOnlyCollection<Skill> Skills => skills.Select(s => new Skill(s)).ToList();

		internal Guid ResourcePoolId { get; private set; }

		internal StoragePeopleAndOrganizations.TeamsInstance OriginalInstance => originalInstance;

		/// <summary>
		/// Adds the specified skill to the team.
		/// </summary>
		/// <param name="skill">The skill to add.</param>
		/// <returns>The current <see cref="Team"/> instance with updated skills.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skill"/> is <see langword="null"/>.</exception>
		public Team AddSkill(Skill skill)
		{
			if (skill == null)
			{
				throw new ArgumentNullException(nameof(skill));
			}

			skills.Add(skill.Name);
			return this;
		}

		/// <summary>
		/// Add the collection of skills to the team, replacing any existing skills.
		/// </summary>
		/// <param name="skills">The skills to set.</param>
		/// <returns>The current <see cref="Team"/> instance with updated skills.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skills"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="skills"/> contains a null element.</exception>
		public Team SetSkills(IEnumerable<Skill> skills)
		{
			if (skills == null)
			{
				throw new ArgumentNullException(nameof(skills));
			}

			if (skills.Any(s => s == null))
			{
				throw new ArgumentException("The collection contains a null skill.", nameof(skills));
			}

			this.skills = new HashSet<string>(skills.Select(s => s.Name));
			return this;
		}

		/// <summary>
		/// Removes the specified skill from the team.
		/// </summary>
		/// <param name="skill">The skill to be removed from the team.</param>
		/// <returns>The current instance of the <see cref="Team"/> instance after the skill has been removed.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skill"/> is <see langword="null"/>.</exception>
		public Team RemoveSkill(Skill skill)
		{
			if (skill == null)
			{
				throw new ArgumentNullException(nameof(skill));
			}

			skills.Remove(skill.Name);
			return this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + Id.GetHashCode();
				hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
				hash = (hash * 23) + (Email != null ? Email.GetHashCode() : 0);
				hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
				hash = (hash * 23) + IsBookable.GetHashCode();
				hash = (hash * 23) + ResourcePoolId.GetHashCode();
				hash = (hash * 23) + State.GetHashCode();

				foreach (var skill in skills.OrderBy(x => x))
				{
					hash = (hash * 23) + skill.GetHashCode();
				}

				return hash;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current Organization instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current Team instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Team other)
			{
				return false;
			}

			if (Id != other.Id
				|| Name != other.Name
				|| Email != other.Email
				|| Description != other.Description
				|| IsBookable != other.IsBookable
				|| ResourcePoolId != other.ResourcePoolId
				|| State != other.State)
			{
				return false;
			}

			if (!skills.SetEquals(other.skills))
			{
				return false;
			}

			return true;
		}

		internal StoragePeopleAndOrganizations.TeamsInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.TeamsInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.TeamInformation.TeamName = Name;
			updatedInstance.TeamInformation.TeamEmail = Email;
			updatedInstance.TeamInformation.TeamDescription = Description;
			updatedInstance.TeamInformation.Skills = skills.ToList();

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.TeamsInstance instance)
		{
			originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.TeamInformation.TeamName;
			Email = instance.TeamInformation.TeamEmail;
			Description = instance.TeamInformation.TeamDescription;
			IsBookable = instance.TeamInformation.Bookable.HasValue ? instance.TeamInformation.Bookable.Value : false;
			skills = new HashSet<string>(instance.TeamInformation.Skills);

			ResourcePoolId = instance.ResourcePool.LinkedResourcePool.HasValue ? instance.ResourcePool.LinkedResourcePool.Value : Guid.Empty;

			State = EnumExtensions.MapEnum<StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.StatusesEnum, TeamState>(instance.Status);
		}
	}
}
