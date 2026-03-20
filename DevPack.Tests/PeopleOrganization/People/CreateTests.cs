namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class CreateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public CreateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void CreateWithExistingIdThrowsException()
		{
			var prefix = Guid.NewGuid();
			var personId = Guid.NewGuid();

			var person1 = new Person(personId)
			{
				Name = $"{prefix}_Person1",
			};
			var person2 = new Person(personId)
			{
				Name = $"{prefix}_Person2",
			};

			objectCreator.CreatePerson(person1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreatePerson(person2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personIdInUseError = personError as PersonIdInUseError;
			Assert.IsNotNull(personIdInUseError);
			Assert.AreEqual(personId, personIdInUseError.Id);
			Assert.AreEqual(errorMessage, personIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var personId = Guid.NewGuid();

			var person1 = new Person(personId)
			{
				Name = $"{prefix}_Person1",
			};
			var person2 = new Person(personId)
			{
				Name = $"{prefix}_Person2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreatePeople([person1, person2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(personId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var personErrors = traceData.ErrorData.OfType<PersonError>().ToList();
			Assert.AreEqual(2, personErrors.Count);

			var errorMessages = new List<string>
			{
				$"Person '{person1.Name}' has a duplicate ID.",
				$"Person '{person2.Name}' has a duplicate ID.",
			};

			foreach (var error in personErrors)
			{
				var personDuplicateIdError = error as PersonDuplicateIdError;
				Assert.IsNotNull(personDuplicateIdError);
				Assert.AreEqual(personId, personDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person1 = new Person
			{
				Name = $"{prefix}_Person",
			};
			var person2 = new Person
			{
				Name = $"{prefix}_Person",
			};

			objectCreator.CreatePerson(person1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreatePerson(person2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personNameExistsError = personError as PersonNameExistsError;
			Assert.IsNotNull(personNameExistsError);
			Assert.AreEqual(person2.Id, personNameExistsError.Id);
			Assert.AreEqual(person2.Name, personNameExistsError.Name);
			Assert.AreEqual(errorMessage, personNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person1 = new Person
			{
				Name = $"{prefix}_Person",
			};
			var person2 = new Person
			{
				Name = $"{prefix}_Person",
			};

			var peopleToCreate = new List<Person> { person1, person2 };

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreatePeople(peopleToCreate);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.TraceDataPerItem.Count);

			foreach (var traceData in expectedException.Result.TraceDataPerItem.Values)
			{
				Assert.AreEqual(1, traceData.ErrorData.Count);
				var personError = traceData.ErrorData.OfType<PersonError>().SingleOrDefault();
				Assert.IsNotNull(personError);

				var personDuplicateNameError = personError as PersonDuplicateNameError;
				Assert.IsNotNull(personDuplicateNameError);

				var person = peopleToCreate.Single(c => c.Id == personDuplicateNameError.Id);
				Assert.IsNotNull(person);

				Assert.AreEqual(person.Name, personDuplicateNameError.Name);
				Assert.AreEqual($"Person '{person.Name}' has a duplicate name.", personDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var person = new Person
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidNameError = personError as PersonInvalidNameError;
			Assert.IsNotNull(personInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", personInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var person = new Person
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreatePerson(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidNameError = personError as PersonInvalidNameError;
			Assert.IsNotNull(personInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", personInvalidNameError.ErrorMessage);
		}
	}
}
