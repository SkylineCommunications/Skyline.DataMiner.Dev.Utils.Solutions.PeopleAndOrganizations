namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to deprecate a person that is currently in use by one or multiple teams.
	/// </summary>
	public class PersonInUseByTeamsError : PersonInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the teams to which the person is assigned.
		/// </summary>
		public IReadOnlyCollection<Guid> TeamIds { get; internal set; } = [];
	}
}
