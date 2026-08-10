namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Tools
{
	using System;
	using System.Diagnostics;
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Logging;

	internal static class DataMinerAgentHelper
	{
		private static readonly string[] DataMinerProcessNames = new[]
		{
			"DataMiner",
			"SLAutomation",
			"SLScripting",
		};

		private static readonly object StateLock = new object();

		private static bool? isRunningOnDataMinerAgent;

		public static bool IsRunningOnDataMinerAgent(ILogger logger)
		{
			lock (StateLock)
			{
				if (!isRunningOnDataMinerAgent.HasValue)
				{
					string currentProcessName = Process.GetCurrentProcess().ProcessName;
					isRunningOnDataMinerAgent = DataMinerProcessNames.Any(x => currentProcessName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));

					if (!isRunningOnDataMinerAgent.Value)
					{
						logger?.Warning("This code isn't running on a DataMiner agent, unable to communicate with Lock Manager as NATS communication will fail, keeping locks in memory");
					}
				}

				return isRunningOnDataMinerAgent.Value;
			}
		}
	}
}
