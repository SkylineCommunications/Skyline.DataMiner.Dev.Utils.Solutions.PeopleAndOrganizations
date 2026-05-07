namespace RT_PeopleAndOrganizations.PeopleOrganization.Organizations
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Regression tests guarding against the bug where a DOM instance in the 'Edit' status caused the
	/// repository read operations to throw or return empty collections (the 'Edit' status is not part
	/// of the public <see cref="OrganizationState"/> enum). The repository is expected to silently
	/// exclude Edit-state instances so callers never observe them.
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
			var (editOrganization, activeOrganization) = CreateEditAndActiveOrganizations(prefix);

			try
			{
				var organizations = TestContext.Api.Organizations.Read().ToList();

				Assert.IsTrue(organizations.Any(o => o.Id == activeOrganization.Id), "The Active organization must be returned by Read().");
				Assert.IsFalse(organizations.Any(o => o.Id == editOrganization.Id), "The Edit-state organization must not be returned by Read().");
			}
			finally
			{
				TransitionEditToActive(editOrganization.Id);
			}
		}

		[TestMethod]
		public void ReadByIdsDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editOrganization, activeOrganization) = CreateEditAndActiveOrganizations(prefix);

			try
			{
				var organizations = TestContext.Api.Organizations.Read(new[] { editOrganization.Id, activeOrganization.Id }).ToList();

				Assert.AreEqual(1, organizations.Count, "Only the Active organization must be returned.");
				Assert.AreEqual(activeOrganization.Id, organizations.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editOrganization.Id);
			}
		}

		[TestMethod]
		public void ReadByFilterDoesNotIncludeEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editOrganization, activeOrganization) = CreateEditAndActiveOrganizations(prefix);

			try
			{
				var organizations = TestContext.Api.Organizations.Read(OrganizationExposers.Name.Contains($"{prefix}_Organization")).ToList();

				Assert.AreEqual(1, organizations.Count, "Only the Active organization must be returned.");
				Assert.AreEqual(activeOrganization.Id, organizations.Single().Id);
			}
			finally
			{
				TransitionEditToActive(editOrganization.Id);
			}
		}

		[TestMethod]
		public void ReadByIdReturnsNullForEditStateInstance()
		{
			var prefix = Guid.NewGuid();
			var (editOrganization, _) = CreateEditAndActiveOrganizations(prefix);

			try
			{
				var organization = TestContext.Api.Organizations.Read(editOrganization.Id);

				Assert.IsNull(organization, "Reading a single Edit-state organization by id must return null.");
			}
			finally
			{
				TransitionEditToActive(editOrganization.Id);
			}
		}

		private (Organization editOrganization, Organization activeOrganization) CreateEditAndActiveOrganizations(Guid prefix)
		{
			var editOrganization = objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_Edit" });
			editOrganization = TestContext.Api.Organizations.Activate(editOrganization);
			TransitionActiveToEdit(editOrganization.Id);

			var activeOrganization = objectCreator.CreateOrganization(new Organization { Name = $"{prefix}_Organization_Active" });
			activeOrganization = TestContext.Api.Organizations.Activate(activeOrganization);

			return (editOrganization, activeOrganization);
		}

		private static void TransitionActiveToEdit(Guid id)
		{
			TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
				new DomInstanceId(id),
				SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Transitions.Active_To_Edit);
		}

		private static void TransitionEditToActive(Guid id)
		{
			try
			{
				TestContext.PeopleOrganizationsDomHelper.DomInstances.DoStatusTransition(
					new DomInstanceId(id),
					SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.Transitions.Edit_To_Active);
			}
			catch
			{
				// Best-effort: leave cleanup to the disposer if the transition fails.
			}
		}
	}
}
