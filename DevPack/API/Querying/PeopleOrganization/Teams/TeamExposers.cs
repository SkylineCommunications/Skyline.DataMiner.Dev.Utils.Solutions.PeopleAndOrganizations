namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Provides exposers for querying and filtering <see cref="Team"/> objects.
	/// </summary>
	public static class TeamExposers
	{
		/// <summary>
		/// Gets an exposer for the <see cref="ApiObject.Id"/> property.
		/// </summary>
		public static readonly Exposer<Team, Guid> Id = new Exposer<Team, Guid>((obj) => obj.Id, "Id");

		/// <summary>
		/// Gets an exposer for the <see cref="Team.Name"/> property.
		/// </summary>
		public static readonly Exposer<Team, string> Name = new Exposer<Team, string>((obj) => obj.Name, "Name");

		/// <summary>
		/// Gets an exposer for the <see cref="Team.Email"/> property.
		/// </summary>
		public static readonly Exposer<Team, string> Email = new Exposer<Team, string>((obj) => obj.Email, "Email");

		/// <summary>
		/// Gets an exposer for the <see cref="Team.Description"/> property.
		/// </summary>
		public static readonly Exposer<Team, string> Description = new Exposer<Team, string>((obj) => obj.Description, "Description");

		/// <summary>
		/// Gets an exposer for the <see cref="Team.IsBookable"/> property.
		/// </summary>
		public static readonly Exposer<Team, bool> IsBookable = new Exposer<Team, bool>((obj) => obj.IsBookable, "IsBookable");

		/// <summary>
		/// Gets an exposer for the <see cref="Team.State"/> property.
		/// </summary>
		public static readonly Exposer<Team, TeamState> State = new Exposer<Team, TeamState>((obj) => obj.State, "State");
	}
}
