namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DeprecatedStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DeprecatedStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void ActivateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Activate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Activate(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to activate a team that is not in Draft state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void DeprecateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Deprecate again
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Deprecate(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to deprecate a team that is not in Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void Delete()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			var teamId = team.Id;

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Delete
			TestContext.Api.Teams.Delete(team);

			team = TestContext.Api.Teams.Read(teamId);
			Assert.IsNull(team);

			var domTeam = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(teamId)).SingleOrDefault();
			Assert.IsNull(domTeam);
		}

		[TestMethod]
		public void MakeBookableThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Make bookable
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.MakeBookable(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to make a team bookable that is not in Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateNameThrowsException()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Team";

			var team = new Team
			{
				Name = name,
			};

			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(name, team.Name);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Update name
			var updatedName = $"{name}_Updated";
			team.Name = updatedName;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Update(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to update a team that is not in Draft or Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignEmailThrowsException()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Assign email
			team.Email = email;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Update(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to update a team that is not in Draft or Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateEmailThrowsException()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var team = new Team
			{
				Name = $"{prefix}_Team",
				Email = email,
			};
			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(email, team.Email);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Update email
			var updatedEmail = "support@skyline.be";
			team.Email = updatedEmail;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Update(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to update a team that is not in Draft or Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void AssignDescriptionThrowsException()
		{
			var prefix = Guid.NewGuid();
			var description = "my description";

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Assign description
			team.Description = description;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Update(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to update a team that is not in Draft or Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}

		[TestMethod]
		public void UpdateDescriptionThrowsException()
		{
			var prefix = Guid.NewGuid();
			var description = "my description";

			var team = new Team
			{
				Name = $"{prefix}_Team",
				Description = description,
			};
			team = objectCreator.CreateTeam(team);
			Assert.IsNotNull(team);
			Assert.AreEqual(description, team.Description);

			// Activate
			team = TestContext.Api.Teams.Activate(team);

			// Deprecate
			team = TestContext.Api.Teams.Deprecate(team);

			// Update description
			var updatedDescription = "my updated description";
			team.Description = updatedDescription;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				team = TestContext.Api.Teams.Update(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidStateError = teamError as TeamInvalidStateError;
			Assert.IsNotNull(teamInvalidStateError);
			Assert.AreEqual("Not allowed to update a team that is not in Draft or Active state.", teamInvalidStateError.ErrorMessage);
			Assert.AreEqual(team.Id, teamInvalidStateError.Id);
		}
	}
}
