namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
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
			var teams = CreateTeams(out var filter);

			QueryAssert.Read(TestContext.Api.Teams, teams, filter.ToQuery().OrderBy(TeamExposers.Name, false));
			QueryAssert.Read(TestContext.Api.Teams, teams.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(TeamExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var teams = CreateTeams(out var filter);

			QueryAssert.Read(TestContext.Api.Teams, teams.Take(1).ToArray(), filter.ToQuery().OrderBy(TeamExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.Teams, teams.Take(3).ToArray(), filter.ToQuery().OrderBy(TeamExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.Teams, teams, filter.ToQuery().OrderBy(TeamExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var teams = CreateTeams(out var filter);

			QueryAssert.Count(TestContext.Api.Teams, teams, filter.ToQuery().OrderBy(TeamExposers.Name, false));
			QueryAssert.Count(TestContext.Api.Teams, Array.Empty<Team>(), filter.AND(TeamExposers.Name.Contains("Unknown")).ToQuery().OrderBy(TeamExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var teams = CreateTeams(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.Teams, teams, filter.ToQuery().OrderBy(TeamExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.Teams, teams.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(TeamExposers.Name, false), 2);
		}

		private Team[] CreateTeams(out FilterElement<Team> filter)
		{
			var prefix = Guid.NewGuid();

			var teams = Enumerable.Range(0, 5)
				.Select(i => new Team { Name = $"{prefix}_Team_{i}" })
				.ToArray();

			objectCreator.CreateTeams(teams);

			filter = new ORFilterElement<Team>(teams.Select(x => TeamExposers.Id.Equal(x.Id)).ToArray());

			return teams;
		}
	}
}
