namespace RT_PeopleAndOrganizations.RegressionTests
{
	public static class TestContextManager
	{
		public static IntegrationTestContext SharedTestContext { get; } = new IntegrationTestContext();

		[AssemblyCleanup]
		public static void Cleanup()
		{
			SharedTestContext.Dispose();
		}
	}
}
