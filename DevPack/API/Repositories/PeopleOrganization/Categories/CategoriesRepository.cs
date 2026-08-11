namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using SLDataGateway.API.Types.Querying;

	internal class CategoriesRepository : Repository, ICategoriesRepository
	{
		private readonly CategoryFilterTranslator filterTranslator = new CategoryFilterTranslator();

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesRepository"/> class.
		/// </summary>
		/// <param name="api">The People and Organizations API instance.</param>
		public CategoriesRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Category>());
		}

		public long Count(FilterElement<Category> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (filter.isEmpty())
			{
				return 0;
			}

			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.TranslateFilter(filter));
		}

		public long Count(IQuery<Category> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (query.Filter.isEmpty())
			{
				return 0;
			}

			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(TranslateToDomQuery(query));
		}

		public IReadOnlyCollection<Category> Create(IEnumerable<Category> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingCategories = list.Where(x => !x.IsNew);
			if (existingCategories.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing categories. Use CreateOrUpdate or Update instead.");
			}

			if (!DomCategoryHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Category(x)).ToList();
		}

		public Category Create(Category oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing category. Use CreateOrUpdate or Update instead.");
			}

			if (!DomCategoryHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Category(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Category> CreateOrUpdate(IEnumerable<Category> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomCategoryHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Category(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var categoryToDelete = Read(apiObjectId);
			if (categoryToDelete == null)
			{
				return;
			}

			if (!DomCategoryHandler.TryDelete(Api, [categoryToDelete], out var result))
			{
				result.ThrowSingleException(categoryToDelete.Id);
			}
		}

		public void Delete(IEnumerable<Guid> apiObjectIds)
		{
			if (apiObjectIds == null)
			{
				throw new ArgumentNullException(nameof(apiObjectIds));
			}

			var categoriesToDelete = Read(apiObjectIds.ToArray());

			if (!DomCategoryHandler.TryDelete(Api, categoriesToDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Category> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Category oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public IEnumerable<Category> Read()
		{
			return Read(new TRUEFilterElement<Category>());
		}

		public Category Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentException(nameof(id));
			}

			var category = Read(CategoryExposers.Id.Equal(id)).FirstOrDefault();

			return category;
		}

		public IEnumerable<Category> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Category>();
			}

			return Read(new ORFilterElement<Category>(ids.Select(x => CategoryExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Category> Read(FilterElement<Category> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (filter.isEmpty())
			{
				return Enumerable.Empty<Category>();
			}

			var categories = Api.DomHelpers.SlcPeopleOrganizationHelper.GetCategories(filterTranslator.TranslateFilter(filter));
			return categories.Select(x => new Category(x));
		}

		public IEnumerable<Category> Read(IQuery<Category> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (query.Filter.isEmpty())
			{
				return Enumerable.Empty<Category>();
			}

			var categories = Api.DomHelpers.SlcPeopleOrganizationHelper.GetCategories(TranslateToDomQuery(query));
			return categories.Select(x => new Category(x));
		}

		public IEnumerable<IPagedResult<Category>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Category>());
		}

		public IEnumerable<IPagedResult<Category>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Category>(), pageSize);
		}

		public IEnumerable<IPagedResult<Category>> ReadPaged(FilterElement<Category> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Category>> ReadPaged(IQuery<Category> query)
		{
			return ReadPaged(query, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Category>> ReadPaged(FilterElement<Category> filter, int pageSize)
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

		public IEnumerable<IPagedResult<Category>> ReadPaged(IQuery<Category> query, int pageSize)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			if (query.Filter.isEmpty())
			{
				return Enumerable.Empty<IPagedResult<Category>>();
			}

			return ReadPagedIterator(query, pageSize);
		}

		public IReadOnlyCollection<Category> Update(IEnumerable<Category> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newCategories = list.Where(x => x.IsNew);
			if (newCategories.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new categories. Use Create or CreateOrUpdate instead.");
			}

			if (!DomCategoryHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Category(x)).ToList();
		}

		public Category Update(Category oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new category. Use Create or CreateOrUpdate instead.");
			}

			if (!DomCategoryHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Category(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Category>> ReadPagedIterator(FilterElement<Category> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.TranslateFilter(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetCategoriesPaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Category>(page.Select(x => new Category(x)), pageNumber++, pageSize, hasNext);
			}
		}

		private IEnumerable<IPagedResult<Category>> ReadPagedIterator(IQuery<Category> query, int pageSize)
		{
			var pageNumber = 0;
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetCategoriesPaged(TranslateToDomQuery(query), pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Category>(page.Select(x => new Category(x)), pageNumber++, pageSize, hasNext);
			}
		}

		private IQuery<DomInstance> TranslateToDomQuery(IQuery<Category> query)
		{
			var domFilter = filterTranslator.TranslateFilter(query.Filter);
			var domOrderBy = filterTranslator.TranslateFullOrderBy(query.Order);

			return query
				.WithFilter(domFilter)
				.WithOrder(domOrderBy)
				.WithLimit(query.Limit);
		}
	}
}
