namespace RT_PeopleAndOrganizations.PeopleOrganization.Roles
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

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
			var role1 = objectCreator.CreateRole(new Role(Guid.NewGuid()) { Name = $"{prefix}_Role_1" });
			objectCreator.CreateRole(new Role(Guid.NewGuid()) { Name = $"{prefix}_Role_2" });

			var returnedRoles = TestContext.Api.Roles.Read(RoleExposers.Id.Equal(role1.Id)).ToArray();

			Assert.IsNotNull(returnedRoles);
			Assert.AreEqual(1, returnedRoles.Length);
			Assert.AreEqual(role1.Id, returnedRoles[0].Id);
			Assert.AreEqual(role1.Name, returnedRoles[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var roles = Enumerable.Range(0, 10)
				.Select(i => new Role { Name = $"{prefix}_Role_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateRoles(roles);

			var returnedRoles = TestContext.Api.Roles.Read(RoleExposers.Name.Equal($"{prefix}_Role_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedRoles);
			Assert.AreEqual(1, returnedRoles.Length);
			Assert.AreEqual($"{prefix}_Role_{postfix}_1", returnedRoles[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var roles = Enumerable.Range(0, 10)
				.Select(i => new Role { Name = $"{prefix}_Role_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateRoles(roles);

			var returnedRoles = TestContext.Api.Roles.Read(RoleExposers.Name.NotEqual($"{prefix}_Role_{postfix}_2")).ToArray();
			var createdRoles = returnedRoles.Where(r => r.Name.StartsWith($"{prefix}_Role_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedRoles);
			Assert.AreEqual(9, createdRoles.Length);
			Assert.IsFalse(createdRoles.Any(r => r.Name == $"{prefix}_Role_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var roles = Enumerable.Range(0, 10)
				.Select(i => new Role { Name = $"{prefix}_Role_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateRoles(roles);

			var returnedRoles = TestContext.Api.Roles.Read(RoleExposers.Name.Contains($"{prefix}_Role_{postfix}")).ToArray();

			Assert.IsNotNull(returnedRoles);
			Assert.AreEqual(10, returnedRoles.Length);
			Assert.IsTrue(returnedRoles.All(r => r.Name.Contains($"{prefix}_Role_{postfix}", StringComparison.Ordinal)));
		}
	}
}
