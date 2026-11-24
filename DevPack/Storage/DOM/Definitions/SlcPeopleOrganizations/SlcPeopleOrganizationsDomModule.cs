namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Interfaces;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal sealed class SlcPeopleOrganizationsDomModule
	{
		internal ModuleSettings ModuleSettings { get; } = new ModuleSettings(SlcPeople_OrganizationsIds.ModuleId);

		internal IEnumerable<IDomDefinitionInfo> Definitions { get; } = new IDomDefinitionInfo[]
		{
			new PeopleDefinition(),
			new TeamsDefinition(),
			new ExperienceDefinition(),
			new RoleDefinition(),
			new CategoryDefinition(),
			new OrganizationsDefinition(),
		};

		internal IEnumerable<CustomSectionDefinition> AdditionalSectionDefinitions { get; } = new[] { BillingSectionDefinition.Create() };

		internal IEnumerable<DomBehaviorDefinition> BehaviorDefinitions { get; } = new[] { PeopleBehaviorDefinition.Create(), OrganizationsBehaviorDefinition.Create(), TeamsBehaviorDefinition.Create() };
	}
}
