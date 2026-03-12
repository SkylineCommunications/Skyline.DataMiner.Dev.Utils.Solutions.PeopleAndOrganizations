namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Linq;

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

		public TestObjectCreator(IntegrationTestContext testContext)
		{
			this.testContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
		}

		private IPeopleAndOrganizationsApi Api => testContext.Api;

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

		private void PeopleCleanup()
		{
		}

		private void TeamsCleanup()
		{
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
	}
}
