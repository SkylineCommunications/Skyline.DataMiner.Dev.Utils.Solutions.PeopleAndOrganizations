namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	using SLDataGateway.API.Querying;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class BasicTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public BasicTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void BasicCrudActions()
		{
			var prefix = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var name = $"{prefix}_Person";

			var person = new Person(personId)
			{
				Name = name,
			};

			// Create
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(personId, person.Id);
			Assert.AreEqual(name, person.Name);
			Assert.AreEqual(PersonState.Draft, person.State);

			var returnedPerson = TestContext.Api.People.Read(personId);
			Assert.IsNotNull(returnedPerson);
			Assert.AreEqual(person.Id, returnedPerson.Id);
			Assert.AreEqual(person.Name, returnedPerson.Name);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(personId)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.People.Id, domPerson.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Draft, domPerson.StatusId);
			Assert.IsTrue(domPerson.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.Id.Id));

			var domPersonInformation = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.Id.Id);
			var fdPersonName = domPersonInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName.Id);
			Assert.IsNotNull(fdPersonName);
			Assert.AreEqual(returnedPerson.Name, Convert.ToString(fdPersonName.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			person.Name = updatedName;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(personId, person.Id);
			Assert.AreEqual(updatedName, person.Name);
			Assert.AreEqual(PersonState.Draft, person.State);

			returnedPerson = TestContext.Api.People.Read(personId);
			Assert.IsNotNull(returnedPerson);
			Assert.AreEqual(person.Id, returnedPerson.Id);
			Assert.AreEqual(person.Name, returnedPerson.Name);

			domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(personId)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.People.Id, domPerson.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Draft, domPerson.StatusId);
			Assert.IsTrue(domPerson.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.Id.Id));

			domPersonInformation = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.Id.Id);
			fdPersonName = domPersonInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName.Id);
			Assert.IsNotNull(fdPersonName);
			Assert.AreEqual(returnedPerson.Name, Convert.ToString(fdPersonName.Value.Value));

			// Delete
			TestContext.Api.People.Delete(person);

			returnedPerson = TestContext.Api.People.Read(personId);
			Assert.IsNull(returnedPerson);

			domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(personId)).SingleOrDefault();
			Assert.IsNull(domPerson);
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person1 = new Person
			{
				Name = $"{prefix}_Person1",
			};
			var person2 = new Person
			{
				Name = $"{prefix}_Person2",
			};

			var createdPeople = objectCreator.CreatePeople([person1, person2]);
			var toUpdate = createdPeople.Single(x => x.Id == person2.Id);
			toUpdate.Name = person1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.People.Update(toUpdate);
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
			Assert.AreEqual(toUpdate.Id, personNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, personNameExistsError.Name);
			Assert.AreEqual(errorMessage, personNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void UpdateUnmodifiedPerson()
		{
			var person = new Person
			{
				Name = $"{Guid.NewGuid()}_Person",
			};

			person = objectCreator.CreatePerson(person);

			var originalPerson = TestContext.Api.People.Read(person.Id);
			var updatedPerson = TestContext.Api.People.Update(originalPerson);

			Assert.AreEqual(originalPerson, updatedPerson);
		}

		[TestMethod]
		public void BulkUpdateWithChangedAndUnchangedPersonReturnsTwoPeople()
		{
			var prefix = Guid.NewGuid();

			var changedPerson = new Person { Name = $"{prefix}_Changed" };
			var unchangedPerson = new Person { Name = $"{prefix}_Unchanged" };

			objectCreator.CreatePeople([changedPerson, unchangedPerson]);

			var changedToUpdate = TestContext.Api.People.Read(changedPerson.Id);
			var unchangedToUpdate = TestContext.Api.People.Read(unchangedPerson.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";

			var updatedPeople = TestContext.Api.People.Update(new[] { changedToUpdate, unchangedToUpdate });

			Assert.AreEqual(2, updatedPeople.Count);
			Assert.IsTrue(updatedPeople.Any(x => x.Id == changedPerson.Id));
			Assert.IsTrue(updatedPeople.Any(x => x.Id == unchangedPerson.Id));

			var changedAfterUpdate = TestContext.Api.People.Read(changedPerson.Id);
			var unchangedAfterUpdate = TestContext.Api.People.Read(unchangedPerson.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(unchangedPerson.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void BulkUpdateWithChangedInvalidAndUnchangedPersonReturnsTwoSuccessfulIds()
		{
			var prefix = Guid.NewGuid();

			var changedPerson = new Person { Name = $"{prefix}_Changed" };
			var invalidPerson = new Person { Name = $"{prefix}_Invalid" };
			var unchangedPerson = new Person { Name = $"{prefix}_Unchanged" };

			objectCreator.CreatePeople([changedPerson, invalidPerson, unchangedPerson]);

			var changedToUpdate = TestContext.Api.People.Read(changedPerson.Id);
			var invalidToUpdate = TestContext.Api.People.Read(invalidPerson.Id);
			var unchangedToUpdate = TestContext.Api.People.Read(unchangedPerson.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";
			invalidToUpdate.Name = string.Empty;

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.People.Update(new[] { changedToUpdate, invalidToUpdate, unchangedToUpdate });
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(changedPerson.Id));
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(unchangedPerson.Id));
			Assert.AreEqual(1, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(invalidPerson.Id));

			var changedAfterUpdate = TestContext.Api.People.Read(changedPerson.Id);
			var invalidAfterUpdate = TestContext.Api.People.Read(invalidPerson.Id);
			var unchangedAfterUpdate = TestContext.Api.People.Read(unchangedPerson.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(invalidPerson.Name, invalidAfterUpdate.Name);
			Assert.AreEqual(unchangedPerson.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var people = TestContext.Api.People.Read(new List<Guid>());
			Assert.IsNotNull(people);
			Assert.AreEqual(0, people.Count());
		}

		[TestMethod]
		public void ReadWithEmptyFilterReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Person>(idsToRetrieve.Select(x => PersonExposers.Id.Equal(x)).ToArray());

			var people = TestContext.Api.People.Read(emptyFilter);
			Assert.IsNotNull(people);
			Assert.AreEqual(0, people.Count());
		}

		[TestMethod]
		public void CountWithEmptyFilterReturnsZero()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Person>(idsToRetrieve.Select(x => PersonExposers.Id.Equal(x)).ToArray());

			var count = TestContext.Api.People.Count(emptyFilter);
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void ReadWithEmptyQueryReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Person>(idsToRetrieve.Select(x => PersonExposers.Id.Equal(x)).ToArray());
			var queryWithEmptyFilter = emptyFilter.ToQuery();

			var people = TestContext.Api.People.Read(queryWithEmptyFilter);
			Assert.IsNotNull(people);
			Assert.AreEqual(0, people.Count());
		}
	}
}
