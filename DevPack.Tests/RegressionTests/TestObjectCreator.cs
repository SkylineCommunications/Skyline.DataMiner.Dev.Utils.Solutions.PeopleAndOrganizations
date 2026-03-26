namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class TestObjectCreator : IDisposable
	{
		private readonly IntegrationTestContext testContext;

		private readonly HashSet<Guid> createdExperienceIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdCategoryIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdRoleIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdOrganizationIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdTeamIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdPersonIds = new HashSet<Guid>();

		private readonly HashSet<string> createdSkillNames = new HashSet<string>();

		private readonly HashSet<Guid> createdResourcePoolIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdResourceIds = new HashSet<Guid>();

		public TestObjectCreator(IntegrationTestContext testContext)
		{
			this.testContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
		}

		private IPeopleAndOrganizationsApi Api => testContext.Api;

		private IMediaOpsPlanApi PlanApi => testContext.PlanApi;

		public void Dispose()
		{
			try
			{
				PeopleCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				TeamsCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				OrganizationsCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				ExperienceCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				CategoriesCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				RolesCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				SkillsCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				ResourcesCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}

			try
			{
				ResourcePoolsCleanup();
			}
			catch
			{
				// Ignore cleanup errors
			}
		}

		public Organization CreateOrganization(Organization organization)
		{
			var createdOrganization = Api.Organizations.Create(organization);
			createdOrganizationIds.Add(createdOrganization.Id);
			return createdOrganization;
		}

		public IReadOnlyCollection<Organization> CreateOrganizations(IEnumerable<Organization> organizations)
		{
			try
			{
				var createdOrganizations = Api.Organizations.Create(organizations);

				foreach (var id in organizations.Select(x => x.Id))
				{
					createdOrganizationIds.Add(id);
				}

				return createdOrganizations;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdOrganizationIds.Add(id);
				}

				throw;
			}
		}

		public Team CreateTeam(Team team)
		{
			var createdTeam = Api.Teams.Create(team);
			createdTeamIds.Add(createdTeam.Id);
			return createdTeam;
		}

		public IReadOnlyCollection<Team> CreateTeams(IEnumerable<Team> teams)
		{
			try
			{
				var createdTeams = Api.Teams.Create(teams);

				foreach (var id in teams.Select(x => x.Id))
				{
					createdTeamIds.Add(id);
				}

				return createdTeams;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdTeamIds.Add(id);
				}

				throw;
			}
		}

		public Person CreatePerson(Person person)
		{
			var createdPerson = Api.People.Create(person);
			createdPersonIds.Add(createdPerson.Id);
			return createdPerson;
		}

		public IReadOnlyCollection<Person> CreatePeople(IEnumerable<Person> people)
		{
			try
			{
				var createdPeople = Api.People.Create(people);

				foreach (var id in people.Select(x => x.Id))
				{
					createdPersonIds.Add(id);
				}

				return createdPeople;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdPersonIds.Add(id);
				}

				throw;
			}
		}

		public Experience CreateExperience(Experience experience)
		{
			var createdExperience = Api.Experience.Create(experience);
			createdExperienceIds.Add(createdExperience.Id);
			return createdExperience;
		}

		public IReadOnlyCollection<Experience> CreateExperience(IEnumerable<Experience> experience)
		{
			try
			{
				var createdExperience = Api.Experience.Create(experience);

				foreach (var id in experience.Select(x => x.Id))
				{
					createdExperienceIds.Add(id);
				}

				return createdExperience;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdExperienceIds.Add(id);
				}

				throw;
			}
		}

		public Category CreateCategory(Category category)
		{
			var createdCategory = Api.Categories.Create(category);
			createdCategoryIds.Add(createdCategory.Id);
			return createdCategory;
		}

		public IReadOnlyCollection<Category> CreateCategories(IEnumerable<Category> categories)
		{
			try
			{
				var createdCategories = Api.Categories.Create(categories);

				foreach (var id in categories.Select(x => x.Id))
				{
					createdCategoryIds.Add(id);
				}

				return createdCategories;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdCategoryIds.Add(id);
				}

				throw;
			}
		}

		public Role CreateRole(Role role)
		{
			var createdRole = Api.Roles.Create(role);
			createdRoleIds.Add(createdRole.Id);
			return createdRole;
		}

		public IReadOnlyCollection<Role> CreateRoles(IEnumerable<Role> roles)
		{
			try
			{
				var createdRoles = Api.Roles.Create(roles);

				foreach (var id in roles.Select(x => x.Id))
				{
					createdRoleIds.Add(id);
				}

				return createdRoles;
			}
			catch (PeopleAndOrganizationsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdRoleIds.Add(id);
				}

				throw;
			}
		}

		public Skill CreateSkill(Skill skill)
		{
			var createdSkill = Api.Skills.Create(skill);
			createdSkillNames.Add(createdSkill.Name);
			return createdSkill;
		}

		public IReadOnlyCollection<Skill> CreateSkills(IEnumerable<Skill> skills)
		{
			try
			{
				var updatedSkills = Api.Skills.Create(skills);

				foreach (var name in skills.Select(x => x.Name))
				{
					createdSkillNames.Add(name);
				}

				return updatedSkills;
			}
			catch (PeopleAndOrganizationsBulkException<string> bulkException)
			{
				foreach (var name in bulkException.Result.SuccessfulIds)
				{
					createdSkillNames.Add(name);
				}

				throw;
			}
		}

		public Skill UpdateSkill(Skill skill)
		{
			var updatedSkill = Api.Skills.Update(skill);
			createdSkillNames.Remove(skill.OriginalName);
			createdSkillNames.Add(updatedSkill.Name);
			return updatedSkill;
		}

		public IReadOnlyCollection<Skill> UpdateSkills(IEnumerable<Skill> skills)
		{
			try
			{
				var updatedSkills = Api.Skills.Update(skills);

				foreach (var skill in skills)
				{
					createdSkillNames.Remove(skill.OriginalName);
					createdSkillNames.Add(skill.Name);
				}

				return updatedSkills;
			}
			catch (PeopleAndOrganizationsBulkException<string> bulkException)
			{
				foreach (var name in bulkException.Result.SuccessfulIds)
				{
					createdSkillNames.Add(name);
				}

				throw;
			}
		}

		public T CreateResource<T>(T resource) where T : Resource
		{
			var createdResource = (T)PlanApi.Resources.Create(resource);
			createdResourceIds.Add(createdResource.Id);
			return createdResource;
		}

		public IReadOnlyCollection<Resource> CreateResources(IEnumerable<Resource> resources)
		{
			try
			{
				var createdResources = PlanApi.Resources.Create(resources);

				foreach (var id in resources.Select(x => x.Id))
				{
					createdResourceIds.Add(id);
				}

				return createdResources;
			}
			catch (MediaOpsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdResourceIds.Add(id);
				}

				throw;
			}
		}

		public ResourcePool CreateResourcePool(ResourcePool resourcePool)
		{
			var createdPool = PlanApi.ResourcePools.Create(resourcePool);
			createdResourcePoolIds.Add(createdPool.Id);
			return createdPool;
		}

		public IReadOnlyCollection<ResourcePool> CreateResourcePools(IEnumerable<ResourcePool> resourcePools)
		{
			try
			{
				var createdPools = PlanApi.ResourcePools.Create(resourcePools);

				foreach (var id in resourcePools.Select(x => x.Id))
				{
					createdResourcePoolIds.Add(id);
				}

				return createdPools;
			}
			catch (MediaOpsBulkException<Guid> bulkException)
			{
				foreach (var id in bulkException.Result.SuccessfulIds)
				{
					createdResourcePoolIds.Add(id);
				}

				throw;
			}
		}

		private void PeopleCleanup()
		{
			var people = Api.People.Read(createdPersonIds.ToArray());

			try
			{
				var toDeprecate = people.Where(x => x.State == PersonState.Active);

				Api.People.Deprecate(toDeprecate);
			}
			catch
			{
				// Ignore cleanup errors
			}

			Api.People.Delete(people.ToArray());
		}

		private void TeamsCleanup()
		{
			var teams = Api.Teams.Read(createdTeamIds.ToArray());

			try
			{
				var toDeprecate = teams.Where(x => x.State == TeamState.Active);

				Api.Teams.Deprecate(toDeprecate);
			}
			catch
			{
				// Ignore cleanup errors
			}

			Api.Teams.Delete(teams.ToArray());
		}

		private void OrganizationsCleanup()
		{
			var organizations = Api.Organizations.Read(createdOrganizationIds.ToArray());

			try
			{
				var toDeprecate = organizations.Where(x => x.State == OrganizationState.Active);

				Api.Organizations.Deprecate(toDeprecate);
			}
			catch
			{
				// Ignore cleanup errors
			}

			Api.Organizations.Delete(organizations.ToArray());
		}

		private void ExperienceCleanup()
		{
			var experience = Api.Experience.Read(createdExperienceIds.ToArray());

			Api.Experience.Delete(experience.ToArray());
		}

		private void CategoriesCleanup()
		{
			var categories = Api.Categories.Read(createdCategoryIds.ToArray());

			Api.Categories.Delete(categories.ToArray());
		}

		private void RolesCleanup()
		{
			var roles = Api.Roles.Read(createdRoleIds.ToArray());

			Api.Roles.Delete(roles.ToArray());
		}

		private void SkillsCleanup()
		{
			var skills = Api.Skills.Read().Where(x => createdSkillNames.Contains(x.Name)).ToArray();

			Api.Skills.Delete(skills);
		}

		private void ResourcesCleanup()
		{
			var resources = PlanApi.Resources.Read(createdResourceIds.ToArray());

			foreach (var resource in resources.Where(r => r.State == ResourceState.Complete))
			{
				try
				{
					PlanApi.Resources.Deprecate(resource.Id);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}

			PlanApi.Resources.Delete(createdResourceIds.ToArray());
		}

		private void ResourcePoolsCleanup()
		{
			var pools = PlanApi.ResourcePools.Read(createdResourcePoolIds.ToArray());

			foreach (var pool in pools.Where(p => p.State == ResourcePoolState.Complete))
			{
				try
				{
					PlanApi.ResourcePools.Deprecate(pool.Id);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}

			PlanApi.ResourcePools.Delete(createdResourcePoolIds.ToArray());
		}
	}
}
