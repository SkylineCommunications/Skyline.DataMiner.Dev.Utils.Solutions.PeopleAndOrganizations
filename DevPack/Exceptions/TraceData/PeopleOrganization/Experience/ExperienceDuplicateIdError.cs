namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when multiple experiences are configured with the same identifier.
	/// </summary>
	/// <remarks>This can only occur when experiences with the same ID are provided to a bulk operation.</remarks>
	public class ExperienceDuplicateIdError : ExperienceError
	{
	}
}
