namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	internal class SkillFilterTranslator
	{
		public IEnumerable<Skill> FilterSkills(IEnumerable<Skill> skills, FilterElement<Skill> filter)
		{
			switch (filter)
			{
				case TRUEFilterElement<Skill>:
					return skills;
				case FALSEFilterElement<Skill>:
					return Array.Empty<Skill>();
				case ORFilterElement<Skill> orFilter:
					return orFilter.subFilters.SelectMany(f => FilterSkills(skills, f)).Distinct();
				case ANDFilterElement<Skill> andFilter:
					return andFilter.subFilters.Aggregate(skills, FilterSkills);
				case NOTFilterElement<Skill> notFilter:
					var excludedSkills = FilterSkills(skills, notFilter.original).ToHashSet();
					return skills.Where(skill => !excludedSkills.Contains(skill));
				case ManagedFilterIdentifier managedFilter:
					return TranslateFilter(skills, managedFilter);
				default:
					throw new NotSupportedException($"Unsupported filter: {filter}");
			}
		}

		private IEnumerable<Skill> TranslateFilter(IEnumerable<Skill> skills, ManagedFilterIdentifier managedFilter)
		{
			var fieldName = managedFilter.getFieldName().fieldName;
			var comparer = managedFilter.getComparer();
			var value = managedFilter.getValue();

			switch (fieldName)
			{
				case "Name":
					return TranslateNameFilter(skills, comparer, (string)value);
				default:
					throw new NotSupportedException($"Unsupported field: {fieldName}");
			}
		}

		private IEnumerable<Skill> TranslateNameFilter(IEnumerable<Skill> skills, Comparer comparer, string value)
		{
			switch (comparer)
			{
				case Comparer.Equals:
					return skills.Where(skill => skill.Name == value);
				case Comparer.NotEquals:
					return skills.Where(skill => skill.Name != value);
				case Comparer.Contains:
					return skills.Where(skill => skill.Name != null && skill.Name.Contains(value));
				case Comparer.NotContains:
					return skills.Where(skill => skill.Name == null || !skill.Name.Contains(value));
				case Comparer.Regex:
					return skills.Where(skill => skill.Name != null && System.Text.RegularExpressions.Regex.IsMatch(skill.Name, value));
				case Comparer.NotRegex:
					return skills.Where(skill => skill.Name == null || !System.Text.RegularExpressions.Regex.IsMatch(skill.Name, value));
				default:
					throw new NotSupportedException($"Unsupported comparer: {comparer}");
			}
		}
	}
}
