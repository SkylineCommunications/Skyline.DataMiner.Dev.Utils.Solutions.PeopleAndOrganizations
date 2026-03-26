namespace RT_PeopleAndOrganizations.PeopleOrganization.Experience
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	[TestCategory("IntegrationTest")]
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
		public void FilterByIdEquals()
		{
			var prefix = Guid.NewGuid();
			var experience1 = objectCreator.CreateExperience(new Experience(Guid.NewGuid()) { Name = $"{prefix}_Experience_1" });
			objectCreator.CreateExperience(new Experience(Guid.NewGuid()) { Name = $"{prefix}_Experience_2" });

			var returnedExperiences = TestContext.Api.Experience.Read(ExperienceExposers.Id.Equal(experience1.Id)).ToArray();

			Assert.IsNotNull(returnedExperiences);
			Assert.AreEqual(1, returnedExperiences.Length);
			Assert.AreEqual(experience1.Id, returnedExperiences[0].Id);
			Assert.AreEqual(experience1.Name, returnedExperiences[0].Name);
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var experiences = Enumerable.Range(0, 10)
				.Select(i => new Experience { Name = $"{prefix}_Experience_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateExperience(experiences);

			var returnedExperiences = TestContext.Api.Experience.Read(ExperienceExposers.Name.Equal($"{prefix}_Experience_{postfix}_1")).ToArray();

			Assert.IsNotNull(returnedExperiences);
			Assert.AreEqual(1, returnedExperiences.Length);
			Assert.AreEqual($"{prefix}_Experience_{postfix}_1", returnedExperiences[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var experiences = Enumerable.Range(0, 10)
				.Select(i => new Experience { Name = $"{prefix}_Experience_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateExperience(experiences);

			var returnedExperiences = TestContext.Api.Experience.Read(ExperienceExposers.Name.NotEqual($"{prefix}_Experience_{postfix}_2")).ToArray();
			var createdExperiences = returnedExperiences.Where(e => e.Name.StartsWith($"{prefix}_Experience_{postfix}_", StringComparison.Ordinal)).ToArray();

			Assert.IsNotNull(returnedExperiences);
			Assert.AreEqual(9, createdExperiences.Length);
			Assert.IsFalse(createdExperiences.Any(e => e.Name == $"{prefix}_Experience_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			var experiences = Enumerable.Range(0, 10)
				.Select(i => new Experience { Name = $"{prefix}_Experience_{postfix}_{i}" })
				.ToArray();

			objectCreator.CreateExperience(experiences);

			var returnedExperiences = TestContext.Api.Experience.Read(ExperienceExposers.Name.Contains($"{prefix}_Experience_{postfix}")).ToArray();

			Assert.IsNotNull(returnedExperiences);
			Assert.AreEqual(10, returnedExperiences.Length);
			Assert.IsTrue(returnedExperiences.All(e => e.Name.Contains($"{prefix}_Experience_{postfix}", StringComparison.Ordinal)));
		}
	}
}
