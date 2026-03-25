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

	using DomOrganization = Storage.DOM.SlcPeople_Organizations.OrganizationsInstance;

	internal class DomOrganizationHandler : DomInstanceApiObjectValidator<DomOrganization>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomOrganizationHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Organization> apiOrganizations, out DomInstanceBulkOperationResult<DomOrganization> result)
		{
			var handler = new DomOrganizationHandler(api);
			handler.CreateOrUpdate(apiOrganizations);

			result = new DomInstanceBulkOperationResult<DomOrganization>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryActivate(PeopleAndOrganizationsApi api, ICollection<Organization> apiOrganizations, out DomInstanceBulkOperationResult<DomOrganization> result)
		{
			var handler = new DomOrganizationHandler(api);
			handler.TransitionToActiveFromDraft(apiOrganizations);

			result = new DomInstanceBulkOperationResult<DomOrganization>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<Organization> apiOrganizations, out DomInstanceBulkOperationResult<DomOrganization> result)
		{
			var handler = new DomOrganizationHandler(api);
			handler.TransitionToDeprecated(apiOrganizations);

			result = new DomInstanceBulkOperationResult<DomOrganization>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Organization> apiOrganizations, out DomInstanceBulkOperationResult<DomOrganization> result)
		{
			var handler = new DomOrganizationHandler(api);
			handler.Delete(apiOrganizations);

			result = new DomInstanceBulkOperationResult<DomOrganization>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			ValidateStateForUpdateAction(apiOrganizations.Where(x => !x.IsNew).ToArray());
			var toValidate = apiOrganizations.Where(IsValid).ToList();

			ValidateIdsNotInUse(toValidate.Where(x => x.IsNew).ToArray());
			ValidateNames(toValidate);
			ValidateCategories(toValidate);

			var validOrganizations = toValidate.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validOrganizations, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided organizations are valid", nameof(apiOrganizations));
			}

			var organizationsToCreate = apiOrganizations.Where(x => x.IsNew).ToList();
			var organizationsToUpdate = apiOrganizations.Except(organizationsToCreate).ToList();

			var changeResults = GetOrganizationsWithChanges(organizationsToUpdate).ToList();

			var toUpdateNameValidation = organizationsToUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName.Id)));
			ValidateDomNames(organizationsToCreate.Concat(toUpdateNameValidation).ToList());

			var toCreateDomInstances = organizationsToCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomOrganization(x.Instance))
				.ToList();

			CreateOrUpdateDom(toCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomOrganization> domOrganizations)
		{
			if (domOrganizations == null)
			{
				throw new ArgumentNullException(nameof(domOrganizations));
			}

			if (domOrganizations.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domOrganizations.Select(x => x.ToInstance()), out var domResult);

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

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomOrganization(x)));
		}

		private void TransitionToActiveFromDraft(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			ValidateStateForActiveFromDraftAction(apiOrganizations);

			var toTransition = apiOrganizations.Where(IsValid).ToList();
			foreach (var organization in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(organization.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Transitions.Draft_To_Active);
					ReportSuccess(new DomOrganization(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(organization.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void TransitionToDeprecated(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			ValidateStateForDeprecateAction(apiOrganizations);
			ValidateOrganizationsAreNotInUse(apiOrganizations.Where(IsValid).ToArray());

			var toTransition = apiOrganizations.Where(IsValid).ToList();
			foreach (var organization in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(organization.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Transitions.Active_To_Deprecated);
					ReportSuccess(new DomOrganization(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(organization.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void Delete(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			var newOrganizations = apiOrganizations.Where(x => x.IsNew).ToList();
			newOrganizations.ForEach(x =>
			{
				var error = new OrganizationInvalidStateError
				{
					ErrorMessage = $"An organization that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			apiOrganizations = apiOrganizations.Except(newOrganizations).ToList();

			ValidateStateForDeleteAction(apiOrganizations);

			var toDelete = apiOrganizations.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(toDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided organizations are valid", nameof(apiOrganizations));
			}

			var toDelete = apiOrganizations.Select(x => x.OriginalInstance.ToInstance()).ToList();
			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryDeleteInBatches(toDelete, out var domResult);

			foreach (var id in domResult.UnsuccessfulIds)
			{
				ReportError(id.Id);

				if (domResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					var peopleOrganizationsTraceData = new PeopleAndOrganizationsTraceData();
					peopleOrganizationsTraceData.Add(new PeopleAndOrganizationsErrorData() { ErrorMessage = traceData.ToString() });
				}
			}

			ReportSuccess(toDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomOrganization(x)));
		}

		private void ValidateIdsNotInUse(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			var organizationsRequiringValidation = apiOrganizations.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (organizationsRequiringValidation.Count == 0)
			{
				return;
			}

			var organizationsWithDuplicateIds = organizationsRequiringValidation
				.GroupBy(role => role.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var organization in organizationsWithDuplicateIds)
			{
				var error = new OrganizationDuplicateIdError
				{
					ErrorMessage = $"Organization '{organization.Name}' has a duplicate ID.",
					Id = organization.Id,
				};

				ReportError(organization.Id, error);

				organizationsRequiringValidation.Remove(organization);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(organizationsRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new OrganizationIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateStateForUpdateAction(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			foreach (var organization in apiOrganizations.Where(x => !new[] { OrganizationState.Draft, OrganizationState.Active }.Contains(x.State)))
			{
				var error = new OrganizationInvalidStateError
				{
					ErrorMessage = "Not allowed to update an organization that is not in Draft or Active state.",
					Id = organization.Id,
				};
				ReportError(organization.Id, error);
			}
		}

		private void ValidateStateForActiveFromDraftAction(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			foreach (var organization in apiOrganizations.Where(x => x.State != OrganizationState.Draft))
			{
				var error = new OrganizationInvalidStateError
				{
					ErrorMessage = "Not allowed to activate an organization that is not in Draft state.",
					Id = organization.Id,
				};
				ReportError(organization.Id, error);
			}
		}

		private void ValidateStateForDeprecateAction(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			foreach (var organization in apiOrganizations.Where(x => x.State != OrganizationState.Active))
			{
				var error = new OrganizationInvalidStateError
				{
					ErrorMessage = "Not allowed to deprecate an organization that is not in Active state.",
					Id = organization.Id,
				};
				ReportError(organization.Id, error);
			}
		}

		private void ValidateStateForDeleteAction(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			foreach (var organization in apiOrganizations.Where(x => !new[] { OrganizationState.Draft, OrganizationState.Deprecated }.Contains(x.State)))
			{
				var error = new OrganizationInvalidStateError
				{
					ErrorMessage = "Not allowed to delete an organization that is not in Draft or Deprecated state.",
					Id = organization.Id,
				};
				ReportError(organization.Id, error);
			}
		}

		private void ValidateNames(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			var organizationsRequiringValidation = apiOrganizations.ToList();

			foreach (var organization in organizationsRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new OrganizationInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = organization.Id,
				};

				ReportError(organization.Id, error);

				organizationsRequiringValidation.Remove(organization);
			}

			foreach (var organization in organizationsRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new OrganizationInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = organization.Id,
					Name = organization.Name,
				};

				ReportError(organization.Id, error);

				organizationsRequiringValidation.Remove(organization);
			}

			var organizationsWithDuplicateNames = organizationsRequiringValidation
				.GroupBy(role => role.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var organization in organizationsWithDuplicateNames)
			{
				var error = new OrganizationDuplicateNameError
				{
					ErrorMessage = $"Organization '{organization.Name}' has a duplicate name.",
					Id = organization.Id,
					Name = organization.Name,
				};

				ReportError(organization.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> Filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName).Equal(name));

			var domOrganizationsByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizations(apiOrganizations.Select(x => x.Name), Filter)
				.GroupBy(x => x.OrganizationInformation.OrganizationName)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomOrganization>)x.ToList());

			foreach (var organization in apiOrganizations)
			{
				if (!domOrganizationsByName.TryGetValue(organization.Name, out var domOrganizations))
				{
					continue;
				}

				var existingOrganizations = domOrganizations.Where(x => x.ID.Id != organization.Id).ToList();
				if (existingOrganizations.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{organization.Name}' is already in use by DOM organization(s) with ID(s)", [existingOrganizations.Select(x => x.ID.Id).ToArray()]);

				var error = new OrganizationNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = organization.Id,
					Name = organization.Name,
				};

				ReportError(organization.Id, error);
			}
		}

		private void ValidateOrganizationsAreNotInUse(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			var filter = new ORFilterElement<Person>(apiOrganizations
				.Select(x => PersonExposers.OrganizationId.Equal(x.Id))
				.ToArray());

			var peopleImplementingOrganizations = api.People.Read(filter);

			var peopleByOrganizationId = peopleImplementingOrganizations
				.GroupBy(x => x.OrganizationId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var organization in apiOrganizations)
			{
				if (!peopleByOrganizationId.TryGetValue(organization.Id, out var people))
				{
					continue;
				}

				var error = new OrganizationInUseByPeopleError
				{
					ErrorMessage = $"Organization '{organization.Name}' is in use by {people.Count} person/people.",
					Id = organization.Id,
					PeopleIds = people.Select(x => x.Id).ToList(),
				};

				ReportError(organization.Id, error);
			}
		}

		private void ValidateCategories(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return;
			}

			var categoryIds = apiOrganizations
				.Where(x => x.CategoryId != Guid.Empty)
				.Select(x => x.CategoryId)
				.Distinct()
				.ToList();
			var categoriesById = api.Categories.Read(categoryIds).ToDictionary(x => x.Id);

			foreach (var organization in apiOrganizations)
			{
				if (organization.CategoryId == Guid.Empty)
				{
					continue;
				}

				if (!categoriesById.TryGetValue(organization.CategoryId, out _))
				{
					var error = new OrganizationCategoryNotFoundError
					{
						ErrorMessage = $"Category with ID '{organization.CategoryId}' not found.",
						CategoryId = organization.CategoryId,
						Id = organization.Id,
					};

					ReportError(organization.Id, error);
				}
			}
		}

		private IEnumerable<DomChangeResults> GetOrganizationsWithChanges(ICollection<Organization> apiOrganizations)
		{
			if (apiOrganizations == null)
			{
				throw new ArgumentNullException(nameof(apiOrganizations));
			}

			if (apiOrganizations.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetOrganizationsWithChangesIterator(apiOrganizations);
		}

		private IEnumerable<DomChangeResults> GetOrganizationsWithChangesIterator(ICollection<Organization> apiOrganizations)
		{
			var organizationsRequiringValidation = apiOrganizations.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (organizationsRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomOrganizationsById = api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizations(organizationsRequiringValidation.Select(x => x.Id))
				.ToDictionary(x => x.ID.Id);
			foreach (var organization in organizationsRequiringValidation)
			{
				if (!storedDomOrganizationsById.TryGetValue(organization.Id, out var stored))
				{
					var error = new OrganizationNotFoundError
					{
						ErrorMessage = $"Organization with ID '{organization.Id}' no longer exists.",
						Id = organization.Id,
					};

					ReportError(organization.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(organization.OriginalInstance, organization.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new OrganizationValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = organization.Id,
						};

						ReportError(organization.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
