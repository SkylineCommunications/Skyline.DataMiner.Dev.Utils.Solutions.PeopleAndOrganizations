namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations
{
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	using Behavior = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior;
	using Ids = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds;
	using Statuses = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses;

	internal static class PeopleBehaviorDefinition
	{
		internal static DomBehaviorDefinition Create()
		{
			var definition = new DomBehaviorDefinition
			{
				ID = Behavior.Id,
				Name = "People_Behavior",
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
			const string script = "People and Organizations - Behavior - People Behavior";
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Confirm, "Confirm", "Activate", Statuses.Draft, Statuses.Edit);
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Archive, "Archive", "Deprecate", Statuses.Active);
			PeopleOrganizationsBehaviorTools.AddActionAndButton(definition, script, Behavior.Actions.Edit, "Edit", "Edit", Statuses.Active);
			foreach (var status in definition.Statuses)
			{
				AddPeopleContactAndMembership(definition, status.Id);
			}

			foreach (var status in definition.Statuses)
			{
				definition.StatusSectionDefinitionLinks.Add(new(new(status.Id, Ids.Sections.Resource.Id))
				{
					AllowMultipleSections = false,
					IsSoftDeleted = false,
					FieldDescriptorLinks = new()
					{
						new(Ids.Sections.Resource.LinkedResource) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = false },
					},
				});
			}

			return definition;
		}

		private static void AddPeopleContactAndMembership(DomBehaviorDefinition definition, string statusId)
		{
			var editable = statusId is Statuses.Draft or Statuses.Edit;
			var deprecated = statusId == Statuses.Deprecated;
			var active = statusId == Statuses.Active;
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.PeopleInformation.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.PeopleInformation.FullName) { Visible = true, RequiredForStatus = editable, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.PeopleInformation.PersonalSkills) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = !deprecated },
					new(Ids.Sections.PeopleInformation.ExperienceLevel) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.PeopleInformation.ProfileImage) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = true },
				},
			});
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.ContactInfo.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.ContactInfo.Email) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.ContactInfo.Phone) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.ContactInfo.StreetAddress) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.ContactInfo.City) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.ContactInfo.ZIP) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
					new(Ids.Sections.ContactInfo.Country) { Visible = true, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = active },
				},
			});
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.Organization.Id))
			{
				AllowMultipleSections = false,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.Organization.Organization_57695f03) { Visible = statusId == Statuses.Draft, RequiredForStatus = false, ReadOnly = deprecated, ClientReadOnly = false },
				},
			});
			definition.StatusSectionDefinitionLinks.Add(new(new(statusId, Ids.Sections.Team.Id))
			{
				AllowMultipleSections = true,
				IsSoftDeleted = false,
				FieldDescriptorLinks = new()
				{
					new(Ids.Sections.Team.Team_144d3379) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = false },
					new(Ids.Sections.Team.TeamRole) { Visible = false, RequiredForStatus = false, ReadOnly = false, ClientReadOnly = false },
				},
			});
		}
	}
}
