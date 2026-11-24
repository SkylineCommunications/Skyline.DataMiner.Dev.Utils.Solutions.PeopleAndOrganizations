namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Interfaces;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal sealed class RoleDefinition : IDomDefinitionInfo
	{
		public DomDefinition Definition { get; } = new DomDefinition("Role")
		{
			ID = SlcPeople_OrganizationsIds.Definitions.Role,
#pragma warning disable CS0612, CS0618 // Preserve the deployed schema's legacy visual metadata.
			VisualStructure = new() { IsHidden = false, SectionDefinitionInfos = new() },
#pragma warning restore CS0612, CS0618
			ModuleSettingsOverrides = new ModuleSettingsOverrides
			{
				NameDefinition = new DomInstanceNameDefinition { ConcatenationItems = { new FieldValueConcatenationItem(SlcPeople_OrganizationsIds.Sections.RoleInformation.Role) } },
			},
			SectionDefinitionLinks = { new SectionDefinitionLink(SlcPeople_OrganizationsIds.Sections.RoleInformation.Id) { IsOptional = false, AllowMultipleSections = false } },
		};

		public IEnumerable<CustomSectionDefinition> SectionDefinitions { get; } = new[] { GetRoleInformation() };

		private static CustomSectionDefinition GetRoleInformation()
		{
			var section = new CustomSectionDefinition { ID = SlcPeople_OrganizationsIds.Sections.RoleInformation.Id, Name = "Role information" };
			section.AddOrReplaceFieldDescriptor(new FieldDescriptor
			{
				ID = SlcPeople_OrganizationsIds.Sections.RoleInformation.Role, Name = "Role", FieldType = typeof(string), IsOptional = false,
				Tooltip = "The role a contact assumes when assigned to a team.",
			});
			return section;
		}
	}
}
