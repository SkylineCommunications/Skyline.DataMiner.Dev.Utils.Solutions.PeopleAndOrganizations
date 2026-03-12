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
	public sealed class DraftStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DraftStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void Activate()
		{
			var prefix = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			// Activate
			organization = TestContext.Api.Organizations.Activate(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(OrganizationState.Active, organization.State);

			var domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organization.Id)).SingleOrDefault();
			Assert.IsNotNull(domOrganization);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Active, domOrganization.StatusId);
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

			// Deprecate
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

			// Delete
			TestContext.Api.Organizations.Delete(organization);

			organization = TestContext.Api.Organizations.Read(organizationId);
			Assert.IsNull(organization);

			var domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organizationId)).SingleOrDefault();
			Assert.IsNull(domOrganization);
		}

		[TestMethod]
		public void UpdateName()
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

			// Update name
			var updatedName = $"{name}_Updated";
			organization.Name = updatedName;

			organization = TestContext.Api.Organizations.Update(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(updatedName, organization.Name);
		}

		[TestMethod]
		public void AssignCategory()
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

			// Assign category
			organization.CategoryId = category.Id;

			organization = TestContext.Api.Organizations.Update(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(category.Id, organization.CategoryId);
		}

		[TestMethod]
		public void UpdateCategory()
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

			// Update category
			organization.CategoryId = category2.Id;

			organization = TestContext.Api.Organizations.Update(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(category2.Id, organization.CategoryId);
		}
	}
}
