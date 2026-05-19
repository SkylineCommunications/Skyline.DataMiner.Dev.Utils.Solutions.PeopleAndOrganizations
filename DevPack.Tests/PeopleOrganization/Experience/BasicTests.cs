namespace RT_PeopleAndOrganizations.PeopleOrganization.Experience
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
			var experienceId = Guid.NewGuid();
			var name = $"{prefix}_Experience";

			var experience = new Experience(experienceId)
			{
				Name = name,
			};

			// Create
			experience = objectCreator.CreateExperience(experience);
			Assert.IsNotNull(experience);
			Assert.AreEqual(experienceId, experience.Id);
			Assert.AreEqual(name, experience.Name);

			var returnedExperience = TestContext.Api.Experience.Read(experienceId);
			Assert.IsNotNull(returnedExperience);
			Assert.AreEqual(experience.Id, returnedExperience.Id);
			Assert.AreEqual(experience.Name, returnedExperience.Name);

			var domExperience = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(experienceId)).SingleOrDefault();
			Assert.IsNotNull(domExperience);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Experience.Id, domExperience.DomDefinitionId.Id);

			Assert.IsTrue(domExperience.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Id.Id));
			var domExperienceInformation = domExperience.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Id.Id);
			var fdExperience = domExperienceInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience.Id);
			Assert.IsNotNull(fdExperience);
			Assert.AreEqual(returnedExperience.Name, Convert.ToString(fdExperience.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			experience.Name = updatedName;

			experience = TestContext.Api.Experience.Update(experience);
			Assert.IsNotNull(experience);
			Assert.AreEqual(experienceId, experience.Id);
			Assert.AreEqual(updatedName, experience.Name);

			returnedExperience = TestContext.Api.Experience.Read(experienceId);
			Assert.IsNotNull(returnedExperience);
			Assert.AreEqual(experience.Id, returnedExperience.Id);
			Assert.AreEqual(experience.Name, returnedExperience.Name);

			domExperience = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(experienceId)).SingleOrDefault();
			Assert.IsNotNull(domExperience);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Experience.Id, domExperience.DomDefinitionId.Id);

			Assert.IsTrue(domExperience.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Id.Id));
			domExperienceInformation = domExperience.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Id.Id);
			fdExperience = domExperienceInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience.Id);
			Assert.IsNotNull(fdExperience);
			Assert.AreEqual(returnedExperience.Name, Convert.ToString(fdExperience.Value.Value));

			// Delete
			TestContext.Api.Experience.Delete(experience);

			returnedExperience = TestContext.Api.Experience.Read(experienceId);
			Assert.IsNull(returnedExperience);

			domExperience = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(experienceId)).SingleOrDefault();
			Assert.IsNull(domExperience);
		}

		[TestMethod]
		public void CreateWithExistingIdThrowsException()
		{
			var prefix = Guid.NewGuid();
			var experienceId = Guid.NewGuid();

			var experience1 = new Experience(experienceId)
			{
				Name = $"{prefix}_Experience1",
			};
			var experience2 = new Experience(experienceId)
			{
				Name = $"{prefix}_Experience2",
			};

			objectCreator.CreateExperience(experience1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateExperience(experience2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceIdInUseError = experienceError as ExperienceIdInUseError;
			Assert.IsNotNull(experienceIdInUseError);
			Assert.AreEqual(experienceId, experienceIdInUseError.Id);
			Assert.AreEqual(errorMessage, experienceIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var experienceId = Guid.NewGuid();

			var experience1 = new Experience(experienceId)
			{
				Name = $"{prefix}_Experience1",
			};
			var experience2 = new Experience(experienceId)
			{
				Name = $"{prefix}_Experience2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateExperience([experience1, experience2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(experienceId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var experienceErrors = traceData.ErrorData.OfType<ExperienceError>().ToList();
			Assert.AreEqual(2, experienceErrors.Count);

			var errorMessages = new List<string>
			{
				$"Experience '{experience1.Name}' has a duplicate ID.",
				$"Experience '{experience2.Name}' has a duplicate ID.",
			};

			foreach (var error in experienceErrors)
			{
				var experienceDuplicateIdError = error as ExperienceDuplicateIdError;
				Assert.IsNotNull(experienceDuplicateIdError);
				Assert.AreEqual(experienceId, experienceDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var experience1 = new Experience
			{
				Name = $"{prefix}_Experience",
			};
			var experience2 = new Experience
			{
				Name = $"{prefix}_Experience",
			};

			objectCreator.CreateExperience(experience1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateExperience(experience2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceNameExistsError = experienceError as ExperienceNameExistsError;
			Assert.IsNotNull(experienceNameExistsError);
			Assert.AreEqual(experience2.Id, experienceNameExistsError.Id);
			Assert.AreEqual(experience2.Name, experienceNameExistsError.Name);
			Assert.AreEqual(errorMessage, experienceNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var experience1 = new Experience
			{
				Name = $"{prefix}_Experience",
			};
			var experience2 = new Experience
			{
				Name = $"{prefix}_Experience",
			};

			var experienceToCreate = new List<Experience> { experience1, experience2};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateExperience(experienceToCreate);
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
				var experienceError = traceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
				Assert.IsNotNull(experienceError);

				var experienceDuplicateNameError = experienceError as ExperienceDuplicateNameError;
				Assert.IsNotNull(experienceDuplicateNameError);

				var experience = experienceToCreate.Single(c => c.Id == experienceDuplicateNameError.Id);
				Assert.IsNotNull(experience);

				Assert.AreEqual(experience.Name, experienceDuplicateNameError.Name);
				Assert.AreEqual($"Experience '{experience.Name}' has a duplicate name.", experienceDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var experience1 = new Experience
			{
				Name = $"{prefix}_Experience1",
			};
			var experience2 = new Experience
			{
				Name = $"{prefix}_Experience2",
			};

			var createdExperience = objectCreator.CreateExperience([experience1, experience2]);
			var toUpdate = createdExperience.Single(x => x.Id == experience2.Id);
			toUpdate.Name = experience1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Experience.Update(toUpdate);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceNameExistsError = experienceError as ExperienceNameExistsError;
			Assert.IsNotNull(experienceNameExistsError);
			Assert.AreEqual(toUpdate.Id, experienceNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, experienceNameExistsError.Name);
			Assert.AreEqual(errorMessage, experienceNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void UpdateUnmodifiedExperience()
		{
			var experience = new Experience
			{
				Name = $"{Guid.NewGuid()}_Experience",
			};

			experience = objectCreator.CreateExperience(experience);

			var originalExperience = TestContext.Api.Experience.Read(experience.Id);
			var updatedExperience = TestContext.Api.Experience.Update(originalExperience);

			Assert.AreEqual(originalExperience, updatedExperience);
		}

		[TestMethod]
		public void BulkUpdateWithChangedAndUnchangedExperienceReturnsTwoExperience()
		{
			var prefix = Guid.NewGuid();

			var changedExperience = new Experience { Name = $"{prefix}_Changed" };
			var unchangedExperience = new Experience { Name = $"{prefix}_Unchanged" };

			objectCreator.CreateExperience([changedExperience, unchangedExperience]);

			var changedToUpdate = TestContext.Api.Experience.Read(changedExperience.Id);
			var unchangedToUpdate = TestContext.Api.Experience.Read(unchangedExperience.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";

			var updatedExperience = TestContext.Api.Experience.Update(new[] { changedToUpdate, unchangedToUpdate });

			Assert.AreEqual(2, updatedExperience.Count);
			Assert.IsTrue(updatedExperience.Any(x => x.Id == changedExperience.Id));
			Assert.IsTrue(updatedExperience.Any(x => x.Id == unchangedExperience.Id));

			var changedAfterUpdate = TestContext.Api.Experience.Read(changedExperience.Id);
			var unchangedAfterUpdate = TestContext.Api.Experience.Read(unchangedExperience.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(unchangedExperience.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void BulkUpdateWithChangedInvalidAndUnchangedExperienceReturnsTwoSuccessfulIds()
		{
			var prefix = Guid.NewGuid();

			var changedExperience = new Experience { Name = $"{prefix}_Changed" };
			var invalidExperience = new Experience { Name = $"{prefix}_Invalid" };
			var unchangedExperience = new Experience { Name = $"{prefix}_Unchanged" };

			objectCreator.CreateExperience([changedExperience, invalidExperience, unchangedExperience]);

			var changedToUpdate = TestContext.Api.Experience.Read(changedExperience.Id);
			var invalidToUpdate = TestContext.Api.Experience.Read(invalidExperience.Id);
			var unchangedToUpdate = TestContext.Api.Experience.Read(unchangedExperience.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";
			invalidToUpdate.Name = string.Empty;

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.Experience.Update(new[] { changedToUpdate, invalidToUpdate, unchangedToUpdate });
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(changedExperience.Id));
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(unchangedExperience.Id));
			Assert.AreEqual(1, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(invalidExperience.Id));

			var changedAfterUpdate = TestContext.Api.Experience.Read(changedExperience.Id);
			var invalidAfterUpdate = TestContext.Api.Experience.Read(invalidExperience.Id);
			var unchangedAfterUpdate = TestContext.Api.Experience.Read(unchangedExperience.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(invalidExperience.Name, invalidAfterUpdate.Name);
			Assert.AreEqual(unchangedExperience.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var experience = TestContext.Api.Experience.Read(new List<Guid>());
			Assert.IsNotNull(experience);
			Assert.AreEqual(0, experience.Count());
		}

		[TestMethod]
		public void ReadWithEmptyFilterReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Experience>(idsToRetrieve.Select(x => ExperienceExposers.Id.Equal(x)).ToArray());

			var experience = TestContext.Api.Experience.Read(emptyFilter);
			Assert.IsNotNull(experience);
			Assert.AreEqual(0, experience.Count());
		}

		[TestMethod]
		public void CountWithEmptyFilterReturnsZero()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Experience>(idsToRetrieve.Select(x => ExperienceExposers.Id.Equal(x)).ToArray());

			var count = TestContext.Api.Experience.Count(emptyFilter);
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void ReadWithEmptyQueryReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Experience>(idsToRetrieve.Select(x => ExperienceExposers.Id.Equal(x)).ToArray());
			var queryWithEmptyFilter = emptyFilter.ToQuery();

			var experience = TestContext.Api.Experience.Read(queryWithEmptyFilter);
			Assert.IsNotNull(experience);
			Assert.AreEqual(0, experience.Count());
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var experience = new Experience
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateExperience(experience);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceInvalidNameError = experienceError as ExperienceInvalidNameError;
			Assert.IsNotNull(experienceInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", experienceInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var experience = new Experience
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateExperience(experience);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var experienceError = expectedException.TraceData.ErrorData.OfType<ExperienceError>().SingleOrDefault();
			Assert.IsNotNull(experienceError);

			var experienceInvalidNameError = experienceError as ExperienceInvalidNameError;
			Assert.IsNotNull(experienceInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", experienceInvalidNameError.ErrorMessage);
		}
	}
}
