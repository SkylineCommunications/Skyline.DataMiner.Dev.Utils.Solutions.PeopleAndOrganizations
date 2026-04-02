namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class MediaOpsResourcePoolHandler : ApiObjectValidator<Team>
	{
		private readonly PeopleAndOrganizationsApi api;

		private MediaOpsResourcePoolHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out ApiObjectBulkOperationResult<Team> result)
		{
			var handler = new MediaOpsResourcePoolHandler(api);
			handler.CreateOrUpdate(apiTeams);

			result = new ApiObjectBulkOperationResult<Team>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryComplete(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out ApiObjectBulkOperationResult<Team> result)
		{
			var handler = new MediaOpsResourcePoolHandler(api);
			handler.Complete(apiTeams);

			result = new ApiObjectBulkOperationResult<Team>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out ApiObjectBulkOperationResult<Team> result)
		{
			var handler = new MediaOpsResourcePoolHandler(api);
			handler.Deprecate(apiTeams);

			result = new ApiObjectBulkOperationResult<Team>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Team> apiTeams, out ApiObjectBulkOperationResult<Team> result)
		{
			var handler = new MediaOpsResourcePoolHandler(api);
			handler.Delete(apiTeams);

			result = new ApiObjectBulkOperationResult<Team>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
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

			var poolIds = apiTeams
				.Where(x => IsValid(x.Id) && x.ResourcePoolId != Guid.Empty)
				.Select(x => x.ResourcePoolId)
				.ToList();
			var poolsById = api.PlanApi.ResourcePools.Read(poolIds).ToDictionary(x => x.Id);
			var teamsByPoolId = new Dictionary<Guid, Team>();

			var poolsToCreateOrUpdate = new List<ResourcePool>();
			foreach (var team in apiTeams)
			{
				ResourcePool pool = null;
				if (team.ResourcePoolId == Guid.Empty)
				{
					pool = new ResourcePool();
				}
				else if (!poolsById.TryGetValue(team.ResourcePoolId, out pool))
				{
					api.Logger.Error(this, $"Resource pool no longer found with ID '{team.ResourcePoolId}'. Will be recreated with original ID.");
					pool = new ResourcePool(team.ResourcePoolId);
				}

				SyncTeamWithResourcePool(team, pool);
				poolsToCreateOrUpdate.Add(pool);

				teamsByPoolId[pool.Id] = team;
			}

			try
			{
				var pools = api.PlanApi.ResourcePools.CreateOrUpdate(poolsToCreateOrUpdate);
				HandleSuccess(pools.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleSuccess(ex.Result.SuccessfulIds.ToList());
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
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

					ReportSuccess(team);
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
						foreach (var error in ComposeTeamErrors(team.Id, traceData))
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

		private void Complete(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => IsValid(x) && x.ResourcePoolId == Guid.Empty))
			{
				var error = new TeamMakeBookableError
				{
					Id = team.Id,
					ErrorMessage = "Cannot complete the resource pool as the team has no resource pool assigned.",
				};
				ReportError(team.Id, error);
			}

			var teamsByPoolId = apiTeams.Where(IsValid).ToDictionary(x => x.ResourcePoolId);

			try
			{
				var pools = api.PlanApi.ResourcePools.Complete(teamsByPoolId.Keys);
				HandleSuccess(pools.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleSuccess(ex.Result.SuccessfulIds.ToList());
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
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

					ReportSuccess(team);
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
						foreach (var error in ComposeTeamErrors(team.Id, traceData))
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

		private void Deprecate(ICollection<Team> apiTeams)
		{
			if (apiTeams == null)
			{
				throw new ArgumentNullException(nameof(apiTeams));
			}

			if (apiTeams.Count == 0)
			{
				return;
			}

			foreach (var team in apiTeams.Where(x => IsValid(x) && x.ResourcePoolId == Guid.Empty))
			{
				var error = new TeamMakeBookableError
				{
					Id = team.Id,
					ErrorMessage = "Cannot deprecate the resource pool as the team has no resource pool assigned.",
				};
				ReportError(team.Id, error);
			}

			var teamsByPoolId = apiTeams.Where(IsValid).ToDictionary(x => x.ResourcePoolId);

			try
			{
				var pools = api.PlanApi.ResourcePools.Deprecate(teamsByPoolId.Keys);
				HandleSuccess(pools.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleSuccess(ex.Result.SuccessfulIds.ToList());
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
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

					ReportSuccess(team);
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
						foreach (var error in ComposeTeamErrors(team.Id, traceData))
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

			foreach (var team in apiTeams.Where(x => IsValid(x) && x.ResourcePoolId == Guid.Empty))
			{
				var error = new TeamMakeBookableError
				{
					Id = team.Id,
					ErrorMessage = "Cannot delete the resource pool as the team has no resource pool assigned.",
				};
				ReportError(team.Id, error);
			}

			var teamsByPoolId = apiTeams.Where(IsValid).ToDictionary(x => x.ResourcePoolId);

			try
			{
				api.PlanApi.ResourcePools.Delete(teamsByPoolId.Keys);
				HandleSuccess(teamsByPoolId.Keys);
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandleSuccess(ex.Result.SuccessfulIds.ToList());
				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
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

					ReportSuccess(team);
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
						foreach (var error in ComposeTeamErrors(team.Id, traceData))
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

		private IEnumerable<PeopleAndOrganizationsErrorData> ComposeTeamErrors(Guid teamId, MediaOpsTraceData traceData)
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

		private bool SyncTeamWithResourcePool(Team team, ResourcePool pool)
		{
			var updateRequired = false;

			updateRequired |= SyncName(team, pool);
			updateRequired |= SyncSkills(team, pool);

			return updateRequired;
		}

		private bool SyncName(Team team, ResourcePool pool)
		{
			if (string.Equals(team.Name, pool.Name))
			{
				return false;
			}

			pool.Name = team.Name;
			return true;
		}

		private bool SyncSkills(Team team, ResourcePool pool)
		{
			var poolCapabilitySettings = pool.Capabilities.FirstOrDefault(x => x.Id == SkillHandler.SkillCapabilityId);
			if (team.Skills.Count == 0)
			{
				if (poolCapabilitySettings != null)
				{
					pool.SetCapabilities([]);
					return true;
				}

				return false;
			}

			if (poolCapabilitySettings == null)
			{
				poolCapabilitySettings = new CapabilitySettings(SkillHandler.SkillCapabilityId)
					.SetDiscretes(team.Skills.Select(x => x.Name).ToList());
				pool.SetCapabilities([poolCapabilitySettings]);

				return true;
			}

			var expectedDiscretes = team.Skills.Select(x => x.Name).ToList();
			if (!poolCapabilitySettings.Discretes.ScrambledEquals(expectedDiscretes))
			{
				poolCapabilitySettings.SetDiscretes(expectedDiscretes);

				return true;
			}

			return false;
		}
	}
}
