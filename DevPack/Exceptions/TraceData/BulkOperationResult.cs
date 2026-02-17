namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Contains the successfully handled items and the <see cref="PeopleAndOrganizationsTraceData"/> per item.
	/// </summary>
	/// <typeparam name="T">The type of objects that were processed.</typeparam>
	internal abstract class BulkOperationResult<T> : IBulkOperationResult<Guid>
		where T : class
	{
		private protected BulkOperationResult(IReadOnlyCollection<T> successItems, IReadOnlyCollection<Guid> successfulIds, IReadOnlyCollection<Guid> unsuccessfulIds, IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> traceDataPerItem)
		{
			SuccessfulItems = successItems ?? throw new ArgumentNullException(nameof(successItems));
			SuccessfulIds = successfulIds ?? throw new ArgumentNullException(nameof(successfulIds));
			UnsuccessfulIds = unsuccessfulIds ?? throw new ArgumentNullException(nameof(unsuccessfulIds));
			TraceDataPerItem = traceDataPerItem ?? throw new ArgumentNullException(nameof(traceDataPerItem));
		}

		/// <summary>
		/// Gets a list of IDs of successfully handled items.
		/// </summary>
		public IReadOnlyCollection<Guid> SuccessfulIds { get; }

		public IReadOnlyCollection<T> SuccessfulItems { get; }

		/// <summary>
		/// Gets the <see cref="PeopleAndOrganizationsTraceData"/> per successfully handled item.
		/// </summary>
		public IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> TraceDataPerItem { get; }

		/// <summary>
		/// Gets a list of IDs of the items that could not get handled.
		/// </summary>
		public IReadOnlyCollection<Guid> UnsuccessfulIds { get; }

		internal bool HasFailures
		{
			get
			{
				return UnsuccessfulIds.Count > 0;
			}
		}

		internal void ThrowBulkException()
		{
			throw new PeopleAndOrganizationsBulkException<Guid>(this);
		}

		internal void ThrowSingleException(Guid key)
		{
			throw new PeopleAndOrganizationsException(TraceDataPerItem[key]);
		}
	}
}
