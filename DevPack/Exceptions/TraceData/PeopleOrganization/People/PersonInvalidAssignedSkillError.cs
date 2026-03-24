namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a person configuration specifies an invalid or unsupported assigned skill.
	/// </summary>
	public class PersonInvalidAssignedSkillError : PersonError
	{
		/// <summary>
		/// Gets the name of the skill.
		/// </summary>
		public string Name { get; internal set; }
	}
}
