namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating a team with invalid configuration.
	/// </summary>
	public class TeamError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the team.
		/// </summary>
		public Guid Id { get; set; }
	}
}
