namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when multiple roles are configured with the same identifier.
	/// </summary>
	/// <remarks>This can only occur when roles with the same ID are provided to a bulk operation.</remarks>
	public class RoleDuplicateIdError : RoleError
	{
	}
}
