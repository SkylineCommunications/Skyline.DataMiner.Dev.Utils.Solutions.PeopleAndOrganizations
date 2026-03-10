namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System.Collections.Generic;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Defines methods for managing <see cref="Skill"/> objects.
	/// </summary>
	public interface ISkillsRepository : ICreatableRepository<Skill>, IRepositoryMarker<Skill>, IReadableRepository<Skill>, IUpdatableRepository<Skill>, IDeletableRepository<Skill>, ICountableRepository<Skill>, IBulkCreatableRepository<Skill>, IBulkUpdatableRepository<Skill>, IBulkDeletableRepository<Skill>
	{
		/// <summary>
		/// Gets the total number of API objects in the repository.
		/// </summary>
		/// <returns>The total count of API objects.</returns>
		long Count();

		/// <summary>
		/// Reads all API objects.
		/// </summary>
		/// <returns>An enumerable collection of all API objects.</returns>
		IEnumerable<Skill> Read();

		//
		// Summary:
		//     Creates or updates a collection of entities in a single operation.
		//
		// Parameters:
		//   oToCreateOrUpdate:
		//     The collection of entities to create or update.
		//
		// Returns:
		//     A read-only collection of the created or updated entities.
		IReadOnlyCollection<Skill> CreateOrUpdate(IEnumerable<Skill> oToCreateOrUpdate);
	}
}
