namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;

    /// <summary>
    /// Represents an object with an identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets the unique identifier of the object.
        /// </summary>
        Guid Id { get; }
    }
}
