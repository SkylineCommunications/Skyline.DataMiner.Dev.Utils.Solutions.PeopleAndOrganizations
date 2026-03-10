namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	internal class StringBulkOperationResult<T> : IBulkOperationResult<string>
		where T : class
	{
		public StringBulkOperationResult(IReadOnlyCollection<T> successItems, IReadOnlyCollection<string> successfulIds, IReadOnlyCollection<string> unsuccessfulIds, IReadOnlyDictionary<string, PeopleAndOrganizationsTraceData> traceDataPerItem)
		{
			SuccessfulItems = successItems ?? throw new ArgumentNullException(nameof(successItems));
			SuccessfulIds = successfulIds ?? throw new ArgumentNullException(nameof(successfulIds));
			UnsuccessfulIds = unsuccessfulIds ?? throw new ArgumentNullException(nameof(unsuccessfulIds));
			TraceDataPerItem = traceDataPerItem ?? throw new ArgumentNullException(nameof(traceDataPerItem));
		}

		public IReadOnlyCollection<string> SuccessfulIds { get; }

		public IReadOnlyCollection<T> SuccessfulItems { get; }

		public IReadOnlyDictionary<string, PeopleAndOrganizationsTraceData> TraceDataPerItem { get; }

		public IReadOnlyCollection<string> UnsuccessfulIds { get; }

		internal bool HasFailures
		{
			get
			{
				return UnsuccessfulIds.Count > 0;
			}
		}

		internal void ThrowBulkException()
		{
			throw new PeopleAndOrganizationsBulkException<string>(this);
		}

		internal void ThrowSingleException(string key)
		{
			throw new PeopleAndOrganizationsException(TraceDataPerItem[key]);
		}
	}
}
