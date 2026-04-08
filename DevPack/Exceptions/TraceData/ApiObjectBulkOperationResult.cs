namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	internal class ApiObjectBulkOperationResult<T> : BulkOperationResult<T> where T: ApiObject
	{
		public ApiObjectBulkOperationResult(IReadOnlyCollection<T> successItems, IReadOnlyCollection<Guid> unsuccessfulIds, IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> traceDataPerItem) : base(successItems, GetSuccessfulIds(successItems), unsuccessfulIds, traceDataPerItem)
		{
		}

		private static IReadOnlyCollection<Guid> GetSuccessfulIds(IReadOnlyCollection<T> successItems)
		{
			return successItems.Select(item => item.Id).ToList();
		}
	}
}
