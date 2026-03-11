namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Skill"/> objects.
	/// </summary>
	public class SkillExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="Skill.Name"/> property.
		/// </summary>
		public static readonly Exposer<Skill, string> Name = new Exposer<Skill, string>((obj) => obj.Name, "Name");
	}
}
