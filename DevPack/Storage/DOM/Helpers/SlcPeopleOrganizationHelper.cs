namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations;

	internal class SlcPeopleOrganizationHelper : DomModuleHelperBase
	{
		public SlcPeopleOrganizationHelper(IConnection connection) : base(SlcPeople_OrganizationsIds.ModuleId, connection)
		{
		}

		public IEnumerable<OrganizationsInstance> GetOrganizations(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetOrganizationIterator(filter);
		}

		public IEnumerable<PeopleInstance> GetPeople(FilterElement<DomInstance> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			return GetPersonIterator(filter);
		}

		private IEnumerable<OrganizationsInstance> GetOrganizationIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new OrganizationsInstance(instance));
		}

		private IEnumerable<PeopleInstance> GetPersonIterator(FilterElement<DomInstance> filter)
		{
			return InstanceFactory.ReadAndCreateInstances(DomHelper, filter, instance => new PeopleInstance(instance));
		}
	}
}
