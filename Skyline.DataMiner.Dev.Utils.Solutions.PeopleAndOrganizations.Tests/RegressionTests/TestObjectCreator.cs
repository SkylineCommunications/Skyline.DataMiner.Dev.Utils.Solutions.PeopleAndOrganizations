namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class TestObjectCreator : IDisposable
	{
		private readonly IntegrationTestContext testContext;

		private readonly HashSet<Guid> createdExperienceIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdCategoryIds = new HashSet<Guid>();

		private readonly HashSet<Guid> createdRoleIds = new HashSet<Guid>();

		public TestObjectCreator(IntegrationTestContext testContext)
		{
			this.testContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
		}

		private IPeopleAndOrganizationsApi Api => testContext.Api;

		public void Dispose()
		{
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
	}
}
