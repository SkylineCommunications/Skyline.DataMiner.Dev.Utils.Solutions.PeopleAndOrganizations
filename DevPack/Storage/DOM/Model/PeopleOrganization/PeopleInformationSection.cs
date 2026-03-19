namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal partial class PeopleInformationSection
	{
		internal IEnumerable<string> Skills
		{
			get
			{
				if (string.IsNullOrWhiteSpace(PersonalSkills))
				{
					return Enumerable.Empty<string>();
				}

				return PersonalSkills.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()); // Trim is needed for backwards compatibility, as skills were previously stored with a comma and a space as separator.
			}

			set
			{
				if (value == null || !value.Any())
				{
					PersonalSkills = string.Empty;
				}
				else
				{
					PersonalSkills = string.Join(",", value);
				}
			}
		}
	}
}
