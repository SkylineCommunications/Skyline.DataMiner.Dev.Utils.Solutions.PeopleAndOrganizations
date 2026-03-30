namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class SkillAssignmentTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public SkillAssignmentTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void WhenUsedByPersonThrowsException()
		{
			var prefix = Guid.NewGuid();

			var skill = new Skill
			{
				Name = $"{prefix}_Skill",
			};
			skill = objectCreator.CreateSkill(skill);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddSkill(skill);
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Skills.Delete(skill);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var skillError = expectedException.TraceData.ErrorData.OfType<SkillError>().SingleOrDefault();
			Assert.IsNotNull(skillError);

			Assert.AreEqual(skill.Name, skillError.Name);
			Assert.AreEqual(expectedException.Message, skillError.ErrorMessage);
			StringAssert.Contains(expectedException.Message, $"Skill '{skill.Name}' is in use by 1 people.");
		}

		[TestMethod]
		public void WhenUsedByPersonInBulkDeleteThrowsExceptionAndDeletesOtherSkills()
		{
			var prefix = Guid.NewGuid();

			var skill1 = new Skill
			{
				Name = $"{prefix}_Skill1",
			};
			var skill2 = new Skill
			{
				Name = $"{prefix}_Skill2",
			};
			var skill3 = new Skill
			{
				Name = $"{prefix}_Skill3",
			};

			var createdSkills = objectCreator.CreateSkills([skill1, skill2, skill3]).ToArray();
			var skillUsedByPerson = createdSkills.Single(x => x.Name == skill2.Name);

			var person = new Person
			{
				Name = $"{prefix}_Person",
			}
			.AddSkill(skillUsedByPerson);
			person = objectCreator.CreatePerson(person);

			PeopleAndOrganizationsBulkException<string>? expectedException = null;
			try
			{
				TestContext.Api.Skills.Delete(createdSkills);
			}
			catch (PeopleAndOrganizationsBulkException<string> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			CollectionAssert.AreEquivalent(
				new[] { skill1.Name, skill3.Name },
				expectedException.Result.SuccessfulIds.ToArray());

			CollectionAssert.AreEquivalent(
				new[] { skillUsedByPerson.Name },
				expectedException.Result.UnsuccessfulIds.ToArray());

			Assert.IsTrue(expectedException.Result.TraceDataPerItem.TryGetValue(skillUsedByPerson.Name, out var traceData), "No trace data found for the failed skill.");

			Assert.AreEqual(1, traceData.ErrorData.Count);
			var skillError = traceData.ErrorData.OfType<SkillError>().SingleOrDefault();
			Assert.IsNotNull(skillError);

			Assert.AreEqual(skillUsedByPerson.Name, skillError.Name);
			Assert.AreEqual($"Skill '{skillUsedByPerson.Name}' is in use by 1 people.", skillError.ErrorMessage);

			Assert.IsNotNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skillUsedByPerson.Name)).SingleOrDefault());
			Assert.IsNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skill1.Name)).SingleOrDefault());
			Assert.IsNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skill3.Name)).SingleOrDefault());
		}

		[TestMethod]
		public void WhenUsedByTeamThrowsException()
		{
			var prefix = Guid.NewGuid();

			var skill = new Skill
			{
				Name = $"{prefix}_Skill",
			};
			skill = objectCreator.CreateSkill(skill);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			}
			.AddSkill(skill);
			team = objectCreator.CreateTeam(team);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Skills.Delete(skill);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var skillError = expectedException.TraceData.ErrorData.OfType<SkillError>().SingleOrDefault();
			Assert.IsNotNull(skillError);

			Assert.AreEqual(skill.Name, skillError.Name);
			Assert.AreEqual(expectedException.Message, skillError.ErrorMessage);
			StringAssert.Contains(expectedException.Message, $"Skill '{skill.Name}' is in use by 1 teams.");
		}

		[TestMethod]
		public void WhenUsedByTeamInBulkDeleteThrowsExceptionAndDeletesOtherSkills()
		{
			var prefix = Guid.NewGuid();

			var skill1 = new Skill
			{
				Name = $"{prefix}_Skill1",
			};
			var skill2 = new Skill
			{
				Name = $"{prefix}_Skill2",
			};
			var skill3 = new Skill
			{
				Name = $"{prefix}_Skill3",
			};

			var createdSkills = objectCreator.CreateSkills([skill1, skill2, skill3]).ToArray();
			var skillUsedByTeam = createdSkills.Single(x => x.Name == skill2.Name);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			}
			.AddSkill(skillUsedByTeam);
			team = objectCreator.CreateTeam(team);

			PeopleAndOrganizationsBulkException<string>? expectedException = null;
			try
			{
				TestContext.Api.Skills.Delete(createdSkills);
			}
			catch (PeopleAndOrganizationsBulkException<string> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			CollectionAssert.AreEquivalent(
				new[] { skill1.Name, skill3.Name },
				expectedException.Result.SuccessfulIds.ToArray());

			CollectionAssert.AreEquivalent(
				new[] { skillUsedByTeam.Name },
				expectedException.Result.UnsuccessfulIds.ToArray());

			Assert.IsTrue(expectedException.Result.TraceDataPerItem.TryGetValue(skillUsedByTeam.Name, out var traceData), "No trace data found for the failed skill.");

			Assert.AreEqual(1, traceData.ErrorData.Count);
			var skillError = traceData.ErrorData.OfType<SkillError>().SingleOrDefault();
			Assert.IsNotNull(skillError);

			Assert.AreEqual(skillUsedByTeam.Name, skillError.Name);
			Assert.AreEqual($"Skill '{skillUsedByTeam.Name}' is in use by 1 teams.", skillError.ErrorMessage);

			Assert.IsNotNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skillUsedByTeam.Name)).SingleOrDefault());
			Assert.IsNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skill1.Name)).SingleOrDefault());
			Assert.IsNull(TestContext.Api.Skills.Read(SkillExposers.Name.Equal(skill3.Name)).SingleOrDefault());
		}
	}
}
