namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	using Team = Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Team;

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
			var person1 = objectCreator.CreatePerson(new Person(Guid.NewGuid()) { Name = $"{prefix}_Person_1" });
			objectCreator.CreatePerson(new Person(Guid.NewGuid()) { Name = $"{prefix}_Person_2" });

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.Id.Equal(person1.Id)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(1, returnedPeople.Length);
			Assert.AreEqual(person1.Id, returnedPeople[0].Id);
			Assert.AreEqual(person1.Name, returnedPeople[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var people = Enumerable.Range(0, 10)
				.Select(i => new Person { Name = $"{prefix}_Person_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreatePeople(people);

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.Name.Equal($"{prefix}_Person_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(1, returnedPeople.Length);
			Assert.AreEqual($"{prefix}_Person_{postfix}_1", returnedPeople[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var people = Enumerable.Range(0, 10)
				.Select(i => new Person { Name = $"{prefix}_Person_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreatePeople(people);

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.Name.NotEqual($"{prefix}_Person_{postfix}_2")).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(9, createdPeople.Length);
			Assert.IsFalse(createdPeople.Any(p => p.Name == $"{prefix}_Person_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var people = Enumerable.Range(0, 10)
				.Select(i => new Person { Name = $"{prefix}_Person_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreatePeople(people);

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.Name.Contains($"{prefix}_Person_{postfix}")).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(10, returnedPeople.Length);
			Assert.IsTrue(returnedPeople.All(p => p.Name.Contains($"{prefix}_Person_{postfix}", StringComparison.Ordinal)));
		}

		[TestMethod]
		public void FilterByEmailEquals()
		{
			var prefix = Guid.NewGuid();
			var email = $"{prefix}@example.com";

			var person1 = objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_1", Email = email });
			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_2", Email = $"{Guid.NewGuid()}@example.com" });

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.Email.Equal(email)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(1, createdPeople.Length);
			Assert.AreEqual(person1.Id, createdPeople[0].Id);
			Assert.AreEqual(email, createdPeople[0].Email);
		}

		[TestMethod]
		public void FilterByStateEquals()
		{
			var prefix = Guid.NewGuid();

			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_Draft" });

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.State.Equal(PersonState.Draft)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.IsTrue(createdPeople.Length >= 1);
			Assert.IsTrue(createdPeople.All(p => p.State == PersonState.Draft));
		}

		[TestMethod]
		public void FilterByExperienceIdEquals()
		{
			var prefix = Guid.NewGuid();
			var experience = objectCreator.CreateExperience(new Experience { Name = $"{prefix}_Experience" });

			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_1", ExperienceId = experience.Id });
			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_2", ExperienceId = experience.Id });
			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_3" });

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.ExperienceId.Equal(experience.Id)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(2, createdPeople.Length);
			Assert.IsTrue(createdPeople.All(p => p.ExperienceId == experience.Id));
		}

		[TestMethod]
		public void FilterByOrganizationIdEquals()
		{
			var prefix = Guid.NewGuid();
			var organization = objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization" });

			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_1", OrganizationId = organization.Id });
			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_2", OrganizationId = organization.Id });
			objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_3" });

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.OrganizationId.Equal(organization.Id)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(2, createdPeople.Length);
			Assert.IsTrue(createdPeople.All(p => p.OrganizationId == organization.Id));
		}

		[TestMethod]
		public void FilterByTeamMembershipTeamIdEquals()
		{
			var prefix = Guid.NewGuid();
			var team = objectCreator.CreateTeam(new Team { Name = $"{prefix}_Team" });

			var person1 = new Person { Name = $"{prefix}_Person_1" }.AddTeamMembership(new TeamMembership(team));
			var person2 = new Person { Name = $"{prefix}_Person_2" }.AddTeamMembership(new TeamMembership(team));
			var person3 = new Person { Name = $"{prefix}_Person_3" };

			objectCreator.CreatePeople([person1, person2, person3]);

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.TeamMemberships.TeamId.Equal(team.Id)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(2, createdPeople.Length);
			Assert.IsTrue(createdPeople.All(p => p.TeamMemberships.Any(m => m.TeamId == team.Id)));
		}

		[TestMethod]
		public void FilterByTeamMembershipRoleIdEquals()
		{
			var prefix = Guid.NewGuid();
			var team = objectCreator.CreateTeam(new Team { Name = $"{prefix}_Team" });
			var role = objectCreator.CreateRole(new Role { Name = $"{prefix}_Role" });

			var person1 = new Person { Name = $"{prefix}_Person_1" }.AddTeamMembership(new TeamMembership(team) { RoleId = role.Id });
			var person2 = new Person { Name = $"{prefix}_Person_2" }.AddTeamMembership(new TeamMembership(team) { RoleId = role.Id });
			var person3 = new Person { Name = $"{prefix}_Person_3" }.AddTeamMembership(new TeamMembership(team));

			objectCreator.CreatePeople([person1, person2, person3]);

			var returnedPeople = TestContext.Api.People.Read(PersonExposers.TeamMemberships.RoleId.Equal(role.Id)).ToArray();
			var createdPeople = returnedPeople.Where(p => p.Name.StartsWith($"{prefix}_Person_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedPeople);
			Assert.AreEqual(2, createdPeople.Length);
			Assert.IsTrue(createdPeople.All(p => p.TeamMemberships.Any(m => m.RoleId == role.Id)));
		}
	}
}
