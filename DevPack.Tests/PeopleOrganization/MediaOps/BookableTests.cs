namespace RT_PeopleAndOrganizations.PeopleOrganization.MediaOps
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

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
		public void ResourcePoolNameInUseThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var resourcePool = new ResourcePool
			{
				Name =team.Name,
			};
			resourcePool = objectCreator.CreateResourcePool(resourcePool);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.MakeBookable(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");
		}
	}
}
