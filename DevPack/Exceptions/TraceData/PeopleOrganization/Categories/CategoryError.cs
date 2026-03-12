namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;

	/// <summary>
	/// Represents an error that occurs when creating or updating a category with invalid configuration.
	/// </summary>
	public class CategoryError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets the unique identifier for the category.
		/// </summary>
		public Guid Id { get; internal set; }
	}
}
