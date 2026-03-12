namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Registration;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Logging;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM;

	/// <summary>
	/// Provides the main entry point for interacting with the People and Organizations API.
	/// </summary>
	public class PeopleAndOrganizationsApi : IPeopleAndOrganizationsApi
	{
		internal static readonly int DefaultPageSize = 200;

		private const string CatalogItemId = "1b67a623-4ca6-4d25-8b3d-ed4e39496a75"; // MediaOps.Plan Catalog Item ID

		private readonly IConnection connection;

		private readonly DomHelpers domHelpers;

		private readonly Lazy<IMediaOpsPlanApi> lazyPlanApi;
		private readonly Lazy<IOrganizationsRepository> lazyOrganizationsRepository;
		private readonly Lazy<IPeopleRepository> lazyPeopleRepository;
		private readonly Lazy<ITeamsRepository> lazyTeamsRepository;
		private readonly Lazy<IExperienceRepository> lazyExperienceRepository;
		private readonly Lazy<ICategoriesRepository> lazyCategoriesRepository;
		private readonly Lazy<IRolesRepository> lazyRolesRepository;
		private readonly Lazy<ISkillsRepository> lazySkillsRepository;
		private readonly Lazy<PeopleAndOrganizations.Tools.LockManager> lazyLockManager;

		private ILogger logger;

		internal PeopleAndOrganizationsApi(IConnection connection)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.logger = new NullLogger();

			domHelpers = new DomHelpers(connection);

			lazyPlanApi = new Lazy<IMediaOpsPlanApi>(() => connection.GetMediaOpsPlanApi());
			lazyOrganizationsRepository = new Lazy<IOrganizationsRepository>(() => new OrganizationsRepository(this));
			lazyPeopleRepository = new Lazy<IPeopleRepository>(() => new PeopleRepository(this));
			lazyTeamsRepository = new Lazy<ITeamsRepository>(() => new TeamsRepository(this));
			lazyExperienceRepository = new Lazy<IExperienceRepository>(() => new ExperienceRepository(this));
			lazyCategoriesRepository = new Lazy<ICategoriesRepository>(() => new CategoriesRepository(this));
			lazyRolesRepository = new Lazy<IRolesRepository>(() => new RolesRepository(this));
			lazySkillsRepository = new Lazy<ISkillsRepository>(() => new SkillsRepository(this));
			lazyLockManager = new Lazy<PeopleAndOrganizations.Tools.LockManager>(() => new PeopleAndOrganizations.Tools.LockManager(this));
		}

		/// <inheritdoc/>
		public IOrganizationsRepository Organizations => lazyOrganizationsRepository.Value;

		/// <inheritdoc/>
		public IPeopleRepository People => lazyPeopleRepository.Value;

		/// <inheritdoc/>
		public ITeamsRepository Teams => lazyTeamsRepository.Value;

		/// <inheritdoc/>
		public IExperienceRepository Experience => lazyExperienceRepository.Value;

		/// <inheritdoc/>
		public ICategoriesRepository Categories => lazyCategoriesRepository.Value;

		/// <inheritdoc/>
		public IRolesRepository Roles => lazyRolesRepository.Value;

		/// <inheritdoc/>
		public ISkillsRepository Skills => lazySkillsRepository.Value;

		internal IConnection Connection => connection;

		internal ILogger Logger => logger;

		internal DomHelpers DomHelpers => domHelpers;

		internal IMediaOpsPlanApi PlanApi => lazyPlanApi.Value;

		internal PeopleAndOrganizations.Tools.LockManager LockManager => lazyLockManager.Value;

		/// <inheritdoc/>
		public bool IsInstalled(out string version)
		{
			var registrar = Connection.GetSdmRegistrar();
			var mediaOpsPlanRegistration = registrar.Solutions.Read(SolutionRegistrationExposers.ID.Equal(CatalogItemId)).FirstOrDefault();
			if (mediaOpsPlanRegistration == null)
			{
				version = null;
				return false;
			}

			version = mediaOpsPlanRegistration.Version;
			return true;
		}

		/// <inheritdoc/>
		public bool IsInstalled()
		{
			return IsInstalled(out _);
		}

		/// <inheritdoc/>
		public void SetLogger(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
	}
}
