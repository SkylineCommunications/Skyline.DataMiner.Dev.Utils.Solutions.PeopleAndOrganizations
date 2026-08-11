namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
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
			var organizations = CreateOrganizations(out var filter);

			QueryAssert.Read(TestContext.Api.Organizations, organizations, filter.ToQuery().OrderBy(OrganizationExposers.Name, false));
			QueryAssert.Read(TestContext.Api.Organizations, organizations.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(OrganizationExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var organizations = CreateOrganizations(out var filter);

			QueryAssert.Read(TestContext.Api.Organizations, organizations.Take(1).ToArray(), filter.ToQuery().OrderBy(OrganizationExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.Organizations, organizations.Take(3).ToArray(), filter.ToQuery().OrderBy(OrganizationExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.Organizations, organizations, filter.ToQuery().OrderBy(OrganizationExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var organizations = CreateOrganizations(out var filter);

			QueryAssert.Count(TestContext.Api.Organizations, organizations, filter.ToQuery().OrderBy(OrganizationExposers.Name, false));
			QueryAssert.Count(TestContext.Api.Organizations, Array.Empty<Organization>(), filter.AND(OrganizationExposers.Name.Contains("Unknown")).ToQuery().OrderBy(OrganizationExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var organizations = CreateOrganizations(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.Organizations, organizations, filter.ToQuery().OrderBy(OrganizationExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.Organizations, organizations.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(OrganizationExposers.Name, false), 2);
		}

		private Organization[] CreateOrganizations(out FilterElement<Organization> filter)
		{
			var prefix = Guid.NewGuid();

			var organizations = Enumerable.Range(0, 5)
				.Select(i => new Organization { Name = $"{prefix}_Organization_{i}" })
				.ToArray();

			objectCreator.CreateOrganizations(organizations);

			filter = new ORFilterElement<Organization>(organizations.Select(x => OrganizationExposers.Id.Equal(x.Id)).ToArray());

			return organizations;
		}
	}
}
