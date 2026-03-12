namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	internal class SkillHandler : StringApiObjectValidator<Skill>
	{
		private readonly PeopleAndOrganizationsApi api;

		private SkillHandler(PeopleAndOrganizationsApi api) : base(skill => skill.Name)
		{
			this.api = api ?? throw new ArgumentNullException(nameof(api));
		}

		internal static Guid SkillCapabilityId => Guid.Parse("4d76ca23-de30-4129-acc9-9a742e054e52");

		internal static bool TryCreateOrUpdate(PeopleAndOrganizationsApi api, ICollection<Skill> apiSkills, out StringBulkOperationResult<Skill> result)
		{
			var handler = new SkillHandler(api);
			handler.CreateOrUpdate(apiSkills);

			result = new StringBulkOperationResult<Skill>(handler.SuccessfulItems, handler.SuccessfulIds, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		internal static bool TryDelete(PeopleAndOrganizationsApi api, ICollection<Skill> apiSKills, out StringBulkOperationResult<Skill> result)
		{
			var handler = new SkillHandler(api);
			handler.Delete(apiSKills);

			result = new StringBulkOperationResult<Skill>(handler.SuccessfulItems, handler.SuccessfulIds, handler.UnsuccessfulItems, handler.TraceDataPerItem);

			return !result.HasFailures;
		}

		internal static IEnumerable<Skill> ReadAll(PeopleAndOrganizationsApi api)
		{
			var handler = new SkillHandler(api);
			var skillsCapability = handler.GetSkillsCapability();
			return skillsCapability.Discretes.Select(name => new Skill { Name = name }).ToList();
		}

		private void CreateOrUpdate(ICollection<Skill> apiSkills)
		{
			if (apiSkills == null)
			{
				throw new ArgumentNullException(nameof(apiSkills));
			}

			if (apiSkills.Count == 0)
			{
				return;
			}

			ValidateNames(apiSkills.Where(IsValid).ToArray());

			var apiSkillsToCreateOrUpdate = apiSkills.Where(IsValid).ToList();
			var toCreate = apiSkillsToCreateOrUpdate.Where(x => x.IsNew).ToList();
			var toUpdate = apiSkillsToCreateOrUpdate.Except(toCreate).ToList();

			var skillsCapability = GetSkillsCapability();

			foreach (var skill in toUpdate)
			{
				skillsCapability.RemoveDiscrete(skill.OriginalName);
				skillsCapability.AddDiscrete(skill.Name);
			}

			foreach (var skill in toCreate)
			{
				skillsCapability.AddDiscrete(skill.Name);
			}

			try
			{
				api.PlanApi.Capabilities.CreateOrUpdate([skillsCapability]);
				ReportSuccess(apiSkillsToCreateOrUpdate);

			}
			catch (Exception ex)
			{
				// TODO: parse exception to find out which skill(s) caused the failure and report those, instead of reporting all of them.
				foreach (var apiSkill in apiSkills) ReportError(apiSkill.Name);
			}
		}

		private void Delete(ICollection<Skill> apiSkills)
		{
			if (apiSkills == null)
			{
				throw new ArgumentNullException(nameof(apiSkills));
			}

			if (apiSkills.Count == 0)
			{
				return;
			}

			ValidateStateForDeletion(apiSkills);
			ValidateNames(apiSkills.Where(IsValid).ToArray());

			if (!apiSkills.Any(IsValid))
			{
				return;
			}

			var skillsCapability = GetSkillsCapability();

			foreach (var skill in apiSkills.Where(IsValid))
			{
				skillsCapability.RemoveDiscrete(skill.Name);
			}

			try
			{
				api.PlanApi.Capabilities.CreateOrUpdate([skillsCapability]);
				ReportSuccess(apiSkills);

			}
			catch (Exception ex)
			{
				// TODO: parse exception to find out which skill(s) caused the failure and report those, instead of reporting all of them.
				foreach (var apiSkill in apiSkills) ReportError(apiSkill.Name);
			}
		}

		private MediaOps.Plan.API.Capability GetSkillsCapability()
		{
			var skillsCapability = api.PlanApi.Capabilities.Read(SkillCapabilityId);
			if (skillsCapability == null)
			{
				skillsCapability = new MediaOps.Plan.API.Capability(SkillCapabilityId)
				{
					Name = "PNO_Skills",
				};
			}

			return skillsCapability;
		}

		private void ValidateStateForDeletion(ICollection<Skill> apiSkills)
		{
			if (apiSkills == null)
			{
				throw new ArgumentNullException(nameof(apiSkills));
			}

			if (apiSkills.Count == 0)
			{
				return;
			}

			var newSkills = apiSkills.Where(x => x.IsNew).ToList();
			newSkills.ForEach(x =>
			{
				var error = new SkillInvalidStateError
				{
					ErrorMessage = $"A skill that was not saved cannot be removed.",
					Name = x.Name,
				};

				ReportError(x.Name, error);
			});
		}

		private void ValidateNames(ICollection<Skill> apiSkills)
		{
			if (apiSkills == null)
			{
				throw new ArgumentNullException(nameof(apiSkills));
			}

			if (apiSkills.Count == 0)
			{
				return;
			}

			var skillsRequiringValidation = apiSkills.ToList();

			foreach (var skill in skillsRequiringValidation.Where(x => !InputValidator.IsNonEmptyText(x.Name)).ToArray())
			{
				var error = new SkillInvalidNameError
				{
					ErrorMessage = "Name cannot be empty.",
					Name = skill.Name,
				};

				ReportError(skill.Name, error);

				skillsRequiringValidation.Remove(skill);
			}

			var skillsWithDuplicateNames = skillsRequiringValidation
				.GroupBy(skill => skill.Name)
				.Where(g => g.Count() > 1)
				.SelectMany(x => x)
				.ToList();

			foreach (var skill in skillsWithDuplicateNames)
			{
				var error = new SkillDuplicateNameError
				{
					ErrorMessage = $"Skill '{skill.Name}' has a duplicate name.",
					Name = skill.Name,
				};

				ReportError(skill.Name, error);
			}
		}
	}
}
