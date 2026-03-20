namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a person configuration specifies an invalid name.
	/// </summary>
	public class PersonInvalidNameError : PersonError
	{
		/// <summary>
		/// Gets the name of the person.
		/// </summary>
		public string Name { get; internal set; }
	}
}
