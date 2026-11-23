namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Tools
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations;

	internal static class DomModuleInstaller
	{
		internal static void Install(Func<DMSMessage[], DMSMessage[]> messageHandler, Action<string> logAction)
		{
			if (messageHandler == null)
			{
				throw new ArgumentNullException(nameof(messageHandler));
			}

			var module = new SlcPeopleOrganizationsDomModule();
			var moduleId = module.ModuleSettings.ModuleId;
			var sections = GetSectionDefinitions(module.Definitions.SelectMany(x => x.SectionDefinitions).Concat(module.AdditionalSectionDefinitions));

			var settingsHelper = new ModuleSettingsHelper(messageHandler);
			var helper = new DomHelper(messageHandler, moduleId);
			CreateOrUpdateModuleSettings(settingsHelper, module.ModuleSettings, logAction);

			// Install sections before the behaviors and definitions that reference them.
			foreach (var section in sections)
			{
				CreateOrUpdateSectionDefinition(helper, section, logAction);
			}

			foreach (var behavior in module.BehaviorDefinitions)
			{
				CreateOrUpdateBehaviorDefinition(helper, behavior, logAction);
			}

			foreach (var definition in module.Definitions)
			{
				CreateOrUpdateDomDefinition(helper, definition.Definition, logAction);
			}

			ReinitializeDomManager(messageHandler, moduleId, logAction);
		}

		internal static CustomSectionDefinition[] GetSectionDefinitions(IEnumerable<CustomSectionDefinition> declarations)
		{
			var sections = new List<CustomSectionDefinition>();
			var sectionsById = new Dictionary<SectionDefinitionID, CustomSectionDefinition>();
			foreach (var section in declarations)
			{
				if (sectionsById.TryGetValue(section.GetID(), out var existing))
				{
					if (!section.Equals(existing))
					{
						throw new ArgumentException($"Conflicting declarations for section '{section.GetID()}'.", nameof(declarations));
					}

					continue;
				}

				sectionsById.Add(section.GetID(), section);
				sections.Add(section);
			}

			return sections.ToArray();
		}

		private static void CreateOrUpdateModuleSettings(ModuleSettingsHelper helper, ModuleSettings settings, Action<string> logAction)
		{
			var existing = helper.ModuleSettings.Read(ModuleSettingsExposers.ModuleId.Equal(settings.ModuleId)).SingleOrDefault();
			if (existing == null)
			{
				Log(logAction, "Creating", settings);
				helper.ModuleSettings.Create(settings);
			}
			else if (settings.Equals(existing))
			{
				Log(logAction, "Skipping", settings);
			}
			else
			{
				Log(logAction, "Updating", settings);
				helper.ModuleSettings.Update(settings);
			}
		}

		private static void CreateOrUpdateSectionDefinition(DomHelper helper, CustomSectionDefinition definition, Action<string> logAction)
		{
			var existing = helper.SectionDefinitions.Read(SectionDefinitionExposers.ID.Equal(definition.GetID())).SingleOrDefault();
			if (existing == null)
			{
				Log(logAction, "Creating", definition);
				helper.SectionDefinitions.Create(definition);
			}
			// The typed custom-section overload compares more than the base overload.
			else if (existing is CustomSectionDefinition customExisting ? definition.Equals(customExisting) : ((SectionDefinition)definition).Equals(existing))
			{
				Log(logAction, "Skipping", definition);
			}
			else
			{
				Log(logAction, "Updating", definition);
				var incomingIds = new HashSet<FieldDescriptorID>(definition.GetAllFieldDescriptors().Select(x => x.ID));
				foreach (var field in existing.GetAllFieldDescriptors().Where(x => !incomingIds.Contains(x.ID)))
				{
					field.IsSoftDeleted = true;
					definition.AddOrReplaceFieldDescriptor(field);
				}

				helper.SectionDefinitions.Update(definition);
			}
		}

		private static void CreateOrUpdateBehaviorDefinition(DomHelper helper, DomBehaviorDefinition definition, Action<string> logAction)
		{
			var existing = helper.DomBehaviorDefinitions.Read(DomBehaviorDefinitionExposers.Id.Equal(definition.ID)).SingleOrDefault();
			if (existing == null)
			{
				Log(logAction, "Creating", definition);
				helper.DomBehaviorDefinitions.Create(definition);
			}
			else if (definition.Equals(existing))
			{
				Log(logAction, "Skipping", definition);
			}
			else
			{
				Log(logAction, "Updating", definition);
				helper.DomBehaviorDefinitions.Update(definition);
			}
		}

		private static void CreateOrUpdateDomDefinition(DomHelper helper, DomDefinition definition, Action<string> logAction)
		{
			var existing = helper.DomDefinitions.Read(DomDefinitionExposers.Id.Equal(definition.ID)).SingleOrDefault();
			if (existing == null)
			{
				Log(logAction, "Creating", definition);
				helper.DomDefinitions.Create(definition);
			}
			else if (definition.Equals(existing))
			{
				Log(logAction, "Skipping", definition);
			}
			else
			{
				Log(logAction, "Updating", definition);
				var incomingIds = new HashSet<SectionDefinitionID>(definition.SectionDefinitionLinks.Select(x => x.SectionDefinitionID));
				foreach (var link in existing.SectionDefinitionLinks.Where(x => !incomingIds.Contains(x.SectionDefinitionID)))
				{
					link.IsSoftDeleted = true;
					definition.SectionDefinitionLinks.Add(link);
				}

				helper.DomDefinitions.Update(definition);
			}
		}

		private static void ReinitializeDomManager(Func<DMSMessage[], DMSMessage[]> messageHandler, string moduleId, Action<string> logAction)
		{
			logAction?.Invoke($"Reinitializing DOM Manager for module '{moduleId}'.");
			var responses = messageHandler(new DMSMessage[] { new ManagerStoreReinitializeCustomManagerRequest(moduleId, "DomManager") });
			var response = responses?.OfType<ManagerStoreReinitializeCustomManagerResponse>().SingleOrDefault();
			if (response?.TraceData == null || !response.TraceData.HasSucceeded())
			{
				throw new InvalidOperationException($"Failed to reinitialize DOM Manager for module '{moduleId}'. Trace: {response?.TraceData?.ToString() ?? "<NULL>"}");
			}
		}

		private static void Log(Action<string> logAction, string action, DataType dataType)
		{
			string name;
			switch (dataType)
			{
				case ModuleSettings settings:
					name = settings.ModuleId;
					break;
				case CustomSectionDefinition section:
					name = section.Name;
					break;
				case DomBehaviorDefinition behavior:
					name = behavior.Name;
					break;
				case DomDefinition definition:
					name = definition.Name;
					break;
				default:
					name = String.Empty;
					break;
			}

			logAction?.Invoke($"{action} {dataType.GetType().Name}: {name} [{dataType.DataTypeID}].");
		}
	}
}
