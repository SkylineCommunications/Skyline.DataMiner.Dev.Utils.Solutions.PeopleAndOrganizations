namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
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
			var organizationId = Guid.NewGuid();

			var organization1 = new Organization(organizationId)
			{
				Name = $"{prefix}_Organization1",
			};
			var organization2 = new Organization(organizationId)
			{
				Name = $"{prefix}_Organization2",
			};

			objectCreator.CreateOrganization(organization1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateOrganization(organization2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationIdInUseError = organizationError as OrganizationIdInUseError;
			Assert.IsNotNull(organizationIdInUseError);
			Assert.AreEqual(organizationId, organizationIdInUseError.Id);
			Assert.AreEqual(errorMessage, organizationIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var organizationId = Guid.NewGuid();

			var organization1 = new Organization(organizationId)
			{
				Name = $"{prefix}_Organization1",
			};
			var organization2 = new Organization(organizationId)
			{
				Name = $"{prefix}_Organization2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateOrganizations([organization1, organization2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(organizationId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var organizationErrors = traceData.ErrorData.OfType<OrganizationError>().ToList();
			Assert.AreEqual(2, organizationErrors.Count);

			var errorMessages = new List<string>
			{
				$"Organization '{organization1.Name}' has a duplicate ID.",
				$"Organization '{organization2.Name}' has a duplicate ID.",
			};

			foreach (var error in organizationErrors)
			{
				var organizationDuplicateIdError = error as OrganizationDuplicateIdError;
				Assert.IsNotNull(organizationDuplicateIdError);
				Assert.AreEqual(organizationId, organizationDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var organization1 = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			var organization2 = new Organization
			{
				Name = $"{prefix}_Organization",
			};

			objectCreator.CreateOrganization(organization1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateOrganization(organization2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationNameExistsError = organizationError as OrganizationNameExistsError;
			Assert.IsNotNull(organizationNameExistsError);
			Assert.AreEqual(organization2.Id, organizationNameExistsError.Id);
			Assert.AreEqual(organization2.Name, organizationNameExistsError.Name);
			Assert.AreEqual(errorMessage, organizationNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var organization1 = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			var organization2 = new Organization
			{
				Name = $"{prefix}_Organization",
			};

			var organizationsToCreate = new List<Organization> { organization1, organization2 };

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateOrganizations(organizationsToCreate);
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
				var organizationError = traceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
				Assert.IsNotNull(organizationError);

				var organizationDuplicateNameError = organizationError as OrganizationDuplicateNameError;
				Assert.IsNotNull(organizationDuplicateNameError);

				var organization = organizationsToCreate.Single(c => c.Id == organizationDuplicateNameError.Id);
				Assert.IsNotNull(organization);

				Assert.AreEqual(organization.Name, organizationDuplicateNameError.Name);
				Assert.AreEqual($"Organization '{organization.Name}' has a duplicate name.", organizationDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var organization = new Organization
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateOrganization(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidNameError = organizationError as OrganizationInvalidNameError;
			Assert.IsNotNull(organizationInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", organizationInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var organization = new Organization
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateOrganization(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidNameError = organizationError as OrganizationInvalidNameError;
			Assert.IsNotNull(organizationInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", organizationInvalidNameError.ErrorMessage);
		}
	}
}
