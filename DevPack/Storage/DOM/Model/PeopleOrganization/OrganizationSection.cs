namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations
{
	using System;

	internal partial class OrganizationSection
	{
		internal Guid OrganizationId
		{
			get
			{
				return Organization_57695f03 ?? Guid.Empty;
			}

			set
			{
				Organization_57695f03 = value == Guid.Empty ? null : value;
			}
		}
	}
}
