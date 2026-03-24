namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Types.Querying;

	internal class ExperienceRepository : Repository, IExperienceRepository
	{
		private readonly ExperienceFilterTranslator filterTranslator = new ExperienceFilterTranslator();

		/// <summary>
		/// Initializes a new instance of the <see cref="ExperienceRepository"/> class.
		/// </summary>
		/// <param name="api">The People and Organizations API instance.</param>
		public ExperienceRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Experience>());
		}

		public long Count(FilterElement<Experience> filter)
		{
			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.Translate(filter));
		}

		public long Count(IQuery<Experience> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Experience> Create(IEnumerable<Experience> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingExperiences = list.Where(x => !x.IsNew);
			if (existingExperiences.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing experience. Use CreateOrUpdate or Update instead.");
			}

			if (!DomExperienceHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Experience(x)).ToList();
		}

		public Experience Create(Experience oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing experience. Use CreateOrUpdate or Update instead.");
			}

			if (!DomExperienceHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Experience(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Experience> CreateOrUpdate(IEnumerable<Experience> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomExperienceHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Experience(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var experienceToDelete = Read(apiObjectId);
			if (experienceToDelete == null)
			{
				return;
			}

			if (!DomExperienceHandler.TryDelete(Api, [experienceToDelete], out var result))
			{
				result.ThrowSingleException(experienceToDelete.Id);
			}
		}

		public void Delete(IEnumerable<Guid> apiObjectIds)
		{
			if (apiObjectIds == null)
			{
				throw new ArgumentNullException(nameof(apiObjectIds));
			}

			var experiencesToDelete = Read(apiObjectIds.ToArray());

			if (!DomExperienceHandler.TryDelete(Api, experiencesToDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Experience> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Experience oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public IEnumerable<Experience> Read()
		{
			return Read(new TRUEFilterElement<Experience>());
		}

		public Experience Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentException(nameof(id));
			}

			var experience = Read(ExperienceExposers.Id.Equal(id)).FirstOrDefault();

			return experience;
		}

		public IEnumerable<Experience> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Experience>();
			}

			return Read(new ORFilterElement<Experience>(ids.Select(x => ExperienceExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Experience> Read(FilterElement<Experience> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var experiences = Api.DomHelpers.SlcPeopleOrganizationHelper.GetExperience(filterTranslator.Translate(filter));
			return experiences.Select(x => new Experience(x));
		}

		public IEnumerable<Experience> Read(IQuery<Experience> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<IPagedResult<Experience>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Experience>());
		}

		public IEnumerable<IPagedResult<Experience>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Experience>(), pageSize);
		}

		public IEnumerable<IPagedResult<Experience>> ReadPaged(FilterElement<Experience> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Experience>> ReadPaged(IQuery<Experience> query)
		{
			return ReadPaged(query.Filter);
		}

		public IEnumerable<IPagedResult<Experience>> ReadPaged(FilterElement<Experience> filter, int pageSize)
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

		public IEnumerable<IPagedResult<Experience>> ReadPaged(IQuery<Experience> query, int pageSize)
		{
			return ReadPaged(query.Filter, pageSize);
		}

		public IReadOnlyCollection<Experience> Update(IEnumerable<Experience> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newExperiences = list.Where(x => x.IsNew);
			if (newExperiences.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new experience. Use Create or CreateOrUpdate instead.");
			}

			if (!DomExperienceHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Experience(x)).ToList();
		}

		public Experience Update(Experience oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new experience. Use Create or CreateOrUpdate instead.");
			}

			if (!DomExperienceHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Experience(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Experience>> ReadPagedIterator(FilterElement<Experience> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.Translate(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetExperiencePaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Experience>(page.Select(x => new Experience(x)), pageNumber++, pageSize, hasNext);
			}
		}
	}
}
