namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Types.Querying;

	internal class RolesRepository : Repository, IRolesRepository
	{
		private readonly RoleFilterTranslator filterTranslator = new RoleFilterTranslator();

		/// <summary>
		/// Initializes a new instance of the <see cref="RolesRepository"/> class.
		/// </summary>
		/// <param name="api">The People and Organizations API instance.</param>
		public RolesRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Role>());
		}

		public long Count(FilterElement<Role> filter)
		{
			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.Translate(filter));
		}

		public long Count(IQuery<Role> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Role> Create(IEnumerable<Role> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingRoles = list.Where(x => !x.IsNew);
			if (existingRoles.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing roles. Use CreateOrUpdate or Update instead.");
			}

			if (!DomRoleHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Role(x)).ToList();
		}

		public Role Create(Role oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing role. Use CreateOrUpdate or Update instead.");
			}

			if (!DomRoleHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Role(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Role> CreateOrUpdate(IEnumerable<Role> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomRoleHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Role(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var roleToDelete = Read(apiObjectId);
			if (roleToDelete == null)
			{
				return;
			}

			if (!DomRoleHandler.TryDelete(Api, [roleToDelete], out var result))
			{
				result.ThrowSingleException(roleToDelete.Id);
			}
		}

		public void Delete(IEnumerable<Guid> apiObjectIds)
		{
			if (apiObjectIds == null)
			{
				throw new ArgumentNullException(nameof(apiObjectIds));
			}

			var rolesToDelete = Read(apiObjectIds.ToArray());

			if (!DomRoleHandler.TryDelete(Api, rolesToDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Role> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Role oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public IEnumerable<Role> Read()
		{
			return Read(new TRUEFilterElement<Role>());
		}

		public Role Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentException(nameof(id));
			}

			var role = Read(RoleExposers.Id.Equal(id)).FirstOrDefault();

			return role;
		}

		public IEnumerable<Role> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Role>();
			}

			return Read(new ORFilterElement<Role>(ids.Select(x => RoleExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Role> Read(FilterElement<Role> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var roles = Api.DomHelpers.SlcPeopleOrganizationHelper.GetRoles(filterTranslator.Translate(filter));
			return roles.Select(x => new Role(x));
		}

		public IEnumerable<Role> Read(IQuery<Role> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<IPagedResult<Role>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Role>());
		}

		public IEnumerable<IPagedResult<Role>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Role>(), pageSize);
		}

		public IEnumerable<IPagedResult<Role>> ReadPaged(FilterElement<Role> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Role>> ReadPaged(IQuery<Role> query)
		{
			return ReadPaged(query.Filter);
		}

		public IEnumerable<IPagedResult<Role>> ReadPaged(FilterElement<Role> filter, int pageSize)
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

		public IEnumerable<IPagedResult<Role>> ReadPaged(IQuery<Role> query, int pageSize)
		{
			return ReadPaged(query.Filter, pageSize);
		}

		public IReadOnlyCollection<Role> Update(IEnumerable<Role> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newRoles = list.Where(x => x.IsNew);
			if (newRoles.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new roles. Use Create or CreateOrUpdate instead.");
			}

			if (!DomRoleHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Role(x)).ToList();
		}

		public Role Update(Role oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new role. Use Create or CreateOrUpdate instead.");
			}

			if (!DomRoleHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Role(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Role>> ReadPagedIterator(FilterElement<Role> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.Translate(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetRolesPaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Role>(page.Select(x => new Role(x)), pageNumber++, pageSize, hasNext);
			}
		}
	}
}
