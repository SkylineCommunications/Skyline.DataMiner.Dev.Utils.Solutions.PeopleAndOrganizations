namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Extensions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	internal class PersonFilterTranslator : DomInstanceFilterTranslator<Person>
	{
		private readonly FilterElement<DomInstance> personDomDefinitionFilter =
			DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.People.Id)
			.AND(DomInstanceExposers.StatusId.NotEqual(SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Edit));
		private readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> filterHandlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			[PersonExposers.Id.fieldName] = HandleGuid,
			[PersonExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName), comparer, (string)value),
			[PersonExposers.Email.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Email), comparer, (string)value),
			[PersonExposers.Phone.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Phone), comparer, (string)value),
			[PersonExposers.StreetAddress.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.StreetAddress), comparer, (string)value),
			[PersonExposers.City.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.City), comparer, (string)value),
			[PersonExposers.Country.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Country), comparer, ConvertCountry((Country)value)),
			[PersonExposers.ZipCode.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.ZIP), comparer, (string)value),
			[PersonExposers.ExperienceId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.ExperienceLevel), comparer, (Guid)value),
			[PersonExposers.OrganizationId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Organization.Organization_57695f03), comparer, (Guid)value),
			[PersonExposers.State.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.StatusId, comparer, ConvertPersonState((PersonState)value)),
			[PersonExposers.ResourceId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Resource.LinkedResource), comparer, (Guid)value),
			[PersonExposers.HasResourceId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.KeyExists(SlcPeople_OrganizationsIds.Sections.Resource.LinkedResource.Id.ToString()), comparer, (bool)value),
			[PersonExposers.TeamMemberships.TeamId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379), comparer, (Guid)value),
			[PersonExposers.TeamMemberships.RoleId.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.TeamRole), comparer, (Guid)value),
		};

		private readonly Dictionary<string, Func<SortOrder, bool, IOrderByElement>> orderByHandlers = new Dictionary<string, Func<SortOrder, bool, IOrderByElement>>
		{
			[PersonExposers.Id.fieldName] = HandleGuid,
			[PersonExposers.Name.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName), sortOrder, naturalSort),
			[PersonExposers.Email.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Email), sortOrder, naturalSort),
			[PersonExposers.Phone.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Phone), sortOrder, naturalSort),
			[PersonExposers.StreetAddress.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.StreetAddress), sortOrder, naturalSort),
			[PersonExposers.City.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.City), sortOrder, naturalSort),
			[PersonExposers.Country.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.Country), sortOrder, naturalSort),
			[PersonExposers.ZipCode.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ContactInfo.ZIP), sortOrder, naturalSort),
			[PersonExposers.ExperienceId.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.ExperienceLevel), sortOrder, naturalSort),
			[PersonExposers.OrganizationId.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Organization.Organization_57695f03), sortOrder, naturalSort),
			[PersonExposers.State.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.StatusId, sortOrder, naturalSort),
			[PersonExposers.ResourceId.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Resource.LinkedResource), sortOrder, naturalSort),
			[PersonExposers.TeamMemberships.TeamId.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379), sortOrder, naturalSort),
			[PersonExposers.TeamMemberships.RoleId.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Team.TeamRole), sortOrder, naturalSort),
		};

		protected override Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> FilterHandlers => filterHandlers;

		protected override Dictionary<string, Func<SortOrder, bool, IOrderByElement>> OrderByHandlers => orderByHandlers;

		protected override FilterElement<DomInstance> DomDefinitionFilter => personDomDefinitionFilter;

		private static string ConvertPersonState(PersonState filterValue)
		{
			switch (filterValue)
			{
				case PersonState.Draft:
					return SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Draft;
				case PersonState.Active:
					return SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Active;
				case PersonState.Deprecated:
					return SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Deprecated;
				default:
					throw new InvalidOperationException($"Unsupported person state: {filterValue}");
			}
		}

		private static int ConvertCountry(Country filterValue)
		{
			return (int)filterValue.MapEnum<Country, SlcPeople_OrganizationsIds.Enums.Country>();
		}
	}
}
