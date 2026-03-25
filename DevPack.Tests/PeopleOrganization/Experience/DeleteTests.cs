namespace RT_PeopleAndOrganizations.PeopleOrganization.Experience
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

			var experience = new Experience
			{
				Name = $"{prefix}_Experience",
			};
			experience = objectCreator.CreateExperience(experience);

			var person = new Person
			{
				Name = $"{prefix}_Person",
				ExperienceId = experience.Id,
			};
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Experience.Delete(experience);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Experience '{experience.Name}' is in use by 1 people.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceInUseByPeopleError = experienceError as ExperienceInUseByPeopleError;
			Assert.IsNotNull(experienceInUseByPeopleError);
			Assert.AreEqual(experience.Id, experienceInUseByPeopleError.Id);
			Assert.AreEqual(errorMessage, experienceInUseByPeopleError.ErrorMessage);
			Assert.AreEqual(1, experienceInUseByPeopleError.PeopleIds.Count);
			Assert.IsTrue(experienceInUseByPeopleError.PeopleIds.Contains(person.Id));
		}
	}
}
