namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal class OrganizationFilterTranslator : DomInstanceFilterTranslator<Organization>
	{
		private readonly FilterElement<DomInstance> organizationDomDefinitionFilter =
			DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id)
			.AND(DomInstanceExposers.StatusId.NotEqual(SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Edit));
		private readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> handlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			[OrganizationExposers.Id.fieldName] = HandleGuid,
			[OrganizationExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName), comparer, (string)value),
			[OrganizationExposers.CategoryId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Category), comparer, (Guid)value),
			[OrganizationExposers.State.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.StatusId, comparer, ConvertOrganizationState((OrganizationState)value)),
		};

		protected override Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> Handlers => handlers;

		protected override FilterElement<DomInstance> DomDefinitionFilter => organizationDomDefinitionFilter;

		private static string ConvertOrganizationState(OrganizationState filterValue)
		{
			switch (filterValue)
			{
				case OrganizationState.Draft:
					return SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Draft;
				case OrganizationState.Active:
					return SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Active;
				case OrganizationState.Deprecated:
					return SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses.Deprecated;
				default:
					throw new InvalidOperationException($"Unsupported organization state: {filterValue}");
			}
		}
	}
}
