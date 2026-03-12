namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	using static Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class BasicTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public BasicTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void BasicCrudActions()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Skill";

			var skill = new Skill
			{
				Name = name,
			};

			// Create
			skill = objectCreator.CreateSkill(skill);
			Assert.IsNotNull(skill);
			Assert.AreEqual(name, skill.Name);

			var returnedskill = TestContext.Api.Skills.Read(SkillExposers.Name.Equal(name)).Single();
			Assert.IsNotNull(returnedskill);
			Assert.AreEqual(skill.Name, returnedskill.Name);

			var parameter = TestContext.PlanApi.Capabilities.Read(SkillHandler.SkillCapabilityId);
			Assert.IsNotNull(parameter);
			Assert.IsTrue(parameter.Discretes.Contains(name));

			// Update
			var updatedName = $"{name}_Updated";
			skill.Name = updatedName;

			skill = TestContext.Api.Skills.Update(skill);
			Assert.IsNotNull(skill);
			Assert.AreEqual(updatedName, skill.Name);

			returnedskill = TestContext.Api.Skills.Read(SkillExposers.Name.Equal(updatedName)).Single();
			Assert.IsNotNull(returnedskill);
			Assert.AreEqual(skill.Name, returnedskill.Name);

			parameter = TestContext.PlanApi.Capabilities.Read(SkillHandler.SkillCapabilityId);
			Assert.IsNotNull(parameter);
			Assert.IsFalse(parameter.Discretes.Contains(name));
			Assert.IsTrue(parameter.Discretes.Contains(updatedName));

			// Delete
			TestContext.Api.Skills.Delete(skill);

			returnedskill = TestContext.Api.Skills.Read(SkillExposers.Name.Equal(updatedName)).SingleOrDefault();
			Assert.IsNull(returnedskill);

			parameter = TestContext.PlanApi.Capabilities.Read(SkillHandler.SkillCapabilityId);
			Assert.IsNotNull(parameter);
			Assert.IsFalse(parameter.Discretes.Contains(name));
			Assert.IsFalse(parameter.Discretes.Contains(updatedName));
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
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

			var createdSkills = objectCreator.CreateSkills([skill1, skill2]);
			var toUpdate = createdSkills.Single(x => x.Name == skill2.Name);
			toUpdate.Name = skill1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.UpdateSkill(toUpdate);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var skillError = expectedException.TraceData.ErrorData.OfType<SkillError>().SingleOrDefault();
			Assert.IsNotNull(skillError);

			//var skillNameExistsError = skillError as SkillNameExistsError;
			//Assert.IsNotNull(skillNameExistsError);
			//Assert.AreEqual(toUpdate.Id, skillNameExistsError.Id);
			//Assert.AreEqual(toUpdate.Name, skillNameExistsError.Name);
			//Assert.AreEqual(errorMessage, skillNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var skill = new Skill
			{
				Name = null,
			};

			try
			{
				skill = objectCreator.CreateSkill(skill);
			}
			catch (ArgumentException ex)
			{
				Assert.AreEqual("Name of skill cannot be null.\r\nParameter name: oToCreate", ex.Message);
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void CreateWithLongNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var skill = new Skill
			{
				Name = $"{prefix}_{new String(Enumerable.Repeat('c', 500).ToArray())}",
			};

			try
			{
				skill = objectCreator.CreateSkill(skill);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				Assert.AreEqual("Name cannot be empty.", ex.Message);
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}
	}
}
