namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal partial class TeamInformationSection
	{
		internal IEnumerable<string> Skills
		{
			get
			{
				if (string.IsNullOrWhiteSpace(TeamSkills))
				{
					return Enumerable.Empty<string>();
				}

				return TeamSkills.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
			}

			set
			{
				if (value == null || !value.Any())
				{
					TeamSkills = string.Empty;
				}
				else
				{
					TeamSkills = string.Join(",", value);
				}
			}
		}
	}
}
