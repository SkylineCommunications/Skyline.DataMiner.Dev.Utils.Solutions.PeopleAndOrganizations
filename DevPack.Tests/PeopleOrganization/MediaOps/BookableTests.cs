namespace RT_PeopleAndOrganizations.PeopleOrganization.MediaOps
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net;
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
		public void test3()
		{
			/*
			 * Make T1, T2 and T3 bookable in bulk.
			 *		T1			T2			T3
			 *		|-> P1		|-> P4		|-> P4
			 *		|-> P2 (X)	|-> P2 (X)	|-> p5
			 *		|-> P3		|-> P5		|-> p6
			 * */
		}

		[TestMethod]
		public void test4()
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
			var resourcePoolId = team.ResourcePoolId;
			Assert.AreEqual(resourcePoolId, team.ResourcePoolId);
			Assert.AreEqual(true, team.IsBookable);

			var resourcePool = TestContext.PlanApi.ResourcePools.Read(resourcePoolId);
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
			Assert.AreEqual(person.Name, resource.Name);
			Assert.AreEqual(ResourceState.Complete, resource.State);
			Assert.AreEqual(0, resource.Capabilities.Count);
			Assert.AreEqual(0, resource.Capacities.Count);
			Assert.AreEqual(0, resource.Properties.Count);
			Assert.AreEqual(1, resource.ResourcePoolIds.Count);
			Assert.IsTrue(resource.ResourcePoolIds.Contains(resourcePool.Id));
		}
	}
}
