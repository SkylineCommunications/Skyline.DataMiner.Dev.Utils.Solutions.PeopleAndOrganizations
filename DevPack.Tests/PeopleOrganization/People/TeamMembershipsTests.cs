namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	using static Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections;

	using Team = Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Team;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class TeamMembershipsTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public TeamMembershipsTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void InitializeWithEmptyGuidThrowsException()
		{
			Assert.ThrowsException<ArgumentException>(() => new TeamMembership(Guid.Empty));
		}

		[TestMethod]
		public void AddWithNullThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};

			Assert.ThrowsException<ArgumentNullException>(() => person.AddTeamMembership(null));
		}

		[TestMethod]
		public void RemoveWithNullThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};

			Assert.ThrowsException<ArgumentNullException>(() => person.RemoveTeamMembership(null));
		}

		[TestMethod]
		public void RemoveNotExistingTeam()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person.RemoveTeamMembership(new TeamMembership(teamId));

			// No exception should be thrown
			Assert.IsTrue(true);
		}

		[TestMethod]
		public void CreateWithNotExistingTeamThrowsException()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(teamId));

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Team with ID '{teamId}' not found.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(teamId, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(Guid.Empty, personInvalidTeamMembershipError.RoleId);
		}

		[TestMethod]
		public void UpdateWithNotExistingTeamThrowsException()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person.AddTeamMembership(new TeamMembership(teamId));
				person = TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Team with ID '{teamId}' not found.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(teamId, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(Guid.Empty, personInvalidTeamMembershipError.RoleId);
		}

		[TestMethod]
		public void CreateWithNotExistingRoleThrowsException()
		{
			var prefix = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team)
			{
				RoleId = roleId,
			});

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Role with ID '{roleId}' not found.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(team.Id, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(roleId, personInvalidTeamMembershipError.RoleId);
		}

		[TestMethod]
		public void UpdateWithNotExistingRoleThrowsException()
		{
			var prefix = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person.AddTeamMembership(new TeamMembership(team)
				{
					RoleId = roleId,
				});
				person = TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Role with ID '{roleId}' not found.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(team.Id, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(roleId, personInvalidTeamMembershipError.RoleId);
		}

		[TestMethod]
		public void CreateWithDuplicateTeamMembershipThrowsException()
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
			.AddTeamMembership(new TeamMembership(team))
			.AddTeamMembership(new TeamMembership(team));

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Team with ID '{team.Id}' is defined {2} times.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(team.Id, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(Guid.Empty, personInvalidTeamMembershipError.RoleId);
		}

		[TestMethod]
		public void UpdateWithDuplicateTeamMembershipThrowsException()
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

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person.AddTeamMembership(new TeamMembership(team));
				person = TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual($"Team with ID '{team.Id}' is defined {2} times.", personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(team.Id, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(Guid.Empty, personInvalidTeamMembershipError.RoleId);
		}
	}
}
