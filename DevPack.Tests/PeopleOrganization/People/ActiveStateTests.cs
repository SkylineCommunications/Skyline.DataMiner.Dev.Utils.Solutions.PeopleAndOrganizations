namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class ActiveStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public ActiveStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ActivateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Activate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Activate(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidStateError = personError as PersonInvalidStateError;
			Assert.IsNotNull(personInvalidStateError);
			Assert.AreEqual("Not allowed to activate a person that is not in Draft state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void Deprecate()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Deprecated, person.State);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Deprecated, domPerson.StatusId);
		}

		[TestMethod]
		public void DeleteThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);
			var personId = person.Id;

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Delete
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.People.Delete(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidStateError = personError as PersonInvalidStateError;
			Assert.IsNotNull(personInvalidStateError);
			Assert.AreEqual("Not allowed to delete a person that is not in Draft or Deprecated state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateName()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Person";

			var person = new Person
			{
				Name = name,
			};

			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(name, person.Name);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update name
			var updatedName = $"{name}_Updated";
			person.Name = updatedName;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedName, person.Name);
		}

		[TestMethod]
		public void AssignEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Assign email
			person.Email = email;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(email, person.Email);
		}

		[TestMethod]
		public void UpdateEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				Email = email,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(email, person.Email);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update email
			var updatedEmail = "support@skyline.be";
			person.Email = updatedEmail;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedEmail, person.Email);
		}

		[TestMethod]
		public void AssignPhone()
		{
			var prefix = Guid.NewGuid();
			var phone = "+32 15 47 00 00";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Assign phone
			person.Phone = phone;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(phone, person.Phone);
		}

		[TestMethod]
		public void UpdatePhone()
		{
			var prefix = Guid.NewGuid();
			var phone = "+32 15 47 00 00";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				Phone = phone,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(phone, person.Phone);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update phone
			var updatedPhone = "+32 15 47 11 11";
			person.Phone = updatedPhone;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedPhone, person.Phone);
		}

		[TestMethod]
		public void AssignStreetAddress()
		{
			var prefix = Guid.NewGuid();
			var streetAddress = "Ambachtenstraat 33";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Assign street address
			person.StreetAddress = streetAddress;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(streetAddress, person.StreetAddress);
		}

		[TestMethod]
		public void UpdateStreetAddress()
		{
			var prefix = Guid.NewGuid();
			var streetAddress = "Ambachtenstraat 33";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				StreetAddress = streetAddress,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(streetAddress, person.StreetAddress);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update street address
			var updatedStreetAddress = "Kardinaal Mercierplein 1";
			person.StreetAddress = updatedStreetAddress;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedStreetAddress, person.StreetAddress);
		}

		[TestMethod]
		public void AssignCity()
		{
			var prefix = Guid.NewGuid();
			var city = "Wommelgem";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Assign city
			person.City = city;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(city, person.City);
		}

		[TestMethod]
		public void UpdateCity()
		{
			var prefix = Guid.NewGuid();
			var city = "Wommelgem";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				City = city,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(city, person.City);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update city
			var updatedCity = "Antwerp";
			person.City = updatedCity;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedCity, person.City);
		}

		[TestMethod]
		public void AssignZipCode()
		{
			var prefix = Guid.NewGuid();
			var zipCode = "2160";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Assign zip code
			person.ZipCode = zipCode;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(zipCode, person.ZipCode);
		}

		[TestMethod]
		public void UpdateZipCode()
		{
			var prefix = Guid.NewGuid();
			var zipCode = "2160";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				ZipCode = zipCode,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(zipCode, person.ZipCode);

			// Activate
			person = TestContext.Api.People.Activate(person);

			// Update zip code
			var updatedZipCode = "2000";
			person.ZipCode = updatedZipCode;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedZipCode, person.ZipCode);
		}
	}
}
