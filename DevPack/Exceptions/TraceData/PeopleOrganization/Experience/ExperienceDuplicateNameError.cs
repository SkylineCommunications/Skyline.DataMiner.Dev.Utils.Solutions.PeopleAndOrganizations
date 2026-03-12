namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an experience is configured with an invalid name.
	/// </summary>
	public class ExperienceDuplicateNameError : ExperienceError
	{
		/// <summary>
		/// Gets the name of the experience.
		/// </summary>
		public string Name { get; internal set; }
	}
}
