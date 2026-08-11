namespace RT_PeopleAndOrganizations.PeopleOrganization.Roles
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
			var roles = CreateRoles(out var filter);

			QueryAssert.Read(TestContext.Api.Roles, roles, filter.ToQuery().OrderBy(RoleExposers.Name, false));
			QueryAssert.Read(TestContext.Api.Roles, roles.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(RoleExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var roles = CreateRoles(out var filter);

			QueryAssert.Read(TestContext.Api.Roles, roles.Take(1).ToArray(), filter.ToQuery().OrderBy(RoleExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.Roles, roles.Take(3).ToArray(), filter.ToQuery().OrderBy(RoleExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.Roles, roles, filter.ToQuery().OrderBy(RoleExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var roles = CreateRoles(out var filter);

			QueryAssert.Count(TestContext.Api.Roles, roles, filter.ToQuery().OrderBy(RoleExposers.Name, false));
			QueryAssert.Count(TestContext.Api.Roles, Array.Empty<Role>(), filter.AND(RoleExposers.Name.Contains("Unknown")).ToQuery().OrderBy(RoleExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var roles = CreateRoles(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.Roles, roles, filter.ToQuery().OrderBy(RoleExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.Roles, roles.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(RoleExposers.Name, false), 2);
		}

		private Role[] CreateRoles(out FilterElement<Role> filter)
		{
			var prefix = Guid.NewGuid();

			var roles = Enumerable.Range(0, 5)
				.Select(i => new Role { Name = $"{prefix}_Role_{i}" })
				.ToArray();

			objectCreator.CreateRoles(roles);

			filter = new ORFilterElement<Role>(roles.Select(x => RoleExposers.Id.Equal(x.Id)).ToArray());

			return roles;
		}
	}
}
