namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a category is configured with an invalid name.
	/// </summary>
	public class CategoryDuplicateNameError : CategoryError
	{
		/// <summary>
		/// Gets the name of the category.
		/// </summary>
		public string Name { get; set; }
	}
}
