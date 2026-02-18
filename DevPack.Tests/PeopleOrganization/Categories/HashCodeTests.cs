namespace RT_PeopleAndOrganizations.PeopleOrganization.Categories
{
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	public sealed class HashCodeTests
	{
		[TestMethod]
		public void Role_TrackableObject_Name()
		{
			var prefix = Guid.NewGuid();

			var category = new Category
			{
				Name = $"{prefix}_Category",
			};
			var initialHash = category.GetHashCode();

			category.Name = $"{category.Name}_Updated";
			var updatedHAsh = category.GetHashCode();

			Assert.AreNotEqual(initialHash, updatedHAsh, "Changing Name should affect the hash code for change tracking.");
		}
	}
}
