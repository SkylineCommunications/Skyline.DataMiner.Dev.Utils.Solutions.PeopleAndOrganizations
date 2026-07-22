namespace RT_PeopleAndOrganizations.PeopleOrganization.People
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
		public void WhenUsedByTeamThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.People.Deprecate(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Person '{person.Name}' is still assigned to 1 team(s).";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInUseByTeamsError = personError as PersonInUseByTeamsError;
			Assert.IsNotNull(personInUseByTeamsError);
			Assert.AreEqual(person.Id, personInUseByTeamsError.Id);
			Assert.AreEqual(errorMessage, personInUseByTeamsError.ErrorMessage);
			Assert.AreEqual(1, personInUseByTeamsError.TeamIds.Count);
			Assert.IsTrue(personInUseByTeamsError.TeamIds.Contains(team.Id));
		}

		[TestMethod]
		public void WhenTeamMembershipIsRemoved_DeprecateSucceeds()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			var membershipToRemove = person.TeamMemberships.Single();
			person.RemoveTeamMembership(membershipToRemove);
			person = TestContext.Api.People.Update(person);

			person = TestContext.Api.People.Deprecate(person);

			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Deprecated, person.State);
			Assert.AreEqual(0, person.TeamMemberships.Count);
		}
	}
}
