namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal class SkillHandler : StringApiObjectValidator<Skill>
	{
		private const string LockName = "PNO_SKILLS";

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
			return skillsCapability.Discretes.Select(name => new Skill(name)).ToList();
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

			var skillsToCreateOrUpdate = apiSkills.Where(IsValid).ToArray();
			var lockResult = api.LockManager.TryLockAndExecute(LockName, () => CreateOrUpdateSkills(skillsToCreateOrUpdate), 20000);
			if (!lockResult)
			{
				foreach (var skill in skillsToCreateOrUpdate)
				{
					var errorForSkill = new SkillError
					{
						ErrorMessage = $"Failed to lock skill {skill.Name}.",
						Name = skill.Name,
					};

					ReportError(skill.Name, errorForSkill);
				}
			}
		}

		private void CreateOrUpdateSkills(ICollection<Skill> apiSkills)
		{
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
			catch (MediaOpsBulkException<Guid> mediaOpsException)
			{
				foreach (var error in mediaOpsException.Result.TraceDataPerItem[skillsCapability.Id].ErrorData)
				{
					switch (error)
					{
						case CapabilityDiscreteInvalidLengthError invalidLengthError:
							foreach (var invalidSkill in invalidLengthError.InvalidDiscretes)
							{
								var errorForSkill = new SkillInvalidNameError
								{
									ErrorMessage = $"Skill name cannot be longer than {invalidLengthError.MaxLength} characters.",
									Name = invalidSkill,
								};

								ReportError(invalidSkill, errorForSkill);
							}

							break;
						case CapabilityDuplicateDiscretesError duplicateDiscretesError:
							foreach (var duplicateSkill in duplicateDiscretesError.Discretes.Distinct())
							{
								var errorForSkill = new SkillDuplicateNameError
								{
									ErrorMessage = $"Skill '{duplicateSkill}' already exists.",
									Name = duplicateSkill,
								};

								ReportError(duplicateSkill, errorForSkill);
							}

							break;
						default:
							foreach (var skill in apiSkillsToCreateOrUpdate)
							{
								var errorForSkill = new SkillError
								{
									ErrorMessage = $"An error occurred while saving the skill: {mediaOpsException.Message}",
									Name = skill.Name,
								};

								ReportError(skill.Name, errorForSkill);
							}

							break;
					}
				}
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

			ValidateSkillsNotInUse(apiSkills.Where(IsValid).ToArray());

			if (!apiSkills.Any(IsValid))
			{
				return;
			}

			var skillsToRemove = apiSkills.Where(IsValid).ToArray();
			var lockResult = api.LockManager.TryLockAndExecute(LockName, () =>
			{
				var skillsCapability = GetSkillsCapability();

				foreach (var skill in skillsToRemove)
				{
					skillsCapability = skillsCapability.RemoveDiscrete(skill.Name);
				}

				try
				{
					api.PlanApi.Capabilities.CreateOrUpdate([skillsCapability]);
					ReportSuccess(skillsToRemove);
				}
				catch (MediaOpsException exception)
				{
					foreach (var apiSkill in skillsToRemove)
					{
						ReportError(apiSkill.Name, new SkillError
						{
							ErrorMessage = $"Unable to delete skill due to {exception.Message}.",
							Name = apiSkill.Name,
						});
					}
				}
			}, 20000);

			if (!lockResult)
			{
				foreach (var skill in skillsToRemove)
				{
					var errorForSkill = new SkillError
					{
						ErrorMessage = $"Failed to lock skill {skill.Name}.",
						Name = skill.Name,
					};

					ReportError(skill.Name, errorForSkill);
				}
			}
		}

		private void ValidateSkillsNotInUse(ICollection<Skill> apiSkills)
		{
			if (apiSkills == null)
			{
				throw new ArgumentNullException(nameof(apiSkills));
			}

			if (apiSkills.Count == 0)
			{
				return;
			}

			ValidateSkillsNotInUseByTeams(apiSkills);
			ValidateSkillsNotInUseByPeople(apiSkills);
		}

		private void ValidateSkillsNotInUseByTeams(ICollection<Skill> apiSkills)
		{
			var domTeamsFilter = new ORFilterElement<DomInstance>(apiSkills.Select(x => DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.TeamInformation.TeamSkills).Contains(x.Name)).ToArray());
			var teams = api.DomHelpers.SlcPeopleOrganizationHelper.GetTeams(domTeamsFilter).Select(x => new Team(x)).ToList();

			var teamsUsingSkills = apiSkills.ToDictionary(
				apiSkill => apiSkill,
				apiSkill => teams.Where(team => team.Skills.Any(skill => skill.Name == apiSkill.Name)).Select(x => x.Id).ToList());

			foreach (var skillWithTeams in teamsUsingSkills.Where(x => x.Value.Count > 0))
			{
				ReportError(skillWithTeams.Key.Name, new SkillInUseByTeamsError
				{
					ErrorMessage = $"Skill '{skillWithTeams.Key.Name}' is in use by {skillWithTeams.Value.Count} teams.",
					Name = skillWithTeams.Key.Name,
					TeamIds = skillWithTeams.Value,
				});
			}
		}

		private void ValidateSkillsNotInUseByPeople(ICollection<Skill> apiSkills)
		{
			var domPeopleFilter = new ORFilterElement<DomInstance>(apiSkills.Select(x => DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.PersonalSkills).Contains(x.Name)).ToArray());
			var people = api.DomHelpers.SlcPeopleOrganizationHelper.GetPeople(domPeopleFilter).Select(x => new Person(x)).ToList();

			var peopleUsingSkills = apiSkills.ToDictionary(
				apiSkill => apiSkill,
				apiSkill => people.Where(person => person.Skills.Any(skill => skill.Name == apiSkill.Name)).Select(x => x.Id).ToList());

			foreach (var skillWithPeople in peopleUsingSkills.Where(x => x.Value.Count > 0))
			{
				ReportError(skillWithPeople.Key.Name, new SkillInUseByPeopleError
				{
					ErrorMessage = $"Skill '{skillWithPeople.Key.Name}' is in use by {skillWithPeople.Value.Count} people.",
					Name = skillWithPeople.Key.Name,
					PeopleIds = skillWithPeople.Value,
				});
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
