namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Person"/> objects.
	/// </summary>
	public class PersonExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Person, Guid> Id = new Exposer<Person, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.Name"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> Name = new Exposer<Person, string>((obj) => obj.Name, "Name");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.ExperienceId"/> property.
		/// </summary>
		public static readonly Exposer<Person, Guid> ExperienceId = new Exposer<Person, Guid>((obj) => obj.ExperienceId, "ExperienceId");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.OrganizationId"/> property.
		/// </summary>
		public static readonly Exposer<Person, Guid> OrganizationId = new Exposer<Person, Guid>((obj) => obj.OrganizationId, "OrganizationId");
	}
}
