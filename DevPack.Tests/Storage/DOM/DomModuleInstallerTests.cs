namespace RT_PeopleAndOrganizations.Storage.DOM
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResponseErrorData;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Tools;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.UnitTesting.Simulation;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	/// <summary>
	/// Tests schema installation without connecting to DataMiner or bootstrapping the regression fixtures.
	/// </summary>
	[TestClass]
	[TestCategory("UnitTest")]
	public sealed class DomModuleInstallerTests
	{
		private const string ModuleId = SlcPeople_OrganizationsIds.ModuleId;

		/// <summary>
		/// Verifies that a fresh install writes only the People and Organizations schema, and a rerun does not write it again.
		/// </summary>
		[TestMethod]
		public void Install_WritesOnlyPeopleAndOrganizationsSchema()
		{
			var messages = new List<DMSMessage>();
			var handleMessages = CreateStore(messages);

			DomModuleInstaller.Install(handleMessages, null);

			Assert.AreEqual(SlcPeople_OrganizationsIds.ModuleId, messages.OfType<ManagerStoreCreateRequest<ModuleSettings>>().Single().Object.ModuleId);
			Assert.AreEqual(13, messages.OfType<ManagerStoreCreateRequest<SectionDefinition>>().Count());
			Assert.AreEqual(34, messages.OfType<ManagerStoreCreateRequest<SectionDefinition>>().Sum(x => x.Object.GetAllFieldDescriptors().Count()));
			Assert.AreEqual(3, messages.OfType<ManagerStoreCreateRequest<DomBehaviorDefinition>>().Count());
			Assert.AreEqual(6, messages.OfType<ManagerStoreCreateRequest<DomDefinition>>().Count());
			var writes = messages.Where(x => x is ManagerStoreCreateRequest<ModuleSettings> or ManagerStoreCreateRequest<SectionDefinition>
				or ManagerStoreCreateRequest<DomBehaviorDefinition> or ManagerStoreCreateRequest<DomDefinition> or ManagerStoreReinitializeCustomManagerRequest);
			CollectionAssert.AreEqual(
				new[] { typeof(ManagerStoreCreateRequest<ModuleSettings>) }
					.Concat(Enumerable.Repeat(typeof(ManagerStoreCreateRequest<SectionDefinition>), 13))
					.Concat(Enumerable.Repeat(typeof(ManagerStoreCreateRequest<DomBehaviorDefinition>), 3))
					.Concat(Enumerable.Repeat(typeof(ManagerStoreCreateRequest<DomDefinition>), 6))
					.Append(typeof(ManagerStoreReinitializeCustomManagerRequest)).ToArray(),
				writes.Select(x => x.GetType()).ToArray());

			messages.Clear();
			var logs = new List<string>();
			DomModuleInstaller.Install(handleMessages, logs.Add);

			Assert.AreEqual(23, logs.Count(x => x.StartsWith("Skipping ", StringComparison.Ordinal)));
			Assert.AreEqual(24, logs.Count);
		}

		/// <summary>
		/// Verifies that upgrades match the original Plan installer's authoritative reconciliation.
		/// </summary>
		[TestMethod]
		public void Install_OverwritesSettingsAndBehavior_AndSoftDeletesUndeclaredFieldsAndLinks()
		{
			var handleMessages = CreateStore(new List<DMSMessage>());
			DomModuleInstaller.Install(handleMessages, null);
			var helper = new DomHelper(handleMessages, ModuleId);
			var settings = new ModuleSettingsHelper(handleMessages);
			var installedSettings = settings.ModuleSettings.ReadAll().Single();
			var expectedSettings = DomSchemaSnapshot.Canonicalize(installedSettings.ToJson()).ToString();
			installedSettings.DomManagerSettings.InformationEventSettings.Enable = !installedSettings.DomManagerSettings.InformationEventSettings.Enable;
			settings.ModuleSettings.Update(installedSettings);

			var collaborationId = SlcPeople_OrganizationsIds.Sections.Collaboration.Id;
			var collaborationFieldId = SlcPeople_OrganizationsIds.Sections.Collaboration.CreateMSTeamsChannel;
			var collaboration = new CustomSectionDefinition { ID = collaborationId, Name = "Collaboration" };
			collaboration.AddOrReplaceFieldDescriptor(new FieldDescriptor { ID = collaborationFieldId, Name = "Create Teams channel", FieldType = typeof(bool) });
			helper.SectionDefinitions.Create(collaboration);
			var previousCollaboration = DomSchemaSnapshot.Canonicalize(collaboration.ToJson()).ToString();

			var teamInfoId = SlcPeople_OrganizationsIds.Sections.TeamInformation.Id;
			var teamInfo = (CustomSectionDefinition)helper.SectionDefinitions.ReadAll().Single(x => x.GetID().Equals(teamInfoId));
			var removedFieldId = new FieldDescriptorID(Guid.NewGuid());
			teamInfo.AddOrReplaceFieldDescriptor(new FieldDescriptor { ID = removedFieldId, Name = "Legacy field", FieldType = typeof(string), Tooltip = "Keep this metadata." });
			helper.SectionDefinitions.Update(teamInfo);

			var team = helper.DomDefinitions.ReadAll().Single(x => x.Name == "Teams");
			team.Name = "Old teams definition";
			team.SectionDefinitionLinks.Add(new SectionDefinitionLink(collaborationId) { IsOptional = true });
			helper.DomDefinitions.Update(team);
			var behavior = helper.DomBehaviorDefinitions.ReadAll().Single(x => x.ID.Equals(team.DomBehaviorDefinitionId));
			var expectedBehavior = DomSchemaSnapshot.Canonicalize(behavior.ToJson()).ToString();
			behavior.Name = "Old team behavior";
			behavior.StatusSectionDefinitionLinks.Add(new DomStatusSectionDefinitionLink(new DomStatusSectionDefinitionLinkId(behavior.InitialStatusId, collaborationId))
			{
				FieldDescriptorLinks = { new DomStatusFieldDescriptorLink(collaborationFieldId) { Visible = true } },
			});
			behavior.StatusSectionDefinitionLinks.First(x => x.Id.SectionDefinitionId.Equals(teamInfoId))
				.FieldDescriptorLinks.Add(new DomStatusFieldDescriptorLink(removedFieldId) { Visible = true });
			helper.DomBehaviorDefinitions.Update(behavior);

			DomModuleInstaller.Install(handleMessages, null);
			DomModuleInstaller.Install(handleMessages, null); // Do not duplicate soft-deleted entries on rerun.

			var installedTeam = helper.DomDefinitions.ReadAll().Single(x => x.ID.Equals(team.ID));
			Assert.AreEqual("Teams", installedTeam.Name);
			var installedLink = installedTeam.SectionDefinitionLinks.Single(x => x.SectionDefinitionID.Equals(collaborationId));
			Assert.IsTrue(installedLink.IsSoftDeleted);
			Assert.IsTrue(installedLink.IsOptional);
			var sections = helper.SectionDefinitions.ReadAll();
			var field = sections.Single(x => x.GetID().Equals(teamInfoId)).GetAllFieldDescriptors().Single(x => x.ID.Equals(removedFieldId));
			Assert.IsTrue(field.IsSoftDeleted);
			Assert.AreEqual("Keep this metadata.", field.Tooltip);
			Assert.AreEqual(previousCollaboration, DomSchemaSnapshot.Canonicalize(sections.Single(x => x.GetID().Equals(collaborationId)).ToJson()).ToString());
			var installedBehavior = helper.DomBehaviorDefinitions.ReadAll().Single(x => x.ID.Equals(behavior.ID));
			Assert.AreEqual(expectedBehavior, DomSchemaSnapshot.Canonicalize(installedBehavior.ToJson()).ToString());
			Assert.AreEqual(expectedSettings, DomSchemaSnapshot.Canonicalize(settings.ModuleSettings.ReadAll().Single().ToJson()).ToString());
		}

		[TestMethod]
		public void Install_OnConfiguredSimulation_UsesInstalledSchemaForInitialStatusAndTransitions()
		{
			using var connection = PeopleAndOrganizationsSimulation.Create().CreateConnection();
			var helper = new DomHelper(connection.HandleMessages, ModuleId);
			Assert.AreEqual(13, helper.SectionDefinitions.ReadAll().Count);
			Assert.AreEqual(3, helper.DomBehaviorDefinitions.ReadAll().Count);
			Assert.AreEqual(6, helper.DomDefinitions.ReadAll().Count);

			connection.GetPeopleAndOrganizationsApi().InstallDomModules();
			var teamInfo = new Section(SlcPeople_OrganizationsIds.Sections.TeamInformation.Id);
			teamInfo.AddOrReplaceFieldValue(new FieldValue(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamName, new ValueWrapper<string>("Test team")));
			var team = helper.DomInstances.Create(new DomInstance
			{
				ID = new DomInstanceId(Guid.NewGuid()) { ModuleId = ModuleId },
				DomDefinitionId = SlcPeople_OrganizationsIds.Definitions.Teams,
				Sections = { teamInfo },
			});

			Assert.AreEqual("Test team", team.Name);
			Assert.AreEqual(SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Draft, team.StatusId);
			var activatedTeam = helper.DomInstances.DoStatusTransition(team.ID, SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Transitions.Draft_To_Active);
			Assert.AreEqual(SlcPeople_OrganizationsIds.Behaviors.Team_Behavior.Statuses.Active, activatedTeam.StatusId);
		}

		[TestMethod]
		public void Install_ThrowsAfterSchemaWritesIfReinitializationFails()
		{
			var handleMessages = CreateStore(new List<DMSMessage>());
			var trace = new TraceData();
			trace.Add(new ResourceManagerErrorData(ResourceManagerErrorData.Reason.ReservationUpdateCausedReservationsToGoToQuarantine, Guid.Empty));

			var exception = Assert.ThrowsException<InvalidOperationException>(() => DomModuleInstaller.Install(
				messages => messages.Single() is ManagerStoreReinitializeCustomManagerRequest
					? new DMSMessage[] { new ManagerStoreReinitializeCustomManagerResponse { TraceData = trace } }
					: handleMessages(messages),
				null));

			StringAssert.Contains(exception.Message, ModuleId);
			Assert.AreEqual(6, new DomHelper(handleMessages, ModuleId).DomDefinitions.ReadAll().Count);
		}

		[TestMethod]
		public void GetSectionDefinitions_RejectsConflictingDeclarationsForOneId()
		{
			var id = SlcPeople_OrganizationsIds.Sections.TeamInformation.Id;
			var first = new CustomSectionDefinition { ID = id, Name = "First" };
			var second = new CustomSectionDefinition { ID = id, Name = "Second" };

			Assert.ThrowsException<ArgumentException>(() => DomModuleInstaller.GetSectionDefinitions(new[] { first, second }));
		}

		private static Func<DMSMessage[], DMSMessage[]> CreateStore(List<DMSMessage> received)
		{
			var dom = new DomSLNetMessageHandler(validateAgainstDefinition: false);
			return messages => messages.Select(message =>
			{
				received.Add(message);
				return message is ManagerStoreReinitializeCustomManagerRequest
					? new ManagerStoreReinitializeCustomManagerResponse { TraceData = new TraceData() }
					: dom.HandleMessage(message);
			}).ToArray();
		}
	}
}
