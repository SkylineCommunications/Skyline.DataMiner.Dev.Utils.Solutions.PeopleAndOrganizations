namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an organization configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when organizations with the same name are provided to a bulk operation.</remarks>
	public class OrganizationDuplicateNameError : OrganizationError
	{
		/// <summary>
		/// Gets the name of the organization.
		/// </summary>
		public string Name { get; internal set; }
	}
}
