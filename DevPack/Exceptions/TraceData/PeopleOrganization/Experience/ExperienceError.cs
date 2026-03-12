namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating an experience with invalid configuration.
	/// </summary>
	public class ExperienceError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the experience.
		/// </summary>
		public Guid Id { get; internal set; }
	}
}
