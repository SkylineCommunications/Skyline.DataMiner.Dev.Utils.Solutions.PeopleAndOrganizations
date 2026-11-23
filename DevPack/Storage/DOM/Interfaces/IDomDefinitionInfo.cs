namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Interfaces
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;

	internal interface IDomDefinitionInfo
	{
		DomDefinition Definition { get; }

		IEnumerable<CustomSectionDefinition> SectionDefinitions { get; }
	}
}
