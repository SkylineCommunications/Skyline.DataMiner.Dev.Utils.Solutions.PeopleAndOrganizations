namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a role configuration specifies an invalid name.
	/// </summary>
	public class RoleInvalidNameError : RoleError
	{
		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		public string Name { get; internal set; }
	}
}
