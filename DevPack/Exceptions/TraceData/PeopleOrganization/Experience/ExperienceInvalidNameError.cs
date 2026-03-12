namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an experience configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when experiences with the same name are provided to a bulk operation.</remarks>
	public class ExperienceInvalidNameError : ExperienceError
	{
		/// <summary>
		/// Gets the name of the experience.
		/// </summary>
		public string Name { get; internal set; }
	}
}
