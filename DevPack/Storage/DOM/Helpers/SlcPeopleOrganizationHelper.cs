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

	using SLDataGateway.API.Types.Querying;

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

		public long CountPeopleOrganizationInstances(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return DomHelper.DomInstances.Count(query);
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetOrganizationIterator(filter);
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetOrganizationIterator(query);
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

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
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
				filter,
				x => GetOrganizationIterator(x));
		}

		public IEnumerable<TeamsInstance> GetTeams(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetTeamIterator(filter);
		}

		public IEnumerable<TeamsInstance> GetTeams(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetTeamIterator(query);
		}

		public IEnumerable<TeamsInstance> GetTeams(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<TeamsInstance>();
			}

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Teams.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
				x => GetTeamIterator(x));
		}

		public IEnumerable<TeamsInstance> GetTeams<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
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
				filter,
				x => GetTeamIterator(x));
		}

		public IEnumerable<PeopleInstance> GetPeople(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetPersonIterator(filter);
		}

		public IEnumerable<PeopleInstance> GetPeople(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetPersonIterator(query);
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

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.People.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
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
				filter,
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

		public IEnumerable<RoleInstance> GetRoles(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetRoleIterator(query);
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

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Role.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
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

		public IEnumerable<CategoryInstance> GetCategories(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetCategoryIterator(filter);
		}

		public IEnumerable<CategoryInstance> GetCategories(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetCategoryIterator(query);
		}

		public IEnumerable<CategoryInstance> GetCategories(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<CategoryInstance>();
			}

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Category.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
				x => GetCategoryIterator(x));
		}

		public IEnumerable<CategoryInstance> GetCategories<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
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
				x => GetCategoryIterator(x));
		}

		public IEnumerable<ExperienceInstance> GetExperience(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetExperienceIterator(filter);
		}

		public IEnumerable<ExperienceInstance> GetExperience(IQuery<DomInstance> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return GetExperienceIterator(query);
		}

		public IEnumerable<ExperienceInstance> GetExperience(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Enumerable.Empty<ExperienceInstance>();
			}

			FilterElement<DomInstance> Filter(Guid id) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Experience.Id)
				.AND(DomInstanceExposers.Id.Equal(id));

			return FilterQueryExecutor.RetrieveFilteredItems(
				ids.Distinct(),
				x => Filter(x),
				x => GetExperienceIterator(x));
		}

		public IEnumerable<ExperienceInstance> GetExperience<T>(IEnumerable<T> values, Func<T, FilterElement<DomInstance>> filter)
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
				x => GetExperienceIterator(x));
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

		internal IEnumerable<IEnumerable<OrganizationsInstance>> GetOrganizationsPaged(FilterElement<DomInstance> paramFilter, int pageSize)
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
			return InstanceFactory.CreateInstances(pages, instance => new OrganizationsInstance(instance));
		}

		internal IEnumerable<IEnumerable<OrganizationsInstance>> GetOrganizationsPaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new OrganizationsInstance(instance));
		}

		internal IEnumerable<IEnumerable<TeamsInstance>> GetTeamsPaged(FilterElement<DomInstance> paramFilter, int pageSize)
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
			return InstanceFactory.CreateInstances(pages, instance => new TeamsInstance(instance));
		}

		internal IEnumerable<IEnumerable<TeamsInstance>> GetTeamsPaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new TeamsInstance(instance));
		}

		internal IEnumerable<IEnumerable<PeopleInstance>> GetPeoplePaged(FilterElement<DomInstance> paramFilter, int pageSize)
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
			return InstanceFactory.CreateInstances(pages, instance => new PeopleInstance(instance));
		}

		internal IEnumerable<IEnumerable<PeopleInstance>> GetPeoplePaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new PeopleInstance(instance));
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

		internal IEnumerable<IEnumerable<RoleInstance>> GetRolesPaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new RoleInstance(instance));
		}

		internal IEnumerable<IEnumerable<CategoryInstance>> GetCategoriesPaged(FilterElement<DomInstance> paramFilter, int pageSize)
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
			return InstanceFactory.CreateInstances(pages, instance => new CategoryInstance(instance));
		}

		internal IEnumerable<IEnumerable<CategoryInstance>> GetCategoriesPaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new CategoryInstance(instance));
		}

		internal IEnumerable<IEnumerable<ExperienceInstance>> GetExperiencePaged(FilterElement<DomInstance> paramFilter, int pageSize)
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
			return InstanceFactory.CreateInstances(pages, instance => new ExperienceInstance(instance));
		}

		internal IEnumerable<IEnumerable<ExperienceInstance>> GetExperiencePaged(IQuery<DomInstance> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			var pages = DomHelper.DomInstances.ReadPaged(query, pageSize);
			return InstanceFactory.CreateInstances(pages, instance => new ExperienceInstance(instance));
		}

		private IEnumerable<OrganizationsInstance> GetOrganizationIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new OrganizationsInstance(instance));
		}

		private IEnumerable<OrganizationsInstance> GetOrganizationIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new OrganizationsInstance(instance));
		}

		private IEnumerable<TeamsInstance> GetTeamIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new TeamsInstance(instance));
		}

		private IEnumerable<TeamsInstance> GetTeamIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new TeamsInstance(instance));
		}

		private IEnumerable<PeopleInstance> GetPersonIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new PeopleInstance(instance));
		}

		private IEnumerable<PeopleInstance> GetPersonIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new PeopleInstance(instance));
		}

		private IEnumerable<RoleInstance> GetRoleIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new RoleInstance(instance));
		}

		private IEnumerable<RoleInstance> GetRoleIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new RoleInstance(instance));
		}

		private IEnumerable<CategoryInstance> GetCategoryIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new CategoryInstance(instance));
		}

		private IEnumerable<CategoryInstance> GetCategoryIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new CategoryInstance(instance));
		}

		private IEnumerable<ExperienceInstance> GetExperienceIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new ExperienceInstance(instance));
		}

		private IEnumerable<ExperienceInstance> GetExperienceIterator(IQuery<DomInstance> query)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, query, instance => new ExperienceInstance(instance));
		}
	}
}
