namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
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
			var organizationId = Guid.NewGuid();
			var name = $"{prefix}_Organization";

			var organization = new Organization(organizationId)
			{
				Name = name,
			};

			// Create
			organization = objectCreator.CreateOrganization(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(organizationId, organization.Id);
			Assert.AreEqual(name, organization.Name);
			Assert.AreEqual(OrganizationState.Draft, organization.State);

			var returnedOrganization = TestContext.Api.Organizations.Read(organizationId);
			Assert.IsNotNull(returnedOrganization);
			Assert.AreEqual(organization.Id, returnedOrganization.Id);
			Assert.AreEqual(organization.Name, returnedOrganization.Name);

			var domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organizationId)).SingleOrDefault();
			Assert.IsNotNull(domOrganization);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Organizations.Id, domOrganization.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Draft, domOrganization.StatusId);
			Assert.IsTrue(domOrganization.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Id.Id));
			Assert.IsFalse(domOrganization.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Contracts.Id.Id));

			var domOrganizationInformation = domOrganization.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Id.Id);
			var fdOrganization = domOrganizationInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName.Id);
			Assert.IsNotNull(fdOrganization);
			Assert.AreEqual(returnedOrganization.Name, Convert.ToString(fdOrganization.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			organization.Name = updatedName;

			organization = TestContext.Api.Organizations.Update(organization);
			Assert.IsNotNull(organization);
			Assert.AreEqual(organizationId, organization.Id);
			Assert.AreEqual(updatedName, organization.Name);
			Assert.AreEqual(OrganizationState.Draft, organization.State);

			returnedOrganization = TestContext.Api.Organizations.Read(organizationId);
			Assert.IsNotNull(returnedOrganization);
			Assert.AreEqual(organization.Id, returnedOrganization.Id);
			Assert.AreEqual(organization.Name, returnedOrganization.Name);

			domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organizationId)).SingleOrDefault();
			Assert.IsNotNull(domOrganization);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Organizations.Id, domOrganization.DomDefinitionId.Id);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Draft, domOrganization.StatusId);
			Assert.IsTrue(domOrganization.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Id.Id));
			Assert.IsFalse(domOrganization.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Contracts.Id.Id));

			domOrganizationInformation = domOrganization.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Id.Id);
			fdOrganization = domOrganizationInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName.Id);
			Assert.IsNotNull(fdOrganization);
			Assert.AreEqual(returnedOrganization.Name, Convert.ToString(fdOrganization.Value.Value));

			// Delete
			TestContext.Api.Organizations.Delete(organization);

			returnedOrganization = TestContext.Api.Organizations.Read(organizationId);
			Assert.IsNull(returnedOrganization);

			domOrganization = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(organizationId)).SingleOrDefault();
			Assert.IsNull(domOrganization);
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var organization1 = new Organization
			{
				Name = $"{prefix}_Organization1",
			};
			var organization2 = new Organization
			{
				Name = $"{prefix}_Organization2",
			};

			var createdOrganizations = objectCreator.CreateOrganizations([organization1, organization2]);
			var toUpdate = createdOrganizations.Single(x => x.Id == organization2.Id);
			toUpdate.Name = organization1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Organizations.Update(toUpdate);
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
			Assert.AreEqual(toUpdate.Id, organizationNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, organizationNameExistsError.Name);
			Assert.AreEqual(errorMessage, organizationNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var organizations = TestContext.Api.Organizations.Read(new List<Guid>());
			Assert.IsNotNull(organizations);
			Assert.AreEqual(0, organizations.Count());
		}
	}
}
