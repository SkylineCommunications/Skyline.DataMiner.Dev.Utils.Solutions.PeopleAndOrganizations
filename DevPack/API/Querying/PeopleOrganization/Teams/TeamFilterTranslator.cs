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

	internal class TeamFilterTranslator : DomInstanceFilterTranslator<Team>
	{
		private readonly FilterElement<DomInstance> teamDomInstanceFilter =
			DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Teams.Id)
			.AND(DomInstanceExposers.StatusId.NotEqual(SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Edit));
		private readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> filterHandlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			[TeamExposers.Id.fieldName] = HandleGuid,
			[TeamExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName), comparer, (string)value),
			[TeamExposers.Email.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamEmail), comparer, (string)value),
			[TeamExposers.Description.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamDescription), comparer, (string)value),
			[TeamExposers.IsBookable.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.Bookable), comparer, (bool)value),
			[TeamExposers.State.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.StatusId, comparer, ConvertTeamState((TeamState)value)),
		};

		private readonly Dictionary<string, Func<SortOrder, bool, IOrderByElement>> orderByHandlers = new Dictionary<string, Func<SortOrder, bool, IOrderByElement>>
		{
			[TeamExposers.Id.fieldName] = HandleGuid,
			[TeamExposers.Name.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName), sortOrder, naturalSort),
			[TeamExposers.Email.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamEmail), sortOrder, naturalSort),
			[TeamExposers.Description.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamDescription), sortOrder, naturalSort),
			[TeamExposers.IsBookable.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.Bookable), sortOrder, naturalSort),
			[TeamExposers.State.fieldName] = (sortOrder, naturalSort) => OrderByElementFactory.Create(DomInstanceExposers.StatusId, sortOrder, naturalSort),
		};

		protected override Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> FilterHandlers => filterHandlers;

		protected override Dictionary<string, Func<SortOrder, bool, IOrderByElement>> OrderByHandlers => orderByHandlers;

		protected override FilterElement<DomInstance> DomDefinitionFilter => teamDomInstanceFilter;

		private static string ConvertTeamState(TeamState filterValue)
		{
			switch (filterValue)
			{
				case TeamState.Draft:
					return SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Draft;
				case TeamState.Active:
					return SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Active;
				case TeamState.Deprecated:
					return SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Deprecated;
				default:
					throw new InvalidOperationException($"Unsupported team state: {filterValue}");
			}
		}
	}
}
