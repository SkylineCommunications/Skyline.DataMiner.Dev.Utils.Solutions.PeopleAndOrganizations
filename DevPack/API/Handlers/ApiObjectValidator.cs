namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Tools;

	// Base class with generic ID support
	internal abstract class ApiObjectValidatorBase<TId>
	{
		protected readonly HashSet<TId> unsuccessfulItems = new HashSet<TId>();
		private readonly Dictionary<TId, PeopleAndOrganizationsTraceData> traceDataPerItem = new Dictionary<TId, PeopleAndOrganizationsTraceData>();

		protected ApiObjectValidatorBase()
		{
		}

		internal IReadOnlyDictionary<TId, PeopleAndOrganizationsTraceData> TraceDataPerItem => traceDataPerItem;

		internal IReadOnlyCollection<TId> UnsuccessfulItems => unsuccessfulItems;

		internal void PassTraceData(ApiObjectValidatorBase<TId> internalValidator)
		{
			if (internalValidator == null)
			{
				throw new ArgumentNullException(nameof(internalValidator));
			}

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

		internal void PassTraceData(TId key, PeopleAndOrganizationsTraceData traceData)
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

		internal bool IsValid(TId id)
		{
			return !TraceDataPerItem.ContainsKey(id);
		}

		protected void ReportError(TId key, PeopleAndOrganizationsErrorData error)
		{
			AddValidationError(key, error);
			ReportError(key);
		}

		protected virtual void ReportError(TId key)
		{
			unsuccessfulItems.Add(key);
		}

		private void AddValidationError(TId key, PeopleAndOrganizationsErrorData error)
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

	internal abstract class ApiObjectValidator<T, TId> : ApiObjectValidatorBase<TId>
	{
		protected readonly HashSet<T> successfulItems = new HashSet<T>();

		internal IReadOnlyCollection<T> SuccessfulItems => successfulItems;

		internal abstract IReadOnlyCollection<TId> SuccessfulIds { get; }

		internal void PassTraceData(ApiObjectValidator<T, TId> internalValidator)
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

		protected override void ReportError(TId key)
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
				if (failedToLockObject.Id is TId id)
				{
					ReportError(id, new PeopleAndOrganizationsErrorData() { ErrorMessage = $"Failed to lock {typeof(T).Name} {failedToLockObject.Id}." });
				}
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
}
