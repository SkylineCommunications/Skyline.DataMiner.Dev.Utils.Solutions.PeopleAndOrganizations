namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Experience"/> objects.
	/// </summary>
	public class ExperienceExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Experience, Guid> Id = new Exposer<Experience, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Experience.Name"/> property.
		/// </summary>
		public static readonly Exposer<Experience, string> Name = new Exposer<Experience, string>((obj) => obj.Name, "Name");
	}
}
