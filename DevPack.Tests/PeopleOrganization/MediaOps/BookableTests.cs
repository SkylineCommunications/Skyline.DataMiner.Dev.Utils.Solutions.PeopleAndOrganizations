namespace RT_PeopleAndOrganizations.PeopleOrganization.MediaOps
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	using CoreResource = Skyline.DataMiner.Net.Messages.Resource;
	using CoreResourcePool = Skyline.DataMiner.Net.Messages.ResourcePool;

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
			Assert.AreNotEqual(Guid.Empty, resourcePoolId);
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
			var name = $"{prefix}_Team";

			var team = new Team
			{
				Name = name,
			};
			objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			// Update Name
			var updatedName = $"{prefix}_Updated Team";
			team.Name = updatedName;
			team = TestContext.Api.Teams.Update(team);

			resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
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
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
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

			resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
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

			resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
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
				Name = team1.Name,
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

			// Verify that the resource pool was not created for team 1
			team1 = TestContext.Api.Teams.Read(team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(Guid.Empty, team1.ResourcePoolId);

			// Verify that the resource pool was created for team 2
			team2 = TestContext.Api.Teams.Read(team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreNotEqual(Guid.Empty, team2.ResourcePoolId);

			var resourcePool2 = TestContext.PlanApi.ResourcePools.Read(team2.ResourcePoolId);
			Assert.IsNotNull(resourcePool2);
			Assert.AreEqual(team2.Name, resourcePool2.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool2.State);
			Assert.AreEqual(0, resourcePool2.Capabilities.Count);
			Assert.AreEqual(0, resourcePool2.LinkedResourcePools.Count);
		}

		[TestMethod]
		public void MakeBookable_WhenCoreResourcePoolNameExists_ThrowsExceptionForConflictingTeam()
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

			var resourcePool = new CoreResourcePool
			{
				Name = team1.Name,
			};
			objectCreator.CreateCoreResourcePool(resourcePool);

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

			// Verify that the resource pool was not created for team 1
			team1 = TestContext.Api.Teams.Read(team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(Guid.Empty, team1.ResourcePoolId);

			var resourcePool1 = TestContext.PlanApi.ResourcePools.Read(ResourcePoolExposers.Name.Equal(team1.Name)).FirstOrDefault();
			Assert.IsNull(resourcePool1);

			// Verify that the resource pool was created for team 2
			team2 = TestContext.Api.Teams.Read(team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreNotEqual(Guid.Empty, team2.ResourcePoolId);

			var resourcePool2 = TestContext.PlanApi.ResourcePools.Read(team2.ResourcePoolId);
			Assert.IsNotNull(resourcePool2);
			Assert.AreEqual(team2.Name, resourcePool2.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool2.State);
			Assert.AreEqual(0, resourcePool2.Capabilities.Count);
			Assert.AreEqual(0, resourcePool2.LinkedResourcePools.Count);
		}

		[TestMethod]
		public void MakeBookable_WhenTeamsMadeBookableSequentially_AddsSharedMembersToAllResourcePools()
		{
			/*
			 * Make T1 and T2 bookable after each other
			 *		T1			T2
			 *		|-> P1		|-> P4
			 *		|-> P2		|-> P2
			 *		|-> P3		|-> P5
			 * */

			var prefix = Guid.NewGuid();

			var team1 = new Team()
			{
				Name = $"{prefix}_Team 1",
			};
			var team2 = new Team()
			{
				Name = $"{prefix}_Team 2",
			};
			objectCreator.CreateTeams([team1, team2]);
			TestContext.Api.Teams.Activate([team1.Id, team2.Id]);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team1))
			.AddTeamMembership(new TeamMembership(team2));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person4 = new Person()
			{
				Name = $"{prefix}_Person 4",
			}
			.AddTeamMembership(new TeamMembership(team2));
			var person5 = new Person()
			{
				Name = $"{prefix}_Person 5",
			}
			.AddTeamMembership(new TeamMembership(team2));
			objectCreator.CreatePeople([person1, person2, person3, person4, person5]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id]);

			// Make team 1 bookable
			team1 = TestContext.Api.Teams.MakeBookable(team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(true, team1.IsBookable);
			Assert.AreNotEqual(Guid.Empty, team1.ResourcePoolId);

			var resourcePool1 = TestContext.PlanApi.ResourcePools.Read(team1.ResourcePoolId);
			Assert.IsNotNull(resourcePool1);
			Assert.AreEqual(team1.Name, resourcePool1.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool1.State);
			Assert.AreEqual(0, resourcePool1.Capabilities.Count);
			Assert.AreEqual(0, resourcePool1.LinkedResourcePools.Count);

			// Get people
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id]);
			var resources = TestContext.PlanApi.Resources.Read(people.Select(x => x.ResourceId).ToList());

			// Verify person 1
			person1 = people.SingleOrDefault(x => x.Id == person1.Id);
			Assert.IsNotNull(person1);
			Assert.AreNotEqual(Guid.Empty, person1.ResourceId);

			var resource1 = resources.SingleOrDefault(x => x.Id == person1.ResourceId);
			Assert.IsNotNull(resource1);
			Assert.AreEqual(person1.Name, resource1.Name);
			Assert.AreEqual(ResourceState.Complete, resource1.State);
			Assert.AreEqual(0, resource1.Capabilities.Count);
			Assert.AreEqual(0, resource1.Capacities.Count);
			Assert.AreEqual(0, resource1.Properties.Count);
			Assert.AreEqual(1, resource1.ResourcePoolIds.Count);
			Assert.IsTrue(resource1.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 2
			person2 = people.SingleOrDefault(x => x.Id == person2.Id);
			Assert.IsNotNull(person2);
			Assert.AreNotEqual(Guid.Empty, person2.ResourceId);

			var resource2 = resources.SingleOrDefault(x => x.Id == person2.ResourceId);
			Assert.IsNotNull(resource2);
			Assert.AreEqual(person2.Name, resource2.Name);
			Assert.AreEqual(ResourceState.Complete, resource2.State);
			Assert.AreEqual(0, resource2.Capabilities.Count);
			Assert.AreEqual(0, resource2.Capacities.Count);
			Assert.AreEqual(0, resource2.Properties.Count);
			Assert.AreEqual(1, resource2.ResourcePoolIds.Count);
			Assert.IsTrue(resource2.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 3
			person3 = people.SingleOrDefault(x => x.Id == person3.Id);
			Assert.IsNotNull(person3);
			Assert.AreNotEqual(Guid.Empty, person3.ResourceId);

			var resource3 = resources.SingleOrDefault(x => x.Id == person3.ResourceId);
			Assert.IsNotNull(resource3);
			Assert.AreEqual(person3.Name, resource3.Name);
			Assert.AreEqual(ResourceState.Complete, resource3.State);
			Assert.AreEqual(0, resource3.Capabilities.Count);
			Assert.AreEqual(0, resource3.Capacities.Count);
			Assert.AreEqual(0, resource3.Properties.Count);
			Assert.AreEqual(1, resource3.ResourcePoolIds.Count);
			Assert.IsTrue(resource3.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 4
			person4 = people.SingleOrDefault(x => x.Id == person4.Id);
			Assert.IsNotNull(person4);
			Assert.AreEqual(Guid.Empty, person4.ResourceId);

			var resource4 = resources.SingleOrDefault(x => x.Id == person4.ResourceId);
			Assert.IsNull(resource4);

			// Verify person 5
			person5 = people.SingleOrDefault(x => x.Id == person5.Id);
			Assert.IsNotNull(person5);
			Assert.AreEqual(Guid.Empty, person5.ResourceId);

			var resource5 = resources.SingleOrDefault(x => x.Id == person5.ResourceId);
			Assert.IsNull(resource5);

			// Make team 2 bookable
			team2 = TestContext.Api.Teams.MakeBookable(team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreEqual(true, team2.IsBookable);
			Assert.AreNotEqual(Guid.Empty, team2.ResourcePoolId);

			var resourcePool2 = TestContext.PlanApi.ResourcePools.Read(team2.ResourcePoolId);
			Assert.IsNotNull(resourcePool2);
			Assert.AreEqual(team2.Name, resourcePool2.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool2.State);
			Assert.AreEqual(0, resourcePool2.Capabilities.Count);
			Assert.AreEqual(0, resourcePool2.LinkedResourcePools.Count);

			// Get people
			people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id]);
			resources = TestContext.PlanApi.Resources.Read(people.Select(x => x.ResourceId).ToList());

			// Verify person 1
			person1 = people.SingleOrDefault(x => x.Id == person1.Id);
			Assert.IsNotNull(person1);
			Assert.AreNotEqual(Guid.Empty, person1.ResourceId);

			resource1 = resources.SingleOrDefault(x => x.Id == person1.ResourceId);
			Assert.IsNotNull(resource1);
			Assert.AreEqual(person1.Name, resource1.Name);
			Assert.AreEqual(ResourceState.Complete, resource1.State);
			Assert.AreEqual(0, resource1.Capabilities.Count);
			Assert.AreEqual(0, resource1.Capacities.Count);
			Assert.AreEqual(0, resource1.Properties.Count);
			Assert.AreEqual(1, resource1.ResourcePoolIds.Count);
			Assert.IsTrue(resource1.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 2
			person2 = people.SingleOrDefault(x => x.Id == person2.Id);
			Assert.IsNotNull(person2);
			Assert.AreNotEqual(Guid.Empty, person2.ResourceId);

			resource2 = resources.SingleOrDefault(x => x.Id == person2.ResourceId);
			Assert.IsNotNull(resource2);
			Assert.AreEqual(person2.Name, resource2.Name);
			Assert.AreEqual(ResourceState.Complete, resource2.State);
			Assert.AreEqual(0, resource2.Capabilities.Count);
			Assert.AreEqual(0, resource2.Capacities.Count);
			Assert.AreEqual(0, resource2.Properties.Count);
			Assert.AreEqual(2, resource2.ResourcePoolIds.Count);
			Assert.IsTrue(resource2.ResourcePoolIds.Contains(resourcePool1.Id));
			Assert.IsTrue(resource2.ResourcePoolIds.Contains(resourcePool2.Id));

			// Verify person 3
			person3 = people.SingleOrDefault(x => x.Id == person3.Id);
			Assert.IsNotNull(person3);
			Assert.AreNotEqual(Guid.Empty, person3.ResourceId);

			resource3 = resources.SingleOrDefault(x => x.Id == person3.ResourceId);
			Assert.IsNotNull(resource3);
			Assert.AreEqual(person3.Name, resource3.Name);
			Assert.AreEqual(ResourceState.Complete, resource3.State);
			Assert.AreEqual(0, resource3.Capabilities.Count);
			Assert.AreEqual(0, resource3.Capacities.Count);
			Assert.AreEqual(0, resource3.Properties.Count);
			Assert.AreEqual(1, resource3.ResourcePoolIds.Count);
			Assert.IsTrue(resource3.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 4
			person4 = people.SingleOrDefault(x => x.Id == person4.Id);
			Assert.IsNotNull(person4);
			Assert.AreNotEqual(Guid.Empty, person4.ResourceId);

			resource4 = resources.SingleOrDefault(x => x.Id == person4.ResourceId);
			Assert.IsNotNull(resource4);
			Assert.AreEqual(person4.Name, resource4.Name);
			Assert.AreEqual(ResourceState.Complete, resource4.State);
			Assert.AreEqual(0, resource4.Capabilities.Count);
			Assert.AreEqual(0, resource4.Capacities.Count);
			Assert.AreEqual(0, resource4.Properties.Count);
			Assert.AreEqual(1, resource4.ResourcePoolIds.Count);
			Assert.IsTrue(resource4.ResourcePoolIds.Contains(resourcePool2.Id));

			// Verify person 5
			person5 = people.SingleOrDefault(x => x.Id == person5.Id);
			Assert.IsNotNull(person5);
			Assert.AreNotEqual(Guid.Empty, person5.ResourceId);

			resource5 = resources.SingleOrDefault(x => x.Id == person5.ResourceId);
			Assert.IsNotNull(resource5);
			Assert.AreEqual(person5.Name, resource5.Name);
			Assert.AreEqual(ResourceState.Complete, resource5.State);
			Assert.AreEqual(0, resource5.Capabilities.Count);
			Assert.AreEqual(0, resource5.Capacities.Count);
			Assert.AreEqual(0, resource5.Properties.Count);
			Assert.AreEqual(1, resource5.ResourcePoolIds.Count);
			Assert.IsTrue(resource5.ResourcePoolIds.Contains(resourcePool2.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenTeamsMadeBookableInBulk_AddsSharedMembersToAllResourcePools()
		{
			/*
			 * Make T1 and T2 bookable in bulk
			 *		T1			T2
			 *		|-> P1		|-> P4
			 *		|-> P2		|-> P2
			 *		|-> P3		|-> P5
			 * */

			var prefix = Guid.NewGuid();

			var team1 = new Team()
			{
				Name = $"{prefix}_Team 1",
			};
			var team2 = new Team()
			{
				Name = $"{prefix}_Team 2",
			};
			objectCreator.CreateTeams([team1, team2]);
			TestContext.Api.Teams.Activate([team1.Id, team2.Id]);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team1))
			.AddTeamMembership(new TeamMembership(team2));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person4 = new Person()
			{
				Name = $"{prefix}_Person 4",
			}
			.AddTeamMembership(new TeamMembership(team2));
			var person5 = new Person()
			{
				Name = $"{prefix}_Person 5",
			}
			.AddTeamMembership(new TeamMembership(team2));
			objectCreator.CreatePeople([person1, person2, person3, person4, person5]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id]);

			// Make bookable
			var teams = TestContext.Api.Teams.MakeBookable([team1.Id, team2.Id]);
			var resourcePools = TestContext.PlanApi.ResourcePools.Read(teams.Select(x => x.ResourcePoolId).ToList());

			// Verify team 1
			team1 = teams.SingleOrDefault(x => x.Id == team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(true, team1.IsBookable);
			Assert.AreNotEqual(Guid.Empty, team1.ResourcePoolId);

			var resourcePool1 = resourcePools.SingleOrDefault(x => x.Id == team1.ResourcePoolId);
			Assert.IsNotNull(resourcePool1);
			Assert.AreEqual(team1.Name, resourcePool1.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool1.State);
			Assert.AreEqual(0, resourcePool1.Capabilities.Count);
			Assert.AreEqual(0, resourcePool1.LinkedResourcePools.Count);

			// Verify team 2
			team2 = teams.SingleOrDefault(x => x.Id == team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreEqual(true, team2.IsBookable);
			Assert.AreNotEqual(Guid.Empty, team2.ResourcePoolId);

			var resourcePool2 = resourcePools.SingleOrDefault(x => x.Id == team2.ResourcePoolId);
			Assert.IsNotNull(resourcePool2);
			Assert.AreEqual(team2.Name, resourcePool2.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool2.State);
			Assert.AreEqual(0, resourcePool2.Capabilities.Count);
			Assert.AreEqual(0, resourcePool2.LinkedResourcePools.Count);

			// Get people
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id]);
			var resources = TestContext.PlanApi.Resources.Read(people.Select(x => x.ResourceId).ToList());

			// Verify person 1
			person1 = people.SingleOrDefault(x => x.Id == person1.Id);
			Assert.IsNotNull(person1);
			Assert.AreNotEqual(Guid.Empty, person1.ResourceId);

			var resource1 = resources.SingleOrDefault(x => x.Id == person1.ResourceId);
			Assert.IsNotNull(resource1);
			Assert.AreEqual(person1.Name, resource1.Name);
			Assert.AreEqual(ResourceState.Complete, resource1.State);
			Assert.AreEqual(0, resource1.Capabilities.Count);
			Assert.AreEqual(0, resource1.Capacities.Count);
			Assert.AreEqual(0, resource1.Properties.Count);
			Assert.AreEqual(1, resource1.ResourcePoolIds.Count);
			Assert.IsTrue(resource1.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 2
			person2 = people.SingleOrDefault(x => x.Id == person2.Id);
			Assert.IsNotNull(person2);
			Assert.AreNotEqual(Guid.Empty, person2.ResourceId);

			var resource2 = resources.SingleOrDefault(x => x.Id == person2.ResourceId);
			Assert.IsNotNull(resource2);
			Assert.AreEqual(person2.Name, resource2.Name);
			Assert.AreEqual(ResourceState.Complete, resource2.State);
			Assert.AreEqual(0, resource2.Capabilities.Count);
			Assert.AreEqual(0, resource2.Capacities.Count);
			Assert.AreEqual(0, resource2.Properties.Count);
			Assert.AreEqual(2, resource2.ResourcePoolIds.Count);
			Assert.IsTrue(resource2.ResourcePoolIds.Contains(resourcePool1.Id));
			Assert.IsTrue(resource2.ResourcePoolIds.Contains(resourcePool2.Id));

			// Verify person 3
			person3 = people.SingleOrDefault(x => x.Id == person3.Id);
			Assert.IsNotNull(person3);
			Assert.AreNotEqual(Guid.Empty, person3.ResourceId);

			var resource3 = resources.SingleOrDefault(x => x.Id == person3.ResourceId);
			Assert.IsNotNull(resource3);
			Assert.AreEqual(person3.Name, resource3.Name);
			Assert.AreEqual(ResourceState.Complete, resource3.State);
			Assert.AreEqual(0, resource3.Capabilities.Count);
			Assert.AreEqual(0, resource3.Capacities.Count);
			Assert.AreEqual(0, resource3.Properties.Count);
			Assert.AreEqual(1, resource3.ResourcePoolIds.Count);
			Assert.IsTrue(resource3.ResourcePoolIds.Contains(resourcePool1.Id));

			// Verify person 4
			person4 = people.SingleOrDefault(x => x.Id == person4.Id);
			Assert.IsNotNull(person4);
			Assert.AreNotEqual(Guid.Empty, person4.ResourceId);

			var resource4 = resources.SingleOrDefault(x => x.Id == person4.ResourceId);
			Assert.IsNotNull(resource4);
			Assert.AreEqual(person4.Name, resource4.Name);
			Assert.AreEqual(ResourceState.Complete, resource4.State);
			Assert.AreEqual(0, resource4.Capabilities.Count);
			Assert.AreEqual(0, resource4.Capacities.Count);
			Assert.AreEqual(0, resource4.Properties.Count);
			Assert.AreEqual(1, resource4.ResourcePoolIds.Count);
			Assert.IsTrue(resource4.ResourcePoolIds.Contains(resourcePool2.Id));

			// Verify person 5
			person5 = people.SingleOrDefault(x => x.Id == person5.Id);
			Assert.IsNotNull(person5);
			Assert.AreNotEqual(Guid.Empty, person5.ResourceId);

			var resource5 = resources.SingleOrDefault(x => x.Id == person5.ResourceId);
			Assert.IsNotNull(resource5);
			Assert.AreEqual(person5.Name, resource5.Name);
			Assert.AreEqual(ResourceState.Complete, resource5.State);
			Assert.AreEqual(0, resource5.Capabilities.Count);
			Assert.AreEqual(0, resource5.Capacities.Count);
			Assert.AreEqual(0, resource5.Properties.Count);
			Assert.AreEqual(1, resource5.ResourcePoolIds.Count);
			Assert.IsTrue(resource5.ResourcePoolIds.Contains(resourcePool2.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenResourcePoolNameExists_DoesNotCreateResourcesForMembers()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team));
			objectCreator.CreatePeople([person1, person2, person3]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id]);

			var resourcePool = new ResourcePool
			{
				Name = team.Name,
			};
			resourcePool = objectCreator.CreateResourcePool(resourcePool);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamMakeBookableError = teamError as TeamMakeBookableError;
			Assert.IsNotNull(teamMakeBookableError);
			Assert.AreEqual("Name is already in use.", teamMakeBookableError.ErrorMessage);
			Assert.AreEqual(team.Id, teamMakeBookableError.Id);

			// Verify that the resource pool was not created for team
			team = TestContext.Api.Teams.Read(team.Id);
			Assert.IsNotNull(team);
			Assert.AreEqual(Guid.Empty, team.ResourcePoolId);

			// Verify that people are not available
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id]);
			Assert.IsTrue(people.All(x => x.ResourceId == Guid.Empty));

			var resources = TestContext.PlanApi.Resources.Read(new ORFilterElement<Resource>(people.Select(x => ResourceExposers.Name.Equal(x.Name)).ToArray())).ToList();
			Assert.AreEqual(0, resources.Count);
		}

		[TestMethod]
		public void MakeBookable_WhenCoreResourcePoolNameExists_DoesNotCreateResourcesForMembers()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team));
			objectCreator.CreatePeople([person1, person2, person3]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id]);

			var resourcePool = new CoreResourcePool
			{
				Name = team.Name,
			};
			objectCreator.CreateCoreResourcePool(resourcePool);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamMakeBookableError = teamError as TeamMakeBookableError;
			Assert.IsNotNull(teamMakeBookableError);
			Assert.AreEqual("Name is already in use.", teamMakeBookableError.ErrorMessage);
			Assert.AreEqual(team.Id, teamMakeBookableError.Id);

			// Verify that the resource pool was not created for team
			team = TestContext.Api.Teams.Read(team.Id);
			Assert.IsNotNull(team);
			Assert.AreEqual(Guid.Empty, team.ResourcePoolId);

			// Verify that people are not available
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id]);
			Assert.IsTrue(people.All(x => x.ResourceId == Guid.Empty));

			var resources = TestContext.PlanApi.Resources.Read(new ORFilterElement<Resource>(people.Select(x => ResourceExposers.Name.Equal(x.Name)).ToArray())).ToList();
			Assert.AreEqual(0, resources.Count);
		}

		[TestMethod]
		public void MakeBookable_WhenBulkOperationAndSharedMemberConflictsWithUnmanagedResource_FailsAffectedTeamsOnly()
		{
			/*
			 * Make T1, T2 and T3 bookable in bulk.
			 *		T1			T2			T3
			 *		|-> P1		|-> P4		|-> P4
			 *		|-> P2 (X)	|-> P2 (X)	|-> p5
			 *		|-> P3		|-> P5		|-> p6
			 * */

			var prefix = Guid.NewGuid();

			var team1 = new Team()
			{
				Name = $"{prefix}_Team 1",
			};
			var team2 = new Team()
			{
				Name = $"{prefix}_Team 2",
			};
			var team3 = new Team()
			{
				Name = $"{prefix}_Team 3",
			};
			objectCreator.CreateTeams([team1, team2, team3]);
			TestContext.Api.Teams.Activate([team1.Id, team2.Id, team3.Id]);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team1))
			.AddTeamMembership(new TeamMembership(team2));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person4 = new Person()
			{
				Name = $"{prefix}_Person 4",
			}
			.AddTeamMembership(new TeamMembership(team2))
			.AddTeamMembership(new TeamMembership(team3));
			var person5 = new Person()
			{
				Name = $"{prefix}_Person 5",
			}
			.AddTeamMembership(new TeamMembership(team2))
			.AddTeamMembership(new TeamMembership(team3));
			var person6 = new Person()
			{
				Name = $"{prefix}_Person 6",
			}
			.AddTeamMembership(new TeamMembership(team3));
			objectCreator.CreatePeople([person1, person2, person3, person4, person5, person6]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id, person6.Id]);

			var resource = new UnmanagedResource
			{
				Name = person2.Name,
			};
			resource = objectCreator.CreateResource(resource);

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable([team1.Id, team2.Id, team3.Id]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(team3.Id));

			Assert.AreEqual(2, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(team1.Id));
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(team2.Id));

			if (!expectedException.Result.TraceDataPerItem.ContainsKey(team1.Id))
			{
				Assert.Fail("Expected trace data for team 1 was not found.");
			}

			if (!expectedException.Result.TraceDataPerItem.ContainsKey(team2.Id))
			{
				Assert.Fail("Expected trace data for team 2 was not found.");
			}

			foreach (var kvp in expectedException.Result.TraceDataPerItem)
			{
				var teamId = kvp.Key;
				var traceData = kvp.Value;

				Assert.AreEqual(2, traceData.ErrorData.Count);
				var teamError = traceData.ErrorData.OfType<TeamError>().SingleOrDefault();
				Assert.IsNotNull(teamError);
				var teamMakeBookableError = teamError as TeamMakeBookableError;
				Assert.IsNotNull(teamMakeBookableError);
				Assert.AreEqual("Failed to create resource for 1 person(s) associated with this team, so the team cannot be made bookable.", teamMakeBookableError.ErrorMessage);
				Assert.AreEqual(teamId, teamMakeBookableError.Id);

				var personError = traceData.ErrorData.OfType<PersonError>().SingleOrDefault();
				Assert.IsNotNull(personError);
				var personMakeBookableError = personError as PersonMakeBookableError;
				Assert.IsNotNull(personMakeBookableError);
				Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
				Assert.AreEqual(person2.Id, personMakeBookableError.Id);
			}

			// Verify that the resource pool was not created for team 1 and team 2
			var teams = TestContext.Api.Teams.Read([team1.Id, team2.Id, team3.Id]);
			var resourcePools = TestContext.PlanApi.ResourcePools.Read(new ORFilterElement<ResourcePool>(teams.Select(x => ResourcePoolExposers.Name.Equal(x.Name)).ToArray()));

			team1 = teams.SingleOrDefault(x => x.Id == team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(Guid.Empty, team1.ResourcePoolId);
			Assert.AreEqual(false, team1.IsBookable);
			var pool1 = resourcePools.SingleOrDefault(x => x.Name == team1.Name);
			Assert.IsNull(pool1);

			team2 = teams.SingleOrDefault(x => x.Id == team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreEqual(Guid.Empty, team2.ResourcePoolId);
			Assert.AreEqual(false, team2.IsBookable);
			var pool2 = resourcePools.SingleOrDefault(x => x.Name == team2.Name);
			Assert.IsNull(pool2);

			team3 = teams.SingleOrDefault(x => x.Id == team3.Id);
			Assert.IsNotNull(team3);
			Assert.AreNotEqual(Guid.Empty, team3.ResourcePoolId);
			Assert.AreEqual(true, team3.IsBookable);
			var pool3 = resourcePools.SingleOrDefault(x => x.Name == team3.Name);
			Assert.IsNotNull(pool3);
			Assert.AreEqual(pool3.Id, team3.ResourcePoolId);
			Assert.AreEqual(ResourcePoolState.Complete, pool3.State);

			// Verify that resources were not created for person 1, person 2 and person 3
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id, person6.Id]);
			var resources = TestContext.PlanApi.Resources.Read(new ORFilterElement<Resource>(people.Select(x => ResourceExposers.Name.Equal(x.Name)).ToArray())).ToList();

			person1 = people.SingleOrDefault(x => x.Id == person1.Id);
			Assert.IsNotNull(person1);
			Assert.AreEqual(Guid.Empty, person1.ResourceId);
			var resource1 = resources.SingleOrDefault(x => x.Name == person1.Name);
			Assert.IsNull(resource1);

			person2 = people.SingleOrDefault(x => x.Id == person2.Id);
			Assert.IsNotNull(person2);
			Assert.AreEqual(Guid.Empty, person2.ResourceId);

			person3 = people.SingleOrDefault(x => x.Id == person3.Id);
			Assert.IsNotNull(person3);
			Assert.AreEqual(Guid.Empty, person3.ResourceId);
			var resource3 = resources.SingleOrDefault(x => x.Name == person3.Name);
			Assert.IsNull(resource3);

			person4 = people.SingleOrDefault(x => x.Id == person4.Id);
			Assert.IsNotNull(person4);
			Assert.AreNotEqual(Guid.Empty, person4.ResourceId);
			Assert.AreEqual(2, person4.TeamMemberships.Count);
			var resource4 = resources.SingleOrDefault(x => x.Name == person4.Name);
			Assert.IsNotNull(resource4);
			Assert.AreEqual(resource4.Id, person4.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource4.State);
			Assert.AreEqual(1, resource4.ResourcePoolIds.Count);
			Assert.IsTrue(resource4.ResourcePoolIds.Contains(pool3.Id));

			person5 = people.SingleOrDefault(x => x.Id == person5.Id);
			Assert.IsNotNull(person5);
			Assert.AreNotEqual(Guid.Empty, person5.ResourceId);
			Assert.AreEqual(2, person5.TeamMemberships.Count);
			var resource5 = resources.SingleOrDefault(x => x.Name == person5.Name);
			Assert.IsNotNull(resource5);
			Assert.AreEqual(resource5.Id, person5.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource5.State);
			Assert.AreEqual(1, resource5.ResourcePoolIds.Count);
			Assert.IsTrue(resource5.ResourcePoolIds.Contains(pool3.Id));

			person6 = people.SingleOrDefault(x => x.Id == person6.Id);
			Assert.IsNotNull(person6);
			Assert.AreNotEqual(Guid.Empty, person6.ResourceId);
			Assert.AreEqual(1, person6.TeamMemberships.Count);
			var resource6 = resources.SingleOrDefault(x => x.Name == person6.Name);
			Assert.IsNotNull(resource6);
			Assert.AreEqual(resource6.Id, person6.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource6.State);
			Assert.AreEqual(1, resource6.ResourcePoolIds.Count);
			Assert.IsTrue(resource6.ResourcePoolIds.Contains(pool3.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenBulkOperationAndSharedMemberConflictsWithCoreResource_FailsAffectedTeamsOnly()
		{
			/*
			 * Make T1, T2 and T3 bookable in bulk.
			 *		T1			T2			T3
			 *		|-> P1		|-> P4		|-> P4
			 *		|-> P2 (X)	|-> P2 (X)	|-> p5
			 *		|-> P3		|-> P5		|-> p6
			 * */

			var prefix = Guid.NewGuid();

			var team1 = new Team()
			{
				Name = $"{prefix}_Team 1",
			};
			var team2 = new Team()
			{
				Name = $"{prefix}_Team 2",
			};
			var team3 = new Team()
			{
				Name = $"{prefix}_Team 3",
			};
			objectCreator.CreateTeams([team1, team2, team3]);
			TestContext.Api.Teams.Activate([team1.Id, team2.Id, team3.Id]);

			var person1 = new Person()
			{
				Name = $"{prefix}_Person 1",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person2 = new Person()
			{
				Name = $"{prefix}_Person 2",
			}
			.AddTeamMembership(new TeamMembership(team1))
			.AddTeamMembership(new TeamMembership(team2));
			var person3 = new Person()
			{
				Name = $"{prefix}_Person 3",
			}
			.AddTeamMembership(new TeamMembership(team1));
			var person4 = new Person()
			{
				Name = $"{prefix}_Person 4",
			}
			.AddTeamMembership(new TeamMembership(team2))
			.AddTeamMembership(new TeamMembership(team3));
			var person5 = new Person()
			{
				Name = $"{prefix}_Person 5",
			}
			.AddTeamMembership(new TeamMembership(team2))
			.AddTeamMembership(new TeamMembership(team3));
			var person6 = new Person()
			{
				Name = $"{prefix}_Person 6",
			}
			.AddTeamMembership(new TeamMembership(team3));
			objectCreator.CreatePeople([person1, person2, person3, person4, person5, person6]);
			TestContext.Api.People.Activate([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id, person6.Id]);

			var resource = new CoreResource
			{
				Name = person2.Name,
			};
			objectCreator.CreateCoreResource(resource);

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable([team1.Id, team2.Id, team3.Id]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(team3.Id));

			Assert.AreEqual(2, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(team1.Id));
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(team2.Id));

			if (!expectedException.Result.TraceDataPerItem.ContainsKey(team1.Id))
			{
				Assert.Fail("Expected trace data for team 1 was not found.");
			}

			if (!expectedException.Result.TraceDataPerItem.ContainsKey(team2.Id))
			{
				Assert.Fail("Expected trace data for team 2 was not found.");
			}

			foreach (var kvp in expectedException.Result.TraceDataPerItem)
			{
				var teamId = kvp.Key;
				var traceData = kvp.Value;

				Assert.AreEqual(2, traceData.ErrorData.Count);
				var teamError = traceData.ErrorData.OfType<TeamError>().SingleOrDefault();
				Assert.IsNotNull(teamError);
				var teamMakeBookableError = teamError as TeamMakeBookableError;
				Assert.IsNotNull(teamMakeBookableError);
				Assert.AreEqual("Failed to complete resource for 1 person(s) associated with this team, so the team cannot be made bookable.", teamMakeBookableError.ErrorMessage);
				Assert.AreEqual(teamId, teamMakeBookableError.Id);

				var personError = traceData.ErrorData.OfType<PersonError>().SingleOrDefault();
				Assert.IsNotNull(personError);
				var personMakeBookableError = personError as PersonMakeBookableError;
				Assert.IsNotNull(personMakeBookableError);
				Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
				Assert.AreEqual(person2.Id, personMakeBookableError.Id);
			}

			// Verify that the resource pool was not created for team 1 and team 2
			var teams = TestContext.Api.Teams.Read([team1.Id, team2.Id, team3.Id]);
			var resourcePools = TestContext.PlanApi.ResourcePools.Read(new ORFilterElement<ResourcePool>(teams.Select(x => ResourcePoolExposers.Name.Equal(x.Name)).ToArray()));

			team1 = teams.SingleOrDefault(x => x.Id == team1.Id);
			Assert.IsNotNull(team1);
			Assert.AreEqual(Guid.Empty, team1.ResourcePoolId);
			Assert.AreEqual(false, team1.IsBookable);
			var pool1 = resourcePools.SingleOrDefault(x => x.Name == team1.Name);
			Assert.IsNull(pool1);

			team2 = teams.SingleOrDefault(x => x.Id == team2.Id);
			Assert.IsNotNull(team2);
			Assert.AreEqual(Guid.Empty, team2.ResourcePoolId);
			Assert.AreEqual(false, team2.IsBookable);
			var pool2 = resourcePools.SingleOrDefault(x => x.Name == team2.Name);
			Assert.IsNull(pool2);

			team3 = teams.SingleOrDefault(x => x.Id == team3.Id);
			Assert.IsNotNull(team3);
			Assert.AreNotEqual(Guid.Empty, team3.ResourcePoolId);
			Assert.AreEqual(true, team3.IsBookable);
			var pool3 = resourcePools.SingleOrDefault(x => x.Name == team3.Name);
			Assert.IsNotNull(pool3);
			Assert.AreEqual(pool3.Id, team3.ResourcePoolId);
			Assert.AreEqual(ResourcePoolState.Complete, pool3.State);

			// Verify that resources were not created for person 1, person 2 and person 3
			var people = TestContext.Api.People.Read([person1.Id, person2.Id, person3.Id, person4.Id, person5.Id, person6.Id]);
			var resources = TestContext.PlanApi.Resources.Read(new ORFilterElement<Resource>(people.Select(x => ResourceExposers.Name.Equal(x.Name)).ToArray())).ToList();

			person1 = people.SingleOrDefault(x => x.Id == person1.Id);
			Assert.IsNotNull(person1);
			Assert.AreEqual(Guid.Empty, person1.ResourceId);
			var resource1 = resources.SingleOrDefault(x => x.Name == person1.Name);
			Assert.IsNull(resource1);

			person2 = people.SingleOrDefault(x => x.Id == person2.Id);
			Assert.IsNotNull(person2);
			Assert.AreEqual(Guid.Empty, person2.ResourceId);

			person3 = people.SingleOrDefault(x => x.Id == person3.Id);
			Assert.IsNotNull(person3);
			Assert.AreEqual(Guid.Empty, person3.ResourceId);
			var resource3 = resources.SingleOrDefault(x => x.Name == person3.Name);
			Assert.IsNull(resource3);

			person4 = people.SingleOrDefault(x => x.Id == person4.Id);
			Assert.IsNotNull(person4);
			Assert.AreNotEqual(Guid.Empty, person4.ResourceId);
			Assert.AreEqual(2, person4.TeamMemberships.Count);
			var resource4 = resources.SingleOrDefault(x => x.Name == person4.Name);
			Assert.IsNotNull(resource4);
			Assert.AreEqual(resource4.Id, person4.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource4.State);
			Assert.AreEqual(1, resource4.ResourcePoolIds.Count);
			Assert.IsTrue(resource4.ResourcePoolIds.Contains(pool3.Id));

			person5 = people.SingleOrDefault(x => x.Id == person5.Id);
			Assert.IsNotNull(person5);
			Assert.AreNotEqual(Guid.Empty, person5.ResourceId);
			Assert.AreEqual(2, person5.TeamMemberships.Count);
			var resource5 = resources.SingleOrDefault(x => x.Name == person5.Name);
			Assert.IsNotNull(resource5);
			Assert.AreEqual(resource5.Id, person5.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource5.State);
			Assert.AreEqual(1, resource5.ResourcePoolIds.Count);
			Assert.IsTrue(resource5.ResourcePoolIds.Contains(pool3.Id));

			person6 = people.SingleOrDefault(x => x.Id == person6.Id);
			Assert.IsNotNull(person6);
			Assert.AreNotEqual(Guid.Empty, person6.ResourceId);
			Assert.AreEqual(1, person6.TeamMemberships.Count);
			var resource6 = resources.SingleOrDefault(x => x.Name == person6.Name);
			Assert.IsNotNull(resource6);
			Assert.AreEqual(resource6.Id, person6.ResourceId);
			Assert.AreEqual(ResourceState.Complete, resource6.State);
			Assert.AreEqual(1, resource6.ResourcePoolIds.Count);
			Assert.IsTrue(resource6.ResourcePoolIds.Contains(pool3.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenPersonAddedToBookableTeam_CreatesResourceAndAddsToPool()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Assign person to team
			person.AddTeamMembership(new TeamMembership(team));

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);
			Assert.AreEqual(0, resource.Capabilities.Count);
			Assert.AreEqual(0, resource.Capacities.Count);
			Assert.AreEqual(0, resource.Properties.Count);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(resourcePool.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenPersonRemovedFromBookableTeam_RemovesResourceFromPool()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);
			Assert.AreEqual(0, resource.Capabilities.Count);
			Assert.AreEqual(0, resource.Capacities.Count);
			Assert.AreEqual(0, resource.Properties.Count);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(resourcePool.Id));

			// Remove person from team
			var membership = person.TeamMemberships.First();
			person.RemoveTeamMembership(membership);

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);
			Assert.AreEqual(0, resource.Capabilities.Count);
			Assert.AreEqual(0, resource.Capacities.Count);
			Assert.AreEqual(0, resource.Properties.Count);
			Assert.AreEqual(0, resource.ResourcePoolIds.Count);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonAddedToBookableTeamAndNameConflictsWithUnmanagedResource_ThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			var resource = new UnmanagedResource
			{
				Name = person.Name,
			};
			objectCreator.CreateResource(resource);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Assign person to team
			person.AddTeamMembership(new TeamMembership(team));

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
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

			var personMakeBookableError = personError as PersonMakeBookableError;
			Assert.IsNotNull(personMakeBookableError);
			Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
			Assert.AreEqual(person.Id, personMakeBookableError.Id);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonAddedToBookableTeamAndNameConflictsWithCoreResource_ThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			var resource = new CoreResource
			{
				Name = person.Name,
			};
			objectCreator.CreateCoreResource(resource);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Assign person to team
			person.AddTeamMembership(new TeamMembership(team));

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
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

			var personMakeBookableError = personError as PersonMakeBookableError;
			Assert.IsNotNull(personMakeBookableError);
			Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
			Assert.AreEqual(person.Id, personMakeBookableError.Id);
		}

		[TestMethod]
		public void MakeBookable_WhenDraftPersonActivatedInBookableTeam_CreatesResourceAndAddsToPool()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(ResourceExposers.Name.Equal(person.Name)).SingleOrDefault();
			Assert.IsNull(resource);

			// Activate
			person = TestContext.Api.People.Activate(person);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);
			Assert.AreEqual(0, resource.Capabilities.Count);
			Assert.AreEqual(0, resource.Capacities.Count);
			Assert.AreEqual(0, resource.Properties.Count);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(resourcePool.Id));
		}

		[TestMethod]
		public void MakeBookable_WhenDraftPersonActivatedAndNameConflictsWithUnmanagedResource_ThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);

			var resource = new UnmanagedResource
			{
				Name = person.Name,
			};
			objectCreator.CreateResource(resource);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Activate
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Activate(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personMakeBookableError = personError as PersonMakeBookableError;
			Assert.IsNotNull(personMakeBookableError);
			Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
			Assert.AreEqual(person.Id, personMakeBookableError.Id);

			// Verify person is not active
			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Draft, person.State);
			Assert.AreEqual(Guid.Empty, person.ResourceId);
		}

		[TestMethod]
		public void MakeBookable_WhenDraftPersonActivatedAndNameConflictsWithCoreResource_ThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team()
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person()
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team));
			person = objectCreator.CreatePerson(person);

			var coreResource = new CoreResource
			{
				Name = person.Name,
			};
			objectCreator.CreateCoreResource(coreResource);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(team.ResourcePoolId);
			Assert.IsNotNull(resourcePool);
			Assert.AreEqual(team.Name, resourcePool.Name);
			Assert.AreEqual(ResourcePoolState.Complete, resourcePool.State);
			Assert.AreEqual(0, resourcePool.Capabilities.Count);
			Assert.AreEqual(0, resourcePool.LinkedResourcePools.Count);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Activate
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Activate(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personMakeBookableError = personError as PersonMakeBookableError;
			Assert.IsNotNull(personMakeBookableError);
			Assert.AreEqual("Name is already in use.", personMakeBookableError.ErrorMessage);
			Assert.AreEqual(person.Id, personMakeBookableError.Id);

			// Verify person is not active
			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Draft, person.State);
			Assert.AreEqual(Guid.Empty, person.ResourceId);

			// Verify resource is not created
			var resource = TestContext.PlanApi.Resources.Read(ResourceExposers.Name.Equal(person.Name)).SingleOrDefault();
			Assert.IsNull(resource);
		}

		[TestMethod]
		public void MakeBookable_WhenTeamIsAlreadyBookable_ThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);
			team = TestContext.Api.Teams.MakeBookable(team);

			Assert.IsNotNull(team);
			Assert.AreEqual(true, team.IsBookable);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Teams.MakeBookable(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");
			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);

			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamMakeBookableError = teamError as TeamMakeBookableError;
			Assert.IsNotNull(teamMakeBookableError);
			Assert.AreEqual($"Team '{team.Name}' is already bookable.", teamMakeBookableError.ErrorMessage);
			Assert.AreEqual(team.Id, teamMakeBookableError.Id);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonWithSkillsMadeBookable_SynchronizesResourceCapabilities()
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
			objectCreator.CreateSkills([skill1, skill2]);

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
			.AddTeamMembership(new TeamMembership(team))
			.SetSkills([skill1, skill2]);
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			// Verify person's resource has skill capabilities
			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);

			var capabilitySetting = resource.Capabilities.SingleOrDefault();
			Assert.IsNotNull(capabilitySetting);
			Assert.AreEqual(2, capabilitySetting.Discretes.Count);
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill1.Name));
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill2.Name));
		}

		[TestMethod]
		public void MakeBookable_WhenPersonNameUpdatedInBookableTeam_SynchronizesResourceName()
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

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(person.Name, resource.Name);

			// Update person name
			var updatedName = $"{prefix}_Person Updated";
			person.Name = updatedName;
			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedName, person.Name);

			// Verify resource name is updated
			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(updatedName, resource.Name);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonSkillsUpdatedInBookableTeam_SynchronizesResourceCapabilities()
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
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team))
			.SetSkills([skill1, skill2]);
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);

			var capabilitySetting = resource.Capabilities.SingleOrDefault();
			Assert.IsNotNull(capabilitySetting);
			Assert.AreEqual(2, capabilitySetting.Discretes.Count);
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill1.Name));
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill2.Name));

			// Update person skills
			person.SetSkills([skill2, skill3]);
			person = TestContext.Api.People.Update(person);

			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			capabilitySetting = resource.Capabilities.SingleOrDefault();
			Assert.IsNotNull(capabilitySetting);
			Assert.AreEqual(2, capabilitySetting.Discretes.Count);
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill2.Name));
			Assert.IsTrue(capabilitySetting.Discretes.Contains(skill3.Name));

			// Remove all skills
			person.RemoveSkill(skill2);
			person.RemoveSkill(skill3);
			person = TestContext.Api.People.Update(person);

			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			capabilitySetting = resource.Capabilities.SingleOrDefault();
			Assert.IsNull(capabilitySetting);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonDeprecatedInBookableTeam_DeprecatesResource()
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

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(ResourceState.Complete, resource.State);

			// Remove person from team
			var membershipToRemove = person.TeamMemberships.First();
			person.RemoveTeamMembership(membershipToRemove);

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);

			// Deprecate person
			person = TestContext.Api.People.Deprecate(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Deprecated, person.State);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			// Verify resource is deprecated
			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(ResourceState.Deprecated, resource.State);
		}

		[TestMethod]
		public void MakeBookable_WhenDeprecatedBookablePersonDeleted_DeletesResource()
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

			// Make bookable
			team = TestContext.Api.Teams.MakeBookable(team);
			Assert.IsNotNull(team);
			Assert.AreNotEqual(Guid.Empty, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resourceId = person.ResourceId;
			var resource = TestContext.PlanApi.Resources.Read(resourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(ResourceState.Complete, resource.State);

			// Remove person from team
			var membershipToRemove = person.TeamMemberships.First();
			person.RemoveTeamMembership(membershipToRemove);

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);

			// Deprecate and delete person
			person = TestContext.Api.People.Deprecate(person);
			Assert.AreEqual(PersonState.Deprecated, person.State);

			TestContext.Api.People.Delete(person);

			// Verify resource is deleted
			resource = TestContext.PlanApi.Resources.Read(resourceId);
			Assert.IsNull(resource);
		}

		[TestMethod]
		public void MakeBookable_WhenPersonMovedBetweenBookableTeams_UpdatesResourcePoolAssignment()
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
			TestContext.Api.Teams.Activate([team1.Id, team2.Id]);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddTeamMembership(new TeamMembership(team1));
			person = objectCreator.CreatePerson(person);
			person = TestContext.Api.People.Activate(person);

			// Make both teams bookable
			var teams = TestContext.Api.Teams.MakeBookable([team1.Id, team2.Id]);
			team1 = teams.SingleOrDefault(x => x.Id == team1.Id);
			team2 = teams.SingleOrDefault(x => x.Id == team2.Id);
			Assert.IsNotNull(team1);
			Assert.IsNotNull(team2);
			Assert.AreNotEqual(Guid.Empty, team1.ResourcePoolId);
			Assert.AreNotEqual(Guid.Empty, team2.ResourcePoolId);

			// Verify person resource is in team 1's pool
			person = TestContext.Api.People.Read(person.Id);
			Assert.IsNotNull(person);
			Assert.AreNotEqual(Guid.Empty, person.ResourceId);

			var resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(team1.ResourcePoolId));

			// Move person from team 1 to team 2
			var membershipToRemove = person.TeamMemberships.Single(x => x.TeamId == team1.Id);
			person.RemoveTeamMembership(membershipToRemove);
			person.AddTeamMembership(new TeamMembership(team2));
			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);

			// Verify resource pool assignment updated
			resource = TestContext.PlanApi.Resources.Read(person.ResourceId);
			Assert.IsNotNull(resource);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(team2.ResourcePoolId));
			Assert.IsFalse(resource.ResourcePoolIds.Contains(team1.ResourcePoolId));
		}
	}
}
