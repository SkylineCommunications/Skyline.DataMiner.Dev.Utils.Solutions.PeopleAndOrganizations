namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class MediaOpsHandler<T> : ApiObjectValidator<T, Guid> where T : ApiObject
	{
		private readonly PeopleAndOrganizationsApi api;
		private readonly List<Guid> successfulIds = new List<Guid>();

		private MediaOpsHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal override IReadOnlyCollection<Guid> SuccessfulIds => successfulIds;

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<T> apiObjects, out ApiObjectBulkOperationResult<T> result)
		{
			var handler = new MediaOpsHandler<T>(api);
			handler.CreateOrUpdate(apiObjects);

			result = new ApiObjectBulkOperationResult<T>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<T> apiObjects, out ApiObjectBulkOperationResult<T> result)
		{
			var handler = new MediaOpsHandler<T>(api);
			handler.Deprecate(apiObjects);

			result = new ApiObjectBulkOperationResult<T>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<T> apiObjects, out ApiObjectBulkOperationResult<T> result)
		{
			var handler = new MediaOpsHandler<T>(api);
			handler.Delete(apiObjects);

			result = new ApiObjectBulkOperationResult<T>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		protected override void ReportSuccess(T item)
		{
			if (unsuccessfulItems.Contains(item.Id))
			{
				throw new InvalidOperationException($"An item cannot be marked as both successful and unsuccessful");
			}

			successfulIds.Add(item.Id);
			successfulItems.Add(item);
		}

		private void CreateOrUpdate(ICollection<T> apiObjects)
		{
			if (apiObjects == null)
			{
				throw new ArgumentNullException(nameof(apiObjects));
			}

			if (apiObjects.Count == 0)
			{
				return;
			}

			var mapping = new Dictionary<Type, Action>
			{
				[typeof(Team)] = () => CreateOrUpdate(apiObjects.Cast<Team>().ToList()),
				[typeof(Person)] = () => CreateOrUpdate(apiObjects.Cast<Person>().ToList()),
			};

			if (!mapping.TryGetValue(typeof(T), out var action))
			{
				throw new NotSupportedException($"Type {typeof(T).Name} is not supported by {nameof(MediaOpsHandler<T>)}");
			}

			action();
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

			if (apiTeams.Any(x => !IsValid(x.Id)))
			{
				throw new ArgumentException($"Not all provided teams are valid", nameof(apiTeams));
			}

			var poolIds = apiTeams
				.Select(x => x.ResourcePoolId)
				.Where(x => x != Guid.Empty)
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

			var successfulCreateOrUpdatePoolIds = new List<Guid>();
			try
			{
				successfulCreateOrUpdatePoolIds = api.PlanApi.ResourcePools.CreateOrUpdate(poolsToCreateOrUpdate).Select(x => x.Id).ToList();
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				successfulCreateOrUpdatePoolIds = ex.Result.SuccessfulIds.ToList();

				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}

			var poolsToComplete = poolsToCreateOrUpdate
				.Where(x => successfulCreateOrUpdatePoolIds.Contains(x.Id) && x.State == ResourcePoolState.Draft)
				.ToList();

			var successfulCompletePoolIds = new List<Guid>();
			try
			{
				successfulCompletePoolIds = api.PlanApi.ResourcePools.Complete(poolsToComplete).Select(x => x.Id).ToList();
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				successfulCompletePoolIds = ex.Result.SuccessfulIds.ToList();

				HandleFailure(ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}

			HandleSuccess(successfulCreateOrUpdatePoolIds.Concat(successfulCompletePoolIds).Distinct().ToList());

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

					ReportSuccess((T)(object)team);
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
		}

		public void Deprecate(ICollection<T> apiObjects)
		{
			if (apiObjects == null)
			{
				throw new ArgumentNullException(nameof(apiObjects));
			}

			if (apiObjects.Count == 0)
			{
				return;
			}

			var mapping = new Dictionary<Type, Action>
			{
				[typeof(Team)] = () => Deprecate(apiObjects.Cast<Team>().ToList()),
				[typeof(Person)] = () => Deprecate(apiObjects.Cast<Person>().ToList()),
			};

			if (!mapping.TryGetValue(typeof(T), out var action))
			{
				throw new NotSupportedException($"Type {typeof(T).Name} is not supported by {nameof(MediaOpsHandler<T>)}");
			}

			action();
		}

		public void Deprecate(ICollection<Team> apiTeams)
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

		public void Deprecate(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}
		}

		public void Delete(ICollection<T> apiObjects)
		{
			if (apiObjects == null)
			{
				throw new ArgumentNullException(nameof(apiObjects));
			}

			if (apiObjects.Count == 0)
			{
				return;
			}

			var mapping = new Dictionary<Type, Action>
			{
				[typeof(Team)] = () => Delete(apiObjects.Cast<Team>().ToList()),
				[typeof(Person)] = () => Delete(apiObjects.Cast<Person>().ToList()),
			};

			if (!mapping.TryGetValue(typeof(T), out var action))
			{
				throw new NotSupportedException($"Type {typeof(T).Name} is not supported by {nameof(MediaOpsHandler<T>)}");
			}

			action();
		}

		public void Delete(ICollection<Team> apiTeams)
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

		public void Delete(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
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

		private void SyncTeamWithResourcePool(Team team, ResourcePool pool)
		{
			pool.Name = team.Name;

			if (team.Skills.Count > 0)
			{
				var capabilitySetting = new CapabilitySettings(SkillHandler.SkillCapabilityId)
						.SetDiscretes(team.Skills.Select(x => x.Name).ToList());
				pool.SetCapabilities([capabilitySetting]);
			}
			else if (pool.Capabilities.Count > 0)
			{
				pool.SetCapabilities([]);
			}
		}
	}
}
