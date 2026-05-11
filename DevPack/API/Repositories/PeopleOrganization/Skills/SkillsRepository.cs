namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using SLDataGateway.API.Types.Querying;

	internal class SkillsRepository : Repository, ISkillsRepository
	{
		private readonly SkillFilterTranslator skillFilterTranslator = new SkillFilterTranslator();

		public SkillsRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Skill>());
		}

		public long Count(FilterElement<Skill> filter)
		{
			return Read(filter).Count();
		}

		public long Count(IQuery<Skill> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Skill> Create(IEnumerable<Skill> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingSkills = list.Where(x => !x.IsNew);
			if (existingSkills.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing skills. Use CreateOrUpdate or Update instead.");
			}

			if (oToCreate.Any(x => x.Name == null))
			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToCreate));
			}

			if (!SkillHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Skill(x.Name)).ToList();
		}

		public Skill Create(Skill oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing skill. Use CreateOrUpdate or Update instead.");
			}

			if (oToCreate.Name == null)

			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToCreate));
			}

			if (!SkillHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Name);
			}

			return new Skill(result.SuccessfulItems.Single().Name);
		}

		public IReadOnlyCollection<Skill> CreateOrUpdate(IEnumerable<Skill> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!SkillHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Skill(x.Name)).ToList();
		}

		public void Delete(IEnumerable<Skill> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			if (oToDelete.Any(x => x.Name == null))
			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToDelete));
			}

			if (!SkillHandler.TryDelete(Api, oToDelete.ToArray(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(Skill oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			if (oToDelete.Name == null)
			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToDelete));
			}

			if (!SkillHandler.TryDelete(Api, [oToDelete], out var result))
			{
				result.ThrowSingleException(oToDelete.Name);
			}
		}

		public IEnumerable<Skill> Read(FilterElement<Skill> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (filter.isEmpty())
			{
				return Enumerable.Empty<Skill>();
			}

			return skillFilterTranslator.FilterSkills(SkillHandler.ReadAll(Api), filter);
		}

		public IEnumerable<Skill> Read(IQuery<Skill> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<Skill> Read()
		{
			return Read(new TRUEFilterElement<Skill>());
		}

		public IReadOnlyCollection<Skill> Update(IEnumerable<Skill> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newSkills = list.Where(x => x.IsNew);
			if (newSkills.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new skills. Use Create or CreateOrUpdate instead.");
			}

			if (oToUpdate.Any(x => x.Name == null))
			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToUpdate));
			}

			if (!SkillHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Skill(x.Name)).ToList();
		}

		public Skill Update(Skill oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new skill. Use Create or CreateOrUpdate instead.");
			}

			if (oToUpdate.Name == null)
			{
				throw new ArgumentException("Name of skill cannot be null.", nameof(oToUpdate));
			}

			if (!SkillHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Name);
			}

			return new Skill(result.SuccessfulItems.Single().Name);
		}
	}
}
