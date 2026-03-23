namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a team that is currently in use by one or multiple people.
	/// </summary>
	public class TeamInUseByPeopleError : TeamInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the people having the team implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> PeopleIds { get; internal set; } = [];
	}
}
