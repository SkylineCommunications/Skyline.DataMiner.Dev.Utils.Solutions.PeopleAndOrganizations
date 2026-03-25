namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class OrganizationAssignmentTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public OrganizationAssignmentTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void CreateWithNotExistingThrowsException()
		{
			var prefix = Guid.NewGuid();
			var organizationId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
				OrganizationId = organizationId,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Organization with ID '{person.OrganizationId}' not found.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personOrganizationNotFoundError = personError as PersonOrganizationNotFoundError;
			Assert.IsNotNull(personOrganizationNotFoundError);
			Assert.AreEqual(errorMessage, personOrganizationNotFoundError.ErrorMessage);
			Assert.AreEqual(person.Id, personOrganizationNotFoundError.Id);
			Assert.AreEqual(organizationId, personOrganizationNotFoundError.OrganizationId);
		}

		[TestMethod]
		public void UpdateWithNotExistingThrowsException()
		{
			var prefix = Guid.NewGuid();
			var organizationId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			person.OrganizationId = organizationId;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Organization with ID '{person.OrganizationId}' not found.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personOrganizationNotFoundError = personError as PersonOrganizationNotFoundError;
			Assert.IsNotNull(personOrganizationNotFoundError);
			Assert.AreEqual(errorMessage, personOrganizationNotFoundError.ErrorMessage);
			Assert.AreEqual(person.Id, personOrganizationNotFoundError.Id);
			Assert.AreEqual(organizationId, personOrganizationNotFoundError.OrganizationId);
		}
	}
}
