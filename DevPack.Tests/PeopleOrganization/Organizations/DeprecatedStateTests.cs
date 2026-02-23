namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
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

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Activate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = TestContext.Api.Organizations.Activate(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidStateError = organizationError as OrganizationInvalidStateError;
			Assert.IsNotNull(organizationInvalidStateError);
			Assert.AreEqual("Not allowed to activate an organization that is not in Draft state.", organizationInvalidStateError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationInvalidStateError.Id);
		}

		[TestMethod]
		public void DeprecateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Deprecate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = TestContext.Api.Organizations.Deprecate(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidStateError = organizationError as OrganizationInvalidStateError;
			Assert.IsNotNull(organizationInvalidStateError);
			Assert.AreEqual("Not allowed to deprecate an organization that is not in Active state.", organizationInvalidStateError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationInvalidStateError.Id);
		}

		[TestMethod]
		public void Delete()
		{
			var prefix = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);
			var organizationId = organization.Id;

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Delete
			TestContext.Api.Organizations.Delete(organization);

			organization = TestContext.Api.Organizations.Read(organizationId);
			Assert.IsNull(organization);

			var domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organizationId)).SingleOrDefault();
			Assert.IsNull(domOrganization);
		}

		[TestMethod]
		public void UpdateNameThrowsException()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Organization";

			var organization = new Organization
			{
				Name = name,
			};

			organization = objectCreator.CreateOrganization(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(name, organization.Name);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Update name
			var updatedName = $"{name}_Updated";
			organization.Name = updatedName;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = TestContext.Api.Organizations.Update(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidStateError = organizationError as OrganizationInvalidStateError;
			Assert.IsNotNull(organizationInvalidStateError);
			Assert.AreEqual("Not allowed to update an organization that is not in Draft or Active state.", organizationInvalidStateError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignCategoryThrowsException()
		{
			var prefix = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			var category = new Category
			{
				Name = $"{prefix}_Category",
			};
			objectCreator.CreateCategory(category);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Assign category
			organization.CategoryId = category.Id;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = TestContext.Api.Organizations.Update(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidStateError = organizationError as OrganizationInvalidStateError;
			Assert.IsNotNull(organizationInvalidStateError);
			Assert.AreEqual("Not allowed to update an organization that is not in Draft or Active state.", organizationInvalidStateError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateCategoryThrowsException()
		{
			var prefix = Guid.NewGuid();

			var category1 = new Category
			{
				Name = $"{prefix}_Category1",
			};
			var category2 = new Category
			{
				Name = $"{prefix}_Category2",
			};
			objectCreator.CreateCategories([category1, category2]);

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
				CategoryId = category1.Id,
			};
			organization = objectCreator.CreateOrganization(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(category1.Id, organization.CategoryId);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);

			// Deprecate
			organization = TestContext.Api.Organizations.Deprecate(organization);

			// Update category
			organization.CategoryId = category2.Id;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = TestContext.Api.Organizations.Update(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationInvalidStateError = organizationError as OrganizationInvalidStateError;
			Assert.IsNotNull(organizationInvalidStateError);
			Assert.AreEqual("Not allowed to update an organization that is not in Draft or Active state.", organizationInvalidStateError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationInvalidStateError.Id);
		}
	}
}
