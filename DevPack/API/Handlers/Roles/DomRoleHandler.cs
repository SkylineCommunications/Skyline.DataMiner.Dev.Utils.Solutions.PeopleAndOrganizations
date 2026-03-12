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

	using DomRole = Storage.DOM.SlcPeople_Organizations.RoleInstance;

	internal class DomRoleHandler : DomInstanceApiObjectValidator<DomRole>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomRoleHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Role> apiRoles, out DomInstanceBulkOperationResult<DomRole> result)
		{
			var handler = new DomRoleHandler(api);
			handler.CreateOrUpdate(apiRoles);

			result = new DomInstanceBulkOperationResult<DomRole>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Role> apiRoles, out DomInstanceBulkOperationResult<DomRole> result)
		{
			var handler = new DomRoleHandler(api);
			handler.Delete(apiRoles);

			result = new DomInstanceBulkOperationResult<DomRole>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			ValidateIdsNotInUse(apiRoles.Where(x => x.IsNew).ToArray());
			ValidateNames(apiRoles);

			var validRoles = apiRoles.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validRoles, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided roles are valid", nameof(apiRoles));
			}

			var rolesToCreate = apiRoles.Where(x => x.IsNew).ToList();
			var rolesToUpdate = apiRoles.Except(rolesToCreate).ToList();

			var changeResults = GetRolesWithChanges(rolesToUpdate);

			var toUpdateNameValidation = rolesToUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role.Id)));
			ValidateDomNames(rolesToCreate.Concat(toUpdateNameValidation).ToList());

			var toCreateDomInstances = rolesToCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomRole(x.Instance))
				.ToList();

			CreateOrUpdateDom(toCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomRole> domRoles)
		{
			if (domRoles == null)
			{
				throw new ArgumentNullException(nameof(domRoles));
			}

			if (domRoles.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domRoles.Select(x => x.ToInstance()), out var domResult);

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

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomRole(x)));
		}

		private void Delete(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			var newRoles = apiRoles.Where(x => x.IsNew).ToList();
			newRoles.ForEach(x =>
			{
				var error = new RoleInvalidStateError
				{
					ErrorMessage = $"A role that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			ValidateRolesAreNotInUse(apiRoles.Except(newRoles).ToList());

			var rolesToDelete = apiRoles.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(rolesToDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			var instancesToDelete = apiRoles.Select(x => x.OriginalInstance.ToInstance());
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

			ReportSuccess(instancesToDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomRole(x)));
		}

		private void ValidateIdsNotInUse(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			var rolesRequiringValidation = apiRoles.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (rolesRequiringValidation.Count == 0)
			{
				return;
			}

			var rolesWithDuplicateIds = rolesRequiringValidation
				.GroupBy(role => role.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var role in rolesWithDuplicateIds)
			{
				var error = new RoleDuplicateIdError
				{
					ErrorMessage = $"Role '{role.Name}' has a duplicate ID.",
					Id = role.Id,
				};

				ReportError(role.Id, error);

				rolesRequiringValidation.Remove(role);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(rolesRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new RoleIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateNames(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			var rolesRequiringValidation = apiRoles.ToList();

			foreach (var role in rolesRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new RoleInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = role.Id,
				};

				ReportError(role.Id, error);

				rolesRequiringValidation.Remove(role);
			}

			foreach (var role in rolesRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new RoleInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = role.Id,
					Name = role.Name,
				};

				ReportError(role.Id, error);

				rolesRequiringValidation.Remove(role);
			}

			var rolesWithDuplicateNames = rolesRequiringValidation
				.GroupBy(role => role.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var role in rolesWithDuplicateNames)
			{
				var error = new RoleDuplicateNameError
				{
					ErrorMessage = $"Role '{role.Name}' has a duplicate name.",
					Id = role.Id,
					Name = role.Name,
				};

				ReportError(role.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> Filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Role.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role).Equal(name));

			var domRolesByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetRoles(apiRoles.Select(x => x.Name), Filter)
				.GroupBy(x => x.RoleInformation.Role)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomRole>)x.ToList());

			foreach (var role in apiRoles)
			{
				if (!domRolesByName.TryGetValue(role.Name, out var domRoles))
				{
					continue;
				}

				var existingRoles = domRoles.Where(x => x.ID.Id != role.Id).ToList();
				if (existingRoles.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{role.Name}' is already in use by DOM role(s) with ID(s)", [existingRoles.Select(x => x.ID.Id).ToArray()]);

				var error = new RoleNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = role.Id,
					Name = role.Name,
				};

				ReportError(role.Id, error);
			}
		}

		private void ValidateRolesAreNotInUse(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return;
			}

			// Todo: finish implementation [AB#40242]
		}

		private IEnumerable<DomChangeResults> GetRolesWithChanges(ICollection<Role> apiRoles)
		{
			if (apiRoles == null)
			{
				throw new ArgumentNullException(nameof(apiRoles));
			}

			if (apiRoles.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetRolesWithChangesIterator(apiRoles);
		}

		private IEnumerable<DomChangeResults> GetRolesWithChangesIterator(ICollection<Role> apiRoles)
		{
			var rolesRequiringValidation = apiRoles.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (rolesRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomRolesById = api.DomHelpers.SlcPeopleOrganizationHelper.GetRoles(rolesRequiringValidation.Select(x => x.Id)).ToDictionary(x => x.ID.Id);
			foreach (var role in rolesRequiringValidation)
			{
				if (!storedDomRolesById.TryGetValue(role.Id, out var stored))
				{
					var error = new RoleNotFoundError
					{
						ErrorMessage = $"Role with ID '{role.Id}' no longer exists.",
						Id = role.Id,
					};

					ReportError(role.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(role.OriginalInstance, role.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new RoleValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = role.Id,
						};

						ReportError(role.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
