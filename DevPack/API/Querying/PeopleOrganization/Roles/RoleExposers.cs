namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Role"/> objects.
	/// </summary>
	public static class RoleExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Role, Guid> Id = new Exposer<Role, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Role.Name"/> property.
		/// </summary>
		public static readonly Exposer<Role, string> Name = new Exposer<Role, string>((obj) => obj.Name, "Name");
	}
}
