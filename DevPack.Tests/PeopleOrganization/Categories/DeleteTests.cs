namespace RT_PeopleAndOrganizations.PeopleOrganization.Categories
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DeleteTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeleteTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void WhenUsedByOrganizationThrowsException()
		{
			var prefix = Guid.NewGuid();

			var category = new Category
			{
				Name = $"{prefix}_Category",
			};
			category = objectCreator.CreateCategory(category);

			var organization = new Organization
			{
				Name = $"{prefix}_Organization",
				CategoryId = category.Id,
			};
			organization = objectCreator.CreateOrganization(organization);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Categories.Delete(category);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Category '{category.Name}' is in use by 1 organization(s).";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryInUseByOrganizationsError = categoryError as CategoryInUseByOrganizationsError;
			Assert.IsNotNull(categoryInUseByOrganizationsError);
			Assert.AreEqual(category.Id, categoryInUseByOrganizationsError.Id);
			Assert.AreEqual(errorMessage, categoryInUseByOrganizationsError.ErrorMessage);
			Assert.AreEqual(1, categoryInUseByOrganizationsError.OrganizationIds.Count);
			Assert.IsTrue(categoryInUseByOrganizationsError.OrganizationIds.Contains(organization.Id));
		}
	}
}
