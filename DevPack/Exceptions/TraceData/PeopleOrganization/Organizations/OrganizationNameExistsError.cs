namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an organization configuration name already exists.
	/// </summary>
	public class OrganizationNameExistsError : OrganizationError
	{
		/// <summary>
		/// Gets the name of the organization.
		/// </summary>
		public string Name { get; set; }
	}
}
