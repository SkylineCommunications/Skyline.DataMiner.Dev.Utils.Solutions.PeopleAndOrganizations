namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class BasicTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public BasicTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void BasicCrudActions()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var name = $"{prefix}_Team";

			var team = new Team(teamId)
			{
				Name = name,
			};

			// Create
			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(teamId, team.Id);
			Assert.AreEqual(name, team.Name);
			Assert.AreEqual(TeamState.Draft, team.State);

			var returnedTeam = TestContext.Api.Teams.Read(teamId);
			Assert.IsNotNull(returnedTeam);
			Assert.AreEqual(team.Id, returnedTeam.Id);
			Assert.AreEqual(team.Name, returnedTeam.Name);

			var domTeam = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(teamId)).SingleOrDefault();
			Assert.IsNotNull(domTeam);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Teams.Id, domTeam.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Draft, domTeam.StatusId);
			Assert.IsTrue(domTeam.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.Id.Id));
			Assert.IsTrue(domTeam.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ResourcePool.Id.Id));

			var domTeamInformation = domTeam.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.Id.Id);
			var fdTeamName = domTeamInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName.Id);
			Assert.IsNotNull(fdTeamName);
			Assert.AreEqual(returnedTeam.Name, Convert.ToString(fdTeamName.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			team.Name = updatedName;

			team = TestContext.Api.Teams.Update(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(teamId, team.Id);
			Assert.AreEqual(updatedName, team.Name);
			Assert.AreEqual(TeamState.Draft, team.State);

			returnedTeam = TestContext.Api.Teams.Read(teamId);
			Assert.IsNotNull(returnedTeam);
			Assert.AreEqual(team.Id, returnedTeam.Id);
			Assert.AreEqual(team.Name, returnedTeam.Name);

			domTeam = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(teamId)).SingleOrDefault();
			Assert.IsNotNull(domTeam);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Teams.Id, domTeam.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Draft, domTeam.StatusId);
			Assert.IsTrue(domTeam.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.Id.Id));
			Assert.IsTrue(domTeam.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ResourcePool.Id.Id));

			domTeamInformation = domTeam.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.Id.Id);
			fdTeamName = domTeamInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName.Id);
			Assert.IsNotNull(fdTeamName);
			Assert.AreEqual(returnedTeam.Name, Convert.ToString(fdTeamName.Value.Value));

			// Delete
			TestContext.Api.Teams.Delete(team);

			returnedTeam = TestContext.Api.Teams.Read(teamId);
			Assert.IsNull(returnedTeam);

			domTeam = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(teamId)).SingleOrDefault();
			Assert.IsNull(domTeam);
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team1 = new Team
			{
				Name = $"{prefix}_Team1",
			};
			var team2 = new Team
			{
				Name = $"{prefix}_Team2",
			};

			var createdTeams = objectCreator.CreateTeams([team1, team2]);
			var toUpdate = createdTeams.Single(x => x.Id == team2.Id);
			toUpdate.Name = team1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Teams.Update(toUpdate);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamNameExistsError = teamError as TeamNameExistsError;
			Assert.IsNotNull(teamNameExistsError);
			Assert.AreEqual(toUpdate.Id, teamNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, teamNameExistsError.Name);
			Assert.AreEqual(errorMessage, teamNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var teams = TestContext.Api.Teams.Read(new List<Guid>());
			Assert.IsNotNull(teams);
			Assert.AreEqual(0, teams.Count());
		}
	}
}
