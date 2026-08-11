namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.UnitTesting.Simulation
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	using Behaviors = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors;
	using Definitions = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions;
	using Sections = Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections;

	/// <summary>
	/// Provides a pre-configured <see cref="SimulatedDms"/> that mirrors the installed state a real
	/// DataMiner Agent has when the People and Organizations solution is deployed (required elements
	/// and DOM module definitions, behaviors and initial statuses).
	/// </summary>
	public static class PeopleAndOrganizationsSimulation
	{
		/// <summary>
		/// The name of the lock manager element the People and Organizations solution communicates with.
		/// </summary>
		public const string LockManagerElementName = "PNO Lock Manager";

		/// <summary>
		/// The name of the lock manager element the MediaOps Plan solution communicates with.
		/// </summary>
		public const string PlanLockManagerElementName = "MOP Lock Manager";

		/// <summary>
		/// Creates a <see cref="SimulatedDms"/> configured for the People and Organizations solution.
		/// </summary>
		/// <returns>The configured <see cref="SimulatedDms"/>.</returns>
		public static SimulatedDms Create()
		{
			var dms = new SimulatedDms();

			dms.AddElement(LockManagerElementName);
			dms.AddElement(PlanLockManagerElementName, elementId: 2);

			RegisterPeopleOrganizationsModule(dms);
			ResourceStudioModule.Register(dms);

			return dms;
		}

		private static void RegisterPeopleOrganizationsModule(SimulatedDms dms)
		{
			var peopleBehavior = BuildStateBehavior(
				Behaviors.People_Behavior.Id,
				Behaviors.People_Behavior.Statuses.Draft,
				Behaviors.People_Behavior.Statuses.Active,
				Behaviors.People_Behavior.Statuses.Deprecated,
				Behaviors.People_Behavior.Statuses.Edit,
				Behaviors.People_Behavior.Transitions.Draft_To_Active,
				Behaviors.People_Behavior.Transitions.Active_To_Deprecated,
				Behaviors.People_Behavior.Transitions.Active_To_Edit,
				Behaviors.People_Behavior.Transitions.Edit_To_Active);

			var teamBehavior = BuildStateBehavior(
				Behaviors.Team_Behavior.Id,
				Behaviors.Team_Behavior.Statuses.Draft,
				Behaviors.Team_Behavior.Statuses.Active,
				Behaviors.Team_Behavior.Statuses.Deprecated,
				Behaviors.Team_Behavior.Statuses.Edit,
				Behaviors.Team_Behavior.Transitions.Draft_To_Active,
				Behaviors.Team_Behavior.Transitions.Active_To_Deprecated,
				Behaviors.Team_Behavior.Transitions.Active_To_Edit,
				Behaviors.Team_Behavior.Transitions.Edit_To_Active);

			var organizationBehavior = BuildStateBehavior(
				Behaviors.Organizations_Behavior.Id,
				Behaviors.Organizations_Behavior.Statuses.Draft,
				Behaviors.Organizations_Behavior.Statuses.Active,
				Behaviors.Organizations_Behavior.Statuses.Deprecated,
				Behaviors.Organizations_Behavior.Statuses.Edit,
				Behaviors.Organizations_Behavior.Transitions.Draft_To_Active,
				Behaviors.Organizations_Behavior.Transitions.Active_To_Deprecated,
				Behaviors.Organizations_Behavior.Transitions.Active_To_Edit,
				Behaviors.Organizations_Behavior.Transitions.Edit_To_Active);

			var definitions = new List<DomDefinition>
			{
				BuildDefinition(Definitions.People, Behaviors.People_Behavior.Id, Sections.PeopleInformation.FullName),
				BuildDefinition(Definitions.Teams, Behaviors.Team_Behavior.Id, Sections.TeamInformation.TeamName),
				BuildDefinition(Definitions.Organizations, Behaviors.Organizations_Behavior.Id, Sections.OrganizationInformation.OrganizationName),
				BuildDefinition(Definitions.Role, null, Sections.RoleInformation.Role),
				BuildDefinition(Definitions.Category, null, Sections.CategoryInformation.Category),
				BuildDefinition(Definitions.Experience, null, Sections.ExperienceInformation.Experience),
			};

			dms.RegisterDomModule(
				SlcPeople_OrganizationsIds.ModuleId,
				definitions,
				new[] { peopleBehavior, teamBehavior, organizationBehavior });
		}

		/// <summary>
		/// Builds the behavior shared by the People, Teams and Organizations definitions, which all use
		/// the same Draft / Active / Deprecated / Edit lifecycle.
		/// </summary>
		private static DomBehaviorDefinition BuildStateBehavior(
			DomBehaviorDefinitionId behaviorDefinitionId,
			string draftStatusId,
			string activeStatusId,
			string deprecatedStatusId,
			string editStatusId,
			string draftToActiveId,
			string activeToDeprecatedId,
			string activeToEditId,
			string editToActiveId)
		{
			return new DomBehaviorDefinition
			{
				ID = behaviorDefinitionId,
				InitialStatusId = draftStatusId,
				StatusTransitions = new List<DomStatusTransition>
				{
					new DomStatusTransition(draftToActiveId, draftStatusId, activeStatusId),
					new DomStatusTransition(activeToDeprecatedId, activeStatusId, deprecatedStatusId),
					new DomStatusTransition(activeToEditId, activeStatusId, editStatusId),
					new DomStatusTransition(editToActiveId, editStatusId, activeStatusId),
				},
			};
		}

		private static DomDefinition BuildDefinition(DomDefinitionId definitionId, DomBehaviorDefinitionId behaviorDefinitionId, FieldDescriptorID nameFieldId)
		{
			var definition = new DomDefinition
			{
				ID = definitionId,
				DomBehaviorDefinitionId = behaviorDefinitionId,
			};

			if (nameFieldId != null)
			{
				definition.ModuleSettingsOverrides = new ModuleSettingsOverrides
				{
					NameDefinition = new DomInstanceNameDefinition
					{
						ConcatenationItems = new List<IDomInstanceConcatenationItem>
						{
							new FieldValueConcatenationItem(nameFieldId),
						},
					},
				};
			}

			return definition;
		}
	}
}
