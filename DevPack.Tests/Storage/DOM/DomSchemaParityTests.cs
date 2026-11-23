namespace RT_PeopleAndOrganizations.Storage.DOM
{
	using Newtonsoft.Json.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Definitions.SlcPeopleOrganizations;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.Tools;

	[TestClass]
	[TestCategory("UnitTest")]
	public sealed class DomSchemaParityTests
	{
		[TestMethod]
		public void Declaration_MatchesReleasedSchema()
		{
			using var stream = typeof(DomSchemaParityTests).Assembly.GetManifestResourceStream("RT_PeopleAndOrganizations.Storage.DOM.PeopleAndOrganizationsSchema.snapshot.json")
				?? throw new InvalidOperationException("The People and Organizations schema snapshot is missing.");
			using var reader = new StreamReader(stream);
			var expected = JObject.Parse(reader.ReadToEnd());

			var module = new SlcPeopleOrganizationsDomModule();
			var sections = DomModuleInstaller.GetSectionDefinitions(module.Definitions.SelectMany(x => x.SectionDefinitions).Concat(module.AdditionalSectionDefinitions));
			var actual = new JObject
			{
				["ModuleSettings"] = new JObject { [module.ModuleSettings.ModuleId] = DomSchemaSnapshot.Canonicalize(module.ModuleSettings.ToJson()) },
				["DomDefinitions"] = new JObject(module.Definitions.Select(x => new JProperty(x.Definition.ID.Id.ToString(), DomSchemaSnapshot.Canonicalize(x.Definition.ToJson())))),
				["DomBehaviorDefinitions"] = new JObject(module.BehaviorDefinitions.Select(x => new JProperty(x.ID.Id.ToString(), DomSchemaSnapshot.Canonicalize(x.ToJson())))),
				["SectionDefinitions"] = new JObject(sections.Select(x => new JProperty(x.GetID().Id.ToString(), DomSchemaSnapshot.Canonicalize(x.ToJson())))),
			};

			CollectionAssert.AreEquivalent(expected.Properties().Select(x => x.Name).ToArray(), actual.Properties().Select(x => x.Name).ToArray());
			foreach (var group in expected.Properties())
			{
				var expectedGroup = (JObject)group.Value;
				var actualGroup = (JObject)actual[group.Name]!;
				CollectionAssert.AreEquivalent(expectedGroup.Properties().Select(x => x.Name).ToArray(), actualGroup.Properties().Select(x => x.Name).ToArray(), group.Name);
				foreach (var expectedObject in expectedGroup.Properties())
				{
					Assert.AreEqual(expectedObject.Value.ToString(), actualGroup[expectedObject.Name]!.ToString(), $"{group.Name}/{expectedObject.Name}");
				}
			}
		}
	}
}
