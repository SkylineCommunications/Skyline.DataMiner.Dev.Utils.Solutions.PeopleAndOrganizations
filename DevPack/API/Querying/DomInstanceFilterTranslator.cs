namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying
{
	using System;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	internal abstract class DomInstanceFilterTranslator<T> : FilterTranslator<T, DomInstance> where T : ApiObject
	{
		protected DomInstanceFilterTranslator()
		{
		}

		protected abstract FilterElement<DomInstance> DomDefinitionFilter { get; }

		public override FilterElement<DomInstance> TranslateFilter(FilterElement<T> filter)
		{
			return base.TranslateFilter(filter).AND(DomDefinitionFilter);
		}

		protected static FilterElement<DomInstance> HandleGuid(Comparer comparer, object value)
		{
			return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
		}

		protected static IOrderByElement HandleGuid(SortOrder sortOrder, bool naturalSort)
		{
			return OrderByElementFactory.Create(DomInstanceExposers.Id, sortOrder, naturalSort);
		}
	}
}
