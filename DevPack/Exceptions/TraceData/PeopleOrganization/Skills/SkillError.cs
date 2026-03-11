namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when creating or updating a skill with invalid configuration.
	/// </summary>
	public class SkillError : PeopleAndOrganizationsErrorData
	{
		/// <summary>
		/// Gets or sets the name of the skill.
		/// </summary>
		public string Name { get; set; }
	}
}
