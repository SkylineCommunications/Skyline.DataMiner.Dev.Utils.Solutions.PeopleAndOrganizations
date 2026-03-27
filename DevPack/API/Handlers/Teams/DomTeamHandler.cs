namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Alphaleonis.Win32.Filesystem;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
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

			var teamsByPoolId = apiTeams.ToDictionary(x => x.ResourcePoolId);
			var poolsbyId = api.PlanApi.ResourcePools.Read(teamsByPoolId.Keys).ToDictionary(x => x.Id);

			var poolsToCreateOrUpdate = new List<MediaOps.Plan.API.ResourcePool>();
			foreach (var kvp in teamsByPoolId)
			{
				if (!poolsbyId.TryGetValue(kvp.Key, out var pool))
				{
					pool = new MediaOps.Plan.API.ResourcePool(kvp.Key);
				}

				pool.Name = kvp.Value.Name;
				ApplyPoolCapabilities(pool, kvp.Value.Skills);

				poolsToCreateOrUpdate.Add(pool);
			}

			try
			{
				api.PlanApi.ResourcePools.CreateOrUpdate(poolsToCreateOrUpdate);
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}

			void ApplyPoolCapabilities(MediaOps.Plan.API.ResourcePool pool, IReadOnlyCollection<Skill> skills)
			{
				if (skills.Count > 0)
				{
					var capabilitySetting = new MediaOps.Plan.API.CapabilitySettings(SkillHandler.SkillCapabilityId)
						.SetDiscretes(skills.Select(x => x.Name).ToList());
					pool.SetCapabilities([capabilitySetting]);

					return;
				}

				if (pool.Capabilities.Count == 0)
				{
					return;
				}

				foreach (var capabilitySetting in pool.Capabilities.ToArray())
				{
					pool.RemoveCapability(capabilitySetting);
				}
			}

			void HandleFailure(ICollection<Guid> poolIds, IReadOnlyDictionary<Guid, MediaOpsTraceData> traceDataPerItem)
			{
				foreach (var poolId in poolIds)
				{
					if (!teamsByPoolId.TryGetValue(poolId, out var team))
					{
						api.Logger.Error(this, $"Received failure result for Resource Pool ID '{poolId}' that cannot be mapped to a team.");
						continue;
					}

					if (traceDataPerItem.TryGetValue(poolId, out var traceData))
					{
						foreach (var error in ComposeErrors(team.Id, traceData))
						{
							ReportError(team.Id, error);
						}
					}
					else
					{
						ReportError(team.Id);
					}
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

			var teamsByPoolId = apiTeams.ToDictionary(x => x.ResourcePoolId);

			try
			{
				api.PlanApi.ResourcePools.Deprecate(teamsByPoolId.Keys);
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}

			void HandleFailure(ICollection<Guid> poolIds, IReadOnlyDictionary<Guid, MediaOpsTraceData> traceDataPerItem)
			{
				foreach (var poolId in poolIds)
				{
					if (!teamsByPoolId.TryGetValue(poolId, out var team))
					{
						api.Logger.Error(this, $"Received failure result for Resource Pool ID '{poolId}' that cannot be mapped to a team.");
						continue;
					}

					if (traceDataPerItem.TryGetValue(poolId, out var traceData))
					{
						foreach (var error in ComposeErrors(team.Id, traceData))
						{
							ReportError(team.Id, error);
						}
					}
					else
					{
						ReportError(team.Id);
					}
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

			DeleteBookableTeams(apiTeams.Where(x => IsValid(x) && x.IsBookable).ToArray());

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

			var teamsByPoolId = apiTeams.ToDictionary(x => x.ResourcePoolId);

			try
			{
				api.PlanApi.ResourcePools.Delete(teamsByPoolId.Keys);
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}

			void HandleFailure(ICollection<Guid> poolIds, IReadOnlyDictionary<Guid, MediaOpsTraceData> traceDataPerItem)
			{
				foreach (var poolId in poolIds)
				{
					if (!teamsByPoolId.TryGetValue(poolId, out var team))
					{
						api.Logger.Error(this, $"Received failure result for Resource Pool ID '{poolId}' that cannot be mapped to a team.");
						continue;
					}

					if (traceDataPerItem.TryGetValue(poolId, out var traceData))
					{
						foreach (var error in ComposeErrors(team.Id, traceData))
						{
							ReportError(team.Id, error);
						}
					}
					else
					{
						ReportError(team.Id);
					}
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

			var teamsByPoolId = new Dictionary<Guid, Team>();

			var poolsToCreate = new List<MediaOps.Plan.API.ResourcePool>();
			foreach (var team in apiTeams)
			{
				var pool = BuildResourcePool(team);

				poolsToCreate.Add(pool);

				teamsByPoolId[pool.Id] = team;
			}

			var teamsToSave = new List<Team>();
			try
			{
				var createdResourcePools = api.PlanApi.ResourcePools.Create(poolsToCreate);
				createdResourcePools = api.PlanApi.ResourcePools.Complete(createdResourcePools);

				HandleSuccess(teamsByPoolId.Keys);
			}
			catch (MediaOpsBulkException<Guid> createException)
			{
				HandleFailure(createException.Result.UnsuccessfulIds.ToList(), createException.Result.TraceDataPerItem);

				try
				{
					api.PlanApi.ResourcePools.Complete(createException.Result.SuccessfulIds);

					HandleSuccess(createException.Result.SuccessfulIds.ToList());
				}
				catch (MediaOpsBulkException<Guid> completeException)
				{
					HandleSuccess(completeException.Result.SuccessfulIds.ToList());
					HandleFailure(completeException.Result.UnsuccessfulIds.ToList(), completeException.Result.TraceDataPerItem);
				}
			}

			if (teamsToSave.Count > 0)
			{
				var domTeams = teamsToSave.Select(x => x.GetInstanceWithChanges()).ToList();

				CreateOrUpdateDom(domTeams);
			}

			void HandleSuccess(ICollection<Guid> poolIds)
			{
				foreach (var poolId in poolIds)
				{
					if (!teamsByPoolId.TryGetValue(poolId, out var team))
					{
						api.Logger.Error(this, $"Received success result for Resource Pool ID '{poolId}' that cannot be mapped to a team.");
						continue;
					}

					team.ResourcePoolId = poolId;
					team.IsBookable = true;

					teamsToSave.Add(team);
				}
			}

			void HandleFailure(ICollection<Guid> poolIds, IReadOnlyDictionary<Guid, MediaOpsTraceData> traceDataPerItem)
			{
				foreach (var poolId in poolIds)
				{
					if (!teamsByPoolId.TryGetValue(poolId, out var team))
					{
						api.Logger.Error(this, $"Received failure result for Resource Pool ID '{poolId}' that cannot be mapped to a team.");
						continue;
					}

					if (traceDataPerItem.TryGetValue(poolId, out var traceData))
					{
						foreach (var error in ComposeErrors(team.Id, traceData))
						{
							ReportError(team.Id, error);
						}
					}
					else
					{
						ReportError(team.Id);
					}
				}
			}
		}

		private MediaOps.Plan.API.ResourcePool BuildResourcePool(Team apiTeam)
		{
			var resourcePool = new MediaOps.Plan.API.ResourcePool
			{
				Name = apiTeam.Name,
			};

			if (apiTeam.Skills.Count > 0)
			{
				var capabilitySetting = new MediaOps.Plan.API.CapabilitySettings(SkillHandler.SkillCapabilityId)
				.SetDiscretes(apiTeam.Skills.Select(x => x.Name).ToList());

				resourcePool.AddCapability(capabilitySetting);
			}

			return resourcePool;
		}

		private IEnumerable<PeopleAndOrganizationsErrorData> ComposeErrors(Guid teamId, MediaOpsTraceData traceData)
		{
			var resourcePoolErrors = traceData.ErrorData.OfType<ResourcePoolError>().ToList();
			if (traceData.ErrorData.Count != resourcePoolErrors.Count)
			{
				yield return new PeopleAndOrganizationsErrorData
				{
					ErrorMessage = traceData.ToString(),
				};
			}

			foreach (var error in resourcePoolErrors)
			{
				yield return new TeamMakeBookableError
				{
					Id = teamId,
					ErrorMessage = error.ErrorMessage,
				};
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
