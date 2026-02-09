namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using SLDataGateway.API.Types.Querying;

	internal class OrganizationsRepository : Repository, IOrganizationsRepository
	{
		private readonly OrganizationFilterTranslator filterTranslator = new OrganizationFilterTranslator();

		public OrganizationsRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public IEnumerable<Organization> Read()
		{
			return Read(new TRUEFilterElement<Organization>());
		}

		public Organization Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(id));
			}

			var organization = Read(OrganizationExposers.Id.Equal(id)).FirstOrDefault();

			if (organization == null)
			{
				return null;
			}

			return organization;
		}

		public IEnumerable<Organization> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Organization>();
			}

			return Read(new ORFilterElement<Organization>(ids.Select(x => OrganizationExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Organization> Read(FilterElement<Organization> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = filterTranslator.Translate(filter);
			IEnumerable<Organization> Iterator()
			{
				foreach (var domOrganization in Api.DomHelpers.SlcPeopleOrganizationHelper.GetOrganizations(domFilter))
				{
					yield return new Organization(domOrganization);
				}
			}

			return Iterator();
		}

		public IEnumerable<Organization> Read(IQuery<Organization> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}
	}
}
