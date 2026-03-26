namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

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
			var organization1 = objectCreator.CreateOrganization(new Organization(Guid.NewGuid()) { Name = $"{prefix}_Organization_1" });
			objectCreator.CreateOrganization(new Organization(Guid.NewGuid()) { Name = $"{prefix}_Organization_2" });

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.Id.Equal(organization1.Id)).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.AreEqual(1, returnedOrganizations.Length);
			Assert.AreEqual(organization1.Id, returnedOrganizations[0].Id);
			Assert.AreEqual(organization1.Name, returnedOrganizations[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var organizations = Enumerable.Range(0, 10)
				.Select(i => new Organization { Name = $"{prefix}_Organization_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateOrganizations(organizations);

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.Name.Equal($"{prefix}_Organization_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.AreEqual(1, returnedOrganizations.Length);
			Assert.AreEqual($"{prefix}_Organization_{postfix}_1", returnedOrganizations[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var organizations = Enumerable.Range(0, 10)
				.Select(i => new Organization { Name = $"{prefix}_Organization_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateOrganizations(organizations);

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.Name.NotEqual($"{prefix}_Organization_{postfix}_2")).ToArray();
			var createdOrganizations = returnedOrganizations.Where(o => o.Name.StartsWith($"{prefix}_Organization_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.AreEqual(9, createdOrganizations.Length);
			Assert.IsFalse(createdOrganizations.Any(o => o.Name == $"{prefix}_Organization_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var organizations = Enumerable.Range(0, 10)
				.Select(i => new Organization { Name = $"{prefix}_Organization_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateOrganizations(organizations);

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.Name.Contains($"{prefix}_Organization_{postfix}")).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.AreEqual(10, returnedOrganizations.Length);
			Assert.IsTrue(returnedOrganizations.All(o => o.Name.Contains($"{prefix}_Organization_{postfix}", StringComparison.Ordinal)));
		}

		[TestMethod]
		public void FilterByCategoryIdEquals()
		{
			var prefix = Guid.NewGuid();
			var category1 = objectCreator.CreateCategory(new Category { Name = $"{prefix}_Category_1" });
			var category2 = objectCreator.CreateCategory(new Category { Name = $"{prefix}_Category_2" });

			objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_1", CategoryId = category1.Id });
			objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_2", CategoryId = category1.Id });
			objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_3", CategoryId = category2.Id });

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.CategoryId.Equal(category1.Id)).ToArray();
			var createdOrganizations = returnedOrganizations.Where(o => o.Name.StartsWith($"{prefix}_Organization_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.AreEqual(2, createdOrganizations.Length);
			Assert.IsTrue(createdOrganizations.All(o => o.CategoryId == category1.Id));
		}

		[TestMethod]
		public void FilterByStateEquals()
		{
			var prefix = Guid.NewGuid();

			objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_Draft" });

			var returnedOrganizations = TestContext.Api.Organizations.Read(OrganizationExposers.State.Equal(OrganizationState.Draft)).ToArray();
			var createdOrganizations = returnedOrganizations.Where(o => o.Name.StartsWith($"{prefix}_Organization_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedOrganizations);
			Assert.IsTrue(createdOrganizations.Length >= 1);
			Assert.IsTrue(createdOrganizations.All(o => o.State == OrganizationState.Draft));
		}
	}
}
