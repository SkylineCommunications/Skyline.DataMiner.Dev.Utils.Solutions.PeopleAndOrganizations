namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete an organization that is currently in use.
	/// </summary>
	internal class OrganizationInUseError : OrganizationError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the people having the organization implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> PeopleIds { get; internal set; } = [];
	}
}
