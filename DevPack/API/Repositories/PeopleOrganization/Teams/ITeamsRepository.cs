namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines methods for managing <see cref="Team"/> objects.
	/// </summary>
	public interface ITeamsRepository : IRepository<Team>
	{
		/// <summary>
		/// Moves the specified <see cref="Team"/> from draft to active state.
		/// </summary>
		/// <param name="team">The team to activate.</param>
		Team Activate(Team team);

		/// <summary>
		/// Moves the specified <see cref="Team"/> from draft to active state.
		/// </summary>
		/// <param name="teamId">The unique identifier of the team to activate.</param>
		Team Activate(Guid teamId);

		/// <summary>
		/// Moves the specified teams from draft to active state.
		/// </summary>
		/// <param name="teams">The teams to activate.</param>
		IReadOnlyCollection<Team> Activate(IEnumerable<Team> teams);

		/// <summary>
		/// Moves the specified teams from draft to active state.
		/// </summary>
		/// <param name="teamIds">The unique identifiers of the teams to activate.</param>
		IReadOnlyCollection<Team> Activate(IEnumerable<Guid> teamIds);

		/// <summary>
		/// Marks the specified team as deprecated, indicating that it is no longer recommended for use.
		/// </summary>
		/// <param name="team">The team to be marked as deprecated. Cannot be null.</param>
		Team Deprecate(Team team);

		/// <summary>
		/// Marks the specified team as deprecated, indicating that it is no longer recommended for use.
		/// </summary>
		/// <param name="teamId">The unique identifier of the team to deprecate.</param>
		Team Deprecate(Guid teamId);

		/// <summary>
		/// Marks the specified teams as deprecated, indicating that they are no longer recommended for use.
		/// </summary>
		/// <param name="teams">A collection of teams to be marked as deprecated. Cannot be null or empty.</param>
		IReadOnlyCollection<Team> Deprecate(IEnumerable<Team> teams);

		/// <summary>
		/// Marks the specified teams as deprecated, indicating that they are no longer recommended for use.
		/// </summary>
		/// <param name="teamIds">The unique identifiers of the teams to deprecate.</param>
		IReadOnlyCollection<Team> Deprecate(IEnumerable<Guid> teamIds);
	}
}
