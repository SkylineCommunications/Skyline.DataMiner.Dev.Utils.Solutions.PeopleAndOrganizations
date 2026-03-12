namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System.Collections.Generic;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Defines methods for managing <see cref="Skill"/> objects.
	/// </summary>
	public interface ISkillsRepository : IReadableRepository<Skill>, ICountableRepository<Skill>, IBulkCreatableRepository<Skill>, IBulkUpdatableRepository<Skill>, IBulkDeletableRepository<Skill>
	{
		/// <summary>
		/// Gets the total number of Skills in the repository.
		/// </summary>
		/// <returns>The total Skill count.</returns>
		long Count();

		/// <summary>
		/// Reads all Skills from the Repository.
		/// </summary>
		/// <returns>All Skills in the repository.</returns>
		IEnumerable<Skill> Read();

		/// <summary>
		/// Creates or updates a collection of Skills in a single operation.
		/// </summary>
		/// <param name="oToCreateOrUpdate">The collection of Skills to create or update.</param>
		/// <returns>A read-only collection of the created or updated Skills.</returns>
		IReadOnlyCollection<Skill> CreateOrUpdate(IEnumerable<Skill> oToCreateOrUpdate);
	}
}
