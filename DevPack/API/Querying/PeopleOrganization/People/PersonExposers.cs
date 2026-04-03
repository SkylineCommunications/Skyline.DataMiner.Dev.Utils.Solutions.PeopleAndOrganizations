namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Person"/> objects.
	/// </summary>
	public static class PersonExposers
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
		/// Gets an exposer for the <see cref="Person.Email"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> Email = new Exposer<Person, string>((obj) => obj.Email, "Email");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.Phone"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> Phone = new Exposer<Person, string>((obj) => obj.Phone, "Phone");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.StreetAddress"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> StreetAddress = new Exposer<Person, string>((obj) => obj.StreetAddress, "StreetAddress");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.City"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> City = new Exposer<Person, string>((obj) => obj.City, "City");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.Country"/> property.
		/// </summary>
		public static readonly Exposer<Person, Country> Country = new Exposer<Person, Country>((obj) => obj.Country.Value, "Country");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.ZipCode"/> property.
		/// </summary>
		public static readonly Exposer<Person, string> ZipCode = new Exposer<Person, string>((obj) => obj.ZipCode, "ZipCode");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.ExperienceId"/> property.
		/// </summary>
		public static readonly Exposer<Person, Guid> ExperienceId = new Exposer<Person, Guid>((obj) => obj.ExperienceId, "ExperienceId");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.OrganizationId"/> property.
		/// </summary>
		public static readonly Exposer<Person, Guid> OrganizationId = new Exposer<Person, Guid>((obj) => obj.OrganizationId, "OrganizationId");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.State"/> property.
		/// </summary>
		public static readonly Exposer<Person, PersonState> State = new Exposer<Person, PersonState>((obj) => obj.State, "State");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.ResourceId"/> property.
		/// </summary>
		internal static readonly Exposer<Person, Guid> ResourceId = new Exposer<Person, Guid>((obj) => obj.ResourceId, "ResourceId");

		/// <summary>
		/// Gets an exposer for the <see cref="Person.ResourceId"/> property.
		/// </summary>
		internal static readonly Exposer<Person, bool> HasResourceId = new Exposer<Person, bool>("HasResourceId");

		/// <summary>
		/// Provides exposers for querying and filtering team memberships.
		/// </summary>
		public static class TeamMemberships
		{
			/// <summary>
			/// Gets a dynamic list exposer for team membership team IDs.
			/// </summary>
			public static readonly DynamicListExposer<Person, Guid> TeamId = DynamicListExposer<Person, Guid>.CreateFromListExposer(new Exposer<Person, IEnumerable>((obj) => obj.TeamMemberships.Where(x => x != null).Select(x => x.TeamId).Where(x => x != Guid.Empty), "TeamMemberships.TeamId"));

			/// <summary>
			/// Gets a dynamic list exposer for team membership role IDs.
			/// </summary>
			public static readonly DynamicListExposer<Person, Guid> RoleId = DynamicListExposer<Person, Guid>.CreateFromListExposer(new Exposer<Person, IEnumerable>((obj) => obj.TeamMemberships.Where(x => x != null).Select(x => x.RoleId).Where(x => x != Guid.Empty), "TeamMemberships.RoleId"));
		}
	}
}
