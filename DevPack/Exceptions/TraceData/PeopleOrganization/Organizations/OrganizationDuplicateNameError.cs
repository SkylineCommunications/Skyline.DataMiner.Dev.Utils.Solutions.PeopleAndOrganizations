namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an organization is configured with an invalid name.
	/// </summary>
	public class OrganizationDuplicateNameError : OrganizationError
	{
		/// <summary>
		/// Gets the name of the organization.
		/// </summary>
		public string Name { get; set; }
	}
}
