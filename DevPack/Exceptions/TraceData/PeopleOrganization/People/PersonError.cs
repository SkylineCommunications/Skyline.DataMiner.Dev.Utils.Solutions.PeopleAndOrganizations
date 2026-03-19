namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating a person with invalid configuration.
	/// </summary>
	public class PersonError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the person.
		/// </summary>
		public Guid Id { get; internal set; }
	}
}
