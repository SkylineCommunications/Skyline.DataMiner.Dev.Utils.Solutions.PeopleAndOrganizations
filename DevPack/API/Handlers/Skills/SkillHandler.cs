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

			var toCreate = apiSkills.Where(x => x.IsNew).ToList();
			var toUpdate = apiSkills.Except(toCreate).ToList();

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

			api.PlanApi.Capabilities.CreateOrUpdate([skillsCapability]);
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

			var skillsCapability = GetSkillsCapability();

			foreach (var skill in apiSkills)
			{
				skillsCapability.RemoveDiscrete(skill.Name);
			}

			api.PlanApi.Capabilities.CreateOrUpdate([skillsCapability]);
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
	}
}
