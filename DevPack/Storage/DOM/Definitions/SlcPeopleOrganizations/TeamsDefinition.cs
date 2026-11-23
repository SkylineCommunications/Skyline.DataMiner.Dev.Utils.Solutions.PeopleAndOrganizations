namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Interfaces;

	using Ids = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds;

	internal sealed class TeamsDefinition : IDomDefinitionInfo
	{
		public DomDefinition Definition { get; } = new DomDefinition("Teams")
		{
			ID = Ids.Definitions.Teams,
			DomBehaviorDefinitionId = Ids.Behaviors.Team_Behavior.Id,
#pragma warning disable CS0612, CS0618 // Preserve the deployed schema's legacy visual metadata.
			VisualStructure = new() { IsHidden = false, SectionDefinitionInfos = new() },
#pragma warning restore CS0612, CS0618
			ModuleSettingsOverrides = new ModuleSettingsOverrides
			{
				NameDefinition = new DomInstanceNameDefinition { ConcatenationItems = { new FieldValueConcatenationItem(Ids.Sections.TeamInformation.TeamName) } },
			},
			SectionDefinitionLinks =
			{
				new SectionDefinitionLink(Ids.Sections.TeamInformation.Id) { IsOptional = false, AllowMultipleSections = false },
				new SectionDefinitionLink(Ids.Sections.ResourcePool.Id) { IsOptional = false, AllowMultipleSections = false },
			},
		};

		public IEnumerable<CustomSectionDefinition> SectionDefinitions { get; } = new[] { GetTeamInformation(), GetResourcePool() };

		private static CustomSectionDefinition GetTeamInformation()
		{
			var section = new CustomSectionDefinition { ID = Ids.Sections.TeamInformation.Id, Name = "Team information" };
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.TeamName, Name = "Team name", FieldType = typeof(string), IsOptional = false, Tooltip = "This team's name.",
			});
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.TeamEmail, Name = "Team email", FieldType = typeof(string), IsOptional = true, Tooltip = "This team's email address.",
			});
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.TeamDescription, Name = "Team description", FieldType = typeof(string), IsOptional = true, Tooltip = "A description of the team's functions and responsibilities.",
			});
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.Bookable, Name = "Bookable", FieldType = typeof(bool), IsOptional = true,
				DefaultValue = new ValueWrapper<bool>(false), Tooltip = "Indicates whether this team's time can be booked and scheduled.",
			});
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.Icon, Name = "Icon", FieldType = typeof(string), IsOptional = true, Tooltip = String.Empty,
			});
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = Ids.Sections.TeamInformation.TeamSkills, Name = "Team skills", FieldType = typeof(string), IsOptional = true, Tooltip = "All members within the team will inherit these skills.",
			});
			return section;
		}

		private static CustomSectionDefinition GetResourcePool()
		{
			var section = new CustomSectionDefinition { ID = Ids.Sections.ResourcePool.Id, Name = "Resource pool" };
			section.AddOrReplaceFieldDescriptor(new DomInstanceFieldDescriptor("(slc)resource_studio")
			{
				ID = Ids.Sections.ResourcePool.LinkedResourcePool, Name = "Linked Resource Pool", FieldType = typeof(Guid), IsOptional = true, Tooltip = String.Empty,
				DomDefinitionIds = { new DomDefinitionId(new Guid("a8262bd8-6f6c-47d4-964e-87de26d1e32e")) { ModuleId = "(slc)resource_studio" } },
			});
			return section;
		}
	}
}
