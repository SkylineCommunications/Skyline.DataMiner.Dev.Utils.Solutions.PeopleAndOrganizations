namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Category"/> objects.
	/// </summary>
	public class CategoryExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Category, Guid> Id = new Exposer<Category, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Role.Name"/> property.
		/// </summary>
		public static readonly Exposer<Category, string> Name = new Exposer<Category, string>((obj) => obj.Name, "Name");
	}
}
