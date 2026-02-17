namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	internal class SlcPeopleOrganizationHelper : DomModuleHelperBase
	{
		public SlcPeopleOrganizationHelper(IConnection connection) : base(SlcPeople_OrganizationsIds.ModuleId, connection)
		{
		}

		public long CountPeopleOrganizationInstances(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return DomHelper.DomInstances.Count(filter);
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetOrganizationIterator(filter);
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<OrganizationsInstance>();
			}

			FilterElement<DomInstance> filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => filter(x),
				x => GetOrganizationIterator(x));
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return FilterQueryExecutor.RetrieveFilteredItems(
				values.Distinct(),
				x => filter(x),
				x => GetOrganizationIterator(x));
		}

		public IEnumerable<PeopleInstance> GetPeople(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetPersonIterator(filter);
		}

		public IEnumerable<PeopleInstance> GetPeople(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<PeopleInstance>();
			}

			FilterElement<DomInstance> filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.People.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => filter(x),
				x => GetPersonIterator(x));
		}

		public IEnumerable<PeopleInstance> GetPeople<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return FilterQueryExecutor.RetrieveFilteredItems(
				values.Distinct(),
				x => filter(x),
				x => GetPersonIterator(x));
		}

		public IEnumerable<RoleInstance> GetRoles(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetRoleIterator(filter);
		}

		public IEnumerable<RoleInstance> GetRoles(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<RoleInstance>();
			}

			FilterElement<DomInstance> filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Role.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => filter(x),
				x => GetRoleIterator(x));
		}

		public IEnumerable<RoleInstance> GetRoles<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return FilterQueryExecutor.RetrieveFilteredItems(
				values.Distinct(),
				x => filter(x),
				x => GetRoleIterator(x));
		}

		public IEnumerable<DomInstance> GetPeopleOrganizationInstances(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<DomInstance>();
			}

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => DomInstanceExposers.Id.Equal(x),
				x => DomHelper.DomInstances.Read(x));
		}

		internal IEnumerable<IEnumerable<RoleInstance>> GetRolesPaged(FilterElement<DomInstance> paramFilter, int pageSize)
		{
			if (paramFilter == null)
			{
				throw new ArgumentNullException(nameof(paramFilter));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(paramFilter, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new RoleInstance(instance));
		}

		private IEnumerable<OrganizationsInstance> GetOrganizationIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new OrganizationsInstance(instance));
		}

		private IEnumerable<PeopleInstance> GetPersonIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new PeopleInstance(instance));
		}

		private IEnumerable<RoleInstance> GetRoleIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new RoleInstance(instance));
		}
	}
}
