namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DeprecateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeprecateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void WhenUsedByPersonThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);

			team = TestContext.Api.Teams.Activate(team);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Teams.Deprecate(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Team '{team.Name}' is in use by 1 people.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInUseByPeopleError = teamError as TeamInUseByPeopleError;
			Assert.IsNotNull(teamInUseByPeopleError);
			Assert.AreEqual(team.Id, teamInUseByPeopleError.Id);
			Assert.AreEqual(errorMessage, teamInUseByPeopleError.ErrorMessage);
			Assert.AreEqual(1, teamInUseByPeopleError.PeopleIds.Count);
			Assert.IsTrue(teamInUseByPeopleError.PeopleIds.Contains(person.Id));
		}
	}
}
