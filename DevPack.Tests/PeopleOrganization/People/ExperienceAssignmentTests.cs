namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class ExperienceAssignmentTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public ExperienceAssignmentTests()
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
			var experienceId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
				ExperienceId = experienceId,
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

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personExperienceNotFoundError = personError as PersonExperienceNotFoundError;
			Assert.IsNotNull(personExperienceNotFoundError);
			Assert.AreEqual($"Experience with ID '{experienceId}' not found.", personExperienceNotFoundError.ErrorMessage);
			Assert.AreEqual(person.Id, personExperienceNotFoundError.Id);
			Assert.AreEqual(experienceId, personExperienceNotFoundError.ExperienceId);
		}

		[TestMethod]
		public void UpdateWithNotExistingThrowsException()
		{
			var prefix = Guid.NewGuid();
			var experienceId = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person.ExperienceId = experienceId;
				TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personExperienceNotFoundError = personError as PersonExperienceNotFoundError;
			Assert.IsNotNull(personExperienceNotFoundError);
			Assert.AreEqual($"Experience with ID '{experienceId}' not found.", personExperienceNotFoundError.ErrorMessage);
			Assert.AreEqual(person.Id, personExperienceNotFoundError.Id);
			Assert.AreEqual(experienceId, personExperienceNotFoundError.ExperienceId);
		}
	}
}
