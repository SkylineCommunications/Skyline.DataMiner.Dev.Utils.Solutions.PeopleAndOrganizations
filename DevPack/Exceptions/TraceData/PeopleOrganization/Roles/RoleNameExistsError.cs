namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a role configuration name already exists.
	/// </summary>
	public class RoleNameExistsError : RoleError
	{
		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		public string Name { get; set; }
	}
}
