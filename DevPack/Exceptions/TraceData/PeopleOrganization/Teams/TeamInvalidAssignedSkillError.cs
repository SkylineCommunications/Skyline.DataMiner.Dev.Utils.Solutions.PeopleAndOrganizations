namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a team configuration specifies an invalid or unsupported assigned skill.
	/// </summary>
	public class TeamInvalidAssignedSkillError : TeamError
	{
		/// <summary>
		/// Gets or sets the name of the skill.
		/// </summary>
		public string Name { get; set; }
	}
}
