namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when no category can be found in the system with a given ID.
	/// </summary>
	public class OrganizationCategoryNotFoundError : OrganizationError
	{
		/// <summary>
		/// Gets the unique identifier for the category.
		/// </summary>
		public Guid CategoryId { get; set; }
	}
}
