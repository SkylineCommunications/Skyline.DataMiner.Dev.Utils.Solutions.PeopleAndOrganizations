namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a role configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when roles with the same name are provided to a bulk operation.</remarks>
	public class RoleInvalidNameError : RoleError
	{
		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		public string Name { get; internal set; }
	}
}
