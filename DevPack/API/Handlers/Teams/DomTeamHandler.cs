namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
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

		internal static bool TryMakeBookable(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out DomInstanceBulkOperationResult<DomTeam> result)
		{
			var handler = new DomTeamHandler(api);
			handler.MakeBookable(apiTeams);

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

			ValidateStateForUpdateAction(apiTeams.Where(x => !x.IsNew).ToArray());
			var toValidate = apiTeams.Where(IsValid).ToList();

			ValidateIdsNotInUse(toValidate.Where(x => x.IsNew).ToArray());
			ValidateNames(toValidate);
			ValidateSkills(toValidate);

			var validTeams = toValidate.Where(IsValid).ToList();
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

			var teamsWithSkillChanges = toUpdate.Where(x =>
				IsValid(x)
				&& changeResults.Any(y => y.Instance.ID.Id == x.Id
					&& y.ChangedFields.Select(z => z.FieldDescriptorId).Contains(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamSkills.Id)));

			var bookableTeamsWithChanges = toUpdateNameValidation
				.Union(teamsWithSkillChanges)
				.Where(x => IsValid(x) && x.IsBookable)
				.ToList();
			UpdateBookableTeams(bookableTeamsWithChanges);

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

		private void UpdateBookableTeams(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			if (apiTeams.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided teams are valid", nameof(apiTeams));
			}

			MediaOpsResourcePoolHandler.TryCreateOrUpdate(api, apiTeams, out var result);

			foreach (var id in result.UnsuccessfulIds)
			{
				ReportError(id);

				if (result.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}
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

			DeprecateBookableTeams(apiTeams.Where(x => IsValid(x) && x.IsBookable).ToArray());

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

		private void DeprecateBookableTeams(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			MediaOpsResourcePoolHandler.TryDeprecate(api, apiTeams, out var result);

			foreach (var id in result.UnsuccessfulIds)
			{
				ReportError(id);

				if (result.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
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

			DeleteBookableTeams(apiTeams.Where(x => x.IsBookable).ToArray());

			var toDelete = apiTeams
				.Where(IsValid)
				.Select(x => x.OriginalInstance.ToInstance())
				.ToList();
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

		private void DeleteBookableTeams(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			MediaOpsResourcePoolHandler.TryDelete(api, apiTeams, out var result);

			foreach (var id in result.UnsuccessfulIds)
			{
				ReportError(id);

				if (result.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}
		}

		private void MakeBookable(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			ValidateStateForMakeBookableAction(apiTeams);
			var toValidate = apiTeams.Where(IsValid).ToList();

			ValidateIfNotAlreadyBookable(toValidate);

			var validTeams = toValidate.Where(IsValid).ToList();
			var lockResult = api.LockManager.LockAndExecute(validTeams, MakeBookableLocked);
			ReportError(lockResult);
		}

		private void MakeBookableLocked(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Any(x => !IsValid(x)))
			{
				throw new ArgumentException($"Not all provided teams are valid", nameof(apiTeams));
			}

			var mapper = TeamPersonBookableMapper.Load(api, apiTeams);
			CreateResourcesForBookablePeople(mapper);
			CompleteResourcesForBookablePeople(mapper);
			CreateResourcePoolsForBookableTeams(mapper);
			CompleteResourcePoolsForBookableTeams(mapper);
			UpdateBookablePeople(mapper);

			var validTeams = mapper.TeamsById.Values.Where(IsValid).ToList();
			CreateOrUpdateDom(validTeams.Select(x => x.GetInstanceWithChanges()).ToList());
		}

		private void CreateResourcesForBookablePeople(TeamPersonBookableMapper mapper)
		{
			if (mapper == null)
			{
				throw new ArgumentNullException(nameof(mapper));
			}

			var toCreate = mapper.PersonsById.Values
				.Where(x => !x.HadResourceIdWhenLoaded)
				.Select(x => x.Person)
				.ToList();
			if (MediaOpsResourceHandler.TryCreateOrUpdate(api, toCreate, out var createResult))
			{
				return;
			}

			var teamIdsWithFailures = GetTeamIdsFromPersonFailures(mapper, createResult.UnsuccessfulIds.ToList());
			PropagatePersonFailuresToTeams(mapper, teamIdsWithFailures, createResult.UnsuccessfulIds.ToList(), createResult.TraceDataPerItem, "create");

			var toDeleteIds = GetPersonIdsOnlyLinkedToFailedTeams(mapper, createResult.SuccessfulIds, teamIdsWithFailures);
			if (toDeleteIds.Count == 0)
			{
				return;
			}

			var toDelete = mapper.PersonsById
				.Where(x => toDeleteIds.Contains(x.Key))
				.Select(x => x.Value.Person)
				.ToList();
			if (!MediaOpsResourceHandler.TryDelete(api, toDelete, out var deleteResult))
			{
				api.Logger.Error(this, $"Failed to delete resources for {deleteResult.UnsuccessfulIds.Count} person(s) that had their resource creation succeed but were associated only with teams that had resource creation failures.", [deleteResult.UnsuccessfulIds.ToArray()]);
			}
		}

		private void CompleteResourcesForBookablePeople(TeamPersonBookableMapper mapper)
		{
			if (mapper == null)
			{
				throw new ArgumentNullException(nameof(mapper));
			}

			var validTeams = mapper.TeamsById.Values.Where(IsValid).ToList();
			var peopleToComplete = validTeams
				.SelectMany(t => mapper.PersonsByTeamId.TryGetValue(t.Id, out var people)
					? people
						.Where(p => !p.HadResourceIdWhenLoaded)
						.Select(p => p.Person)
					: new List<Person>())
				.Distinct()
				.ToList();

			if (MediaOpsResourceHandler.TryComplete(api, peopleToComplete, out var completeResult))
			{
				return;
			}

			var teamIdsWithFailures = GetTeamIdsFromPersonFailures(mapper, completeResult.UnsuccessfulIds.ToList());
			PropagatePersonFailuresToTeams(mapper, teamIdsWithFailures, completeResult.UnsuccessfulIds.ToList(), completeResult.TraceDataPerItem, "complete");

			var toDeprecateIds = GetPersonIdsOnlyLinkedToFailedTeams(mapper, completeResult.SuccessfulIds, teamIdsWithFailures);
			if (toDeprecateIds.Count == 0)
			{
				return;
			}

			var toDeprecate = mapper.PersonsById
				.Where(x => toDeprecateIds.Contains(x.Key))
				.Select(x => x.Value.Person)
				.ToList();
			if (!MediaOpsResourceHandler.TryDeprecate(api, toDeprecate, out var deprecateResult))
			{
				api.Logger.Error(this, $"Failed to deprecate resources for {deprecateResult.UnsuccessfulIds.Count} person(s) that had their resource completion succeed but were associated only with teams that had resource completion failures.", [deprecateResult.UnsuccessfulIds.ToArray()]);
			}

			var toDeleteIds = completeResult.UnsuccessfulIds.Concat(deprecateResult.SuccessfulIds).ToList();
			var toDelete = mapper.PersonsById
				.Where(x => toDeleteIds.Contains(x.Key))
				.Select(x => x.Value.Person)
				.ToList();
			if (!MediaOpsResourceHandler.TryDelete(api, toDelete, out var deleteResult))
			{
				api.Logger.Error(this, $"Failed to delete resources for {deleteResult.UnsuccessfulIds.Count} person(s) that had their resource completion succeed but were associated only with teams that had resource completion failures or had their resource deprecated due to being associated only with teams that had resource completion failures.", [deleteResult.UnsuccessfulIds.ToArray()]);
			}
		}

		private void CreateResourcePoolsForBookableTeams(TeamPersonBookableMapper mapper)
		{
			if (mapper == null)
			{
				throw new ArgumentNullException(nameof(mapper));
			}

			var validTeams = mapper.TeamsById.Values.Where(IsValid).ToList();

			if (MediaOpsResourcePoolHandler.TryCreateOrUpdate(api, validTeams, out var createResult))
			{
				return;
			}

			foreach (var id in createResult.UnsuccessfulIds)
			{
				ReportError(id);

				if (createResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}

			var teamIdsWithFailures = createResult.UnsuccessfulIds.ToHashSet();
			RevertPeopleForFailedTeams(mapper, teamIdsWithFailures, "creation");
		}

		private void CompleteResourcePoolsForBookableTeams(TeamPersonBookableMapper mapper)
		{
			if (mapper == null)
			{
				throw new ArgumentNullException(nameof(mapper));
			}

			var validTeams = mapper.TeamsById.Values.Where(IsValid).ToList();

			if (MediaOpsResourcePoolHandler.TryComplete(api, validTeams, out var completeResult))
			{
				return;
			}

			foreach (var id in completeResult.UnsuccessfulIds)
			{
				ReportError(id);

				if (completeResult.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}

			var teamIdsWithFailures = completeResult.UnsuccessfulIds.ToHashSet();
			RevertPeopleForFailedTeams(mapper, teamIdsWithFailures, "completion");

			var toDeleteTeams = mapper.TeamsById
				.Where(x => teamIdsWithFailures.Contains(x.Key))
				.Select(x => x.Value)
				.ToList();
			if (!MediaOpsResourcePoolHandler.TryDelete(api, toDeleteTeams, out var deleteTeamsResults))
			{
				api.Logger.Error(this, $"Failed to delete {deleteTeamsResults.UnsuccessfulIds.Count} team(s) that had resource pool completion failures.", [deleteTeamsResults.UnsuccessfulIds.ToArray()]);
			}
		}

		private void UpdateBookablePeople(TeamPersonBookableMapper mapper)
		{
			if (mapper == null)
			{
				throw new ArgumentNullException(nameof(mapper));
			}

			var validTeams = mapper.TeamsById.Values.Where(IsValid).ToList();
			var peopleToUpdate = validTeams
				.SelectMany(t => mapper.PersonsByTeamId.TryGetValue(t.Id, out var people)
					? people
					: new List<TeamPersonBookableMapper.WrappedPerson>())
				.DistinctBy(x => x.Person.Id)
				.ToList();

			// Set cache
			foreach (var person in peopleToUpdate)
			{
				if (!mapper.TeamsByPersonId.TryGetValue(person.Person.Id, out var teams))
				{
					continue;
				}

				person.Person.ApiObjectCache.SetCache<Team>(teams);
			}

			var domUpdates = peopleToUpdate.Where(x => !x.HadResourceIdWhenLoaded).Select(x => x.Person).ToList();
			var directUpdates = peopleToUpdate.Where(x => x.HadResourceIdWhenLoaded).Select(x => x.Person).ToList();

			if (domUpdates.Count > 0)
			{
				DomPersonHandler.TryCreateOrUpdate(api, domUpdates, out var result);
			}

			if (directUpdates.Count > 0)
			{
				MediaOpsResourceHandler.TryCreateOrUpdate(api, directUpdates, out var result);
			}
		}

		private HashSet<Guid> GetTeamIdsFromPersonFailures(TeamPersonBookableMapper mapper, ICollection<Guid> failedPersonIds)
		{
			var teamIdsWithFailures = new HashSet<Guid>();
			foreach (var personId in failedPersonIds)
			{
				if (!mapper.TeamsByPersonId.TryGetValue(personId, out var teams))
				{
					continue;
				}

				foreach (var team in teams)
				{
					teamIdsWithFailures.Add(team.Id);
				}
			}

			return teamIdsWithFailures;
		}

		private void PropagatePersonFailuresToTeams(TeamPersonBookableMapper mapper, HashSet<Guid> teamIdsWithFailures, ICollection<Guid> failedPersonIds, IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> traceDataPerItem, string actionVerb)
		{
			foreach (var teamId in teamIdsWithFailures)
			{
				if (!mapper.PersonsByTeamId.TryGetValue(teamId, out var people))
				{
					continue;
				}

				var failedPeople = people.Where(x => failedPersonIds.Contains(x.Person.Id)).ToList();

				var error = new TeamMakeBookableError
				{
					ErrorMessage = $"Failed to {actionVerb} resource for {failedPeople.Count} person(s) associated with this team, so the team cannot be made bookable.",
					Id = teamId,
				};
				ReportError(teamId, error);

				foreach (var person in failedPeople)
				{
					if (!traceDataPerItem.TryGetValue(person.Person.Id, out var traceData))
					{
						continue;
					}

					PassTraceData(teamId, traceData);
				}
			}
		}

		private static HashSet<Guid> GetPersonIdsOnlyLinkedToFailedTeams(TeamPersonBookableMapper mapper, IEnumerable<Guid> personIds, HashSet<Guid> teamIdsWithFailures)
		{
			var result = new HashSet<Guid>();
			foreach (var personId in personIds)
			{
				if (!mapper.TeamsByPersonId.TryGetValue(personId, out var teams))
				{
					continue;
				}

				if (teams.All(t => teamIdsWithFailures.Contains(t.Id)))
				{
					result.Add(personId);
				}
			}

			return result;
		}

		private void RevertPeopleForFailedTeams(TeamPersonBookableMapper mapper, HashSet<Guid> teamIdsWithFailures, string operationName)
		{
			var toDeprecatePersonIds = GetPersonIdsOnlyLinkedToFailedTeams(mapper, mapper.PersonsById.Keys, teamIdsWithFailures);
			if (toDeprecatePersonIds.Count == 0)
			{
				return;
			}

			var toDeprecatePeople = mapper.PersonsById
				.Where(x => toDeprecatePersonIds.Contains(x.Key))
				.Select(x => x.Value.Person)
				.ToList();

			api.Logger.Warning(this, $"Reverting {toDeprecatePeople.Count} person(s) due to resource pool {operationName} failures for their associated teams.");

			if (!MediaOpsResourceHandler.TryDeprecate(api, toDeprecatePeople, out var deprecateResult))
			{
				api.Logger.Error(this, $"Failed to deprecate resources for {deprecateResult.UnsuccessfulIds.Count} person(s) that were associated only with teams that had resource pool {operationName} failures.", [deprecateResult.UnsuccessfulIds.ToArray()]);
			}

			var toDeletePersonIds = deprecateResult.SuccessfulIds.ToList();
			var toDeletePeople = mapper.PersonsById
				.Where(x => toDeletePersonIds.Contains(x.Key))
				.Select(x => x.Value.Person)
				.ToList();
			if (!MediaOpsResourceHandler.TryDelete(api, toDeletePeople, out var deleteResult))
			{
				api.Logger.Error(this, $"Failed to delete resources for {deleteResult.UnsuccessfulIds.Count} person(s) that were associated only with teams that had resource pool {operationName} failures.", [deleteResult.UnsuccessfulIds.ToArray()]);
			}
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

		private void ValidateStateForMakeBookableAction(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => x.IsNew || x.State != TeamState.Active))
			{
				var error = new TeamInvalidStateError
				{
					ErrorMessage = team.IsNew
					? "A team that was not saved cannot be made bookable."
					: "Not allowed to make a team bookable that is not in Active state.",
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

			foreach (var team in teamsRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new TeamInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Id = team.Id,
				};

				ReportError(team.Id, error);

				teamsRequiringValidation.Remove(team);
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

			var filter = new ORFilterElement<Person>(apiTeams
				.Select(x => PersonExposers.TeamMemberships.TeamId.Equal(x.Id))
				.ToArray());

			var peopleImplementingTeams = api.People.Read(filter);

			var peopleByTeamId = peopleImplementingTeams
				.SelectMany(x => x.TeamMemberships.Select(tm => new { tm.TeamId, Person = x }))
				.GroupBy(x => x.TeamId)
				.ToDictionary(x => x.Key, x => x.Select(y => y.Person).ToList());

			foreach (var team in apiTeams)
			{
				if (!peopleByTeamId.TryGetValue(team.Id, out var people))
				{
					continue;
				}

				var error = new TeamInUseByPeopleError
				{
					ErrorMessage = $"Team '{team.Name}' is in use by {people.Count} people.",
					Id = team.Id,
					PeopleIds = people.Select(x => x.Id).ToList(),
				};

				ReportError(team.Id, error);
			}
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

		private void ValidateIfNotAlreadyBookable(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => x.IsBookable))
			{
				var error = new TeamMakeBookableError
				{
					ErrorMessage = $"Team '{team.Name}' is already bookable.",
					Id = team.Id,
				};

				ReportError(team.Id, error);
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
			var unchangedTeams = apiTeams
				.Where(x => !x.IsNew && !x.HasChanges)
				.Select(x => x.OriginalInstance)
				.ToList();
			ReportSuccess(unchangedTeams);

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

		private sealed class TeamPersonBookableMapper
		{
			private readonly Dictionary<Guid, WrappedPerson> personsById = new();
			private readonly Dictionary<Guid, Team> teamsById = new();

			private readonly Dictionary<Guid, List<WrappedPerson>> personsByTeamId = new();
			private readonly Dictionary<Guid, List<Team>> teamsByPersonId = new();

			private TeamPersonBookableMapper(ICollection<Team> apiTeams)
			{
				if (apiTeams == null)
				{
					throw new ArgumentNullException(nameof(apiTeams));
				}

				teamsById = apiTeams.ToDictionary(x => x.Id);
			}

			public IReadOnlyDictionary<Guid, WrappedPerson> PersonsById => personsById;

			public IReadOnlyDictionary<Guid, Team> TeamsById => teamsById;

			public IReadOnlyDictionary<Guid, List<WrappedPerson>> PersonsByTeamId => personsByTeamId;

			public IReadOnlyDictionary<Guid, List<Team>> TeamsByPersonId => teamsByPersonId;

			public static TeamPersonBookableMapper Load(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams)
			{
				if (api == null)
				{
					throw new ArgumentNullException(nameof(api));
				}

				var mapper = new TeamPersonBookableMapper(apiTeams);

				var filter = new ANDFilterElement<Person>(
					PersonExposers.State.Equal(PersonState.Active),
					/*new ORFilterElement<Person>(
						PersonExposers.HasResourceId.Equal(false),
						PersonExposers.ResourceId.Equal(Guid.Empty)),*/
					new ORFilterElement<Person>(apiTeams.Select(x => PersonExposers.TeamMemberships.TeamId.Equal(x.Id)).ToArray()));

				foreach (var person in api.People.Read(filter))
				{
					mapper.personsById[person.Id] = new WrappedPerson(person);

					foreach (var teamMembership in person.TeamMemberships)
					{
						if (!mapper.teamsById.ContainsKey(teamMembership.TeamId))
						{
							continue;
						}

						if (!mapper.personsByTeamId.TryGetValue(teamMembership.TeamId, out var personsInTeam))
						{
							personsInTeam = new List<WrappedPerson>();
							mapper.personsByTeamId[teamMembership.TeamId] = personsInTeam;
						}

						if (!mapper.teamsByPersonId.TryGetValue(person.Id, out var teamsOfPerson))
						{
							teamsOfPerson = new List<Team>();
							mapper.teamsByPersonId[person.Id] = teamsOfPerson;
						}

						personsInTeam.Add(new WrappedPerson(person));
						teamsOfPerson.Add(mapper.teamsById[teamMembership.TeamId]);
					}
				}

				return mapper;
			}

			internal sealed class WrappedPerson
			{
				public WrappedPerson(Person person)
				{
					Person = person ?? throw new ArgumentNullException(nameof(person));

					HadResourceIdWhenLoaded = person.ResourceId != Guid.Empty;
				}

				public Person Person { get; }

				public bool HadResourceIdWhenLoaded { get; }
			}
		}
	}
}
