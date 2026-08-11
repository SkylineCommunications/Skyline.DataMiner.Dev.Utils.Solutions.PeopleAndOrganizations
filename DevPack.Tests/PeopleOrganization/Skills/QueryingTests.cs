namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	[TestClass]
	[TestCategory("IntegrationTest")]
	[DoNotParallelize]
	public sealed class QueryingTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public QueryingTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ReadWithQueryOrdersResults()
		{
			var skills = CreateSkills(out var filter);

			var ascending = TestContext.Api.Skills.Read(filter.ToQuery().OrderBy(SkillExposers.Name, false)).ToArray();
			var descending = TestContext.Api.Skills.Read(filter.ToQuery().OrderByDescending(SkillExposers.Name, false)).ToArray();

			CollectionAssert.AreEqual(skills.Select(x => x.Name).ToArray(), ascending.Select(x => x.Name).ToArray());
			CollectionAssert.AreEqual(skills.AsEnumerable().Reverse().Select(x => x.Name).ToArray(), descending.Select(x => x.Name).ToArray());
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var skills = CreateSkills(out var filter);

			var limited = TestContext.Api.Skills.Read(filter.ToQuery().OrderBy(SkillExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3))).ToArray();

			CollectionAssert.AreEqual(skills.Take(3).Select(x => x.Name).ToArray(), limited.Select(x => x.Name).ToArray());
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var skills = CreateSkills(out var filter);

			Assert.AreEqual(skills.Length, TestContext.Api.Skills.Count(filter.ToQuery().OrderBy(SkillExposers.Name, false)));
			Assert.AreEqual(3, TestContext.Api.Skills.Count(filter.ToQuery().WithLimit(LimitBy.Default.WithLimit(3))));
		}

		private Skill[] CreateSkills(out FilterElement<Skill> filter)
		{
			var prefix = Guid.NewGuid();

			var skills = Enumerable.Range(0, 5)
				.Select(i => new Skill { Name = $"{prefix}_Skill_{i}" })
				.ToArray();

			objectCreator.CreateSkills(skills);

			filter = SkillExposers.Name.Contains($"{prefix}_Skill_");

			return skills;
		}
	}
}
