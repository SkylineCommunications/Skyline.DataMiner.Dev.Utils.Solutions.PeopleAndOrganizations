namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class MediaOpsResourceHandler : ApiObjectValidator<Person>
	{
		private readonly PeopleAndOrganizationsApi api;

		private MediaOpsResourceHandler(PeopleAndOrganizationsApi api)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out ApiObjectBulkOperationResult<Person> result)
		{
			var handler = new MediaOpsResourceHandler(api);
			handler.CreateOrUpdate(apiPeople);

			result = new ApiObjectBulkOperationResult<Person>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryComplete(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out ApiObjectBulkOperationResult<Person> result)
		{
			var handler = new MediaOpsResourceHandler(api);
			handler.Complete(apiPeople);

			result = new ApiObjectBulkOperationResult<Person>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDeprecate(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out ApiObjectBulkOperationResult<Person> result)
		{
			var handler = new MediaOpsResourceHandler(api);
			handler.Deprecate(apiPeople);

			result = new ApiObjectBulkOperationResult<Person>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople, out ApiObjectBulkOperationResult<Person> result)
		{
			var handler = new MediaOpsResourceHandler(api);
			handler.Delete(apiPeople);

			result = new ApiObjectBulkOperationResult<Person>(handler.SuccessfulItems, handler.UnsuccessfulItems, handler.TraceDataPerItem);
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

			var personsByResourceId = new Dictionary<Guid, Person>();
			var resourcesToCreateOrUpdate = new List<Resource>();
			foreach (var mapping in PersonResourceMapping.GetMappings(api, apiPeople))
			{
				SyncPersonWithResource(mapping.Person, mapping.Resource);
				resourcesToCreateOrUpdate.Add(mapping.Resource);
				personsByResourceId[mapping.Resource.Id] = mapping.Person;
			}

			try
			{
				var resources = api.PlanApi.Resources.CreateOrUpdate(resourcesToCreateOrUpdate);
				HandlePersonResourceSuccess(personsByResourceId, resources.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandlePersonResourceSuccess(personsByResourceId, ex.Result.SuccessfulIds.ToList());
				HandlePersonResourceFailures(personsByResourceId, ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}
		}

		private void Complete(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => IsValid(x) && x.ResourceId == Guid.Empty))
			{
				var error = new PersonMakeBookableError
				{
					Id = person.Id,
					ErrorMessage = "Cannot complete the resource as the person has no resource assigned.",
				};
				ReportError(person.Id, error);
			}

			var personsByResourceId = apiPeople.Where(IsValid).ToDictionary(x => x.ResourceId);

			try
			{
				var resources = api.PlanApi.Resources.Complete(personsByResourceId.Keys);
				HandlePersonResourceSuccess(personsByResourceId, resources.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandlePersonResourceSuccess(personsByResourceId, ex.Result.SuccessfulIds.ToList());
				HandlePersonResourceFailures(personsByResourceId, ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}
		}

		private void Deprecate(ICollection<Person> apiPeople)
		{
			if (apiPeople == null)
			{
				throw new ArgumentNullException(nameof(apiPeople));
			}

			if (apiPeople.Count == 0)
			{
				return;
			}

			foreach (var person in apiPeople.Where(x => IsValid(x) && x.ResourceId == Guid.Empty))
			{
				var error = new PersonMakeBookableError
				{
					Id = person.Id,
					ErrorMessage = "Cannot deprecate the resource as the person has no resource assigned.",
				};
				ReportError(person.Id, error);
			}

			var personsByResourceId = apiPeople.Where(IsValid).ToDictionary(x => x.ResourceId);

			try
			{
				var resources = api.PlanApi.Resources.Deprecate(personsByResourceId.Keys);
				HandlePersonResourceSuccess(personsByResourceId, resources.Select(x => x.Id).ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandlePersonResourceSuccess(personsByResourceId, ex.Result.SuccessfulIds.ToList());
				HandlePersonResourceFailures(personsByResourceId, ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
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

			foreach (var person in apiPeople.Where(x => IsValid(x) && x.ResourceId == Guid.Empty))
			{
				var error = new PersonMakeBookableError
				{
					Id = person.Id,
					ErrorMessage = "Cannot delete the resource as the person has no resource assigned.",
				};
				ReportError(person.Id, error);
			}

			var personsByResourceId = apiPeople.Where(IsValid).ToDictionary(x => x.ResourceId);

			try
			{
				api.PlanApi.Resources.Delete(personsByResourceId.Keys);
				HandlePersonResourceSuccess(personsByResourceId, personsByResourceId.Keys.ToList());
			}
			catch (MediaOpsBulkException<Guid> ex)
			{
				HandlePersonResourceSuccess(personsByResourceId, ex.Result.SuccessfulIds.ToList());
				HandlePersonResourceFailures(personsByResourceId, ex.Result.UnsuccessfulIds.ToList(), ex.Result.TraceDataPerItem);
			}
		}

		private void HandlePersonResourceSuccess(Dictionary<Guid, Person> personsByResourceId, ICollection<Guid> resourceIds)
		{
			foreach (var resourceId in resourceIds)
			{
				if (!personsByResourceId.TryGetValue(resourceId, out var person))
				{
					api.Logger.Error(this, $"Received success result for Resource ID '{resourceId}' that cannot be mapped to a person.");
					continue;
				}

				person.ResourceId = resourceId;

				ReportSuccess(person);
			}
		}

		private void HandlePersonResourceFailures(Dictionary<Guid, Person> personsByResourceId, ICollection<Guid> resourceIds, IReadOnlyDictionary<Guid, MediaOpsTraceData> traceDataPerItem)
		{
			foreach (var resourceId in resourceIds)
			{
				if (!personsByResourceId.TryGetValue(resourceId, out var person))
				{
					api.Logger.Error(this, $"Received failure result for Resource ID '{resourceId}' that cannot be mapped to a person.");
					continue;
				}

				if (traceDataPerItem.TryGetValue(resourceId, out var traceData))
				{
					foreach (var error in ComposePersonErrors(person.Id, traceData))
					{
						ReportError(person.Id, error);
					}
				}
				else
				{
					ReportError(person.Id);
				}
			}
		}

		private IEnumerable<PeopleAndOrganizationsErrorData> ComposePersonErrors(Guid personId, MediaOpsTraceData traceData)
		{
			var resourceErrors = traceData.ErrorData.OfType<ResourceError>().ToList();
			if (traceData.ErrorData.Count != resourceErrors.Count)
			{
				yield return new PeopleAndOrganizationsErrorData
				{
					ErrorMessage = traceData.ToString(),
				};
			}

			foreach (var error in resourceErrors)
			{
				yield return new PersonMakeBookableError
				{
					Id = personId,
					ErrorMessage = error.ErrorMessage,
				};
			}
		}

		private bool SyncPersonWithResource(Person person, Resource resource)
		{
			var updateRequired = false;

			updateRequired |= SyncName(person, resource);
			updateRequired |= SyncSkills(person, resource);
			updateRequired |= SyncPools(person, resource);

			return updateRequired;
		}

		private bool SyncName(Person person, Resource resource)
		{
			if (string.Equals(person.Name, resource.Name))
			{
				return false;
			}

			resource.Name = person.Name;
			return true;
		}

		private bool SyncSkills(Person person, Resource resource)
		{
			var resourceCapabilitySettings = resource.Capabilities.FirstOrDefault(x => x.Id == SkillHandler.SkillCapabilityId);
			if (person.Skills.Count == 0)
			{
				if (resourceCapabilitySettings != null)
				{
					resource.SetCapabilities([]);
					return true;
				}

				return false;
			}

			if (resourceCapabilitySettings == null)
			{
				resourceCapabilitySettings = new CapabilitySettings(SkillHandler.SkillCapabilityId)
					.SetDiscretes(person.Skills.Select(x => x.Name).ToList());
				resource.SetCapabilities([resourceCapabilitySettings]);

				return true;
			}

			var expectedDiscretes = person.Skills.Select(x => x.Name).ToList();
			if (!resourceCapabilitySettings.Discretes.ScrambledEquals(expectedDiscretes))
			{
				resourceCapabilitySettings.SetDiscretes(expectedDiscretes);

				return true;
			}

			return false;
		}

		private bool SyncPools(Person person, Resource resource)
		{
			var teamIds = person.TeamMemberships.Select(x => x.TeamId).ToList();
			var cachedTeamsById = person.ApiObjectCache.GetFromCache<Team>().ToDictionary(x => x.Id);

			var missingTeamIds = teamIds.Where(x => !cachedTeamsById.ContainsKey(x));
			var teams = api.Teams.Read(missingTeamIds).ToList();
			teams.AddRange(cachedTeamsById.Values);

			var poolIds = teams.Select(x => x.ResourcePoolId).Where(x => x!= Guid.Empty).ToList();

			if (resource.ResourcePoolIds.ScrambledEquals(poolIds))
			{
				return false;
			}

			resource.SetPools(poolIds);

			return true;
		}

		private sealed class PersonResourceMapping
		{
			private PersonResourceMapping(Person person)
				: this(person, BuildResource(person.ResourceId))
			{
			}

			private PersonResourceMapping(Person person, Resource resource)
			{
				Person = person ?? throw new ArgumentNullException(nameof(person));
				Resource = resource ?? throw new ArgumentNullException(nameof(resource));
			}

			public Person Person { get; }

			public Resource Resource { get; }

			public static IEnumerable<PersonResourceMapping> GetMappings(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople)
			{
				if (api == null)
				{
					throw new ArgumentNullException(nameof(api));
				}

				if (apiPeople == null)
				{
					throw new ArgumentNullException(nameof(apiPeople));
				}

				if (apiPeople.Count == 0)
				{
					return [];
				}

				return GetMappingsIterator(api, apiPeople);
			}

			private static IEnumerable<PersonResourceMapping> GetMappingsIterator(PeopleAndOrganizationsApi api, ICollection<Person> apiPeople)
			{
				var resourceIds = apiPeople
				.Select(x => x.ResourceId)
				.Where(x => x != Guid.Empty)
				.ToList();
				var resourcesById = api.PlanApi.Resources.Read(resourceIds).ToDictionary(x => x.Id);

				foreach (var person in apiPeople)
				{
					if (person.ResourceId == Guid.Empty)
					{
						yield return new PersonResourceMapping(person);
						continue;
					}
					else if (!resourcesById.TryGetValue(person.ResourceId, out var resource))
					{
						var mapping = new PersonResourceMapping(person);
						api.Logger.Error(mapping, $"Resource no longer found with ID '{person.ResourceId}'. Will be recreated with original ID.");

						yield return mapping;
						continue;
					}
					else
					{
						yield return new PersonResourceMapping(person, resource);
					}
				}
			}

			private static Resource BuildResource(Guid id)
			{
				if (id != Guid.Empty)
				{
					return new UnmanagedResource(id);
				}

				return new UnmanagedResource();
			}
		}
	}
}
