namespace RT_PeopleAndOrganizations.PeopleOrganization.Categories
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.Querying;
	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class QueryingTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public QueryingTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ReadWithQueryOrdersResults()
		{
			var categories = CreateCategorys(out var filter);

			QueryAssert.Read(TestContext.Api.Categories, categories, filter.ToQuery().OrderBy(CategoryExposers.Name, false));
			QueryAssert.Read(TestContext.Api.Categories, categories.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(CategoryExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var categories = CreateCategorys(out var filter);

			QueryAssert.Read(TestContext.Api.Categories, categories.Take(1).ToArray(), filter.ToQuery().OrderBy(CategoryExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.Categories, categories.Take(3).ToArray(), filter.ToQuery().OrderBy(CategoryExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.Categories, categories, filter.ToQuery().OrderBy(CategoryExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var categories = CreateCategorys(out var filter);

			QueryAssert.Count(TestContext.Api.Categories, categories, filter.ToQuery().OrderBy(CategoryExposers.Name, false));
			QueryAssert.Count(TestContext.Api.Categories, Array.Empty<Category>(), filter.AND(CategoryExposers.Name.Contains("Unknown")).ToQuery().OrderBy(CategoryExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var categories = CreateCategorys(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.Categories, categories, filter.ToQuery().OrderBy(CategoryExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.Categories, categories.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(CategoryExposers.Name, false), 2);
		}

		private Category[] CreateCategorys(out FilterElement<Category> filter)
		{
			var prefix = Guid.NewGuid();

			var categories = Enumerable.Range(0, 5)
				.Select(i => new Category { Name = $"{prefix}_Category_{i}" })
				.ToArray();

			objectCreator.CreateCategories(categories);

			filter = new ORFilterElement<Category>(categories.Select(x => CategoryExposers.Id.Equal(x.Id)).ToArray());

			return categories;
		}
	}
}
