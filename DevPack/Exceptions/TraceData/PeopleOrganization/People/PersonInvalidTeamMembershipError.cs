namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when a person team membership is configured with invalid or unsupported settings.
	/// </summary>
	public class PersonInvalidTeamMembershipError : PersonError
	{
		/// <summary>
		/// Gets the unique identifier of the team.
		/// </summary>
		public Guid TeamId { get; internal set; }

		/// <summary>
		/// Gets the unique identifier of the role.
		/// </summary>
		public Guid RoleId { get; internal set; }
	}
}
