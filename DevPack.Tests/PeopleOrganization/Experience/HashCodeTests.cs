namespace RT_PeopleAndOrganizations.PeopleOrganization.Experience
{
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	public sealed class HashCodeTests
	{
		[TestMethod]
		public void Role_TrackableObject_Name()
		{
			var prefix = Guid.NewGuid();

			var experience = new Experience
			{
				Name = $"{prefix}_Experience",
			};
			var initialHash = experience.GetHashCode();

			experience.Name = $"{experience.Name}_Updated";
			var updatedHAsh = experience.GetHashCode();

			Assert.AreNotEqual(initialHash, updatedHAsh, "Changing Name should affect the hash code for change tracking.");
		}
	}
}
