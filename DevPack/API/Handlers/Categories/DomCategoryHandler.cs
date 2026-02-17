namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using DomCategory = Storage.DOM.SlcPeople_Organizations.CategoryInstance;

	internal class DomCategoryHandler : DomInstanceApiObjectValidator<DomCategory>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomCategoryHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Category> apiCategories, out DomInstanceBulkOperationResult<DomCategory> result)
		{
			var handler = new DomCategoryHandler(api);
			handler.CreateOrUpdate(apiCategories);

			result = new DomInstanceBulkOperationResult<DomCategory>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Category> apiCategories, out DomInstanceBulkOperationResult<DomCategory> result)
		{
			var handler = new DomCategoryHandler(api);
			handler.Delete(apiCategories);

			result = new DomInstanceBulkOperationResult<DomCategory>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			ValidateIdsNotInUse(apiCategories.Where(x => x.IsNew).ToArray());
			ValidateNames(apiCategories);

			var validCategories = apiCategories.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validCategories, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided categories are valid", nameof(apiCategories));
			}

			var categoriesToCreate = apiCategories.Where(x => x.IsNew).ToList();
			var categoriesToUpdate = apiCategories.Except(categoriesToCreate).ToList();

			var changeResults = GetCategoriesWithChanges(categoriesToUpdate);

			var toUpdateNameValidation = categoriesToUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category.Id)));
			ValidateDomNames(categoriesToCreate.Concat(toUpdateNameValidation).ToList());

			var ToCreateDomInstances = categoriesToCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomCategory(x.Instance))
				.ToList();

			CreateOrUpdateDom(ToCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomCategory> domCategories)
		{
			if (domCategories == null)
			{
				throw new ArgumentNullException(nameof(domCategories));
			}

			if (domCategories.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domCategories.Select(x => x.ToInstance()), out var domResult);

			foreach (var id in domResult.UnsuccessfulIds)
			{
				ReportError(id.Id);

				if (domResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					var peopleOrganizationsTraceData = new PeopleAndOrganizationsTraceData();
					peopleOrganizationsTraceData.Add(new PeopleAndOrganizationsErrorData() { ErrorMessage = traceData.ToString() });
				}
			}

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomCategory(x)));
		}

		private void Delete(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			var newCategories = apiCategories.Where(x => x.IsNew).ToList();
			newCategories.ForEach(x =>
			{
				var error = new CategoryInvalidStateError
				{
					ErrorMessage = $"A category that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			ValidateCategoriesAreNotInUse(apiCategories.Except(newCategories).ToList());

			var categoriesToDelete = apiCategories.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(categoriesToDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			var instancesToDelete = apiCategories.Select(x => x.OriginalInstance.ToInstance());
			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryDeleteInBatches(instancesToDelete, out var domResult);

			foreach (var id in domResult.UnsuccessfulIds)
			{
				ReportError(id.Id);

				if (domResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					var peopleOrganizationsTraceData = new PeopleAndOrganizationsTraceData();
					peopleOrganizationsTraceData.Add(new PeopleAndOrganizationsErrorData { ErrorMessage = traceData.ToString() });
				}
			}

			ReportSuccess(instancesToDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomCategory(x)));
		}

		private void ValidateIdsNotInUse(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			var categoriesRequiringValidation = apiCategories.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (categoriesRequiringValidation.Count == 0)
			{
				return;
			}

			var categoriesWithDuplicateIds = categoriesRequiringValidation
				.GroupBy(category => category.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var category in categoriesWithDuplicateIds)
			{
				var error = new CategoryDuplicateIdError
				{
					ErrorMessage = $"Category '{category.Name}' has a duplicate ID.",
					Id = category.Id,
				};

				ReportError(category.Id, error);

				categoriesRequiringValidation.Remove(category);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(categoriesRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new CategoryIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateNames(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			var categoriesRequiringValidation = apiCategories.ToList();

			foreach (var category in categoriesRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new CategoryInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = category.Id,
				};

				ReportError(category.Id, error);

				categoriesRequiringValidation.Remove(category);
			}

			foreach (var category in categoriesRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new CategoryInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = category.Id,
					Name = category.Name,
				};

				ReportError(category.Id, error);

				categoriesRequiringValidation.Remove(category);
			}

			var categoriesWithDuplicateNames = categoriesRequiringValidation
				.GroupBy(category => category.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var category in categoriesWithDuplicateNames)
			{
				var error = new CategoryDuplicateNameError
				{
					ErrorMessage = $"Category '{category.Name}' has a duplicate name.",
					Id = category.Id,
					Name = category.Name,
				};

				ReportError(category.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Category.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category).Equal(name));

			var domCategoriesByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetCategories(apiCategories.Select(x => x.Name), filter)
				.GroupBy(x => x.CategoryInformation.Category)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomCategory>)x.ToList());

			foreach (var category in apiCategories)
			{
				if (!domCategoriesByName.TryGetValue(category.Name, out var domCategories))
				{
					continue;
				}

				var existingCategories = domCategories.Where(x => x.ID.Id != category.Id).ToList();
				if (existingCategories.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{category.Name}' is already in use by DOM category/categories with ID(s)", [existingCategories.Select(x => x.ID.Id).ToArray()]);

				var error = new CategoryNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = category.Id,
					Name = category.Name,
				};

				ReportError(category.Id, error);
			}
		}

		private void ValidateCategoriesAreNotInUse(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return;
			}

			var filter = new ORFilterElement<Organization>(apiCategories
				.Select(x => OrganizationExposers.CategoryId.Equal(x.Id))
				.ToArray());

			var organizationsImplementingCategories = api.Organizations.Read(filter);

			var OrganizationsByCategoryId = organizationsImplementingCategories
				.GroupBy(x => x.CategoryId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var category in apiCategories)
			{
				if (!OrganizationsByCategoryId.TryGetValue(category.Id, out var organizations))
				{
					continue;
				}

				var error = new CategoryInUseError
				{
					ErrorMessage = $"Category '{category.Name}' is in use by {organizations.Count} organization(s).",
					Id = category.Id,
					OrganizationIds = organizations.Select(x => x.Id).ToList(),
				};

				ReportError(category.Id, error);
			}
		}

		private IEnumerable<DomChangeResults> GetCategoriesWithChanges(ICollection<Category> apiCategories)
		{
			if (apiCategories == null)
			{
				throw new ArgumentNullException(nameof(apiCategories));
			}

			if (apiCategories.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetCategoriesWithChangesIterator(apiCategories);
		}

		private IEnumerable<DomChangeResults> GetCategoriesWithChangesIterator(ICollection<Category> apiCategories)
		{
			var categoriesRequiringValidation = apiCategories.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (categoriesRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomCategoriesById = api.DomHelpers.SlcPeopleOrganizationHelper.GetCategories(categoriesRequiringValidation.Select(x => x.Id)).ToDictionary(x => x.ID.Id);
			foreach (var category in categoriesRequiringValidation)
			{
				if (!storedDomCategoriesById.TryGetValue(category.Id, out var stored))
				{
					var error = new CategoryNotFoundError
					{
						ErrorMessage = $"Category with ID '{category.Id}' no longer exists.",
						Id = category.Id,
					};

					ReportError(category.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(category.OriginalInstance, category.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new CategoryValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = category.Id,
						};

						ReportError(category.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
