namespace RT_PeopleAndOrganizations.PeopleOrganization.Roles
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DeleteTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeleteTests()
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

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			role = objectCreator.CreateRole(role);

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
				RoleId = role.Id,
			});
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Roles.Delete(role);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Role '{role.Name}' is in use by 1 people.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleInUseByPeopleError = roleError as RoleInUseByPeopleError;
			Assert.IsNotNull(roleInUseByPeopleError);
			Assert.AreEqual(role.Id, roleInUseByPeopleError.Id);
			Assert.AreEqual(errorMessage, roleInUseByPeopleError.ErrorMessage);
			Assert.AreEqual(1, roleInUseByPeopleError.PeopleIds.Count);
			Assert.IsTrue(roleInUseByPeopleError.PeopleIds.Contains(person.Id));
		}
	}
}
