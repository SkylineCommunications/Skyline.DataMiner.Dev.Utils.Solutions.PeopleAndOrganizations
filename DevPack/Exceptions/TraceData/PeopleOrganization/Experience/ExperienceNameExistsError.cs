namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an experience configuration name already exists.
	/// </summary>
	public class ExperienceNameExistsError : ExperienceError
	{
		/// <summary>
		/// Gets the name of the experience.
		/// </summary>
		public string Name { get; internal set; }
	}
}
