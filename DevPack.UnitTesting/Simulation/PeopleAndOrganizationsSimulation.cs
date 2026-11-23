namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.UnitTesting.Simulation
{
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	/// <summary>
	/// Provides a pre-configured <see cref="SimulatedDms"/> that mirrors the installed state a real
	/// DataMiner Agent has when the People and Organizations solution is deployed (required elements
	/// and DOM module definitions, behaviors and initial statuses).
	/// </summary>
	public static class PeopleAndOrganizationsSimulation
	{
		/// <summary>
		/// The name of the lock manager element the People and Organizations solution communicates with.
		/// </summary>
		public const string LockManagerElementName = "PNO Lock Manager";

		/// <summary>
		/// The name of the lock manager element the MediaOps Plan solution communicates with.
		/// </summary>
		public const string PlanLockManagerElementName = "MOP Lock Manager";

		/// <summary>
		/// Creates a <see cref="SimulatedDms"/> configured for the People and Organizations solution.
		/// </summary>
		/// <returns>The configured <see cref="SimulatedDms"/>.</returns>
		public static SimulatedDms Create()
		{
			var dms = new SimulatedDms();

			dms.AddElement(LockManagerElementName);
			dms.AddElement(PlanLockManagerElementName, elementId: 2);

			using var connection = dms.CreateConnection();
			connection.GetPeopleAndOrganizationsApi().InstallDomModules();
			ResourceStudioModule.Register(dms);

			return dms;
		}
	}
}
