namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when no organization can be found in the system with a given ID.
	/// </summary>
	public class PersonOrganizationNotFoundError : PersonError
	{
		/// <summary>
		/// Gets the unique identifier for the organization.
		/// </summary>
		public Guid OrganizationId { get; internal set; }
	}
}
