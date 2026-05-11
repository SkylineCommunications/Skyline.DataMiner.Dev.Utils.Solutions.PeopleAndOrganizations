namespace RT_PeopleAndOrganizations.PeopleOrganization.Categories
{
	using System;
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
			var categoryId = Guid.NewGuid();
			var name = $"{prefix}_Category";

			var category = new Category(categoryId)
			{
				Name = name,
			};

			// Create
			category = objectCreator.CreateCategory(category);
			Assert.IsNotNull(category);
			Assert.AreEqual(categoryId, category.Id);
			Assert.AreEqual(name, category.Name);

			var returnedCategory = TestContext.Api.Categories.Read(categoryId);
			Assert.IsNotNull(returnedCategory);
			Assert.AreEqual(category.Id, returnedCategory.Id);
			Assert.AreEqual(category.Name, returnedCategory.Name);

			var domCategory = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(categoryId)).SingleOrDefault();
			Assert.IsNotNull(domCategory);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Category.Id, domCategory.DomDefinitionId.Id);

			Assert.IsTrue(domCategory.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Id.Id));
			var domCategoryInformation = domCategory.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Id.Id);
			var fdCategory = domCategoryInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category.Id);
			Assert.IsNotNull(fdCategory);
			Assert.AreEqual(returnedCategory.Name, Convert.ToString(fdCategory.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			category.Name = updatedName;

			category = TestContext.Api.Categories.Update(category);
			Assert.IsNotNull(category);
			Assert.AreEqual(categoryId, category.Id);
			Assert.AreEqual(updatedName, category.Name);

			returnedCategory = TestContext.Api.Categories.Read(categoryId);
			Assert.IsNotNull(returnedCategory);
			Assert.AreEqual(category.Id, returnedCategory.Id);
			Assert.AreEqual(category.Name, returnedCategory.Name);

			domCategory = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(categoryId)).SingleOrDefault();
			Assert.IsNotNull(domCategory);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Category.Id, domCategory.DomDefinitionId.Id);

			Assert.IsTrue(domCategory.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Id.Id));
			domCategoryInformation = domCategory.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Id.Id);
			fdCategory = domCategoryInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category.Id);
			Assert.IsNotNull(fdCategory);
			Assert.AreEqual(returnedCategory.Name, Convert.ToString(fdCategory.Value.Value));

			// Delete
			TestContext.Api.Categories.Delete(category);

			returnedCategory = TestContext.Api.Categories.Read(categoryId);
			Assert.IsNull(returnedCategory);

			domCategory = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(categoryId)).SingleOrDefault();
			Assert.IsNull(domCategory);
		}

		[TestMethod]
		public void CreateWithExistingIdThrowsException()
		{
			var prefix = Guid.NewGuid();
			var categoryId = Guid.NewGuid();

			var category1 = new Category(categoryId)
			{
				Name = $"{prefix}_Category1",
			};
			var category2 = new Category(categoryId)
			{
				Name = $"{prefix}_Category2",
			};

			objectCreator.CreateCategory(category1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateCategory(category2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryIdInUseError = categoryError as CategoryIdInUseError;
			Assert.IsNotNull(categoryIdInUseError);
			Assert.AreEqual(categoryId, categoryIdInUseError.Id);
			Assert.AreEqual(errorMessage, categoryIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var categoryId = Guid.NewGuid();

			var category1 = new Category(categoryId)
			{
				Name = $"{prefix}_Category1",
			};
			var category2 = new Category(categoryId)
			{
				Name = $"{prefix}_Category2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateCategories([category1, category2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(categoryId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var categoryErrors = traceData.ErrorData.OfType<CategoryError>().ToList();
			Assert.AreEqual(2, categoryErrors.Count);

			var errorMessages = new List<string>
			{
				$"Category '{category1.Name}' has a duplicate ID.",
				$"Category '{category2.Name}' has a duplicate ID.",
			};

			foreach (var error in categoryErrors)
			{
				var categoryDuplicateIdError = error as CategoryDuplicateIdError;
				Assert.IsNotNull(categoryDuplicateIdError);
				Assert.AreEqual(categoryId, categoryDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var category1 = new Category
			{
				Name = $"{prefix}_Category",
			};
			var category2 = new Category
			{
				Name = $"{prefix}_Category",
			};

			objectCreator.CreateCategory(category1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateCategory(category2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryNameExistsError = categoryError as CategoryNameExistsError;
			Assert.IsNotNull(categoryNameExistsError);
			Assert.AreEqual(category2.Id, categoryNameExistsError.Id);
			Assert.AreEqual(category2.Name, categoryNameExistsError.Name);
			Assert.AreEqual(errorMessage, categoryNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var category1 = new Category
			{
				Name = $"{prefix}_Category",
			};
			var category2 = new Category
			{
				Name = $"{prefix}_Category",
			};

			var categoriesToCreate = new List<Category> { category1, category2};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateCategories(categoriesToCreate);
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
				var categoryError = traceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
				Assert.IsNotNull(categoryError);

				var categoryDuplicateNameError = categoryError as CategoryDuplicateNameError;
				Assert.IsNotNull(categoryDuplicateNameError);

				var category = categoriesToCreate.Single(c => c.Id == categoryDuplicateNameError.Id);
				Assert.IsNotNull(category);

				Assert.AreEqual(category.Name, categoryDuplicateNameError.Name);
				Assert.AreEqual($"Category '{category.Name}' has a duplicate name.", categoryDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
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

			var createdCategories = objectCreator.CreateCategories([category1, category2]);
			var toUpdate = createdCategories.Single(x => x.Id == category2.Id);
			toUpdate.Name = category1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Categories.Update(toUpdate);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryNameExistsError = categoryError as CategoryNameExistsError;
			Assert.IsNotNull(categoryNameExistsError);
			Assert.AreEqual(toUpdate.Id, categoryNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, categoryNameExistsError.Name);
			Assert.AreEqual(errorMessage, categoryNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var categories = TestContext.Api.Categories.Read(new List<Guid>());
			Assert.IsNotNull(categories);
			Assert.AreEqual(0, categories.Count());
		}

		[TestMethod]
		public void ReadWithEmptyFilterReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Category>(idsToRetrieve.Select(x => CategoryExposers.Id.Equal(x)).ToArray());

			var categories = TestContext.Api.Categories.Read(emptyFilter);
			Assert.IsNotNull(categories);
			Assert.AreEqual(0, categories.Count());
		}

		[TestMethod]
		public void CountWithEmptyFilterReturnsZero()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Category>(idsToRetrieve.Select(x => CategoryExposers.Id.Equal(x)).ToArray());

			var count = TestContext.Api.Categories.Count(emptyFilter);
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void ReadWithEmptyQueryReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Category>(idsToRetrieve.Select(x => CategoryExposers.Id.Equal(x)).ToArray());
			var queryWithEmptyFilter = emptyFilter.ToQuery();

			var categories = TestContext.Api.Categories.Read(queryWithEmptyFilter);
			Assert.IsNotNull(categories);
			Assert.AreEqual(0, categories.Count());
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var category = new Category
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateCategory(category);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryInvalidNameError = categoryError as CategoryInvalidNameError;
			Assert.IsNotNull(categoryInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", categoryInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var category = new Category
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateCategory(category);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var categoryError = expectedException.TraceData.ErrorData.OfType<CategoryError>().SingleOrDefault();
			Assert.IsNotNull(categoryError);

			var categoryInvalidNameError = categoryError as CategoryInvalidNameError;
			Assert.IsNotNull(categoryInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", categoryInvalidNameError.ErrorMessage);
		}
	}
}
