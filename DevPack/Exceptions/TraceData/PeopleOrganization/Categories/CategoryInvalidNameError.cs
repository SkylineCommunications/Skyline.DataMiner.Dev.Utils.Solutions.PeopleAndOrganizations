namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a category configuration specifies an invalid name.
	/// </summary>
	public class CategoryInvalidNameError : CategoryError
	{
		/// <summary>
		/// Gets the name of the category.
		/// </summary>
		public string Name { get; internal set; }
	}
}
