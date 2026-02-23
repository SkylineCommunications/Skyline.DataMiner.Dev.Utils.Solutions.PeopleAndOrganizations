namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class CategoryAssignmentTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public CategoryAssignmentTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void CreateWithNotExistingThrowsException()
		{
			var prefix = Guid.NewGuid();
			var categoryId = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
				CategoryId = categoryId,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization = objectCreator.CreateOrganization(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationCategoryNotFoundError = organizationError as OrganizationCategoryNotFoundError;
			Assert.IsNotNull(organizationCategoryNotFoundError);
			Assert.AreEqual($"Category with ID '{categoryId}' not found.", organizationCategoryNotFoundError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationCategoryNotFoundError.Id);
			Assert.AreEqual(categoryId, organizationCategoryNotFoundError.CategoryId);
		}

		[TestMethod]
		public void UpdateWithNotExistingThrowsException()
		{
			var prefix = Guid.NewGuid();
			var categoryId = Guid.NewGuid();

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
			};
			organization = objectCreator.CreateOrganization(organization);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				organization.CategoryId = categoryId;
				TestContext.Api.Organizations.Update(organization);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var organizationError = expectedException.TraceData.ErrorData.OfType<OrganizationError>().SingleOrDefault();
			Assert.IsNotNull(organizationError);

			var organizationCategoryNotFoundError = organizationError as OrganizationCategoryNotFoundError;
			Assert.IsNotNull(organizationCategoryNotFoundError);
			Assert.AreEqual($"Category with ID '{categoryId}' not found.", organizationCategoryNotFoundError.ErrorMessage);
			Assert.AreEqual(organization.Id, organizationCategoryNotFoundError.Id);
			Assert.AreEqual(categoryId, organizationCategoryNotFoundError.CategoryId);
		}
	}
}
