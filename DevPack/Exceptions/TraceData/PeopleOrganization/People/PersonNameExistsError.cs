namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a person configuration name already exists.
	/// </summary>
	public class PersonNameExistsError : PersonError
	{
		/// <summary>
		/// Gets the name of the person.
		/// </summary>
		public string Name { get; internal set; }
	}
}
