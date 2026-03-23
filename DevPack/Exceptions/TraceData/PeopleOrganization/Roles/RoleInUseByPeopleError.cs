namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a role that is currently in use by one or multiple people.
	/// </summary>
	public class RoleInUseByPeopleError : RoleInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the people having the role implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> PeopleIds { get; internal set; } = [];
	}
}
