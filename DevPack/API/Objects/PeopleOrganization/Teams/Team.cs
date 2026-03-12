namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Extensions;

	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Represents a team in People and Organizations.
	/// </summary>
	public class Team : ApiObject
	{
		private StoragePeopleAndOrganizations.TeamsInstance originalInstance;
		private StoragePeopleAndOrganizations.TeamsInstance updatedInstance;

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
		/// The name of the team.
		/// </summary>
		public override string Name { get; set; }

		/// <summary>
		/// The email address of the team.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// A description of the functions and responsibilities of the team.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Indicates whether the team is bookable.
		/// </summary>
		public bool IsBookable { get; private set; }

		/// <summary>
		/// Gets the state of the team.
		/// </summary>
		public TeamState State { get; private set; }

		internal Guid ResourcePoolId { get; private set; }

		internal StoragePeopleAndOrganizations.TeamsInstance OriginalInstance => originalInstance;

		internal StoragePeopleAndOrganizations.TeamsInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.TeamsInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.TeamInformation.TeamName = Name;
			updatedInstance.TeamInformation.TeamEmail = Email;
			updatedInstance.TeamInformation.TeamDescription = Description;

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.TeamsInstance instance)
		{
			originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.TeamInformation.TeamName;
			Email = instance.TeamInformation.TeamEmail;
			Description = instance.TeamInformation.TeamDescription;
			IsBookable = instance.TeamInformation.Bookable ?? false;

			ResourcePoolId = instance.ResourcePool.LinkedResourcePool.HasValue ? instance.ResourcePool.LinkedResourcePool.Value : Guid.Empty;

			State = EnumExtensions.MapEnum<StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.StatusesEnum, TeamState>(instance.Status);
		}
	}
}
