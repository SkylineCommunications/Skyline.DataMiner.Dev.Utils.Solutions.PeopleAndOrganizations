namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a category configuration name already exists.
	/// </summary>
	public class CategoryNameExistsError : CategoryError
	{
		/// <summary>
		/// Gets the name of the category.
		/// </summary>
		public string Name { get; internal set; }
	}
}
