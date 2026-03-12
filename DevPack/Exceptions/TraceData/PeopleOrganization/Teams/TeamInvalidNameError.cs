namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a team configuration has a duplicate name.
	/// </summary>
	/// <remarks>This can only occur when teams with the same name are provided to a bulk operation.</remarks>
	public class TeamInvalidNameError : TeamError
	{
		/// <summary>
		/// Gets the name of the team.
		/// </summary>
		public string Name { get; internal set; }
	}
}
