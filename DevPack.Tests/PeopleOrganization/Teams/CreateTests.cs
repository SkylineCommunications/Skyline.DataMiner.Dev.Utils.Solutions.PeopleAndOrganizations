namespace RT_PeopleAndOrganizations.PeopleOrganization.Teams
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class CreateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public CreateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void CreateWithExistingIdThrowsException()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			var team1 = new Team(teamId)
			{
				Name = $"{prefix}_Team1",
			};
			var team2 = new Team(teamId)
			{
				Name = $"{prefix}_Team2",
			};

			objectCreator.CreateTeam(team1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateTeam(team2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamIdInUseError = teamError as TeamIdInUseError;
			Assert.IsNotNull(teamIdInUseError);
			Assert.AreEqual(teamId, teamIdInUseError.Id);
			Assert.AreEqual(errorMessage, teamIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			var team1 = new Team(teamId)
			{
				Name = $"{prefix}_Team1",
			};
			var team2 = new Team(teamId)
			{
				Name = $"{prefix}_Team2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateTeams([team1, team2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(teamId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var teamErrors = traceData.ErrorData.OfType<TeamError>().ToList();
			Assert.AreEqual(2, teamErrors.Count);

			var errorMessages = new List<string>
			{
				$"Team '{team1.Name}' has a duplicate ID.",
				$"Team '{team2.Name}' has a duplicate ID.",
			};

			foreach (var error in teamErrors)
			{
				var teamDuplicateIdError = error as TeamDuplicateIdError;
				Assert.IsNotNull(teamDuplicateIdError);
				Assert.AreEqual(teamId, teamDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team1 = new Team
			{
				Name = $"{prefix}_Team",
			};
			var team2 = new Team
			{
				Name = $"{prefix}_Team",
			};

			objectCreator.CreateTeam(team1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateTeam(team2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamNameExistsError = teamError as TeamNameExistsError;
			Assert.IsNotNull(teamNameExistsError);
			Assert.AreEqual(team2.Id, teamNameExistsError.Id);
			Assert.AreEqual(team2.Name, teamNameExistsError.Name);
			Assert.AreEqual(errorMessage, teamNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var team1 = new Team
			{
				Name = $"{prefix}_Team",
			};
			var team2 = new Team
			{
				Name = $"{prefix}_Team",
			};

			var teamsToCreate = new List<Team> { team1, team2 };

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateTeams(teamsToCreate);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.TraceDataPerItem.Count);

			foreach (var traceData in expectedException.Result.TraceDataPerItem.Values)
			{
				Assert.AreEqual(1, traceData.ErrorData.Count);
				var teamError = traceData.ErrorData.OfType<TeamError>().SingleOrDefault();
				Assert.IsNotNull(teamError);

				var teamDuplicateNameError = teamError as TeamDuplicateNameError;
				Assert.IsNotNull(teamDuplicateNameError);

				var team = teamsToCreate.Single(c => c.Id == teamDuplicateNameError.Id);
				Assert.IsNotNull(team);

				Assert.AreEqual(team.Name, teamDuplicateNameError.Name);
				Assert.AreEqual($"Team '{team.Name}' has a duplicate name.", teamDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var team = new Team
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateTeam(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidNameError = teamError as TeamInvalidNameError;
			Assert.IsNotNull(teamInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", teamInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var team = new Team
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateTeam(team);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var teamError = expectedException.TraceData.ErrorData.OfType<TeamError>().SingleOrDefault();
			Assert.IsNotNull(teamError);

			var teamInvalidNameError = teamError as TeamInvalidNameError;
			Assert.IsNotNull(teamInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", teamInvalidNameError.ErrorMessage);
		}
	}
}
