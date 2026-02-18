namespace RT_PeopleAndOrganizations.PeopleOrganization.Roles
{
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	public sealed class HashCodeTests
	{
		[TestMethod]
		public void Role_TrackableObject_Name()
		{
			var prefix = Guid.NewGuid();

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			var initialHash = role.GetHashCode();

			role.Name = $"{role.Name}_Updated";
			var updatedHAsh = role.GetHashCode();

			Assert.AreNotEqual(initialHash, updatedHAsh, "Changing Name should affect the hash code for change tracking.");
		}
	}
}
