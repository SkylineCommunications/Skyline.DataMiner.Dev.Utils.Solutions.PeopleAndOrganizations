namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a team configuration name already exists.
	/// </summary>
	public class TeamNameExistsError : TeamError
	{
		/// <summary>
		/// Gets the name of the team.
		/// </summary>
		public string Name { get; set; }
	}
}
