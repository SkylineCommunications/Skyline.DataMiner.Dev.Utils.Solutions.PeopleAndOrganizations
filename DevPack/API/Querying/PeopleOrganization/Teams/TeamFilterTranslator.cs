namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.Querying;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal class TeamFilterTranslator : DomInstanceFilterTranslator<Team>
	{
		private readonly FilterElement<DomInstance> roleDomDefinitionFilter = DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Teams.Id);
		private readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> handlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			[TeamExposers.Id.fieldName] = HandleGuid,
			[TeamExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName), comparer, (string)value),
			[TeamExposers.Email.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamEmail), comparer, (string)value),
			[TeamExposers.Description.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamDescription), comparer, (string)value),
			[TeamExposers.IsBookable.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.Bookable), comparer, (bool)value),
			[TeamExposers.State.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.StatusId, comparer, ConvertTeamState((TeamState)value)),
		};

		protected override Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> Handlers => handlers;

		protected override FilterElement<DomInstance> DomDefinitionFilter => roleDomDefinitionFilter;

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
