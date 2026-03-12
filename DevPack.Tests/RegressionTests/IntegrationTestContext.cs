namespace RT_PeopleAndOrganizations.RegressionTests
{
	using System;

	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Solutions.MediaOps.Plan.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	using DMConnection = Skyline.DataMiner.Net.Connection;

	public sealed class IntegrationTestContext : IDisposable
	{
		private readonly DMConnection connection;

		public IntegrationTestContext()
		{
			var config = Config.Load();

			connection = Skyline.DataMiner.Net.ConnectionSettings.GetConnection(config.BaseUrl) ?? throw new NullReferenceException("Unable to connect to DataMiner");
			connection.Authenticate(config.Username, config.Password, config.Domain);

			Api = new PeopleAndOrganizationsApi(connection) ?? throw new NullReferenceException("Unable to create PeopleAndOrganizationsApi");
			PlanApi = connection.GetMediaOpsPlanApi() ?? throw new NullReferenceException("Unable to get MediaOpsPlanApi");
			Dms = connection.GetDms() ?? throw new NullReferenceException("Unable to get DMS");

			PeopleOrganizationsDomHelper = new DomHelper(connection.HandleMessages, "(slc)people_organizations") ?? throw new NullReferenceException("Unable to create PeopleOrganizationsDomHelper");
		}

		public IPeopleAndOrganizationsApi Api { get; private set; }

		public IMediaOpsPlanApi PlanApi { get; private set; }

		public IDms Dms { get; private set; }

		public DomHelper PeopleOrganizationsDomHelper { get; private set; }

		public void Dispose()
		{
			connection.Dispose();
		}
	}
}
