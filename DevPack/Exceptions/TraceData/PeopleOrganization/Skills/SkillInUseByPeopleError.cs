namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a skill that is currently in use by one or multiple people.
	/// </summary>
	public class SkillInUseByPeopleError : SkillInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the people having the skill implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> PeopleIds { get; internal set; } = [];
	}
}
