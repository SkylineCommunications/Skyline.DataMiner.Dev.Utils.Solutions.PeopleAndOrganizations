namespace RT_PeopleAndOrganizations.Storage.DOM
{
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Tools;

	[TestClass]
	[TestCategory("UnitTest")]
	public sealed class DomSchemaParityTests
	{
		// Unchanged (slc)people_organizations hashes from Plan PR #416 (Collaboration excluded).
		// Hashes cover complete schema objects, including nested fields and status rules.
		[TestMethod]
		[DataRow("ModuleSettings", "(slc)people_organizations", "68ef510671667d63fe323639e8b3d1248f323760d00471d8ea08aae061b36497")]
		[DataRow("DomBehaviorDefinitions", "1636e21e-f44d-4343-8a09-c09780399c74", "2f6ca67bb82700092044626ab4681a71ec5ffa7cf127a73c6534219b84e9f9f5")]
		[DataRow("DomBehaviorDefinitions", "80136b8f-4c76-4b7c-a1dc-526d3a9bdf5a", "9de2343e473644b0be011ddddc69cec278e968b360f7a9911080aa8b40540187")]
		[DataRow("DomBehaviorDefinitions", "a7b03edc-0a89-4f02-9fa3-5458cd11d43e", "bbc9f4eacc2707036339b0ec980ba1f0f467581fe860ddd59f1a91f2abf4ecb4")]
		[DataRow("DomDefinitions", "1624a01d-de2e-4c36-849c-3ee048a971ee", "cda06f3f4e684940f272346a318468f9fe8fd0cdff0d9d8b8770b65c8bca738f")]
		[DataRow("DomDefinitions", "5150bc87-7e63-4b41-a839-64da947a1986", "e82788edd55abea3c465bd5047a5bd5a416be619a3c2cd4f02f473c6feced0a9")]
		[DataRow("DomDefinitions", "6474a13e-3de9-42f8-b3d9-aa49a1fcab7d", "2c712a8cbe717e46d608b3b04a95ab10afd17b0e3251cf982ea54b9e5ff50a0f")]
		[DataRow("DomDefinitions", "86b3dff8-764e-4378-9d19-de4bc8de05c2", "0585435505b149d9af5ed4f2c6659538662ed1a26f2cd09625dc03a0c2a7ca7d")]
		[DataRow("DomDefinitions", "bc1fe15f-a85d-4601-ab26-0329a9acf61a", "045abfbfc2b42d02c810e242c8528a0ee10f48fb5dba576bb0fa780aead1c577")]
		[DataRow("DomDefinitions", "c769a960-e401-4cb6-8099-b0563b270c52", "056320910372ffddbebf85e40b33b6a0d61d1fc6a1448b64c0318dcda2d2eb29")]
		[DataRow("SectionDefinitions", "03ba0fd8-114a-4f97-b392-7ac8a1f76803", "230f145f777ddffd968c3bd6a87a740438cf03b66c47789104fb5dd2a7922828")]
		[DataRow("SectionDefinitions", "0c47b6fe-c60f-4378-a85c-3f98dc431828", "84eae9ca8df5a9aeabbd850122bbdf100e44e7209fee71fe1ad259bd2cc778c3")]
		[DataRow("SectionDefinitions", "141a95ca-4b5b-4086-b329-209ffe8f1af8", "1ceb73e98c51dcdd9dc65da057c0be72a8ed485aafeceef8fca0b0664f0b72f2")]
		[DataRow("SectionDefinitions", "21ffec53-130a-4a66-9eb3-50194d2c1e43", "d40b9fb259f2faca2cd150006f055e17763dae3c8d905203d43f4d02abfb8e8a")]
		[DataRow("SectionDefinitions", "30aef0e9-658a-436a-ad71-6a48d88faa0b", "62901b08cfc6642227512078bfa4c1c05248c722677ae141043a9f4752a26d5a")]
		[DataRow("SectionDefinitions", "397134f8-19eb-4eec-9021-8b1af51d0c1b", "3d74d0fda4902bb93673cf9b5341e0cb641b8cf46cb0f7505f9bb2144e06cca6")]
		[DataRow("SectionDefinitions", "5c4be963-44ea-4ae2-911f-164f92534cb1", "c165818335374165ff2daf2fb11704fd5024e3eb203b966b83bb620a449edcf8")]
		[DataRow("SectionDefinitions", "8341db0f-59bf-4a71-a124-67f8c5fbbb06", "7d1db540c3fa5677db5ee6d6cfeef7a5fcbdb8e4abf287c544c1de9a64480d1e")]
		[DataRow("SectionDefinitions", "b4abf511-028c-42c1-b0d0-dc750043f1d2", "e906bde8086fb45e702c922e2c5fe2fe23774b3d72f4187cbefac8537a9274e7")]
		[DataRow("SectionDefinitions", "ceadbd12-73f7-474e-9d1f-effbc9d81684", "e96031ba3d6669af0268afea666ce66c6638ea0603692e0e4ed666fb787ab4cc")]
		[DataRow("SectionDefinitions", "e47d729d-1658-41bc-8931-88f4c055b955", "9d7967ef9c416b22ba52672d98644a4ffa0407d2754e7b95f45a052b2c629179")]
		[DataRow("SectionDefinitions", "f38e6d21-e481-4348-b639-445a510592ec", "f1f2348bf1a5155e78ac71341754e41a081caa98c16f1558ff362642f9758f26")]
		[DataRow("SectionDefinitions", "fcf83fa0-0d38-4fe6-ad55-4d36e4457f91", "db08af06460d38681791bd7ff9286dd15a97a354b271d1b57112d680b9a0dd7a")]
		public void Declaration_MatchesApprovedSchema(string kind, string id, string expectedHash)
		{
			var module = new SlcPeopleOrganizationsDomModule();
			var json = kind switch
			{
				"ModuleSettings" => module.ModuleSettings.ToJson(),
				"DomDefinitions" => module.Definitions.Single(x => x.Definition.ID.Id == Guid.Parse(id)).Definition.ToJson(),
				"DomBehaviorDefinitions" => module.BehaviorDefinitions.Single(x => x.ID.Id == Guid.Parse(id)).ToJson(),
				"SectionDefinitions" => module.Definitions.SelectMany(x => x.SectionDefinitions).Concat(module.AdditionalSectionDefinitions)
					.First(x => x.GetID().Id == Guid.Parse(id)).ToJson(),
				_ => throw new ArgumentException("Unknown schema object kind.", nameof(kind)),
			};

			Assert.AreEqual(expectedHash, DomSchemaSnapshot.Hash(json), $"{kind}/{id}: {DomSchemaSnapshot.Canonicalize(json)}");
		}

		[TestMethod]
		public void Declaration_UsesTheSameIdsAsTheGeneratedWrappers()
		{
			var module = new SlcPeopleOrganizationsDomModule();
			Assert.AreEqual(SlcPeople_OrganizationsIds.ModuleId, module.ModuleSettings.ModuleId);
			CollectionAssert.AreEquivalent(
				typeof(SlcPeople_OrganizationsIds.Definitions).GetProperties().Select(x => (DomDefinitionId)x.GetValue(null)!).ToArray(),
				module.Definitions.Select(x => x.Definition.ID).ToArray());

			var sections = DomModuleInstaller.GetSectionDefinitions(module.Definitions.SelectMany(x => x.SectionDefinitions).Concat(module.AdditionalSectionDefinitions));
			var wrapperSections = typeof(SlcPeople_OrganizationsIds.Sections).GetNestedTypes()
				.Where(x => x != typeof(SlcPeople_OrganizationsIds.Sections.Collaboration)).ToArray();
			Assert.AreEqual(wrapperSections.Length, sections.Length);
			foreach (var wrapper in wrapperSections)
			{
				var id = (SectionDefinitionID)wrapper.GetProperty("Id")!.GetValue(null)!;
				var section = sections.Single(x => x.GetID().Equals(id));
				CollectionAssert.AreEquivalent(
					wrapper.GetProperties().Where(x => x.PropertyType == typeof(FieldDescriptorID)).Select(x => (FieldDescriptorID)x.GetValue(null)!).ToArray(),
					section.GetAllFieldDescriptors().Select(x => x.ID).ToArray(),
					wrapper.Name);
			}

			var wrapperBehaviors = typeof(SlcPeople_OrganizationsIds.Behaviors).GetNestedTypes();
			Assert.AreEqual(wrapperBehaviors.Length, module.BehaviorDefinitions.Count());
			foreach (var wrapper in wrapperBehaviors)
			{
				var id = (DomBehaviorDefinitionId)wrapper.GetProperty("Id")!.GetValue(null)!;
				var behavior = module.BehaviorDefinitions.Single(x => x.ID.Equals(id));
				CollectionAssert.AreEquivalent(
					wrapper.GetNestedType("Statuses")!.GetFields().Select(x => (string)x.GetRawConstantValue()!).ToArray(),
					behavior.Statuses.Select(x => x.Id).ToArray(),
					wrapper.Name);
			}
		}
	}
}
