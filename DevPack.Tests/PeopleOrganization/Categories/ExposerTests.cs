namespace RT_PeopleAndOrganizations.PeopleOrganization.Categories
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class ExposerTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public ExposerTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void FilterByIdEquals()
		{
			var prefix = Guid.NewGuid();
			var category1 = objectCreator.CreateCategory(new Category(Guid.NewGuid()) { Name = $"{prefix}_Category_1" });
			objectCreator.CreateCategory(new Category(Guid.NewGuid()) { Name = $"{prefix}_Category_2" });

			var returnedCategories = TestContext.Api.Categories.Read(CategoryExposers.Id.Equal(category1.Id)).ToArray();

			Assert.IsNotNull(returnedCategories);
			Assert.AreEqual(1, returnedCategories.Length);
			Assert.AreEqual(category1.Id, returnedCategories[0].Id);
			Assert.AreEqual(category1.Name, returnedCategories[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var categories = Enumerable.Range(0, 10)
				.Select(i => new Category
				{
					Name = $"{prefix}_Category_{postfix}_{i}",
				})
				.ToArray();

			objectCreator.CreateCategories(categories);

			var returnedCategories = TestContext.Api.Categories.Read(CategoryExposers.Name.Equal($"{prefix}_Category_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedCategories);
			Assert.AreEqual(1, returnedCategories.Length);
			Assert.AreEqual($"{prefix}_Category_{postfix}_1", returnedCategories[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var categories = Enumerable.Range(0, 10)
				.Select(i => new Category
				{
					Name = $"{prefix}_Category_{postfix}_{i}",
				})
				.ToArray();

			objectCreator.CreateCategories(categories);

			var returnedCategories = TestContext.Api.Categories.Read(CategoryExposers.Name.NotEqual($"{prefix}_Category_{postfix}_2")).ToArray();
			var createdCategories = returnedCategories.Where(c => c.Name.StartsWith($"{prefix}_Category_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedCategories);
			Assert.AreEqual(9, createdCategories.Length);
			Assert.IsFalse(createdCategories.Any(c => c.Name == $"{prefix}_Category_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var categories = Enumerable.Range(0, 10)
				.Select(i => new Category
				{
					Name = $"{prefix}_Category_{postfix}_{i}",
				})
				.ToArray();

			objectCreator.CreateCategories(categories);

			var returnedCategories = TestContext.Api.Categories.Read(CategoryExposers.Name.Contains($"{prefix}_Category_{postfix}")).ToArray();

			Assert.IsNotNull(returnedCategories);
			Assert.AreEqual(10, returnedCategories.Length);
			Assert.IsTrue(returnedCategories.All(c => c.Name.Contains($"{prefix}_Category_{postfix}", StringComparison.Ordinal)));
		}
	}
}
