namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a team configuration specifies an invalid name.
	/// </summary>
	public class TeamInvalidNameError : TeamError
	{
		/// <summary>
		/// Gets the name of the team.
		/// </summary>
		public string Name { get; internal set; }
	}
}
