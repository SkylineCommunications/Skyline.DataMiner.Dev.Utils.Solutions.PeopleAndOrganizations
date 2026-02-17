namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when multiple organizations are configured with the same identifier.
	/// </summary>
	/// <remarks>This can only occur when organizations with the same ID are provided to a bulk operation.</remarks>
	public class OrganizationDuplicateIdError : OrganizationError
	{
	}
}
