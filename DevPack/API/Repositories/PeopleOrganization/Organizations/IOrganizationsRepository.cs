namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines methods for managing <see cref="Organization"/> objects.
	/// </summary>
	public interface IOrganizationsRepository : IRepository<Organization>
	{
		/// <summary>
		/// Moves the specified <see cref="Organization"/> from draft to active state.
		/// </summary>
		/// <param name="organization">The organization to activate.</param>
		Organization Activate(Organization organization);

		/// <summary>
		/// Moves the specified <see cref="Organization"/> from draft to active state.
		/// </summary>
		/// <param name="organizationId">The unique identifier of the organization to activate.</param>
		Organization Activate(Guid organizationId);

		/// <summary>
		/// Moves the specified organizations from draft to active state.
		/// </summary>
		/// <param name="organizations">The organizations to activate.</param>
		IReadOnlyCollection<Organization> Activate(IEnumerable<Organization> organizations);

		/// <summary>
		/// Moves the specified organizations from draft to active state.
		/// </summary>
		/// <param name="organizationIds">The unique identifiers of the organizations to activate.</param>
		IReadOnlyCollection<Organization> Activate(IEnumerable<Guid> organizationIds);

		/// <summary>
		/// Marks the specified organization as deprecated, indicating that it is no longer recommended for use.
		/// </summary>
		/// <param name="organization">The organization to be marked as deprecated. Cannot be null.</param>
		Organization Deprecate(Organization organization);

		/// <summary>
		/// Marks the specified organization as deprecated, indicating that it is no longer recommended for use.
		/// </summary>
		/// <param name="organizationId">The unique identifier of the organization to deprecate.</param>
		Organization Deprecate(Guid organizationId);

		/// <summary>
		/// Marks the specified organizations as deprecated, indicating that they are no longer recommended for use.
		/// </summary>
		/// <param name="organizations">A collection of organizations to deprecate. Cannot be null or empty.</param>
		IReadOnlyCollection<Organization> Deprecate(IEnumerable<Organization> organizations);

		/// <summary>
		/// Marks the specified organizations as deprecated, indicating that they are no longer recommended for use.
		/// </summary>
		/// <param name="organizationIds">The unique identifiers of the organizations to deprecate.</param>
		IReadOnlyCollection<Organization> Deprecate(IEnumerable<Guid> organizationIds);
	}
}
