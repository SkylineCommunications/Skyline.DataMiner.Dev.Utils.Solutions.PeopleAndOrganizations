namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a role is configured with an invalid name.
	/// </summary>
	public class RoleDuplicateNameError : RoleError
	{
		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		public string Name { get; set; }
	}
}
