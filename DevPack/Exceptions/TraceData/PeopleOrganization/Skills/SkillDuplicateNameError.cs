namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a skill configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when skills with the same name are provided to a bulk operation.</remarks>
	public class SkillDuplicateNameError : SkillError
	{
	}
}
