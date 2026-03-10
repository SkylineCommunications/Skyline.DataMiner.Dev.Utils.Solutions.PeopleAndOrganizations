namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	internal class StringBulkOperationResult : IBulkOperationResult<string>
	{
		public StringBulkOperationResult(IReadOnlyCollection<string> successfulIds, IReadOnlyCollection<string> unsuccessfulIds, IReadOnlyDictionary<string, PeopleAndOrganizationsTraceData> traceDataPerItem)
		{
			SuccessfulIds = successfulIds ?? throw new ArgumentNullException(nameof(successfulIds));
			UnsuccessfulIds = unsuccessfulIds ?? throw new ArgumentNullException(nameof(unsuccessfulIds));
			TraceDataPerItem = traceDataPerItem ?? throw new ArgumentNullException(nameof(traceDataPerItem));
		}

		public IReadOnlyCollection<string> SuccessfulIds { get; }

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
