namespace RT_PeopleAndOrganizations.PeopleOrganization.People
{
	using System;
	using System.Linq;
	using System.Runtime.Remoting.Metadata.W3cXsd2001;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class DraftStateTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public DraftStateTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void Activate()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Activate
			person = TestContext.Api.People.Activate(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(PersonState.Active, person.State);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Statuses.Active, domPerson.StatusId);
		}

		[TestMethod]
		public void DeprecateThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Deprecate
			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				person = TestContext.Api.People.Deprecate(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidStateError = personError as PersonInvalidStateError;
			Assert.IsNotNull(personInvalidStateError);
			Assert.AreEqual("Not allowed to deprecate a person that is not in Active state.", personInvalidStateError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidStateError.Id);
		}

		[TestMethod]
		public void Delete()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);
			var personId = person.Id;

			// Delete
			TestContext.Api.People.Delete(person);

			person = TestContext.Api.People.Read(personId);
			Assert.IsNull(person);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(personId)).SingleOrDefault();
			Assert.IsNull(domPerson);
		}

		[TestMethod]
		public void UpdateName()
		{
			var prefix = Guid.NewGuid();
			var name = $"{prefix}_Person";

			var person = new Person
			{
				Name = name,
			};

			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(name, person.Name);

			// Update name
			var updatedName = $"{name}_Updated";
			person.Name = updatedName;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedName, person.Name);
		}

		[TestMethod]
		public void AssignEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Assign email
			person.Email = email;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(email, person.Email);
		}

		[TestMethod]
		public void UpdateEmail()
		{
			var prefix = Guid.NewGuid();
			var email = "info@skyline.be";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				Email = email,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(email, person.Email);

			// Update email
			var updatedEmail = "support@skyline.be";
			person.Email = updatedEmail;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedEmail, person.Email);
		}

		[TestMethod]
		public void AssignPhone()
		{
			var prefix = Guid.NewGuid();
			var phone = "+32 15 47 00 00";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Assign phone
			person.Phone = phone;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(phone, person.Phone);
		}

		[TestMethod]
		public void UpdatePhone()
		{
			var prefix = Guid.NewGuid();
			var phone = "+32 15 47 00 00";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				Phone = phone,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(phone, person.Phone);

			// Update phone
			var updatedPhone = "+32 15 47 11 11";
			person.Phone = updatedPhone;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedPhone, person.Phone);
		}

		[TestMethod]
		public void AssignStreetAddress()
		{
			var prefix = Guid.NewGuid();
			var streetAddress = "Ambachtenstraat 33";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Assign street address
			person.StreetAddress = streetAddress;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(streetAddress, person.StreetAddress);
		}

		[TestMethod]
		public void UpdateStreetAddress()
		{
			var prefix = Guid.NewGuid();
			var streetAddress = "Ambachtenstraat 33";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				StreetAddress = streetAddress,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(streetAddress, person.StreetAddress);

			// Update street address
			var updatedStreetAddress = "Kardinaal Mercierplein 1";
			person.StreetAddress = updatedStreetAddress;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedStreetAddress, person.StreetAddress);
		}

		[TestMethod]
		public void AssignCity()
		{
			var prefix = Guid.NewGuid();
			var city = "Wommelgem";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Assign city
			person.City = city;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(city, person.City);
		}

		[TestMethod]
		public void UpdateCity()
		{
			var prefix = Guid.NewGuid();
			var city = "Wommelgem";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				City = city,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(city, person.City);

			// Update city
			var updatedCity = "Antwerp";
			person.City = updatedCity;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedCity, person.City);
		}

		[TestMethod]
		public void AssignZipCode()
		{
			var prefix = Guid.NewGuid();
			var zipCode = "2160";

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			// Assign zip code
			person.ZipCode = zipCode;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(zipCode, person.ZipCode);
		}

		[TestMethod]
		public void UpdateZipCode()
		{
			var prefix = Guid.NewGuid();
			var zipCode = "2160";

			var person = new Person
			{
				Name = $"{prefix}_Person",
				ZipCode = zipCode,
			};
			person = objectCreator.CreatePerson(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(zipCode, person.ZipCode);

			// Update zip code
			var updatedZipCode = "2000";
			person.ZipCode = updatedZipCode;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(updatedZipCode, person.ZipCode);
		}

		[TestMethod]
		public void AddTeamMembershipWithDraftTeam()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			person.AddTeamMembership(new TeamMembership(team));

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNull(fdRoleId);
		}

		[TestMethod]
		public void AddTeamMembershipWithDraftTeamAndRole()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			role = objectCreator.CreateRole(role);

			person.AddTeamMembership(new TeamMembership(team)
			{
				RoleId = role.Id,
			});

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNotNull(fdRoleId);
			Assert.AreEqual(role.Id, (Guid)fdRoleId.Value.Value);
		}

		[TestMethod]
		public void AddTeamMembershipWithActiveTeam()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			person.AddTeamMembership(new TeamMembership(team));

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNull(fdRoleId);
		}

		[TestMethod]
		public void AddTeamMembershipWithActiveTeamAndRole()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			role = objectCreator.CreateRole(role);

			person.AddTeamMembership(new TeamMembership(team)
			{
				RoleId = role.Id,
			});

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNotNull(fdRoleId);
			Assert.AreEqual(role.Id, (Guid)fdRoleId.Value.Value);
		}

		[TestMethod]
		public void AddTeamMembershipWithDeprecatedTeamThrowsException()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);
			team = TestContext.Api.Teams.Activate(team);
			team = TestContext.Api.Teams.Deprecate(team);

			person.AddTeamMembership(new TeamMembership(team));

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.People.Update(person);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = $"Team with ID '{team.Id}' is deprecated.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var personError = expectedException.TraceData.ErrorData.OfType<PersonError>().SingleOrDefault();
			Assert.IsNotNull(personError);

			var personInvalidTeamMembershipError = personError as PersonInvalidTeamMembershipError;
			Assert.IsNotNull(personInvalidTeamMembershipError);
			Assert.AreEqual(errorMessage, personInvalidTeamMembershipError.ErrorMessage);
			Assert.AreEqual(person.Id, personInvalidTeamMembershipError.Id);
			Assert.AreEqual(team.Id, personInvalidTeamMembershipError.TeamId);
			Assert.AreEqual(Guid.Empty, personInvalidTeamMembershipError.RoleId);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.IsFalse(domPerson.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
		}

		[TestMethod]
		public void UpdateTeamMembershipWithRole()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			role = objectCreator.CreateRole(role);

			person.AddTeamMembership(new TeamMembership(team));

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			// Update role
			var teamMembership = person.TeamMemberships.Single();
			teamMembership.RoleId = role.Id;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNotNull(fdRoleId);
			Assert.AreEqual(role.Id, (Guid)fdRoleId.Value.Value);
		}

		[TestMethod]
		public void UpdateTeamMembershipWithNoRole()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			var role = new Role
			{
				Name = $"{prefix}_Role",
			};
			role = objectCreator.CreateRole(role);

			person.AddTeamMembership(new TeamMembership(team)
			{
				RoleId = role.Id,
			});

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			// Remove role
			var teamMembership = person.TeamMemberships.Single();
			teamMembership.RoleId = Guid.Empty;

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.AreEqual(1, domPerson.Sections.Count(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
			var domTeam = domPerson.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id);
			var fdTeamId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Team_144d3379.Id);
			Assert.IsNotNull(fdTeamId);
			Assert.AreEqual(team.Id, (Guid)fdTeamId.Value.Value);
			var fdRoleId = domTeam.FieldValues.SingleOrDefault(fd => fd.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.TeamRole.Id);
			Assert.IsNull(fdRoleId);
		}

		[TestMethod]
		public void RemoveTeamMembership()
		{
			var prefix = Guid.NewGuid();

			var person = new Person
			{
				Name = $"{prefix}_Person",
			};
			person = objectCreator.CreatePerson(person);

			var team = new Team
			{
				Name = $"{prefix}_Team",
			};
			team = objectCreator.CreateTeam(team);

			person.AddTeamMembership(new TeamMembership(team));

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(1, person.TeamMemberships.Count);

			// Remove team membership
			var teamMembership = person.TeamMemberships.Single();
			person.RemoveTeamMembership(teamMembership);

			person = TestContext.Api.People.Update(person);
			Assert.IsNotNull(person);
			Assert.AreEqual(0, person.TeamMemberships.Count);

			var domPerson = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(person.Id)).SingleOrDefault();
			Assert.IsNotNull(domPerson);
			Assert.IsFalse(domPerson.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.Team.Id.Id));
		}
	}
}
