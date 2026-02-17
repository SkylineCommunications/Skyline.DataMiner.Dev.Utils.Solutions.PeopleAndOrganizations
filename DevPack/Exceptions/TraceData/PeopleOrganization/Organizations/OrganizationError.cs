namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating an organization with invalid configuration.
	/// </summary>
	public class OrganizationError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the organization.
		/// </summary>
		public Guid Id { get; set; }
	}
}
