namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating a role with invalid configuration.
	/// </summary>
	public class RoleError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the role.
		/// </summary>
		public Guid Id { get; internal set; }
	}
}
