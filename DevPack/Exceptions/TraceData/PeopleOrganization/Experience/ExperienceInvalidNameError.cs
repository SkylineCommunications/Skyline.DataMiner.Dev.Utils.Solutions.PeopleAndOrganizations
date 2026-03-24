namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an experience configuration specifies an invalid name.
	/// </summary>
	public class ExperienceInvalidNameError : ExperienceError
	{
		/// <summary>
		/// Gets the name of the experience.
		/// </summary>
		public string Name { get; internal set; }
	}
}
