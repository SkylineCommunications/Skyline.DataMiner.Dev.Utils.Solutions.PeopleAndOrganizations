namespace RT_PeopleAndOrganizations.RegressionTests
{
	[TestClass]
	public class TestContextManager
	{
		public static IntegrationTestContext SharedTestContext { get; } = new IntegrationTestContext();

		[AssemblyCleanup]
		public static void Cleanup()
		{
			SharedTestContext.Dispose();
		}
	}
}
