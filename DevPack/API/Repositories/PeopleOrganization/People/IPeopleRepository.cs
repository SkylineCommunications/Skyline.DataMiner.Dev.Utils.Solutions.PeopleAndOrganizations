namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines methods for managing <see cref="Person"/> objects.
    /// </summary>
    public interface IPeopleRepository : IRepository<Person>
    {
        /// <summary>
        /// Moves the specified <see cref="Person"/> from draft to active state.
        /// </summary>
        /// <param name="person">The person to activate.</param>
        /// <returns>The activated person.</returns>
        Person Activate(Person person);

        /// <summary>
        /// Moves the specified <see cref="Person"/> from draft to active state.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to activate.</param>
        /// <returns>The activated person.</returns>
        Person Activate(Guid personId);

        /// <summary>
        /// Moves the specified people from draft to active state.
        /// </summary>
        /// <param name="people">The people to activate.</param>
        /// <returns>A read-only collection of activated people.</returns>
        IReadOnlyCollection<Person> Activate(IEnumerable<Person> people);

        /// <summary>
        /// Moves the specified people from draft to active state.
        /// </summary>
        /// <param name="personIds">The unique identifiers of the people to activate.</param>
        /// <returns>A read-only collection of activated people.</returns>
        IReadOnlyCollection<Person> Activate(IEnumerable<Guid> personIds);

        /// <summary>
        /// Marks the specified person as deprecated, indicating that it is no longer recommended for use.
        /// </summary>
        /// <param name="person">The person to be marked as deprecated. Cannot be null.</param>
        /// <returns>The deprecated person.</returns>
        Person Deprecate(Person person);

        /// <summary>
        /// Marks the specified person as deprecated, indicating that it is no longer recommended for use.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to deprecate.</param>
        /// <returns>The deprecated person.</returns>
        Person Deprecate(Guid personId);

        /// <summary>
        /// Marks the specified people as deprecated, indicating that they are no longer recommended for use.
        /// </summary>
        /// <param name="people">A collection of people to be marked as deprecated. Cannot be null or empty.</param>
        /// <returns>A read-only collection of deprecated people.</returns>
        IReadOnlyCollection<Person> Deprecate(IEnumerable<Person> people);

        /// <summary>
        /// Marks the specified people as deprecated, indicating that they are no longer recommended for use.
        /// </summary>
        /// <param name="personIds">The unique identifiers of the people to deprecate.</param>
        /// <returns>A read-only collection of deprecated people.</returns>
        IReadOnlyCollection<Person> Deprecate(IEnumerable<Guid> personIds);
    }
}
