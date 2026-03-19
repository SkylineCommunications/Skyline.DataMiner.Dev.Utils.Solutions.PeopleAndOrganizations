namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
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

	using DomTeam = Storage.DOM.SlcPeople_Organizations.TeamsInstance;

	internal class DomTeamHandler : DomInstanceApiObjectValidator<DomTeam>
	{
		private readonly PeopleAndOrganizationsApi api;

		private DomTeamHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out DomInstanceBulkOperationResult<DomTeam> result)
		{
			var handler = new DomTeamHandler(api);
			handler.CreateOrUpdate(apiTeams);

			result = new DomInstanceBulkOperationResult<DomTeam>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryActivate(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out DomInstanceBulkOperationResult<DomTeam> result)
		{
			var handler = new DomTeamHandler(api);
			handler.TransitionToActiveFromDraft(apiTeams);

			result = new DomInstanceBulkOperationResult<DomTeam>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out DomInstanceBulkOperationResult<DomTeam> result)
		{
			var handler = new DomTeamHandler(api);
			handler.TransitionToDeprecated(apiTeams);

			result = new DomInstanceBulkOperationResult<DomTeam>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out DomInstanceBulkOperationResult<DomTeam> result)
		{
			var handler = new DomTeamHandler(api);
			handler.Delete(apiTeams);

			result = new DomInstanceBulkOperationResult<DomTeam>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		private void CreateOrUpdate(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			ValidateIdsNotInUse(apiTeams.Where(x => x.IsNew).ToArray());
			ValidateStateForUpdateAction(apiTeams.Where(x => !x.IsNew).ToArray());
			ValidateNames(apiTeams);
			ValidateSkills(apiTeams);

			var validTeams = apiTeams.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validTeams, CreateOrUpdateLocked);
			ReportError(lockResult);
		}

		private void CreateOrUpdateLocked(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided teams are valid", nameof(apiTeams));
			}

			var toCreate = apiTeams.Where(x => x.IsNew).ToList();
			var toUpdate = apiTeams.Except(toCreate).ToList();

			var changeResults = GetTeamsWithChanges(toUpdate).ToList();

			var toUpdateNameValidation = toUpdate.Where(x => changeResults.Any(y => y.Instance.ID.Id == x.Id && y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName.Id)));
			ValidateDomNames(toCreate.Concat(toUpdateNameValidation).ToList());

			var toCreateDomInstances = toCreate
				.Where(IsValid)
				.Select(x => x.GetInstanceWithChanges())
				.ToList();

			var toUpdateDomInstances = changeResults
				.Where(IsValid)
				.Select(x => new DomTeam(x.Instance))
				.ToList();

			CreateOrUpdateDom(toCreateDomInstances.Concat(toUpdateDomInstances).ToList());
		}

		private void CreateOrUpdateDom(ICollection<DomTeam> domTeams)
		{
			if (domTeams == null)
			{
				throw new ArgumentNullException(nameof(domTeams));
			}

			if (domTeams.Count == 0)
			{
				return;
			}

			api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.TryCreateOrUpdateInBatches(domTeams.Select(x => x.ToInstance()), out var domResult);

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

			ReportSuccess(domResult.SuccessfulItems.Select(x => new DomTeam(x)));
		}

		private void TransitionToActiveFromDraft(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			ValidateStateForActiveFromDraftAction(apiTeams);

			var toTransition = apiTeams.Where(IsValid).ToList();
			foreach (var team in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(team.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Transitions.Draft_To_Active);
					ReportSuccess(new DomTeam(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(team.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void TransitionToDeprecated(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			ValidateStateForDeprecateAction(apiTeams);
			ValidateTeamsAreNotInUse(apiTeams.Where(IsValid).ToArray());

			var toTransition = apiTeams.Where(IsValid).ToList();
			foreach (var team in toTransition)
			{
				try
				{
					var transitionedInstance = api.DomHelpers.SlcPeopleOrganizationHelper.DomHelper.DomInstances.DoStatusTransition(team.OriginalInstance.ID, SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Transitions.Active_To_Deprecated);
					ReportSuccess(new DomTeam(transitionedInstance));
				}
				catch (Exception ex)
				{
					ReportError(team.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = ex.ToString() });
				}
			}
		}

		private void Delete(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			var newTeams = apiTeams.Where(x => x.IsNew).ToList();
			newTeams.ForEach(x =>
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = $"A team that was not saved cannot be removed.",
					Id = x.Id,
				};

				ReportError(x.Id, error);
			});

			apiTeams = apiTeams.Except(newTeams).ToList();

			ValidateStateForDeleteAction(apiTeams);

			var toDelete = apiTeams.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(toDelete, DeleteLocked);
			ReportError(lockResult);
		}

		private void DeleteLocked(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided teams are valid", nameof(apiTeams));
			}

			var toDelete = apiTeams.Select(x => x.OriginalInstance.ToInstance()).ToList();
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

			ReportSuccess(toDelete.Where(x => domResult.SuccessfulIds.Contains(x.ID)).Select(x => new DomTeam(x)));
		}

		private void ValidateIdsNotInUse(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			var teamsRequiringValidation = apiTeams.Where(x => x.IsNew && x.HasUserDefinedId).ToList();
			if (teamsRequiringValidation.Count == 0)
			{
				return;
			}

			var teamsWithDuplicateIds = teamsRequiringValidation
				.GroupBy(role => role.Id)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var team in teamsWithDuplicateIds)
			{
				var error = new TeamDuplicateIdError
				{
					ErrorMessage = $"Team '{team.Name}' has a duplicate ID.",
					Id = team.Id,
				};

				ReportError(team.Id, error);

				teamsRequiringValidation.Remove(team);
			}

			foreach (var foundInstance in api.DomHelpers.SlcPeopleOrganizationHelper.GetPeopleOrganizationInstances(teamsRequiringValidation.Select(x => x.Id)))
			{
				api.Logger.Information(this, $"ID is already in use by a People and Organization instance.", [foundInstance.ID.Id]);

				var error = new TeamIdInUseError
				{
					ErrorMessage = "ID is already in use.",
					Id = foundInstance.ID.Id,
				};

				ReportError(foundInstance.ID.Id, error);
			}
		}

		private void ValidateStateForUpdateAction(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => !new[] { TeamState.Draft, TeamState.Active }.Contains(x.State)))
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = "Not allowed to update a team that is not in Draft or Active state.",
					Id = team.Id,
				};
				ReportError(team.Id, error);
			}
		}

		private void ValidateStateForActiveFromDraftAction(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => x.State != TeamState.Draft))
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = "Not allowed to activate a team that is not in Draft state.",
					Id = team.Id,
				};
				ReportError(team.Id, error);
			}
		}

		private void ValidateStateForDeprecateAction(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => x.State != TeamState.Active))
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = "Not allowed to deprecate a team that is not in Active state.",
					Id = team.Id,
				};
				ReportError(team.Id, error);
			}
		}

		private void ValidateStateForDeleteAction(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => !new[] { TeamState.Draft, TeamState.Deprecated }.Contains(x.State)))
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = "Not allowed to delete a team that is not in Draft or Deprecated state.",
					Id = team.Id,
				};

				ReportError(team.Id, error);
			}
		}

		private void ValidateNames(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			var teamsRequiringValidation = apiTeams.ToList();

			foreach (var organization in teamsRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new TeamInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = organization.Id,
				};

				ReportError(organization.Id, error);

				teamsRequiringValidation.Remove(organization);
			}

			foreach (var team in teamsRequiringValidation.Where(x => !InputValidator.HasValidTextLength(x.Name)).ToArray())
			{
				var error = new TeamInvalidNameError
				{
					ErrorMessage = $"Name exceeds maximum length of {InputValidator.DefaultMaxTextLength} characters.",
					Id = team.Id,
					Name = team.Name,
				};

				ReportError(team.Id, error);

				teamsRequiringValidation.Remove(team);
			}

			var teamsWithDuplicateNames = teamsRequiringValidation
				.GroupBy(role => role.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var team in teamsWithDuplicateNames)
			{
				var error = new TeamDuplicateNameError
				{
					ErrorMessage = $"Team '{team.Name}' has a duplicate name.",
					Id = team.Id,
					Name = team.Name,
				};

				ReportError(team.Id, error);
			}
		}

		private void ValidateDomNames(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			FilterElement<DomInstance> Filter(string name) =>
				DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Teams.Id)
				.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName).Equal(name));

			var domTeamsByName = api.DomHelpers.SlcPeopleOrganizationHelper.GetTeams(apiTeams.Select(x => x.Name), Filter)
				.GroupBy(x => x.TeamInformation.TeamName)
				.ToDictionary(x => x.Key, x => (IReadOnlyCollection<DomTeam>)x.ToList());

			foreach (var team in apiTeams)
			{
				if (!domTeamsByName.TryGetValue(team.Name, out var domTeams))
				{
					continue;
				}

				var existingTeams = domTeams.Where(x => x.ID.Id != team.Id).ToList();
				if (existingTeams.Count == 0)
				{
					continue;
				}

				api.Logger.Information(this, $"Name '{team.Name}' is already in use by DOM team(s) with ID(s)", [existingTeams.Select(x => x.ID.Id).ToArray()]);

				var error = new TeamNameExistsError
				{
					ErrorMessage = "Name is already in use.",
					Id = team.Id,
					Name = team.Name,
				};

				ReportError(team.Id, error);
			}
		}

		private void ValidateTeamsAreNotInUse(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			// Todo: implement logic to check if teams are in use by any person and report errors for those that are.
		}

		private void ValidateSkills(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			var skillValues = api.Skills.Read().Select(x => x.Name).ToList();

			foreach (var team in apiTeams)
			{
				foreach (var skill in team.Skills)
				{
					if (!skillValues.Contains(skill.Name))
					{
						var error = new TeamInvalidAssignedSkillError
						{
							ErrorMessage = $"Skill '{skill.Name}' does not exist.",
							Id = team.Id,
							Name = skill.Name,
						};
						ReportError(team.Id, error);
					}
				}
			}
		}

		private IEnumerable<DomChangeResults> GetTeamsWithChanges(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return Array.Empty<DomChangeResults>();
			}

			return GetTeamsWithChangesIterator(apiTeams);
		}

		private IEnumerable<DomChangeResults> GetTeamsWithChangesIterator(ICollection<Team> apiTeams)
		{
			var teamsRequiringValidation = apiTeams.Where(x => !x.IsNew && x.HasChanges).ToList();
			if (teamsRequiringValidation.Count == 0)
			{
				yield break;
			}

			var storedDomTeamsById = api.DomHelpers.SlcPeopleOrganizationHelper.GetTeams(teamsRequiringValidation.Select(x => x.Id))
				.ToDictionary(x => x.ID.Id);
			foreach (var team in teamsRequiringValidation)
			{
				if (!storedDomTeamsById.TryGetValue(team.Id, out var stored))
				{
					var error = new TeamNotFoundError
					{
						ErrorMessage = $"Team with ID '{team.Id}' no longer exists.",
						Id = team.Id,
					};

					ReportError(team.Id, error);

					continue;
				}

				var changeResult = DomChangeHandler.HandleChanges(team.OriginalInstance, team.GetInstanceWithChanges(), stored);
				if (changeResult.HasErrors)
				{
					foreach (var errorDetails in changeResult.Errors)
					{
						var error = new TeamValueAlreadyChangedError
						{
							ErrorMessage = errorDetails.Message,
							Id = team.Id,
						};

						ReportError(team.Id, error);
					}
				}

				yield return changeResult;
			}
		}
	}
}
