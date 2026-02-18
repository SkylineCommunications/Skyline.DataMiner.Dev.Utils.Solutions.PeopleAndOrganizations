namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	/// <summary>
	/// Represents an error that occurs when a team is configured with an invalid name.
	/// </summary>
	public class TeamDuplicateNameError : TeamError
	{
		/// <summary>
		/// Gets the name of the team.
		/// </summary>
		public string Name { get; set; }
	}
}
