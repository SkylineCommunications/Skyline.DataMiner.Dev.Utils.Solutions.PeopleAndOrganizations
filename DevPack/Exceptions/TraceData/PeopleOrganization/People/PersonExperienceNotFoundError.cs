namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when no experience can be found in the system with a given ID.
	/// </summary>
	public class PersonExperienceNotFoundError : PersonError
	{
		/// <summary>
		/// Gets the unique identifier for the experience.
		/// </summary>
		public Guid ExperienceId { get; internal set; }
	}
}
