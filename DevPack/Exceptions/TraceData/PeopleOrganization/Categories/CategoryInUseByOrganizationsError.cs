namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a category that is currently in use by one or multiple organizations.
	/// </summary>
	public class CategoryInUseByOrganizationsError : CategoryInUseError
	{
		/// <summary>
		/// Gets the collection of unique identifiers of the organizations having the category implemented.
		/// </summary>
		public IReadOnlyCollection<Guid> OrganizationIds { get; internal set; } = [];
	}
}
