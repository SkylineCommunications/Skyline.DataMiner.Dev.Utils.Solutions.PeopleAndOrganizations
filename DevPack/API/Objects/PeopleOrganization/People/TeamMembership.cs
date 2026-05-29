namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

#if !ABSTRACTIONS
	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;
#endif

	/// <summary>
	/// Represents a team membership.
	/// </summary>
	public class TeamMembership : TrackableObject
	{
#if !ABSTRACTIONS
		private StoragePeopleAndOrganizations.TeamSection originalSection;
		private StoragePeopleAndOrganizations.TeamSection updatedSection;
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="TeamMembership"/> class with the team.
		/// </summary>
		/// <param name="team">The team.</param>
		public TeamMembership(Team team) : this(team?.Id ?? throw new ArgumentNullException(nameof(team)))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TeamMembership"/> class with the team ID.
		/// </summary>
		/// <param name="teamId">The unique identifier of the team.</param>
		public TeamMembership(Guid teamId)
		{
			if (teamId == Guid.Empty)
			{
				throw new ArgumentException(nameof(teamId));
			}

			TeamId = teamId;

			IsNew = true;
		}

#if !ABSTRACTIONS
		internal TeamMembership(StoragePeopleAndOrganizations.TeamSection section)
		{
			ParseSection(section);
			InitTracking();
		}
#endif

		/// <summary>
		/// Gets the unique identifier of the team.
		/// </summary>
		public Guid TeamId { get; private set; }

		/// <summary>
		/// Gets or sets the unique identifier of the associated role.
		/// </summary>
		public Guid RoleId { get; set; }

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.TeamSection OriginalSection => originalSection;
#endif

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + TeamId.GetHashCode();
				hash = (hash * 23) + (RoleId != Guid.Empty ? RoleId.GetHashCode() : 0);
				return hash;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current TeamMembership instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current TeamMembership instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not TeamMembership other)
			{
				return false;
			}

			return TeamId == other.TeamId
				&& RoleId == other.RoleId;
		}

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.TeamSection GetSectionWithChanges()
		{
			if (updatedSection == null)
			{
				updatedSection = IsNew ? new StoragePeopleAndOrganizations.TeamSection() : originalSection.Clone();
			}

			updatedSection.Team_144d3379 = TeamId;
			updatedSection.TeamRole = RoleId != Guid.Empty ? RoleId : null;

			return updatedSection;
		}

		private void ParseSection(StoragePeopleAndOrganizations.TeamSection section)
		{
			originalSection = section ?? throw new ArgumentNullException(nameof(section));

			TeamId = section.Team_144d3379 ?? Guid.Empty;
			RoleId = section.TeamRole ?? Guid.Empty;
		}
#endif
	}
}
