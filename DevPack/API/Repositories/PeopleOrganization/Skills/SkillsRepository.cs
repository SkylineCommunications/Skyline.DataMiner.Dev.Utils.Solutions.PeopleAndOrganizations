namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Types.Querying;

	internal class SkillsRepository : Repository, ISkillsRepository
	{
		public SkillsRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Skill>());
		}

		public long Count(FilterElement<Skill> filter)
		{
			throw new NotImplementedException();
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

			if (!SkillHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			throw new NotImplementedException();
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

			if (!SkillHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Name);
			}

			throw new NotImplementedException();
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

			throw new NotImplementedException();
		}

		public void Delete(IEnumerable<Skill> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
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

			throw new NotImplementedException();
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

			if (!SkillHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			throw new NotImplementedException();
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

			if (!SkillHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Name);
			}

			throw new NotImplementedException();
		}
	}
}
