namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System.Linq;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	public sealed class StorageTests
	{
		[TestMethod]
		public void SetCache_HappyPath()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team()
			{
				Name = "Team 1",
			};
			var team2 = new Team()
			{
				Name = "Team 2",
			};
			person.Cache.SetCache<Team>([team1, team2]);

			var organization1 = new Organization()
			{
				Name = "Organization 1",
			};
			var organization2 = new Organization()
			{
				Name = "Organization 2",
			};
			person.Cache.SetCache<Organization>([organization1, organization2]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(2, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team2.Id));

			var cachedOrganizations = person.Cache.GetFromCache<Organization>().ToList();
			Assert.IsNotNull(cachedOrganizations);
			Assert.AreEqual(2, cachedOrganizations.Count);
			Assert.IsTrue(cachedOrganizations.Exists(x => x.Id == organization1.Id));
			Assert.IsTrue(cachedOrganizations.Exists(x => x.Id == organization2.Id));
		}

		[TestMethod]
		public void SetCache_Update()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team()
			{
				Name = "Team 1",
			};
			var team2 = new Team()
			{
				Name = "Team 2",
			};
			person.Cache.SetCache<Team>([team1, team2]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(2, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id && x.Name == "Team 1"));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team2.Id && x.Name == "Team 2"));

			team1.Name = "Updated Team 1";
			var team3 = new Team()
			{
				Name = "Team 3",
			};
			person.Cache.SetCache<Team>([team1, team3]);

			cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(2, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id && x.Name == "Updated Team 1"));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team3.Id && x.Name == "Team 3"));
		}

		[TestMethod]
		public void SetCache_NullCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			try
			{
				person.Cache.SetCache<Team>(null);
			}
			catch (ArgumentNullException)
			{
				// Expected
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void SetCache_EmptyCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			person.Cache.SetCache<Team>([]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.AreEqual(0, cachedTeams.Count);
		}

		[TestMethod]
		public void SetCache_CollectionWithNulls()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team();
			var team2 = new Team();

			try
			{
				person.Cache.SetCache<Team?>([team1, null, team2, null]);
			}
			catch (ArgumentException)
			{
				// Expected
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void SetCache_MixedCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team();
			var team2 = new Team();

			var organization1 = new Organization();
			var organization2 = new Organization();

			try
			{
				person.Cache.SetCache<ApiObject>([team1, team2, organization1, organization2]);
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Cannot use ApiObject directly. Use a derived type.", ex.Message);
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void AddToCache_HappyPath()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team()
			{
				Name = "Team 1",
			};
			var team2 = new Team()
			{
				Name = "Team 2",
			};
			person.Cache.AddToCache<Team>([team1, team2]);

			var organization1 = new Organization()
			{
				Name = "Organization 1",
			};
			var organization2 = new Organization()
			{
				Name = "Organization 2",
			};
			person.Cache.AddToCache<Organization>([organization1, organization2]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(2, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team2.Id));

			var cachedOrganizations = person.Cache.GetFromCache<Organization>().ToList();
			Assert.IsNotNull(cachedOrganizations);
			Assert.AreEqual(2, cachedOrganizations.Count);
			Assert.IsTrue(cachedOrganizations.Exists(x => x.Id == organization1.Id));
			Assert.IsTrue(cachedOrganizations.Exists(x => x.Id == organization2.Id));
		}

		[TestMethod]
		public void AddToCache_Update()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team()
			{
				Name = "Team 1",
			};
			var team2 = new Team()
			{
				Name = "Team 2",
			};
			person.Cache.AddToCache<Team>([team1, team2]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(2, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id && x.Name == "Team 1"));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team2.Id && x.Name == "Team 2"));

			team1.Name = "Updated Team 1";
			var team3 = new Team()
			{
				Name = "Team 3",
			};
			person.Cache.AddToCache<Team>([team1, team3]);

			cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.IsNotNull(cachedTeams);
			Assert.AreEqual(3, cachedTeams.Count);
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team1.Id && x.Name == "Updated Team 1"));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team2.Id && x.Name == "Team 2"));
			Assert.IsTrue(cachedTeams.Exists(x => x.Id == team3.Id && x.Name == "Team 3"));
		}

		[TestMethod]
		public void AddToCache_NullCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			try
			{
				person.Cache.AddToCache<Team>(null);
			}
			catch (ArgumentNullException)
			{
				// Expected
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void AddToCache_EmptyCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			person.Cache.AddToCache<Team>([]);

			var cachedTeams = person.Cache.GetFromCache<Team>().ToList();
			Assert.AreEqual(0, cachedTeams.Count);
		}

		[TestMethod]
		public void AddToCache_CollectionWithNulls()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team();
			var team2 = new Team();

			try
			{
				person.Cache.AddToCache<Team?>([team1, null, team2, null]);
			}
			catch (ArgumentException)
			{
				// Expected
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void AddToCache_MixedCollection()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team();
			var team2 = new Team();

			var organization1 = new Organization();
			var organization2 = new Organization();

			try
			{
				person.Cache.AddToCache<ApiObject>([team1, team2, organization1, organization2]);
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Cannot use ApiObject directly. Use a derived type.", ex.Message);
				return;
			}

			Assert.Fail("Expected exception was not thrown.");
		}

		[TestMethod]
		public void GetFromCache_NotCachedInstance()
		{
			var person = new Person()
			{
				Name = "Test Person",
			};
			var team1 = new Team();
			var team2 = new Team();
			person.Cache.SetCache<Team>([team1, team2]);

			var cachedOrganizations = person.Cache.GetFromCache<Organization>().ToList();
			Assert.IsNotNull(cachedOrganizations);
			Assert.AreEqual(0, cachedOrganizations.Count);
		}
	}
}
