namespace RT_PeopleAndOrganizations.PeopleOrganization.Skills
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

			try
			{
				objectCreator.UpdateSkill(toUpdate);
			}
			catch (PeopleAndOrganizationsException exception)
			{
				Assert.IsNotNull(exception, "Expected exception was not thrown.");

				var errorMessage = $"Skill '{skill1.Name}' already exists.";
				Assert.AreEqual(errorMessage, exception!.Message);

				Assert.AreEqual(1, exception!.TraceData.ErrorData.Count);

				var skillNameExistsError = exception!.TraceData.ErrorData.OfType<SkillDuplicateNameError>().SingleOrDefault();
				Assert.IsNotNull(skillNameExistsError);
				Assert.AreEqual(toUpdate.Name, skillNameExistsError.Name);
				Assert.AreEqual(errorMessage, skillNameExistsError.ErrorMessage);

				return;
			}

			Assert.Fail("Expected exception was not thrown.");
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
			catch (PeopleAndOrganizationsException exception)
			{
				Assert.AreEqual("Skill name cannot be longer than 150 characters.", exception.Message);
				Assert.AreEqual(1, exception.TraceData.ErrorData.Count);

				var skillError = exception.TraceData.ErrorData.OfType<SkillInvalidNameError>().SingleOrDefault();
				Assert.IsNotNull(skillError);

				Assert.AreEqual(skill.Name, skillError.Name);
				Assert.AreEqual("Skill name cannot be longer than 150 characters.", skillError.ErrorMessage);

				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void FilterByNameEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_1")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(1, returnedskills.Length);
			Assert.AreEqual($"{prefix}_Skill_{postfix}_1", returnedskills[0].Name);
		}

		[TestMethod]
		public void FilterByNameNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.NotEqual($"{prefix}_Skill_{postfix}_2")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.IsFalse(returnedskills.Any(s => s.Name == $"{prefix}_Skill_{postfix}_2"));
		}

		[TestMethod]
		public void FilterByNameContains()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Contains($"{prefix}_Skill_{postfix}")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(10, returnedskills.Length);
			Assert.IsTrue(returnedskills.All(s => s.Name.Contains($"{prefix}_Skill_{postfix}")));
		}

		[TestMethod]
		public void FilterByNameContainsNotEquals()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.NotContains($"{prefix}_Skill_{postfix}")).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.IsFalse(returnedskills.Any(s => s.Name.Contains($"{prefix}_Skill_{postfix}")));
		}

		[TestMethod]
		public void FilterByAndFilter()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Contains($"{prefix}_Skill_{postfix}").AND(SkillExposers.Name.NotEqual($"{prefix}_Skill_{postfix}_{2}"))).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(9, returnedskills.Length);
			Assert.IsFalse(returnedskills.Any(s => s.Name == $"{prefix}_Skill_{postfix}_{2}"));
		}

		[TestMethod]
		public void FilterByOrFilter()
		{
			var prefix = Guid.NewGuid();
			var postfix = Guid.NewGuid();
			var name = $"{prefix}_Skill_{postfix}";

			List<Skill> skills = new List<Skill>();
			for (int i = 0; i < 10; i++)
			{
				skills.Add(new Skill { Name = $"{prefix}_Skill_{postfix}_{i}" });
			}

			objectCreator.CreateSkills(skills);

			var returnedskills = TestContext.Api.Skills.Read(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_{2}").OR(SkillExposers.Name.Equal($"{prefix}_Skill_{postfix}_{3}"))).ToArray();
			Assert.IsNotNull(returnedskills);
			Assert.AreEqual(2, returnedskills.Length);
			CollectionAssert.Contains(returnedskills, skills[2]);
			CollectionAssert.Contains(returnedskills, skills[3]);
		}
	}
}
