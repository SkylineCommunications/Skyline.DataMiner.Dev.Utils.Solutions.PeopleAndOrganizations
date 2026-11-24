namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	using Behavior = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior;
	using Ids = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds;
	using Statuses = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Statuses;

	internal static class OrganizationsBehaviorDefinition
	{
		internal static DomBehaviorDefinition Create()
		{
			var definition = new DomBehaviorDefinition
			{
				ID = Behavior.Id,
				Name = "Organizations_Behavior",
				InitialStatusId = Statuses.Draft,
				Statuses = { new(Statuses.Draft, "Draft"), new(Statuses.Active, "Active"), new(Statuses.Deprecated, "Deprecated"), new(Statuses.Edit, "Edit") },
				StatusTransitions =
				{
					new(Behavior.Transitions.Draft_To_Active, Statuses.Draft, Statuses.Active),
					new(Behavior.Transitions.Active_To_Deprecated, Statuses.Active, Statuses.Deprecated),
					new(Behavior.Transitions.Active_To_Edit, Statuses.Active, Statuses.Edit),
					new(Behavior.Transitions.Edit_To_Active, Statuses.Edit, Statuses.Active) { FlowLevel = 1 },
				},
			};
			const string script = "People and Organizations - Behavior - Organization Behavior";
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Confirm, "Confirm", "Activate", Statuses.Draft, Statuses.Edit);
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Edit, "Edit", "Edit", Statuses.Active);
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Archive, "Archive", "Deprecate", Statuses.Active);
			foreach (var status in definition.Statuses)
			{
				AddStatusSections(definition, status.Id);
			}

			return definition;
		}

		private static void AddStatusSections(DomBehaviorDefinition definition, string statusId)
		{
			var editable = statusId is Statuses.Draft or Statuses.Edit;
			var deprecated = statusId == Statuses.Deprecated;
			var active = statusId == Statuses.Active;
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.OrganizationInformation.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.OrganizationInformation.OrganizationName) { Visible = true, RequiredForStatus = editable, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.OrganizationInformation.Category) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.OrganizationInformation.Icon) { Visible = false, RequiredForStatus = false, ReadOnly = statusId is Statuses.Draft or Statuses.Deprecated, ClientReadOnly = statusId is Statuses.Active or Statuses.Edit },
				},
			});
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.Contracts.Id))
			{
				AllowMultipleSections = true,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.Contracts.Contract) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = false },
				},
			});
		}
	}
}
