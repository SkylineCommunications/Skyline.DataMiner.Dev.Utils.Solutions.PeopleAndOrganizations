namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	using SLDataGateway.API.Types.Querying;

	internal class TeamsRepository : Repository, ITeamsRepository
	{
		private readonly TeamFilterTranslator filterTranslator = new TeamFilterTranslator();

		public TeamsRepository(PeopleAndOrganizationsApi api) : base(api)
		{
		}

		public Team Activate(Team team)
		{
			if (team == null)
			{
				throw new ArgumentNullException(nameof(team));
			}

			return Activate(team.Id);
		}

		public Team Activate(Guid teamId)
		{
			var team = Read(teamId);
			if (team == null)
			{
				return null;
			}

			if (!DomTeamHandler.TryActivate(Api, [team], out var result))
			{
				result.ThrowSingleException(team.Id);
			}

			return new Team(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Team> Activate(IEnumerable<Team> teams)
		{
			if (teams == null)
			{
				throw new ArgumentNullException(nameof(teams));
			}

			return Activate(teams.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Team> Activate(IEnumerable<Guid> teamIds)
		{
			if (teamIds == null)
			{
				throw new ArgumentNullException(nameof(teamIds));
			}

			var teams = Read(teamIds);
			if (!DomTeamHandler.TryActivate(Api, teams?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public long Count()
		{
			return Count(new TRUEFilterElement<Team>());
		}

		public long Count(FilterElement<Team> filter)
		{
			if (filter.isEmpty())
			{
				return 0;
			}

			return Api.DomHelpers.SlcPeopleOrganizationHelper.CountPeopleOrganizationInstances(filterTranslator.Translate(filter));
		}

		public long Count(IQuery<Team> query)
		{
			return Count(query.Filter);
		}

		public IReadOnlyCollection<Team> Create(IEnumerable<Team> oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			var list = oToCreate.ToList();

			var existingTeams = list.Where(x => !x.IsNew);
			if (existingTeams.Any())
			{
				throw new InvalidOperationException("Not possible to use method Create for existing teams. Use CreateOrUpdate or Update instead.");
			}

			if (!DomTeamHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public Team Create(Team oToCreate)
		{
			if (oToCreate == null)
			{
				throw new ArgumentNullException(nameof(oToCreate));
			}

			if (!oToCreate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Create for existing team. Use CreateOrUpdate or Update instead.");
			}

			if (!DomTeamHandler.TryCreateOrUpdate(Api, [oToCreate], out var result))
			{
				result.ThrowSingleException(oToCreate.Id);
			}

			return new Team(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Team> CreateOrUpdate(IEnumerable<Team> oToCreateOrUpdate)
		{
			if (oToCreateOrUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToCreateOrUpdate));
			}

			var list = oToCreateOrUpdate.ToList();

			if (!DomTeamHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public void Delete(Guid apiObjectId)
		{
			var toDelete = Read(apiObjectId);
			if (toDelete == null)
			{
				return;
			}

			if (!DomTeamHandler.TryDelete(Api, [toDelete], out var result))
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

			if (!DomTeamHandler.TryDelete(Api, toDelete?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}
		}

		public void Delete(IEnumerable<Team> oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Select(x => x.Id).ToArray());
		}

		public void Delete(Team oToDelete)
		{
			if (oToDelete == null)
			{
				throw new ArgumentNullException(nameof(oToDelete));
			}

			Delete(oToDelete.Id);
		}

		public Team Deprecate(Team team)
		{
			if (team == null)
			{
				throw new ArgumentNullException(nameof(team));
			}

			return Deprecate(team.Id);
		}

		public Team Deprecate(Guid teamId)
		{
			var team = Read(teamId);
			if (team == null)
			{
				return null;
			}

			if (!DomTeamHandler.TryDeprecate(Api, [team], out var result))
			{
				result.ThrowSingleException(team.Id);
			}

			return new Team(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Team> Deprecate(IEnumerable<Team> teams)
		{
			if (teams == null)
			{
				throw new ArgumentNullException(nameof(teams));
			}

			return Deprecate(teams.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Team> Deprecate(IEnumerable<Guid> teamIds)
		{
			if (teamIds == null)
			{
				throw new ArgumentNullException(nameof(teamIds));
			}

			var teams = Read(teamIds);
			if (!DomTeamHandler.TryDeprecate(Api, teams?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public Team MakeBookable(Team team)
		{
			if (team == null)
			{
				throw new ArgumentNullException(nameof(team));
			}

			return MakeBookable(team.Id);
		}

		public Team MakeBookable(Guid teamId)
		{
			var team = Read(teamId);
			if (team == null)
			{
				return null;
			}

			if (!DomTeamHandler.TryMakeBookable(Api, [team], out var result))
			{
				result.ThrowSingleException(team.Id);
			}

			return new Team(result.SuccessfulItems.Single());
		}

		public IReadOnlyCollection<Team> MakeBookable(IEnumerable<Team> teams)
		{
			if (teams == null)
			{
				throw new ArgumentNullException(nameof(teams));
			}

			return MakeBookable(teams.Select(x => x.Id).ToArray());
		}

		public IReadOnlyCollection<Team> MakeBookable(IEnumerable<Guid> teamIds)
		{
			if (teamIds == null)
			{
				throw new ArgumentNullException(nameof(teamIds));
			}

			var teams = Read(teamIds);
			if (!DomTeamHandler.TryMakeBookable(Api, teams?.ToList(), out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public IEnumerable<Team> Read()
		{
			return Read(new TRUEFilterElement<Team>());
		}

		public Team Read(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(id));
			}

			var team = Read(TeamExposers.Id.Equal(id)).FirstOrDefault();

			return team;
		}

		public IEnumerable<Team> Read(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException(nameof(ids));
			}

			if (!ids.Any())
			{
				return Array.Empty<Team>();
			}

			return Read(new ORFilterElement<Team>(ids.Select(x => TeamExposers.Id.Equal(x)).ToArray()));
		}

		public IEnumerable<Team> Read(FilterElement<Team> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (filter.isEmpty())
			{
				return Enumerable.Empty<Team>();
			}

			var teams = Api.DomHelpers.SlcPeopleOrganizationHelper.GetTeams(filterTranslator.Translate(filter));
			return teams.Select(x => new Team(x));
		}

		public IEnumerable<Team> Read(IQuery<Team> query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			return Read(query.Filter);
		}

		public IEnumerable<IPagedResult<Team>> ReadPaged()
		{
			return ReadPaged(new TRUEFilterElement<Team>());
		}

		public IEnumerable<IPagedResult<Team>> ReadPaged(int pageSize)
		{
			return ReadPaged(new TRUEFilterElement<Team>(), pageSize);
		}

		public IEnumerable<IPagedResult<Team>> ReadPaged(FilterElement<Team> filter)
		{
			return ReadPaged(filter, PeopleAndOrganizationsApi.DefaultPageSize);
		}

		public IEnumerable<IPagedResult<Team>> ReadPaged(IQuery<Team> query)
		{
			return ReadPaged(query.Filter);
		}

		public IEnumerable<IPagedResult<Team>> ReadPaged(FilterElement<Team> filter, int pageSize)
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

		public IEnumerable<IPagedResult<Team>> ReadPaged(IQuery<Team> query, int pageSize)
		{
			return ReadPaged(query.Filter, pageSize);
		}

		public IReadOnlyCollection<Team> Update(IEnumerable<Team> oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			var list = oToUpdate.ToList();

			var newTeams = list.Where(x => x.IsNew);
			if (newTeams.Any())
			{
				throw new InvalidOperationException("Not possible to use method Update for new teams. Use Create or CreateOrUpdate instead.");
			}

			if (!DomTeamHandler.TryCreateOrUpdate(Api, list, out var result))
			{
				result.ThrowBulkException();
			}

			return result.SuccessfulItems.Select(x => new Team(x)).ToList();
		}

		public Team Update(Team oToUpdate)
		{
			if (oToUpdate == null)
			{
				throw new ArgumentNullException(nameof(oToUpdate));
			}

			if (oToUpdate.IsNew)
			{
				throw new InvalidOperationException("Not possible to use method Update for new team. Use Create or CreateOrUpdate instead.");
			}

			if (!DomTeamHandler.TryCreateOrUpdate(Api, [oToUpdate], out var result))
			{
				result.ThrowSingleException(oToUpdate.Id);
			}

			return new Team(result.SuccessfulItems.Single());
		}

		private IEnumerable<IPagedResult<Team>> ReadPagedIterator(FilterElement<Team> filter, int pageSize)
		{
			var pageNumber = 0;
			var paramFilter = filterTranslator.Translate(filter);
			var items = Api.DomHelpers.SlcPeopleOrganizationHelper.GetTeamsPaged(paramFilter, pageSize);
			var enumerator = items.GetEnumerator();
			var hasNext = enumerator.MoveNext();

			while (hasNext)
			{
				var page = enumerator.Current;
				hasNext = enumerator.MoveNext();
				yield return new PagedResult<Team>(page.Select(x => new Team(x)), pageNumber++, pageSize, hasNext);
			}
		}
	}
}
