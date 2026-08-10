namespace RT_PeopleAndOrganizations.PeopleOrganization.Experience
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.Querying;
	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	[TestClass]
	[TestCategory("IntegrationTest")]
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
			var experiences = CreateExperience(out var filter);

			QueryAssert.Read(TestContext.Api.Experience, experiences, filter.ToQuery().OrderBy(ExperienceExposers.Name, false));
			QueryAssert.Read(TestContext.Api.Experience, experiences.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(ExperienceExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var experiences = CreateExperience(out var filter);

			QueryAssert.Read(TestContext.Api.Experience, experiences.Take(1).ToArray(), filter.ToQuery().OrderBy(ExperienceExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.Experience, experiences.Take(3).ToArray(), filter.ToQuery().OrderBy(ExperienceExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.Experience, experiences, filter.ToQuery().OrderBy(ExperienceExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var experiences = CreateExperience(out var filter);

			QueryAssert.Count(TestContext.Api.Experience, experiences, filter.ToQuery().OrderBy(ExperienceExposers.Name, false));
			QueryAssert.Count(TestContext.Api.Experience, Array.Empty<Experience>(), filter.AND(ExperienceExposers.Name.Contains("Unknown")).ToQuery().OrderBy(ExperienceExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var experiences = CreateExperience(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.Experience, experiences, filter.ToQuery().OrderBy(ExperienceExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.Experience, experiences.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(ExperienceExposers.Name, false), 2);
		}

		private Experience[] CreateExperience(out FilterElement<Experience> filter)
		{
			var prefix = Guid.NewGuid();

			var experiences = Enumerable.Range(0, 5)
				.Select(i => new Experience { Name = $"{prefix}_Experience_{i}" })
				.ToArray();

			objectCreator.CreateExperience(experiences);

			filter = new ORFilterElement<Experience>(experiences.Select(x => ExperienceExposers.Id.Equal(x.Id)).ToArray());

			return experiences;
		}
	}
}
