namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM
{
	using System;

	using Skyline.DataMiner.Net;

	internal class DomHelpers
	{
		private readonly Lazy<SlcPeopleOrganizationHelper> lazySlcPeopleOrganizationHelper;

		public DomHelpers(IConnection connection)
		{
			lazySlcPeopleOrganizationHelper = new Lazy<SlcPeopleOrganizationHelper>(() => new SlcPeopleOrganizationHelper(connection));
		}

		public SlcPeopleOrganizationHelper SlcPeopleOrganizationHelper => lazySlcPeopleOrganizationHelper.Value;
	}
}
