namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;

	internal class DomInstanceBulkOperationResult<T> : BulkOperationResult<T> where T : DomInstanceBase
	{
		public DomInstanceBulkOperationResult(IReadOnlyCollection<T> successItems, IReadOnlyCollection<Guid> unsuccessfulIds, IReadOnlyDictionary<Guid, PeopleAndOrganizationsTraceData> traceDataPerItem) : base(successItems, GetSuccessfulIds(successItems), unsuccessfulIds, traceDataPerItem)
		{
		}

		private static IReadOnlyCollection<Guid> GetSuccessfulIds(IReadOnlyCollection<T> successItems)
		{
			return successItems.Select(item => item.ID.Id).ToList();
		}
	}
}
