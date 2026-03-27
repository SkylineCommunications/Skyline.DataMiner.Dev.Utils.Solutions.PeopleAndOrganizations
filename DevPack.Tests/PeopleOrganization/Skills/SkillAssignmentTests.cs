namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
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
	[DoNotParallelize]
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
			StringAssert.Contains(expectedException.Message, "Unable to delete skill due to");
			StringAssert.Contains(expectedException.Message, person.Name);
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
			StringAssert.Contains(expectedException.Message, "Unable to delete skill due to");
			StringAssert.Contains(expectedException.Message, team.Name);
		}
	}
}
