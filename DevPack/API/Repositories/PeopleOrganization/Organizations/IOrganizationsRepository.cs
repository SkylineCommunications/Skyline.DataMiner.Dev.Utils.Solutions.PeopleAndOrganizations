namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM;

    /// <summary>
    /// Defines methods for managing <see cref="Organization"/> objects.
    /// </summary>
    public interface IOrganizationsRepository : IReadableRepository<Organization>
    {
        /// <summary>
        /// Reads all Organizations.
        /// </summary>
        /// <returns>An enumerable collection of all Organizations.</returns>
        IEnumerable<Organization> Read();

        /// <summary>
        /// Reads a single Organization by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Organization.</param>
        /// <returns>The Organization with the specified identifier, or <c>null</c> if not found.</returns>
        Organization Read(Guid id);

        /// <summary>
        /// Reads multiple Organizations by their unique identifiers.
        /// </summary>
        /// <param name="ids">A collection of unique identifiers.</param>
        /// <returns>An enumerable collection of Organizations matching the specified identifiers.</returns>
        IEnumerable<Organization> Read(IEnumerable<Guid> ids);
    }
}
