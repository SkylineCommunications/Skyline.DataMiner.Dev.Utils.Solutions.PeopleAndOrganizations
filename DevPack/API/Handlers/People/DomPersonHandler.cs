namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using static Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections;

	using DomPerson = Storage.DOM.SlcPeople_Organizations.PeopleInstance;

	internal class DomPersonHandler : DomInstanceApiObjectValidator<DomPerson>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomPersonHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out DomInstanceBulkOperationResult<DomPerson> result)
		{
			var handler = new DomPersonHandler(api);
			handler.CreateOrUpdate(apiPeople);

			result = new DomInstanceBulkOperationResult<DomPerson>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryActivate(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out DomInstanceBulkOperationResult<DomPerson> result)
		{
			var handler = new DomPersonHandler(api);
			handler.TransitionToActiveFromDraft(apiPeople);

			result = new DomInstanceBulkOperationResult<DomPerson>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out DomInstanceBulkOperationResult<DomPerson> result)
		{
			var handler = new DomPersonHandler(api);
			handler.TransitionToDeprecated(apiPeople);

			result = new DomInstanceBulkOperationResult<DomPerson>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out DomInstanceBulkOperationResult<DomPerson> result)
		{
			var handler = new DomPersonHandler(api);
			handler.Delete(apiPeople);

			result = new DomInstanceBulkOperationResult<DomPerson>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			ValidateIdsNotInUse(apiPeople.Where(x => x.IsNew).ToArray());
			ValidateStateForUpdateAction(apiPeople.Where(x => !x.IsNew).ToArray());
			ValidateNames(apiPeople);
			ValidateExperience(apiPeople);
			ValidateOrganizations(apiPeople);
			ValidateSkills(apiPeople);
			ValidateTeamMemberships(apiPeople);

			var validPeople = apiPeople.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validPeople, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided people are valid", nameof(apiPeople));
			}

			var toCreate = apiPeople.Where(x => x.IsNew).ToList();
			var toUpdate = apiPeople.Except(toCreate).ToList();

			var changeResults = GetPeopleWithChanges(toUpdate).ToList();

			var toUpdateNameValidation = toUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName.Id)));
			ValidateDomNames(toCreate.Concat(toUpdateNameValidation).ToList());

			var toCreateDomInstances = toCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomPerson(x.Instance))
				.ToList();

			CreateOrUpdateDom(toCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomPerson> domPeople)
		{
			if (domPeople == null)
			{
				throw new ArgumentNullException(nameof(domPeople));
			}

			if (domPeople.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domPeople.Select(x => x.ToInstance()), out var domResult);

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

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomPerson(x)));
		}

		private void TransitionToActiveFromDraft(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			ValidateStateForActiveFromDraftAction(apiPeople);

			var toTransition = apiPeople.Where(IsValid).ToList();
			foreach (var person in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(person.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Transitions.Draft_To_Active);
					ReportSuccess(new DomPerson(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(person.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void TransitionToDeprecated(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			ValidateStateForDeprecateAction(apiPeople);

			// Todo: find people with resource link and try to deprecate those first.
			var toTransition = apiPeople.Where(IsValid).ToList();
			foreach (var person in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(person.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Transitions.Active_To_Deprecated);
					ReportSuccess(new DomPerson(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(person.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void Delete(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var newPeople = apiPeople.Where(x => x.IsNew).ToList();
			newPeople.ForEach(x =>
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = $"A person that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			apiPeople = apiPeople.Except(newPeople).ToList();

			ValidateStateForDeleteAction(apiPeople);

			var toDelete = apiPeople.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(toDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided people are valid", nameof(apiPeople));
			}

			var toDelete = apiPeople.Select(x => x.OriginalInstance.ToInstance()).ToList();
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

			ReportSuccess(toDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomPerson(x)));
		}

		private void ValidateStateForUpdateAction(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => !new[] { PersonState.Draft, PersonState.Active }.Contains(x.State)))
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = "Not allowed to update a person that is not in Draft or Active state.",
					Id = person.Id,
				};
				ReportError(person.Id, error);
			}
		}

		private void ValidateStateForActiveFromDraftAction(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => x.State != PersonState.Draft))
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = "Not allowed to activate a person that is not in Draft state.",
					Id = person.Id,
				};
				ReportError(person.Id, error);
			}
		}

		private void ValidateStateForDeprecateAction(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => x.State != PersonState.Active))
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = "Not allowed to deprecate a person that is not in Active state.",
					Id = person.Id,
				};
				ReportError(person.Id, error);
			}
		}

		private void ValidateStateForDeleteAction(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => !new[] { PersonState.Draft, PersonState.Deprecated }.Contains(x.State)))
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = "Not allowed to delete a person that is not in Draft or Deprecated state.",
					Id = person.Id,
				};

				ReportError(person.Id, error);
			}
		}

		private void ValidateIdsNotInUse(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var peopleRequiringValidation = apiPeople.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (peopleRequiringValidation.Count == 0)
			{
				return;
			}

			var peopleWithDuplicateIds = peopleRequiringValidation
				.GroupBy(role => role.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var person in peopleWithDuplicateIds)
			{
				var error = new PersonDuplicateIdError
				{
					ErrorMessage = $"Person '{person.Name}' has a duplicate ID.",
					Id = person.Id,
				};

				ReportError(person.Id, error);

				peopleRequiringValidation.Remove(person);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(peopleRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new PersonIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateNames(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var peopleRequiringValidation = apiPeople.ToList();

			foreach (var person in peopleRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new PersonInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = person.Id,
				};

				ReportError(person.Id, error);

				peopleRequiringValidation.Remove(person);
			}

			foreach (var person in peopleRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new PersonInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = person.Id,
					Name = person.Name,
				};

				ReportError(person.Id, error);

				peopleRequiringValidation.Remove(person);
			}

			var peopleWithDuplicateNames = peopleRequiringValidation
				.GroupBy(role => role.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var person in peopleWithDuplicateNames)
			{
				var error = new PersonDuplicateNameError
				{
					ErrorMessage = $"Person '{person.Name}' has a duplicate name.",
					Id = person.Id,
					Name = person.Name,
				};

				ReportError(person.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> Filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.People.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName).Equal(name));

			var domPeopleByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetPeople(apiPeople.Select(x => x.Name), Filter)
				.GroupBy(x => x.PeopleInformation.FullName)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomPerson>)x.ToList());

			foreach (var person in apiPeople)
			{
				if (!domPeopleByName.TryGetValue(person.Name, out var domTeams))
				{
					continue;
				}

				var existingPeople = domTeams.Where(x => x.ID.Id != person.Id).ToList();
				if (existingPeople.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{person.Name}' is already in use by DOM person/people with ID(s)", [existingPeople.Select(x => x.ID.Id).ToArray()]);

				var error = new PersonNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = person.Id,
					Name = person.Name,
				};

				ReportError(person.Id, error);
			}
		}

		private void ValidateOrganizations(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var organzationIds = apiPeople
				.Where(x => x.OrganizationId != Guid.Empty)
				.Select(x => x.OrganizationId)
				.Distinct()
				.ToList();
			var organizationsById = api.Organizations.Read(organzationIds).ToDictionary(x => x.Id);

			foreach (var person in apiPeople)
			{
				if (person.OrganizationId == Guid.Empty)
				{
					continue;
				}

				if (!organizationsById.TryGetValue(person.OrganizationId, out _))
				{
					var error = new PersonOrganizationNotFoundError
					{
						ErrorMessage = $"Organization with ID '{person.OrganizationId}' not found.",
						OrganizationId = person.OrganizationId,
						Id = person.Id,
					};

					ReportError(person.Id, error);
				}
			}
		}

		private void ValidateExperience(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var experienceIds = apiPeople
				.Where(x => x.ExperienceId != Guid.Empty)
				.Select(x => x.ExperienceId)
				.Distinct()
				.ToList();
			var experienceById = api.Experience.Read(experienceIds).ToDictionary(x => x.Id);

			foreach (var person in apiPeople)
			{
				if (person.ExperienceId == Guid.Empty)
				{
					continue;
				}

				if (!experienceById.TryGetValue(person.ExperienceId, out _))
				{
					var error = new PersonExperienceNotFoundError
					{
						ErrorMessage = $"Experience with ID '{person.ExperienceId}' not found.",
						ExperienceId = person.ExperienceId,
						Id = person.Id,
					};

					ReportError(person.Id, error);
				}
			}
		}

		private void ValidateSkills(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var skillValues = api.Skills.Read().Select(x => x.Name).ToList();

			foreach (var person in apiPeople)
			{
				foreach (var skill in person.Skills)
				{
					if (!skillValues.Contains(skill.Name))
					{
						var error = new PersonInvalidAssignedSkillError
						{
							ErrorMessage = $"Skill '{skill.Name}' does not exist.",
							Id = person.Id,
							Name = skill.Name,
						};
						ReportError(person.Id, error);
					}
				}
			}
		}

		private void ValidateTeamMemberships(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			var teamIds = apiPeople
				.SelectMany(x => x.TeamMemberships)
				.Select(x => x.TeamId)
				.Distinct()
				.ToList();
			var teamsById = api.Teams.Read(teamIds).ToDictionary(x => x.Id);

			var roleIds = apiPeople
				.SelectMany(x => x.TeamMemberships)
				.Select(x => x.RoleId)
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToList();
			var rolesById = api.Roles.Read(roleIds).ToDictionary(x => x.Id);

			foreach (var person in apiPeople)
			{
				var duplicateSettings = person.TeamMemberships
					.GroupBy(x => x.TeamId)
					.Where(g => g.Count() > 1)
					.ToDictionary(x => x.Key, x => x.Count());

				foreach (var kvp in duplicateSettings)
				{
					var error = new PersonInvalidTeamMembershipError
					{
						Id = person.Id,
						TeamId = kvp.Key,
						ErrorMessage = $"Team with ID '{kvp.Key}' is defined {kvp.Value} times.",
					};

					ReportError(person.Id, error);
				}

				if (duplicateSettings.Count > 0)
				{
					continue;
				}

				foreach (var teamMembership in person.TeamMemberships)
				{
					if (teamMembership.TeamId == Guid.Empty)
					{
						var error = new PersonInvalidTeamMembershipError
						{
							Id = person.Id,
							TeamId = teamMembership.TeamId,
							ErrorMessage = "Team ID cannot be empty.",
						};

						ReportError(person.Id, error);
						continue;
					}

					if (!teamsById.TryGetValue(teamMembership.TeamId, out _))
					{
						var error = new PersonInvalidTeamMembershipError
						{
							Id = person.Id,
							TeamId = teamMembership.TeamId,
							ErrorMessage = $"Team with ID '{teamMembership.TeamId}' not found.",
						};

						ReportError(person.Id, error);
						continue;
					}

					if (teamMembership.RoleId == Guid.Empty)
					{
						continue;
					}

					if (!rolesById.TryGetValue(teamMembership.RoleId, out _))
					{
						var error = new PersonInvalidTeamMembershipError
						{
							Id = person.Id,
							TeamId = teamMembership.TeamId,
							RoleId = teamMembership.RoleId,
							ErrorMessage = $"Role with ID '{teamMembership.RoleId}' not found.",
						};

						ReportError(person.Id, error);
					}
				}
			}
		}

		private IEnumerable<DomChangeResults> GetPeopleWithChanges(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetPeopleWithChangesIterator(apiPeople);
		}

		private IEnumerable<DomChangeResults> GetPeopleWithChangesIterator(ICollection<Person> apiPeople)
		{
			var peopleRequiringValidation = apiPeople.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (peopleRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomPeopleById = api.DomHelpers.SlcPeopleOrganizationHelper.GetPeople(peopleRequiringValidation.Select(x => x.Id))
				.ToDictionary(x => x.ID.Id);
			foreach (var person in peopleRequiringValidation)
			{
				if (!storedDomPeopleById.TryGetValue(person.Id, out var stored))
				{
					var error = new PersonNotFoundError
					{
						ErrorMessage = $"Person with ID '{person.Id}' no longer exists.",
						Id = person.Id,
					};

					ReportError(person.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(person.OriginalInstance, person.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new PersonValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = person.Id,
						};

						ReportError(person.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
