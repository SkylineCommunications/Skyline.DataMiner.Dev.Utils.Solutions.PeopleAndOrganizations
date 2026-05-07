namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Regression tests guarding against the bug where a DOM instance in the 'Edit' status caused the
	/// repository read operations to throw or return empty collections (the 'Edit' status is not part
	/// of the public <see cref="TeamState"/> enum). The repository is expected to silently exclude
	/// Edit-state instances so callers never observe them.
	/// </summary>
	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class EditStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public EditStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ReadAllDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editTeam, activeTeam) = CreateEditAndActiveTeams(prefix);

			try
			{
				var teams = TestContext.Api.Teams.Read().ToList();

				Assert.IsTrue(teams.Any(t => t.Id == activeTeam.Id), "The Active team must be returned by Read().");
				Assert.IsFalse(teams.Any(t => t.Id == editTeam.Id), "The Edit-state team must not be returned by Read().");
			}
			finally
			{
				TransitionEditToActive(editTeam.Id);
			}
		}

		[TestMethod]
		public void ReadByIdsDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editTeam, activeTeam) = CreateEditAndActiveTeams(prefix);

			try
			{
				var teams = TestContext.Api.Teams.Read(new[] { editTeam.Id, activeTeam.Id }).ToList();

				Assert.AreEqual(1, teams.Count, "Only the Active team must be returned.");
				Assert.AreEqual(activeTeam.Id, teams.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editTeam.Id);
			}
		}

		[TestMethod]
		public void ReadByFilterDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editTeam, activeTeam) = CreateEditAndActiveTeams(prefix);

			try
			{
				var teams = TestContext.Api.Teams.Read(TeamExposers.Name.Contains($"{prefix}_Team")).ToList();

				Assert.AreEqual(1, teams.Count, "Only the Active team must be returned.");
				Assert.AreEqual(activeTeam.Id, teams.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editTeam.Id);
			}
		}

		[TestMethod]
		public void ReadByIdReturnsNullForEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editTeam, _) = CreateEditAndActiveTeams(prefix);

			try
			{
				var team = TestContext.Api.Teams.Read(editTeam.Id);

				Assert.IsNull(team, "Reading a single Edit-state team by id must return null.");
			}
			finally
			{
				TransitionEditToActive(editTeam.Id);
			}
		}

		private (Team editTeam, Team activeTeam) CreateEditAndActiveTeams(Guid prefix)
		{
			var editTeam = objectCreator.CreateTeam(new Team { Name = $"{prefix}_Team_Edit" });
			editTeam = TestContext.Api.Teams.Activate(editTeam);
			TransitionActiveToEdit(editTeam.Id);

			var activeTeam = objectCreator.CreateTeam(new Team { Name = $"{prefix}_Team_Active" });
			activeTeam = TestContext.Api.Teams.Activate(activeTeam);

			return (editTeam, activeTeam);
		}

		private static void TransitionActiveToEdit(Guid id)
		{
			TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
				new DomInstanceId(id),
				SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Transitions.Active_To_Edit);
		}

		private static void TransitionEditToActive(Guid id)
		{
			try
			{
				TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
					new DomInstanceId(id),
					SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Transitions.Edit_To_Active);
			}
			catch
			{
				// Best-effort: leave cleanup to the disposer if the transition fails.
			}
		}
	}
}
