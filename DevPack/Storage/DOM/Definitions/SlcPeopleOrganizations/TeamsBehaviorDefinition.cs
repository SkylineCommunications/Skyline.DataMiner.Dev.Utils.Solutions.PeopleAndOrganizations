namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	using Behavior = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior;
	using Ids = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds;
	using Statuses = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses;

	internal static class TeamsBehaviorDefinition
	{
		internal static DomBehaviorDefinition Create()
		{
			var definition = new DomBehaviorDefinition
			{
				ID = Behavior.Id,
				Name = "Team_Behavior",
				InitialStatusId = Statuses.Draft,
				Statuses = { new(Statuses.Draft, "Draft"), new(Statuses.Active, "Active"), new(Statuses.Deprecated, "Deprecated"), new(Statuses.Edit, "Edit") },
				StatusTransitions =
				{
					new(Behavior.Transitions.Draft_To_Active, Statuses.Draft, Statuses.Active),
					new(Behavior.Transitions.Active_To_Deprecated, Statuses.Active, Statuses.Deprecated),
					new(Behavior.Transitions.Active_To_Edit, Statuses.Active, Statuses.Edit),
					new(Behavior.Transitions.Edit_To_Active, Statuses.Edit, Statuses.Active),
				},
			};
			const string script = "People and Organizations - Behavior - Team Behavior";
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
			var deprecated = statusId == Statuses.Deprecated;
			var active = statusId == Statuses.Active;
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.TeamInformation.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.TeamInformation.TeamName) { Visible = true, RequiredForStatus = !deprecated, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.TeamInformation.TeamEmail) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.TeamInformation.TeamDescription) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.TeamInformation.TeamSkills) { Visible = true, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = true },
					new(Ids.Sections.TeamInformation.Bookable) { Visible = deprecated, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = false },
					new(Ids.Sections.TeamInformation.Icon) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = true },
				},
			});
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.ResourcePool.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.ResourcePool.LinkedResourcePool) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = false },
				},
			});
		}
	}
}
