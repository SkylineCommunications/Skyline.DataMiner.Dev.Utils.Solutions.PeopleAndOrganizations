namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DeprecateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeprecateTests()
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

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			var person = new Person
			{
				Name = $"{prefix}_Person",
				OrganizationId = organization.Id,
			};
			person = objectCreator.CreatePerson(person);

			organization = TestContext.Api.Organizations.Activate(organization);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Organizations.Deprecate(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Organization '{organization.Name}' is in use by 1 person/people.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInUseError = organizationError as OrganizationInUseError;
			Assert.IsNotNull(organizationInUseError);
			Assert.AreEqual(organization.Id, organizationInUseError.Id);
			Assert.AreEqual(errorMessage, organizationInUseError.ErrorMessage);
			Assert.AreEqual(1, organizationInUseError.PeopleIds.Count);
			Assert.IsTrue(organizationInUseError.PeopleIds.Contains(person.Id));
		}
	}
}
