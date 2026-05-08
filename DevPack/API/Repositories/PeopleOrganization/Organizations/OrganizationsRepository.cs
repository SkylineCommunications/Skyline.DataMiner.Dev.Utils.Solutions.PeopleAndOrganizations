namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using SLDataGateway.API.Types.Querying;

	internal class OrganizationsRepository : Repository, IOrganizationsRepository
	{
		private readonly OrganizationFilterTranslator filterTranslator = new OrganizationFilterTranslator();

		public OrganizationsRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public Organization Activate(Organization organization)
		{
			if (organization == null)
			{
				throw new ArgumentNullException(nameof(organization));
			}

			return Activate(organization.Id);
		}

		public Organization Activate(Guid organizationId)
		{
			var organization = Read(organizationId);
			if (organization == null)
			{
				return null;
			}

			if (!DomOrganizationHandler.TryActivate(Api, [organization], out var result))
			{
				result.ThrowSingleException(organization.Id);
			}

			return new Organization(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Organization> Activate(IEnumerable<Organization> organizations)
		{
			if (organizations == null)
			{
				throw new ArgumentNullException(nameof(organizations));
			}

			return Activate(organizations.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Organization> Activate(IEnumerable<Guid> organizationIds)
		{
			if (organizationIds == null)
			{
				throw new ArgumentNullException(nameof(organizationIds));
			}

			var organizations = Read(organizationIds);
			if (!DomOrganizationHandler.TryActivate(Api, organizations?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Organization(x)).ToList();
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Organization>());
		}

		public long Count(FilterElement<Organization> filter)
		{
			if (filter.isEmpty())
			{
				return 0;
			}

			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.Translate(filter));
		}

		public long Count(IQuery<Organization> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Organization> Create(IEnumerable<Organization> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingOrganizations = list.Where(x => !x.IsNew);
			if (existingOrganizations.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing organizations. Use CreateOrUpdate or Update instead.");
			}

			if (!DomOrganizationHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Organization(x)).ToList();
		}

		public Organization Create(Organization oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing organization. Use CreateOrUpdate or Update instead.");
			}

			if (!DomOrganizationHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Organization(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Organization> CreateOrUpdate(IEnumerable<Organization> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomOrganizationHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Organization(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var toDelete = Read(apiObjectId);
			if (toDelete == null)
			{
				return;
			}

			if (!DomOrganizationHandler.TryDelete(Api, [toDelete], out var result))
			{
				result.ThrowSingleException(toDelete.Id);
			}
		}

		public void Delete(IEnumerable<Guid> apiObjectIds)
		{
			if (apiObjectIds == null)
			{
				throw new ArgumentNullException(nameof(apiObjectIds));
			}

			var toDelete = Read(apiObjectIds.ToArray());

			if (!DomOrganizationHandler.TryDelete(Api, toDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Organization> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Organization oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public Organization Deprecate(Organization organization)
		{
			if (organization == null)
			{
				throw new ArgumentNullException(nameof(organization));
			}

			return Deprecate(organization.Id);
		}

		public Organization Deprecate(Guid organizationId)
		{
			var organization = Read(organizationId);
			if (organization == null)
			{
				return null;
			}

			if (!DomOrganizationHandler.TryDeprecate(Api, [organization], out var result))
			{
				result.ThrowSingleException(organization.Id);
			}

			return new Organization(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Organization> Deprecate(IEnumerable<Organization> organizations)
		{
			if (organizations == null)
			{
				throw new ArgumentNullException(nameof(organizations));
			}

			return Deprecate(organizations.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Organization> Deprecate(IEnumerable<Guid> organizationIds)
		{
			if (organizationIds == null)
			{
				throw new ArgumentNullException(nameof(organizationIds));
			}

			var organizations = Read(organizationIds);
			if (!DomOrganizationHandler.TryDeprecate(Api, organizations?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Organization(x)).ToList();
		}

		public IEnumerable<Organization> Read()
		{
			return Read(new TRUEFilterElement<Organization>());
		}

		public Organization Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(id));
			}

			var organization = Read(OrganizationExposers.Id.Equal(id)).FirstOrDefault();

			return organization;
		}

		public IEnumerable<Organization> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Organization>();
			}

			return Read(new ORFilterElement<Organization>(ids.Select(x => OrganizationExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Organization> Read(FilterElement<Organization> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (filter.isEmpty())
			{
				return Enumerable.Empty<Organization>();
			}

			var organizations = Api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizations(filterTranslator.Translate(filter));
			return organizations.Select(x => new Organization(x));
		}

		public IEnumerable<Organization> Read(IQuery<Organization> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Organization>());
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Organization>(), pageSize);
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged(FilterElement<Organization> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged(IQuery<Organization> query)
		{
			return ReadPaged(query.Filter);
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged(FilterElement<Organization> filter, int pageSize)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			return ReadPagedIterator(filter, pageSize);
		}

		public IEnumerable<IPagedResult<Organization>> ReadPaged(IQuery<Organization> query, int pageSize)
		{
			return ReadPaged(query.Filter, pageSize);
		}

		public IReadOnlyCollection<Organization> Update(IEnumerable<Organization> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newRoles = list.Where(x => x.IsNew);
			if (newRoles.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new organizations. Use Create or CreateOrUpdate instead.");
			}

			if (!DomOrganizationHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Organization(x)).ToList();
		}

		public Organization Update(Organization oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new organization. Use Create or CreateOrUpdate instead.");
			}

			if (!DomOrganizationHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Organization(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Organization>> ReadPagedIterator(FilterElement<Organization> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.Translate(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizationsPaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Organization>(page.Select(x => new Organization(x)), pageNumber++, pageSize, hasNext);
			}
		}
	}
}
