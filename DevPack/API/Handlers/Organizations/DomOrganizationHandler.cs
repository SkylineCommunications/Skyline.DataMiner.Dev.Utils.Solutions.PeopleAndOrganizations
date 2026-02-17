namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using static Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections;

	using domOrganization = Storage.DOM.SlcPeople_Organizations.OrganizationsInstance;

	internal class DomOrganizationHandler : DomInstanceApiObjectValidator<domOrganization>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomOrganizationHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Organization> apiOrganizations, out DomInstanceBulkOperationResult<domOrganization> result)
		{
			var handler = new DomOrganizationHandler(api);
			handler.CreateOrUpdate(apiOrganizations);

			result = new DomInstanceBulkOperationResult<domOrganization>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

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

			ValidateIdsNotInUse(apiOrganizations.Where(x => x.IsNew).ToArray());
			ValidateNames(apiOrganizations);

			var validOrganizations = apiOrganizations.Where(IsValid).ToList();
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

			var changeResults = GetOrganizationsWithChanges(organizationsToUpdate);

			var toUpdateNameValidation = organizationsToUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName.Id)));
			ValidateDomNames(organizationsToCreate.Concat(toUpdateNameValidation).ToList());

			var ToCreateDomInstances = organizationsToCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new domOrganization(x.Instance))
				.ToList();

			CreateOrUpdateDom(ToCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<domOrganization> domOrganizations)
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
				}
			}

			ReportSuccess(domResult.SuccessfulItems.Select(x => new domOrganization(x)));
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

			FilterElement<DomInstance> filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName).Equal(name));

			var domOrganizationsByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizations(apiOrganizations.Select(x => x.Name), filter)
				.GroupBy(x => x.OrganizationInformation.OrganizationName)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<domOrganization>)x.ToList());

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
