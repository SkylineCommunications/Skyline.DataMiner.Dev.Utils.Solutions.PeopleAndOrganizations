namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	internal class RoleFilterTranslator : DomInstanceFilterTranslator<Role>
	{
		private readonly FilterElement<DomInstance> roleDomDefinitionFilter = DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Role.Id);
		private readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> filterHandlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			[RoleExposers.Id.fieldName] = HandleGuid,
			[RoleExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role), comparer, (string)value),
		};

		private readonly Dictionary<string, Func<SortOrder, bool, IOrderByElement>> orderByHandlers = new Dictionary<string, Func<SortOrder, bool, IOrderByElement>>
		{
			[RoleExposers.Id.fieldName] = HandleGuid,
			[RoleExposers.Name.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role), sortOrder, naturalSort),
		};

		protected override Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> FilterHandlers => filterHandlers;

		protected override Dictionary<string, Func<SortOrder, bool, IOrderByElement>> OrderByHandlers => orderByHandlers;

		protected override FilterElement<DomInstance> DomDefinitionFilter => roleDomDefinitionFilter;
	}
}
