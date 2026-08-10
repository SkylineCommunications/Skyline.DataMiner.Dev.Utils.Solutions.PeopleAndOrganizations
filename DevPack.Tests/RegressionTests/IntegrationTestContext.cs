namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;

	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.UnitTesting.Simulation;

	public sealed class IntegrationTestContext : IDisposable
	{
		private readonly Config config;

		private IConnection connection;

		public IntegrationTestContext()
		{
			config = Config.Load();

			connection = config.UseRealDma
				? CreateRealConnection(config)
				: CreateSimulatedConnection();

			Api = new PeopleAndOrganizationsApi(connection) ?? throw new NullReferenceException("Unable to create PeopleAndOrganizationsApi");
			Api.SetLogger(new ConsoleLogger());

			PlanApi = connection.GetMediaOpsPlanApi() ?? throw new NullReferenceException("Unable to get MediaOpsPlanApi");
			Dms = connection.GetDms() ?? throw new NullReferenceException("Unable to get DMS");

			PeopleOrganizationsDomHelper = new DomHelper(connection.HandleMessages, "(slc)people_organizations") ?? throw new NullReferenceException("Unable to create PeopleOrganizationsDomHelper");

			ResourceManagerHelper = new ResourceManagerHelper(connection.HandleSingleResponseMessage) ?? throw new NullReferenceException("Unable to create ResourceManagerHelper");
		}

		public IPeopleAndOrganizationsApi Api { get; private set; }

		public IMediaOpsPlanApi PlanApi { get; private set; }

		public IDms Dms { get; private set; }

		public DomHelper PeopleOrganizationsDomHelper { get; private set; }

		public ResourceManagerHelper ResourceManagerHelper { get; private set; }

		public void Dispose()
		{
			connection.Dispose();
		}

		private static IConnection CreateRealConnection(Config config)
		{
			var connection = Skyline.DataMiner.Net.ConnectionSettings.GetConnection(config.BaseUrl)
				?? throw new NullReferenceException("Unable to connect to DataMiner");

			connection.Authenticate(config.Username, config.Password, config.Domain);

			return connection;
		}

		private static IConnection CreateSimulatedConnection()
		{
			var dms = PeopleAndOrganizationsSimulation.Create();
			return dms.CreateConnection();
		}
	}
}
