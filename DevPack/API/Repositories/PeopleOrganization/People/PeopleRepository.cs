namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Types.Querying;

	internal class PeopleRepository : Repository, IPeopleRepository
	{
		private readonly PersonFilterTranslator filterTranslator = new PersonFilterTranslator();

		public PeopleRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public Person Activate(Person person)
		{
			if (person == null)
			{
				throw new ArgumentNullException(nameof(person));
			}

			return Activate(person.Id);
		}

		public Person Activate(Guid personId)
		{
			var person = Read(personId);
			if (person == null)
			{
				return null;
			}

			if (!DomPersonHandler.TryActivate(Api, [person], out var result))
			{
				result.ThrowSingleException(person.Id);
			}

			return new Person(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Person> Activate(IEnumerable<Person> people)
		{
			if (people == null)
			{
				throw new ArgumentNullException(nameof(people));
			}

			return Activate(people.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Person> Activate(IEnumerable<Guid> personIds)
		{
			if (personIds == null)
			{
				throw new ArgumentNullException(nameof(personIds));
			}

			var people = Read(personIds);
			if (!DomPersonHandler.TryActivate(Api, people?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Person(x)).ToList();
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Person>());
		}

		public long Count(FilterElement<Person> filter)
		{
			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.Translate(filter));
		}

		public long Count(IQuery<Person> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Person> Create(IEnumerable<Person> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingPeople = list.Where(x => !x.IsNew);
			if (existingPeople.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing people. Use CreateOrUpdate or Update instead.");
			}

			if (!DomPersonHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Person(x)).ToList();
		}

		public Person Create(Person oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing person. Use CreateOrUpdate or Update instead.");
			}

			if (!DomPersonHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Person(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Person> CreateOrUpdate(IEnumerable<Person> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomPersonHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Person(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var toDelete = Read(apiObjectId);
			if (toDelete == null)
			{
				return;
			}

			if (!DomPersonHandler.TryDelete(Api, [toDelete], out var result))
			{
				result.ThrowSingleException(toDelete.Id);
			}
		}

		public void Delete(IEnumerable<Guid> apiObjectIds)
		{
			if (apiObjectIds == null)
			{
				throw new ArgumentNullException(nameof(apiObjectIds));
			}

			var toDelete = Read(apiObjectIds.ToArray());

			if (!DomPersonHandler.TryDelete(Api, toDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Person> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Person oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public Person Deprecate(Person person)
		{
			if (person == null)
			{
				throw new ArgumentNullException(nameof(person));
			}

			return Deprecate(person.Id);
		}

		public Person Deprecate(Guid personId)
		{
			var person = Read(personId);
			if (person == null)
			{
				return null;
			}

			if (!DomPersonHandler.TryDeprecate(Api, [person], out var result))
			{
				result.ThrowSingleException(person.Id);
			}

			return new Person(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Person> Deprecate(IEnumerable<Person> people)
		{
			if (people == null)
			{
				throw new ArgumentNullException(nameof(people));
			}

			return Deprecate(people.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Person> Deprecate(IEnumerable<Guid> personIds)
		{
			if (personIds == null)
			{
				throw new ArgumentNullException(nameof(personIds));
			}

			var people = Read(personIds);
			if (!DomPersonHandler.TryDeprecate(Api, people?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Person(x)).ToList();
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

			var people = Api.DomHelpers.SlcPeopleOrganizationHelper.GetPeople(filterTranslator.Translate(filter));
			return people.Select(x => new Person(x));
		}

		public IEnumerable<Person> Read(IQuery<Person> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Person>());
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Person>(), PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged(FilterElement<Person> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged(IQuery<Person> query)
		{
			return ReadPaged(query.Filter);
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged(FilterElement<Person> filter, int pageSize)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			return ReadPagedIterator(filter, pageSize);
		}

		public IEnumerable<IPagedResult<Person>> ReadPaged(IQuery<Person> query, int pageSize)
		{
			return ReadPaged(query.Filter, pageSize);
		}

		public IReadOnlyCollection<Person> Update(IEnumerable<Person> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newPeople = list.Where(x => x.IsNew);
			if (newPeople.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new people. Use Create or CreateOrUpdate instead.");
			}

			if (!DomPersonHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Person(x)).ToList();
		}

		public Person Update(Person oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new person. Use Create or CreateOrUpdate instead.");
			}

			if (!DomPersonHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Person(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Person>> ReadPagedIterator(FilterElement<Person> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.Translate(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetPeoplePaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Person>(page.Select(x => new Person(x)), pageNumber++, pageSize, hasNext);
			}
		}
	}
}
