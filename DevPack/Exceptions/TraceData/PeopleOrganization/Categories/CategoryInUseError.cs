namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that occurs when attempting to delete a category that is currently in use.
	/// </summary>
	internal class CategoryInUseError : CategoryError
	{
		/// <summary>
		/// Gets or sets the collection of unique identifiers of the organizations having the category implemented.
		/// </summary>
		public List<Guid> OrganizationIds { get; set; } = [];
	}
}
