namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DraftStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DraftStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void Activate()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(TeamState.Active, team.State);

			var domTeam = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(team.Id)).SingleOrDefault();
			Assert.IsNotNull(domTeam);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Active, domTeam.StatusId);
		}

		[TestMethod]
		public void DeprecateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Deprecate
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Deprecate(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to deprecate a team that is not in Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void Delete()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			var teamId = team.Id;

			// Delete
			TestContext.Api.Teams.Delete(team);

			team = TestContext.Api.Teams.Read(teamId);
			Assert.IsNull(team);

			var domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(teamId)).SingleOrDefault();
			Assert.IsNull(domOrganization);
		}

		[TestMethod]
		public void UpdateName()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Team";

			var team = new Team
			{
				Name = name,
			};

			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(name, team.Name);

			// Update name
			var updatedName = $"{name}_Updated";
			team.Name = updatedName;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(updatedName, team.Name);
		}

		[TestMethod]
		public void AssignEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Assign email
			team.Email = email;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(email, team.Email);
		}

		[TestMethod]
		public void UpdateEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var team = new Team
			{
				Name = $"{prefix}_Team",
				Email = email,
			};
			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(email, team.Email);

			// Update email
			var updatedEmail = "support@skyline.be";
			team.Email = updatedEmail;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(updatedEmail, team.Email);
		}

		[TestMethod]
		public void AssignDescription()
		{
			var prefix = Guid.NewGuid();
			var description = "my description";

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Assign description
			team.Description = description;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(description, team.Description);
		}

		[TestMethod]
		public void UpdateDescription()
		{
			var prefix = Guid.NewGuid();
			var description = "my description";

			var team = new Team
			{
				Name = $"{prefix}_Team",
				Description = description,
			};
			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(description, team.Description);

			// Update description
			var updatedDescription = "my updated description";
			team.Description = updatedDescription;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(updatedDescription, team.Description);
		}
	}
}
