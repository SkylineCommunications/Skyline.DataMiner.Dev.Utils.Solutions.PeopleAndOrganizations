namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.UnitTesting.Simulation
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
	using Skyline.DataMiner.Net.Sections;

	/// <summary>
	/// Registers the Resource Studio DOM module of the MediaOps Plan solution on a
	/// <see cref="SimulatedDms"/>. The People and Organizations API links teams and people to
	/// MediaOps Plan resource pools and resources, so those definitions must be installed for the
	/// simulation to behave like a real DataMiner Agent.
	/// </summary>
	/// <remarks>
	/// The identifiers below mirror the Resource Studio DOM module shipped with the MediaOps Plan
	/// solution. They are duplicated here because the generated identifiers of that solution are
	/// internal to the MediaOps Plan package.
	/// </remarks>
	internal static class ResourceStudioModule
	{
		private const string ModuleId = "(slc)resource_studio";

		private const string DraftStatusId = "draft";
		private const string CompleteStatusId = "complete";
		private const string DeprecatedStatusId = "deprecated";

		private const string DraftToCompleteId = "draft_to_complete";
		private const string CompleteToDeprecatedId = "complete_to_deprecated";
		private const string DeprecatedToCompleteId = "deprecated_to_complete";

		public static void Register(SimulatedDms dms)
		{
			if (dms is null)
			{
				throw new ArgumentNullException(nameof(dms));
			}

			var resourcePoolBehaviorId = new DomBehaviorDefinitionId(new Guid("cc539721-a544-415b-a45c-8ce7ed102975")) { ModuleId = ModuleId };
			var resourceBehaviorId = new DomBehaviorDefinitionId(new Guid("6bac6e39-e58d-43c0-b354-81119f5828dc")) { ModuleId = ModuleId };

			var resourcePoolBehavior = BuildBehavior(
				resourcePoolBehaviorId,
				DraftStatusId,
				new DomStatusTransition(DraftToCompleteId, DraftStatusId, CompleteStatusId),
				new DomStatusTransition(CompleteToDeprecatedId, CompleteStatusId, DeprecatedStatusId));

			var resourceBehavior = BuildBehavior(
				resourceBehaviorId,
				DraftStatusId,
				new DomStatusTransition(DraftToCompleteId, DraftStatusId, CompleteStatusId),
				new DomStatusTransition(CompleteToDeprecatedId, CompleteStatusId, DeprecatedStatusId),
				new DomStatusTransition(DeprecatedToCompleteId, DeprecatedStatusId, CompleteStatusId));

			var definitions = new List<DomDefinition>
			{
				BuildDefinition(
					new DomDefinitionId(new Guid("a8262bd8-6f6c-47d4-964e-87de26d1e32e")) { ModuleId = ModuleId },
					resourcePoolBehaviorId,
					new FieldDescriptorID(new Guid("2cebe78f-be32-455d-8daf-86f815aa3b82"))), // ResourcePoolInfo.Name
				BuildDefinition(
					new DomDefinitionId(new Guid("d2afc1dc-f39c-49c8-a70f-9120bfbfc0a0")) { ModuleId = ModuleId },
					resourceBehaviorId,
					new FieldDescriptorID(new Guid("13833c8f-6874-44e9-9aeb-9a9914e26771"))), // ResourceInfo.Name
			};

			dms.RegisterDomModule(ModuleId, definitions, new[] { resourcePoolBehavior, resourceBehavior });
		}

		private static DomDefinition BuildDefinition(DomDefinitionId definitionId, DomBehaviorDefinitionId behaviorDefinitionId, FieldDescriptorID nameFieldId)
		{
			return new DomDefinition
			{
				ID = definitionId,
				DomBehaviorDefinitionId = behaviorDefinitionId,
				ModuleSettingsOverrides = new ModuleSettingsOverrides
				{
					NameDefinition = new DomInstanceNameDefinition
					{
						ConcatenationItems = new List<IDomInstanceConcatenationItem>
						{
							new FieldValueConcatenationItem(nameFieldId),
						},
					},
				},
			};
		}

		private static DomBehaviorDefinition BuildBehavior(DomBehaviorDefinitionId behaviorDefinitionId, string initialStatusId, params DomStatusTransition[] transitions)
		{
			return new DomBehaviorDefinition
			{
				ID = behaviorDefinitionId,
				InitialStatusId = initialStatusId,
				StatusTransitions = new List<DomStatusTransition>(transitions),
			};
		}
	}
}
