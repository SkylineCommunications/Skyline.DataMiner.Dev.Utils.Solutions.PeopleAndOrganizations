namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a category configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when categories with the same name are provided to a bulk operation.</remarks>
	public class CategoryInvalidNameError : CategoryError
	{
		/// <summary>
		/// Gets the name of the category.
		/// </summary>
		public string Name { get; internal set; }
	}
}
