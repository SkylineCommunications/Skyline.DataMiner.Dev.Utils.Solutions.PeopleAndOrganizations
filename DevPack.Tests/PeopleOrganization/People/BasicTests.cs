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
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var people = TestContext.Api.People.Read(new List<Guid>());
			Assert.IsNotNull(people);
			Assert.AreEqual(0, people.Count());
		}
	}
}
