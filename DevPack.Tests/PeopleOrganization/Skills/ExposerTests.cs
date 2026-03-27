namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	[TestCategory("IntegrationTest")]
	[DoNotParallelize]
	public sealed class ExposerTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public ExposerTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_1")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(1, returnedskills.Length);
			Assert.AreEqual($"{prefix}_Skill_{postfix}_1", returnedskills[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.NotEqual($"{prefix}_Skill_{postfix}_2")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.IsFalse(returnedskills.Any(s => s.Name == $"{prefix}_Skill_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Contains($"{prefix}_Skill_{postfix}")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(10, returnedskills.Length);
			Assert.IsTrue(returnedskills.All(s => s.Name.Contains($"{prefix}_Skill_{postfix}")));
		}

		[TestMethod]
		public void FilterByNameContainsNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.NotContains($"{prefix}_Skill_{postfix}")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.IsFalse(returnedskills.Any(s => s.Name.Contains($"{prefix}_Skill_{postfix}")));
		}

		[TestMethod]
		public void FilterByAndFilter()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Contains($"{prefix}_Skill_{postfix}").AND(SkillExposers.Name.NotEqual($"{prefix}_Skill_{postfix}_{2}"))).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(9, returnedskills.Length);
			Assert.IsFalse(returnedskills.Any(s => s.Name == $"{prefix}_Skill_{postfix}_{2}"));
		}

		[TestMethod]
		public void FilterByOrFilter()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_{2}").OR(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_{3}"))).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(2, returnedskills.Length);
			CollectionAssert.Contains(returnedskills, skills[2]);
			CollectionAssert.Contains(returnedskills, skills[3]);
		}
	}
}
