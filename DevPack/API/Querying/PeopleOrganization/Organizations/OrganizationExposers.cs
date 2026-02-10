namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Organization"/> objects.
	/// </summary>
	public class OrganizationExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Organization, Guid> Id = new Exposer<Organization, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Organization.Name"/> property.
		/// </summary>
		public static readonly Exposer<Organization, string> Name = new Exposer<Organization, string>((obj) => obj.Name, "Name");
	}
}
