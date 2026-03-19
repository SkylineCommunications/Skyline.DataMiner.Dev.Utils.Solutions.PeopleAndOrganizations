namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Extensions;

	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Represents a person in People and Organizations.
	/// </summary>
	public class Person : ApiObject
	{
		private readonly HashSet<Skill> skills = [];
		private readonly List<TeamMembership> teamMemberships = [];

		private StoragePeopleAndOrganizations.PeopleInstance originalInstance;
		private StoragePeopleAndOrganizations.PeopleInstance updatedInstance;

		/// <summary>
		/// Initializes a new instance of the <see cref="Person"/> class.
		/// </summary>
		public Person() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Person"/> class with a specific person ID.
		/// </summary>
		/// <param name="personId">The unique identifier of the person.</param>
		public Person(Guid personId) : base(personId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

		internal Person(StoragePeopleAndOrganizations.PeopleInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}

		/// <summary>
		/// Gets or sets the full name of the person.
		/// </summary>
		public override string Name { get; set; }

		/// <summary>
		/// Gets or sets the email address of the person.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the phone number of the person.
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Gets or sets the street address of the person.
		/// </summary>
		public string StreetAddress { get; set; }

		/// <summary>
		/// Gets or sets the name of the city of the person.
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// Gets or sets the country of the person.
		/// </summary>
		public Country? Country { get; set; }

		/// <summary>
		/// Gets or sets the postal code of the person.
		/// </summary>
		public string ZipCode { get; set; }

		/// <summary>
		/// Gets or sets the Experience ID of the person.
		/// </summary>
		public Guid ExperienceId { get; set; }

		/// <summary>
		/// Gets or sets the Organization ID of the person.
		/// </summary>
		public Guid OrganizationId { get; set; }

		/// <summary>
		/// Gets the state of the person.
		/// </summary>
		public PersonState State { get; private set; }

		/// <summary>
		/// Gets the collection of skills assigned to the person.
		/// </summary>
		public IReadOnlyCollection<Skill> Skills => skills;

		/// <summary>
		/// Gets the collection of team memberships to which this person belongs.
		/// </summary>
		public IReadOnlyCollection<TeamMembership> TeamMemberships => teamMemberships;

		internal Guid ResourceId { get; private set; }

		/// <summary>
		/// Adds the specified skill to the person.
		/// </summary>
		/// <param name="skill">The skill to add.</param>
		/// <returns>The current <see cref="Person"/> instance with updated skills.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skill"/> is <see langword="null"/>.</exception>
		public Person AddSkill(Skill skill)
		{
			if (skill == null)
			{
				throw new ArgumentNullException(nameof(skill));
			}

			skills.Add(skill);
			return this;
		}

		/// <summary>
		/// Add the collection of skills to the person, replacing any existing skills.
		/// </summary>
		/// <param name="skills">The skills to set.</param>
		/// <returns>The current <see cref="Person"/> instance with updated skills.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skills"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="skills"/> contains a null element.</exception>
		public Person SetSkills(IEnumerable<Skill> skills)
		{
			if (skills == null)
			{
				throw new ArgumentNullException(nameof(skills));
			}

			if (skills.Any(s => s == null))
			{
				throw new ArgumentException("The collection contains a null skill.", nameof(skills));
			}

			this.skills.Clear();
			foreach (var skill in skills)
			{
				this.skills.Add(skill);
			}

			return this;
		}

		/// <summary>
		/// Removes the specified skill from the person.
		/// </summary>
		/// <param name="skill">The skill to be removed from the person.</param>
		/// <returns>The current instance of the <see cref="Person"/> instance after the skill has been removed.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="skill"/> is <see langword="null"/>.</exception>
		public Person RemoveSkill(Skill skill)
		{
			if (skill == null)
			{
				throw new ArgumentNullException(nameof(skill));
			}

			skills.Remove(skill);
			return this;
		}

		public Person AddTeamMembership(TeamMembership teamMembership)
		{
			if (teamMembership == null)
			{
				throw new ArgumentNullException(nameof(teamMembership));
			}

			if (!teamMembership.IsNew)
			{
				return this;
			}

			teamMemberships.Add(teamMembership);
			return this;
		}

		public Person RemoveTeamMembership(TeamMembership teamMembership)
		{
			if (teamMembership == null)
			{
				throw new ArgumentNullException(nameof(teamMembership));
			}

			if (teamMembership.OriginalSection == null)
			{
				return this;
			}

			var toRemove = teamMemberships.SingleOrDefault(x => x.OriginalSection.ID == teamMembership.OriginalSection.ID);
			if (toRemove == null)
			{
				return this;
			}

			teamMemberships.Remove(teamMembership);
			return this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + Id.GetHashCode();
				hash = (hash * 23) + (Name?.GetHashCode() ?? 0);
				hash = (hash * 23) + (Email?.GetHashCode() ?? 0);
				hash = (hash * 23) + (Phone?.GetHashCode() ?? 0);
				hash = (hash * 23) + (StreetAddress?.GetHashCode() ?? 0);
				hash = (hash * 23) + (City?.GetHashCode() ?? 0);
				hash = (hash * 23) + (Country?.GetHashCode() ?? 0);
				hash = (hash * 23) + (ZipCode?.GetHashCode() ?? 0);
				hash = (hash * 23) + ExperienceId.GetHashCode();
				hash = (hash * 23) + OrganizationId.GetHashCode();
				hash = (hash * 23) + ResourceId.GetHashCode();
				hash = (hash * 23) + State.GetHashCode();

				foreach (var skill in skills.OrderBy(x => x).ToList())
				{
					hash = (hash * 23) + skill.GetHashCode();
				}

				foreach (var teamMembership in TeamMemberships.OrderBy(x => x.TeamId).ToArray())
				{
					hash = (hash * 23) + teamMembership.GetHashCode();
				}

				return hash;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current Person instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current Person instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Person other)
			{
				return false;
			}

			if (Id != other.Id
				|| Name != other.Name
				|| Email != other.Email
				|| Phone != other.Phone
				|| StreetAddress != other.StreetAddress
				|| City != other.City
				|| Country != other.Country
				|| ZipCode != other.ZipCode
				|| ExperienceId != other.ExperienceId
				|| OrganizationId != other.OrganizationId
				|| ResourceId != other.ResourceId
				|| State != other.State)
			{
				return false;
			}

			if (!skills.SetEquals(other.skills)
				|| TeamMemberships.SequenceEqual(other.TeamMemberships))
			{
				return false;
			}

			return true;
		}

		internal StoragePeopleAndOrganizations.PeopleInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.PeopleInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.PeopleInformation.FullName = Name;
			updatedInstance.PeopleInformation.ExperienceLevel = ExperienceId != Guid.Empty ? ExperienceId : null;
			updatedInstance.PeopleInformation.Skills = Skills.Select(s => s.Name);

			updatedInstance.ContactInfo.Email = Email;
			updatedInstance.ContactInfo.Phone = Phone;
			updatedInstance.ContactInfo.StreetAddress = StreetAddress;
			updatedInstance.ContactInfo.City = City;
			updatedInstance.ContactInfo.ZIP = ZipCode;
			updatedInstance.ContactInfo.Country = Country.HasValue ? EnumExtensions.MapEnum<Country, StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Enums.Country>(Country.Value) : null;

			updatedInstance.Team.Clear();
			foreach (var teamMembership in teamMemberships)
			{
				updatedInstance.Team.Add(teamMembership.GetSectionWithChanges());
			}

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.PeopleInstance instance)
		{
			this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.PeopleInformation.FullName;
			ExperienceId = instance.PeopleInformation.ExperienceLevel ?? Guid.Empty;
			SetSkills(instance.PeopleInformation.Skills.Select(s => new Skill(s)));

			Email = instance.ContactInfo.Email;
			Phone = instance.ContactInfo.Phone;
			StreetAddress = instance.ContactInfo.StreetAddress;
			City = instance.ContactInfo.City;
			ZipCode = instance.ContactInfo.ZIP;
			if (instance.ContactInfo.Country.HasValue)
			{
				Country = EnumExtensions.MapEnum<StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Enums.Country, Country>(instance.ContactInfo.Country.Value);
			}

			OrganizationId = instance.Organization.OrganizationId;
			ResourceId = instance.Resource.LinkedResource ?? Guid.Empty;

			foreach (var section in instance.Team)
			{
				var teamMembership = new TeamMembership(section);
				if (teamMembership.TeamId != Guid.Empty)
				{
					teamMemberships.Add(teamMembership);
				}
			}

			State = EnumExtensions.MapEnum<StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.StatusesEnum, PersonState>(instance.Status);
		}
	}
}
