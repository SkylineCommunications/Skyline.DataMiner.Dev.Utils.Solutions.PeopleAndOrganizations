namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when an organization configuration specifies an invalid name.
	/// </summary>
	public class OrganizationInvalidNameError : OrganizationError
	{
		/// <summary>
		/// Gets the name of the organization.
		/// </summary>
		public string Name { get; internal set; }
	}
}
