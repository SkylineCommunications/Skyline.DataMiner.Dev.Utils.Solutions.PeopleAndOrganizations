namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
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

			foreach (var person in apiPeople.Where(x => !new[] {PersonState.Draft, PersonState.Deprecated}.Contains(x.State)))
			{
				var error = new PersonInvalidStateError
				{
					ErrorMessage = "Not allowed to delete a person that is not in Draft or Deprecated state.",
					Id = person.Id,
				};

				ReportError(person.Id, error);
			}
		}
	}
}
