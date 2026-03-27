namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions.TraceData.PeopleOrganization.Skills
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a skill that is currently in use by one or multiple teams.
	/// </summary>
	public class SkillInUseByTeamsError : SkillInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the teams having the skill implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> TeamIds { get; internal set; } = [];
	}
}
