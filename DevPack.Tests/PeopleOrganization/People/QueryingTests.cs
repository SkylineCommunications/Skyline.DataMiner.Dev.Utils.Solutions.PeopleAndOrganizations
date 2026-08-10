namespace RT_PeopleAndOrganizations.PeopleOrganization.People
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
			var people = CreatePeople(out var filter);

			QueryAssert.Read(TestContext.Api.People, people, filter.ToQuery().OrderBy(PersonExposers.Name, false));
			QueryAssert.Read(TestContext.Api.People, people.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(PersonExposers.Name, false));
		}

		[TestMethod]
		public void ReadWithQueryLimitsResults()
		{
			var people = CreatePeople(out var filter);

			QueryAssert.Read(TestContext.Api.People, people.Take(1).ToArray(), filter.ToQuery().OrderBy(PersonExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(1)));
			QueryAssert.Read(TestContext.Api.People, people.Take(3).ToArray(), filter.ToQuery().OrderBy(PersonExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(3)));
			QueryAssert.Read(TestContext.Api.People, people, filter.ToQuery().OrderBy(PersonExposers.Name, false).WithLimit(LimitBy.Default.WithLimit(100)));
		}

		[TestMethod]
		public void CountWithQuery()
		{
			var people = CreatePeople(out var filter);

			QueryAssert.Count(TestContext.Api.People, people, filter.ToQuery().OrderBy(PersonExposers.Name, false));
			QueryAssert.Count(TestContext.Api.People, Array.Empty<Person>(), filter.AND(PersonExposers.Name.Contains("Unknown")).ToQuery().OrderBy(PersonExposers.Name, false));
		}

		[TestMethod]
		public void ReadPagedWithQueryOrdersResults()
		{
			var people = CreatePeople(out var filter);

			QueryAssert.ReadPaged(TestContext.Api.People, people, filter.ToQuery().OrderBy(PersonExposers.Name, false));
			QueryAssert.ReadPaged(TestContext.Api.People, people.AsEnumerable().Reverse().ToArray(), filter.ToQuery().OrderByDescending(PersonExposers.Name, false), 2);
		}

		private Person[] CreatePeople(out FilterElement<Person> filter)
		{
			var prefix = Guid.NewGuid();

			var people = Enumerable.Range(0, 5)
				.Select(i => new Person { Name = $"{prefix}_Person_{i}" })
				.ToArray();

			objectCreator.CreatePeople(people);

			filter = new ORFilterElement<Person>(people.Select(x => PersonExposers.Id.Equal(x.Id)).ToArray());

			return people;
		}
	}
}
