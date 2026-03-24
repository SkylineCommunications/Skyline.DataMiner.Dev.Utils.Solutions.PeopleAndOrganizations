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
	public sealed class DeprecatedStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeprecatedStateTests()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

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
		public void DeprecateThrowsException()
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

			// Deprecate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Deprecate(person);
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
			Assert.AreEqual("Not allowed to deprecate a person that is not in Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void Delete()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Delete
			TestContext.Api.People.Delete(person);

			person = TestContext.Api.People.Read(personId);
			Assert.IsNull(person);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(personId)).SingleOrDefault();
			Assert.IsNull(domPerson);
		}

		[TestMethod]
		public void UpdateNameThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update name
			var updatedName = $"{name}_Updated";
			person.Name = updatedName;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignEmailThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Assign email
			person.Email = email;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateEmailThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update email
			var updatedEmail = "support@skyline.be";
			person.Email = updatedEmail;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignPhoneThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Assign phone
			person.Phone = phone;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdatePhoneThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update phone
			var updatedPhone = "+32 15 47 11 11";
			person.Phone = updatedPhone;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignStreetAddressThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Assign street address
			person.StreetAddress = streetAddress;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateStreetAddressThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update street address
			var updatedStreetAddress = "Kardinaal Mercierplein 1";
			person.StreetAddress = updatedStreetAddress;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignCityThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Assign city
			person.City = city;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateCityThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update city
			var updatedCity = "Antwerp";
			person.City = updatedCity;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignZipCodeThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Assign zip code
			person.ZipCode = zipCode;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateZipCodeThrowsException()
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

			// Deprecate
			person = TestContext.Api.People.Deprecate(person);

			// Update zip code
			var updatedZipCode = "2000";
			person.ZipCode = updatedZipCode;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Update(person);
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
			Assert.AreEqual("Not allowed to update a person that is not in Draft or Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}
	}
}
