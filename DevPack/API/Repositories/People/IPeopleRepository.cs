namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM;

    /// <summary>
    /// Defines methods for managing <see cref="Person"/> objects.
    /// </summary>
    public interface IPeopleRepository : IReadableRepository<Person>
    {
        /// <summary>
        /// Reads all People.
        /// </summary>
        /// <returns>An enumerable collection of all People.</returns>
        IEnumerable<Person> Read();

        /// <summary>
        /// Reads a single Person by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Person.</param>
        /// <returns>The Person with the specified identifier, or <c>null</c> if not found.</returns>
        Person Read(Guid id);

        /// <summary>
        /// Reads multiple Persons by their unique identifiers.
        /// </summary>
        /// <param name="ids">A collection of unique identifiers.</param>
        /// <returns>An enumerable collection of Persons matching the specified identifiers.</returns>
        IEnumerable<Person> Read(IEnumerable<Guid> ids);
    }
}
