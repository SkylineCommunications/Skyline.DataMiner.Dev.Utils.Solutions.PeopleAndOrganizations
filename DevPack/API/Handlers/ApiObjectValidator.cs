namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Tools;

	internal abstract class ApiObjectValidator<T> : ApiObjectValidator
	{
		protected readonly HashSet<T> successfulItems = new HashSet<T>();

		internal IReadOnlyCollection<T> SuccessfulItems => successfulItems;

		internal abstract IReadOnlyCollection<Guid> SuccessfulIds { get; }

		internal void PassTraceData(ApiObjectValidator<T> internalValidator)
		{
			if (internalValidator == null) throw new ArgumentNullException(nameof(internalValidator));

			// Pass items in error state
			foreach (var id in internalValidator.UnsuccessfulItems)
			{
				ReportError(id);
				if (internalValidator.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}

			// Pass successful items
			foreach (var itemId in internalValidator.SuccessfulItems)
			{
				ReportSuccess(itemId);
			}
		}

		protected override void ReportError(Guid key)
		{
			if (SuccessfulIds.Contains(key))
			{
				throw new InvalidOperationException($"An item cannot be marked as both successful and unsuccessful");
			}

			unsuccessfulItems.Add(key);
		}

		protected void ReportError<K>(LockManager.LockResult<K> result) where K : ApiObject
		{
			foreach (var failedToLockObject in result.FailedToLockObjects)
			{
				ReportError(failedToLockObject.Id, new PeopleAndOrganizationsErrorData() { ErrorMessage = $"Failed to lock {typeof(T).Name} {failedToLockObject.Id}." });
			}
		}

		protected abstract void ReportSuccess(T item);

		protected void ReportSuccess(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				ReportSuccess(item);
			}
		}
	}

	internal class ApiObjectValidator
	{
		private readonly Dictionary<Guid, PeopleAndOrganizationsTraceData> traceDataPerItem = new Dictionary<Guid, PeopleAndOrganizationsTraceData>();
		protected readonly HashSet<Guid> unsuccessfulItems = new HashSet<Guid>();

		internal IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> TraceDataPerItem => traceDataPerItem;

		internal IReadOnlyCollection<Guid> UnsuccessfulItems => unsuccessfulItems;

		protected ApiObjectValidator()
		{
		}

		internal void PassTraceData(ApiObjectValidator internalValidator)
		{
			if (internalValidator == null) throw new ArgumentNullException(nameof(internalValidator));

			// Pass items in error state
			foreach (var id in internalValidator.UnsuccessfulItems)
			{
				ReportError(id);
				if (internalValidator.TraceDataPerItem.TryGetValue(id, out var traceData))
				{
					PassTraceData(id, traceData);
				}
			}
		}

		internal void PassTraceData(Guid key, PeopleAndOrganizationsTraceData traceData)
		{
			if (!traceDataPerItem.TryGetValue(key, out var existingTraceData))
			{
				traceDataPerItem.Add(key, traceData);
			}
			else
			{
				foreach (var error in traceData.ErrorData)
				{
					existingTraceData.Add(error);
				}
			}
		}

		protected void ReportError(Guid key, PeopleAndOrganizationsErrorData error)
		{
			AddValidationError(key, error);
			ReportError(key);
		}

		protected virtual void ReportError(Guid key)
		{
			unsuccessfulItems.Add(key);
		}

		protected bool IsValid(IIdentifiable identifiable)
		{
			return !TraceDataPerItem.Keys.Contains(identifiable.Id);
		}

		private void AddValidationError(Guid key, PeopleAndOrganizationsErrorData error)
		{
			if (error == null)
			{
				throw new ArgumentNullException(nameof(error));
			}

			if (!traceDataPerItem.TryGetValue(key, out var mediaOpsTraceData))
			{
				mediaOpsTraceData = new PeopleAndOrganizationsTraceData();
				traceDataPerItem.Add(key, mediaOpsTraceData);
			}

			mediaOpsTraceData.Add(error);
		}
	}
}
