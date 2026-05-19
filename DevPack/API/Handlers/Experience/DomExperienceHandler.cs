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

	using DomExperience = Storage.DOM.SlcPeople_Organizations.ExperienceInstance;

	internal class DomExperienceHandler : DomInstanceApiObjectValidator<DomExperience>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomExperienceHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Experience> apiExperience, out DomInstanceBulkOperationResult<DomExperience> result)
		{
			var handler = new DomExperienceHandler(api);
			handler.CreateOrUpdate(apiExperience);

			result = new DomInstanceBulkOperationResult<DomExperience>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Experience> apiExperience, out DomInstanceBulkOperationResult<DomExperience> result)
		{
			var handler = new DomExperienceHandler(api);
			handler.Delete(apiExperience);

			result = new DomInstanceBulkOperationResult<DomExperience>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			ValidateIdsNotInUse(apiExperience.Where(x => x.IsNew).ToArray());
			ValidateNames(apiExperience);

			var validExperience = apiExperience.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validExperience, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided experience are valid", nameof(apiExperience));
			}

			var experienceToCreate = apiExperience.Where(x => x.IsNew).ToList();
			var experienceToUpdate = apiExperience.Except(experienceToCreate).ToList();

			var changeResults = GetExperienceWithChanges(experienceToUpdate);

			var toUpdateNameValidation = experienceToUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience.Id)));
			ValidateDomNames(experienceToCreate.Concat(toUpdateNameValidation).ToList());

			var toCreateDomInstances = experienceToCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomExperience(x.Instance))
				.ToList();

			CreateOrUpdateDom(toCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomExperience> domExperience)
		{
			if (domExperience == null)
			{
				throw new ArgumentNullException(nameof(domExperience));
			}

			if (domExperience.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domExperience.Select(x => x.ToInstance()), out var domResult);

			foreach (var id in domResult.UnsuccessfulIds)
			{
				ReportError(id.Id);

				if (domResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					var peopleOrganizationsTraceData = new PeopleAndOrganizationsTraceData();
					peopleOrganizationsTraceData.Add(new PeopleAndOrganizationsErrorData() { ErrorMessage = traceData.ToString() });

					PassTraceData(id.Id, peopleOrganizationsTraceData);
				}
			}

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomExperience(x)));
		}

		private void Delete(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			var newExperience = apiExperience.Where(x => x.IsNew).ToList();
			newExperience.ForEach(x =>
			{
				var error = new ExperienceInvalidStateError
				{
					ErrorMessage = $"An experience that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			ValidateExperienceAreNotInUse(apiExperience.Except(newExperience).ToList());

			var experienceToDelete = apiExperience.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(experienceToDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			var instancesToDelete = apiExperience.Select(x => x.OriginalInstance.ToInstance());
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

			ReportSuccess(instancesToDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomExperience(x)));
		}

		private void ValidateIdsNotInUse(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			var experienceRequiringValidation = apiExperience.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (experienceRequiringValidation.Count == 0)
			{
				return;
			}

			var experienceWithDuplicateIds = experienceRequiringValidation
				.GroupBy(experience => experience.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var experience in experienceWithDuplicateIds)
			{
				var error = new ExperienceDuplicateIdError
				{
					ErrorMessage = $"Experience '{experience.Name}' has a duplicate ID.",
					Id = experience.Id,
				};

				ReportError(experience.Id, error);

				experienceRequiringValidation.Remove(experience);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(experienceRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new ExperienceIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateNames(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			var experienceRequiringValidation = apiExperience.ToList();

			foreach (var experience in experienceRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new ExperienceInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = experience.Id,
				};

				ReportError(experience.Id, error);

				experienceRequiringValidation.Remove(experience);
			}

			foreach (var experience in experienceRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new ExperienceInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = experience.Id,
					Name = experience.Name,
				};

				ReportError(experience.Id, error);

				experienceRequiringValidation.Remove(experience);
			}

			var experienceWithDuplicateNames = experienceRequiringValidation
				.GroupBy(experience => experience.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var experience in experienceWithDuplicateNames)
			{
				var error = new ExperienceDuplicateNameError
				{
					ErrorMessage = $"Experience '{experience.Name}' has a duplicate name.",
					Id = experience.Id,
					Name = experience.Name,
				};

				ReportError(experience.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> Filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Experience.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience).Equal(name));

			var domExperienceByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetExperience(apiExperience.Select(x => x.Name), Filter)
				.GroupBy(x => x.ExperienceInformation.Experience)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomExperience>)x.ToList());

			foreach (var experience in apiExperience)
			{
				if (!domExperienceByName.TryGetValue(experience.Name, out var domExperience))
				{
					continue;
				}

				var existingExperience = domExperience.Where(x => x.ID.Id != experience.Id).ToList();
				if (existingExperience.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{experience.Name}' is already in use by DOM experience with ID(s)", [existingExperience.Select(x => x.ID.Id).ToArray()]);

				var error = new ExperienceNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = experience.Id,
					Name = experience.Name,
				};

				ReportError(experience.Id, error);
			}
		}

		private void ValidateExperienceAreNotInUse(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return;
			}

			var filter = new ORFilterElement<Person>(apiExperience
				.Select(x => PersonExposers.ExperienceId.Equal(x.Id))
				.ToArray());

			var peopleImplementingExperience = api.People.Read(filter);

			var peopleByExperienceId = peopleImplementingExperience
				.GroupBy(x => x.ExperienceId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var experience in apiExperience)
			{
				if (!peopleByExperienceId.TryGetValue(experience.Id, out var people))
				{
					continue;
				}

				var error = new ExperienceInUseByPeopleError
				{
					ErrorMessage = $"Experience '{experience.Name}' is in use by {people.Count} people.",
					Id = experience.Id,
					PeopleIds = people.Select(x => x.Id).ToList(),
				};

				ReportError(experience.Id, error);
			}
		}

		private IEnumerable<DomChangeResults> GetExperienceWithChanges(ICollection<Experience> apiExperience)
		{
			if (apiExperience == null)
			{
				throw new ArgumentNullException(nameof(apiExperience));
			}

			if (apiExperience.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetExperienceWithChangesIterator(apiExperience);
		}

		private IEnumerable<DomChangeResults> GetExperienceWithChangesIterator(ICollection<Experience> apiExperience)
		{
			var unchangedExperience = apiExperience
				.Where(x => !x.IsNew && !x.HasChanges)
				.Select(x => x.OriginalInstance)
				.ToList();
			ReportSuccess(unchangedExperience);

			var experienceRequiringValidation = apiExperience.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (experienceRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomExperienceById = api.DomHelpers.SlcPeopleOrganizationHelper.GetExperience(experienceRequiringValidation.Select(x => x.Id)).ToDictionary(x => x.ID.Id);
			foreach (var experience in experienceRequiringValidation)
			{
				if (!storedDomExperienceById.TryGetValue(experience.Id, out var stored))
				{
					var error = new ExperienceNotFoundError
					{
						ErrorMessage = $"Experience with ID '{experience.Id}' no longer exists.",
						Id = experience.Id,
					};

					ReportError(experience.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(experience.OriginalInstance, experience.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new ExperienceValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = experience.Id,
						};

						ReportError(experience.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
