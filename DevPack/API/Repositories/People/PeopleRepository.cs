namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using SLDataGateway.API.Types.Querying;

	internal class PeopleRepository : Repository, IPeopleRepository
	{
		private readonly PersonFilterTranslator filterTranslator = new PersonFilterTranslator();

		public PeopleRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public IEnumerable<Person> Read()
		{
			return Read(new TRUEFilterElement<Person>());
		}

		public Person Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(id));
			}

			var person = Read(PersonExposers.Id.Equal(id)).FirstOrDefault();

			if (person == null)
			{
				return null;
			}

			return person;
		}

		public IEnumerable<Person> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Person>();
			}

			return Read(new ORFilterElement<Person>(ids.Select(x => PersonExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Person> Read(FilterElement<Person> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = filterTranslator.Translate(filter);
			IEnumerable<Person> Iterator()
			{
				foreach (var domPerson in Api.DomHelpers.SlcPeopleOrganizationHelper.GetPeople(domFilter))
				{
					yield return new Person(domPerson);
				}
			}

			return Iterator();
		}

		public IEnumerable<Person> Read(IQuery<Person> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}
	}
}
