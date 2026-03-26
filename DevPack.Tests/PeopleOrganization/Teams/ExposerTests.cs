namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
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
			var team1 = objectCreator.CreateTeam(new Team(Guid.NewGuid()) { Name = $"{prefix}_Team_1" });
			objectCreator.CreateTeam(new Team(Guid.NewGuid()) { Name = $"{prefix}_Team_2" });

			var returnedTeams = TestContext.Api.Teams.Read(TeamExposers.Id.Equal(team1.Id)).ToArray();

			Assert.IsNotNull(returnedTeams);
			Assert.AreEqual(1, returnedTeams.Length);
			Assert.AreEqual(team1.Id, returnedTeams[0].Id);
			Assert.AreEqual(team1.Name, returnedTeams[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var teams = Enumerable.Range(0, 10)
				.Select(i => new Team { Name = $"{prefix}_Team_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateTeams(teams);

			var returnedTeams = TestContext.Api.Teams.Read(TeamExposers.Name.Equal($"{prefix}_Team_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedTeams);
			Assert.AreEqual(1, returnedTeams.Length);
			Assert.AreEqual($"{prefix}_Team_{postfix}_1", returnedTeams[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var teams = Enumerable.Range(0, 10)
				.Select(i => new Team { Name = $"{prefix}_Team_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateTeams(teams);

			var returnedTeams = TestContext.Api.Teams.Read(TeamExposers.Name.NotEqual($"{prefix}_Team_{postfix}_2")).ToArray();
			var createdTeams = returnedTeams.Where(t => t.Name.StartsWith($"{prefix}_Team_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedTeams);
			Assert.AreEqual(9, createdTeams.Length);
			Assert.IsFalse(createdTeams.Any(t => t.Name == $"{prefix}_Team_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var teams = Enumerable.Range(0, 10)
				.Select(i => new Team { Name = $"{prefix}_Team_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateTeams(teams);

			var returnedTeams = TestContext.Api.Teams.Read(TeamExposers.Name.Contains($"{prefix}_Team_{postfix}")).ToArray();

			Assert.IsNotNull(returnedTeams);
			Assert.AreEqual(10, returnedTeams.Length);
			Assert.IsTrue(returnedTeams.All(t => t.Name.Contains($"{prefix}_Team_{postfix}", StringComparison.Ordinal)));
		}

		[TestMethod]
		public void FilterByStateEquals()
		{
			var prefix = Guid.NewGuid();

			objectCreator.CreateTeam(new Team { Name = $"{prefix}_Team_Draft" });

			var returnedTeams = TestContext.Api.Teams.Read(TeamExposers.State.Equal(TeamState.Draft)).ToArray();
			var createdTeams = returnedTeams.Where(t => t.Name.StartsWith($"{prefix}_Team_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedTeams);
			Assert.IsTrue(createdTeams.Length >= 1);
			Assert.IsTrue(createdTeams.All(t => t.State == TeamState.Draft));
		}
	}
}
