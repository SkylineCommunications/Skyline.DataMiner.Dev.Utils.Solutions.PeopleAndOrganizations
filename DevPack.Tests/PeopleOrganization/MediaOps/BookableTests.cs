namespace RT_PeopleAndOrganizations.PeopleOrganization.MediaOps
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class BookableTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public BookableTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void MakeBookable_ThenDeprecateAndDelete_ManagesResourcePoolLifecycleCorrectly()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			var resourcePoolId = team.ResourcePoolId;
			Assert.AreEqual(resourcePoolId, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(resourcePoolId, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Deprecated, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			// Delete
			TestContext.Api.Teams.Delete(team);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNull(resourcePool);
		}

		[TestMethod]
		public void MakeBookable_WhenNameUpdated_SynchronizesResourcePoolName()
		{
			var prefix = Guid.NewGuid();
			var name= $"{prefix}_Team";

			var team = new Team
			{
				Name = name,
			};
			objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			var resourcePoolId = team.ResourcePoolId;
			Assert.AreEqual(resourcePoolId, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			// Update Name
			var updatedName = $"{prefix}_Updated Team";
			team.Name = updatedName;
			team = TestContext.Api.Teams.Update(team);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(updatedName, resourcePool.Name);
		}

		[TestMethod]
		public void MakeBookable_WhenSkillsUpdated_SynchronizesResourcePoolCapabilities()
		{
			var prefix = Guid.NewGuid();

			var skill1 = new Skill
			{
				Name = $"{prefix}_Skill 1",
			};
			var skill2 = new Skill
			{
				Name = $"{prefix}_Skill 2",
			};
			var skill3 = new Skill
			{
				Name = $"{prefix}_Skill 3",
			};
			objectCreator.CreateSkills([skill1, skill2, skill3]);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			}
			.SetSkills([skill1, skill2]);
			objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			var resourcePoolId = team.ResourcePoolId;
			Assert.AreEqual(resourcePoolId, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			var capabilitySetting = resourcePool.Capabilities.SingleOrDefault();
			Assert.IsNotNull(capabilitySetting);
			Assert.AreEqual(2, capabilitySetting.Discretes.Count);
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill1.Name));
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill2.Name));
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			// Update team skills
			team.SetSkills([skill1, skill3]);
			team = TestContext.Api.Teams.Update(team);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			capabilitySetting = resourcePool.Capabilities.SingleOrDefault();
			Assert.IsNotNull(capabilitySetting);
			Assert.AreEqual(2, capabilitySetting.Discretes.Count);
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill1.Name));
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill3.Name));

			// Remove team skills
			team.RemoveSkill(skill1);
			team.RemoveSkill(skill3);
			team = TestContext.Api.Teams.Update(team);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
			Assert.IsNotNull(resourcePool);
			capabilitySetting = resourcePool.Capabilities.SingleOrDefault();
			Assert.IsNull(capabilitySetting);
		}

		[TestMethod]
		public void MakeBookable_WhenResourcePoolNameExists_ThrowsExceptionForConflictingTeam()
		{
			var prefix = Guid.NewGuid();

			var team1 = new Team
			{
				Name = $"{prefix}_Team 1",
			};
			var team2 = new Team
			{
				Name = $"{prefix}_Team 2",
			};
			objectCreator.CreateTeams([team1, team2]);

			var resourcePool = new ResourcePool
			{
				Name =team1.Name,
			};
			resourcePool = objectCreator.CreateResourcePool(resourcePool);

			// Activate
			TestContext.Api.Teams.Activate([team1.Id, team2.Id]);

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable([team1.Id, team2.Id]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(team2.Id));

			Assert.AreEqual(1, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(team1.Id));

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(team1.Id, out var traceData))
			{
				Assert.Fail("Expected trace data for the unsuccessful item was not found.");
			}

			Assert.AreEqual(1, traceData.ErrorData.Count);
			var teamError = traceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamMakeBookableError = teamError as TeamMakeBookableError;
			Assert.IsNotNull(teamMakeBookableError);
			Assert.AreEqual("Name is already in use.", teamMakeBookableError.ErrorMessage);
			Assert.AreEqual(team1.Id, teamMakeBookableError.Id);
		}
	}
}
