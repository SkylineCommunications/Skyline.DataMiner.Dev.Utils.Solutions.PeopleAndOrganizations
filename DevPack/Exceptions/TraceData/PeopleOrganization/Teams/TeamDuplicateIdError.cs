namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when multiple teams are configured with the same identifier.
	/// </summary>
	/// <remarks>This can only occur when teams with the same ID are provided to a bulk operation.</remarks>
	public class TeamDuplicateIdError : TeamError
	{
	}
}
