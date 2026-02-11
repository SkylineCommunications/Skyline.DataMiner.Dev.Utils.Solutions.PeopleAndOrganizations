namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete an experience that is currently in use.
	/// </summary>
	public class ExperienceInUseError : ExperienceError
	{
		/// <summary>
		/// Gets or sets the collection of unique identifiers of the people having the category implemented.
		/// </summary>
		public List<Guid> PeopleIds { get; set; } = [];
	}
}
