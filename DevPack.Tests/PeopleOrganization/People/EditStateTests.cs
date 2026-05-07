namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Regression tests guarding against the bug where a DOM instance in the 'Edit' status caused the
	/// repository read operations to throw or return empty collections (the 'Edit' status is not part
	/// of the public <see cref="PersonState"/> enum). The repository is expected to silently exclude
	/// Edit-state instances so callers never observe them.
	/// </summary>
	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class EditStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public EditStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ReadAllDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editPerson, activePerson) = CreateEditAndActivePeople(prefix);

			try
			{
				var people = TestContext.Api.People.Read().ToList();

				Assert.IsTrue(people.Any(p => p.Id == activePerson.Id), "The Active person must be returned by Read().");
				Assert.IsFalse(people.Any(p => p.Id == editPerson.Id), "The Edit-state person must not be returned by Read().");
			}
			finally
			{
				TransitionEditToActive(editPerson.Id);
			}
		}

		[TestMethod]
		public void ReadByIdsDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editPerson, activePerson) = CreateEditAndActivePeople(prefix);

			try
			{
				var people = TestContext.Api.People.Read(new[] { editPerson.Id, activePerson.Id }).ToList();

				Assert.AreEqual(1, people.Count, "Only the Active person must be returned.");
				Assert.AreEqual(activePerson.Id, people.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editPerson.Id);
			}
		}

		[TestMethod]
		public void ReadByFilterDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editPerson, activePerson) = CreateEditAndActivePeople(prefix);

			try
			{
				var people = TestContext.Api.People.Read(PersonExposers.Name.Contains($"{prefix}_Person")).ToList();

				Assert.AreEqual(1, people.Count, "Only the Active person must be returned.");
				Assert.AreEqual(activePerson.Id, people.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editPerson.Id);
			}
		}

		[TestMethod]
		public void ReadByIdReturnsNullForEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editPerson, _) = CreateEditAndActivePeople(prefix);

			try
			{
				var person = TestContext.Api.People.Read(editPerson.Id);

				Assert.IsNull(person, "Reading a single Edit-state person by id must return null.");
			}
			finally
			{
				TransitionEditToActive(editPerson.Id);
			}
		}

		private (Person editPerson, Person activePerson) CreateEditAndActivePeople(Guid prefix)
		{
			var editPerson = objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_Edit" });
			editPerson = TestContext.Api.People.Activate(editPerson);
			TransitionActiveToEdit(editPerson.Id);

			var activePerson = objectCreator.CreatePerson(new Person { Name = $"{prefix}_Person_Active" });
			activePerson = TestContext.Api.People.Activate(activePerson);

			return (editPerson, activePerson);
		}

		private static void TransitionActiveToEdit(Guid id)
		{
			TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
				new DomInstanceId(id),
				SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Transitions.Active_To_Edit);
		}

		private static void TransitionEditToActive(Guid id)
		{
			try
			{
				TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
					new DomInstanceId(id),
					SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Transitions.Edit_To_Active);
			}
			catch
			{
				// Best-effort: leave cleanup to the disposer if the transition fails.
			}
		}
	}
}
