namespace RT_PeopleAndOrganizations.PeopleOrganization.Roles
{
	using System;
	using System.Linq;

	using RT_PeopleAndOrganizations.RegressionTests;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Exceptions;

	using SLDataGateway.API.Querying;

	[TestClass]
	[TestCategory("IntegrationTest")]
	public sealed class BasicTests : IDisposable
	{
		private readonly TestObjectCreator objectCreator;

		public BasicTests()
		{
			objectCreator = new TestObjectCreator(TestContext);
		}

		private static IntegrationTestContext TestContext => TestContextManager.SharedTestContext;

		public void Dispose()
		{
			objectCreator.Dispose();
		}

		[TestMethod]
		public void BasicCrudActions()
		{
			var prefix = Guid.NewGuid();
			var roleId = Guid.NewGuid();
			var name = $"{prefix}_Role";

			var role = new Role(roleId)
			{
				Name = name,
			};

			// Create
			role = objectCreator.CreateRole(role);
			Assert.IsNotNull(role);
			Assert.AreEqual(roleId, role.Id);
			Assert.AreEqual(name, role.Name);

			var returnedRole = TestContext.Api.Roles.Read(roleId);
			Assert.IsNotNull(returnedRole);
			Assert.AreEqual(role.Id, returnedRole.Id);
			Assert.AreEqual(role.Name, returnedRole.Name);

			var domRole = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(roleId)).SingleOrDefault();
			Assert.IsNotNull(domRole);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Role.Id, domRole.DomDefinitionId.Id);

			Assert.IsTrue(domRole.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Id.Id));
			var domRoleInformation = domRole.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Id.Id);
			var fdRole = domRoleInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Role.Id);
			Assert.IsNotNull(fdRole);
			Assert.AreEqual(returnedRole.Name, Convert.ToString(fdRole.Value.Value));

			// Update
			var updatedName = $"{name}_Updated";
			role.Name = updatedName;

			role = TestContext.Api.Roles.Update(role);
			Assert.IsNotNull(role);
			Assert.AreEqual(roleId, role.Id);
			Assert.AreEqual(updatedName, role.Name);

			returnedRole = TestContext.Api.Roles.Read(roleId);
			Assert.IsNotNull(returnedRole);
			Assert.AreEqual(role.Id, returnedRole.Id);
			Assert.AreEqual(role.Name, returnedRole.Name);

			domRole = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(roleId)).SingleOrDefault();
			Assert.IsNotNull(domRole);
			Assert.AreEqual(Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Definitions.Role.Id, domRole.DomDefinitionId.Id);

			Assert.IsTrue(domRole.Sections.Exists(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Id.Id));
			domRoleInformation = domRole.Sections.Single(s => s.SectionDefinitionID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Id.Id);
			fdRole = domRoleInformation.FieldValues.SingleOrDefault(f => f.FieldDescriptorID.Id == Skyline.DataMiner.Solutions.PeopleAndOrganizations.Storage.DOM.SlcPeople_Organizations.SlcPeople_OrganizationsIds.Sections.RoleInformation.Role.Id);
			Assert.IsNotNull(fdRole);
			Assert.AreEqual(returnedRole.Name, Convert.ToString(fdRole.Value.Value));

			// Delete
			TestContext.Api.Roles.Delete(role);

			returnedRole = TestContext.Api.Roles.Read(roleId);
			Assert.IsNull(returnedRole);

			domRole = TestContext.PeopleOrganizationsDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(roleId)).SingleOrDefault();
			Assert.IsNull(domRole);
		}

		[TestMethod]
		public void CreateWithExistingIdThrowsException()
		{
			var prefix = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var role1 = new Role(roleId)
			{
				Name = $"{prefix}_Role1",
			};
			var role2 = new Role(roleId)
			{
				Name = $"{prefix}_Role2",
			};

			objectCreator.CreateRole(role1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateRole(role2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "ID is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleIdInUseError = roleError as RoleIdInUseError;
			Assert.IsNotNull(roleIdInUseError);
			Assert.AreEqual(roleId, roleIdInUseError.Id);
			Assert.AreEqual(errorMessage, roleIdInUseError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameIdInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();
			var roleId = Guid.NewGuid();

			var role1 = new Role(roleId)
			{
				Name = $"{prefix}_Role1",
			};
			var role2 = new Role(roleId)
			{
				Name = $"{prefix}_Role2",
			};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateRoles([role1, role2]);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			if (!expectedException.Result.TraceDataPerItem.TryGetValue(roleId, out var traceData))
			{
				Assert.Fail("No trace data found for the failed ID");
			}

			Assert.AreEqual(2, traceData.ErrorData.Count);
			var roleErrors = traceData.ErrorData.OfType<RoleError>().ToList();
			Assert.AreEqual(2, roleErrors.Count);

			var errorMessages = new List<string>
			{
				$"Role '{role1.Name}' has a duplicate ID.",
				$"Role '{role2.Name}' has a duplicate ID.",
			};

			foreach (var error in roleErrors)
			{
				var roleDuplicateIdError = error as RoleDuplicateIdError;
				Assert.IsNotNull(roleDuplicateIdError);
				Assert.AreEqual(roleId, roleDuplicateIdError.Id);
				Assert.IsTrue(errorMessages.Contains(error.ErrorMessage));

				errorMessages.Remove(error.ErrorMessage);
			}
		}

		[TestMethod]
		public void CreateWithExistingNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var role1 = new Role
			{
				Name = $"{prefix}_Role",
			};
			var role2 = new Role
			{
				Name = $"{prefix}_Role",
			};

			objectCreator.CreateRole(role1);

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateRole(role2);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleNameExistsError = roleError as RoleNameExistsError;
			Assert.IsNotNull(roleNameExistsError);
			Assert.AreEqual(role2.Id, roleNameExistsError.Id);
			Assert.AreEqual(role2.Name, roleNameExistsError.Name);
			Assert.AreEqual(errorMessage, roleNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithSameNameInBulkThrowsException()
		{
			var prefix = Guid.NewGuid();

			var role1 = new Role
			{
				Name = $"{prefix}_Role",
			};
			var role2 = new Role
			{
				Name = $"{prefix}_Role",
			};

			var rolesToCreate = new List<Role> { role1, role2};

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				objectCreator.CreateRoles(rolesToCreate);
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.TraceDataPerItem.Count);

			foreach (var traceData in expectedException.Result.TraceDataPerItem.Values)
			{
				Assert.AreEqual(1, traceData.ErrorData.Count);
				var roleError = traceData.ErrorData.OfType<RoleError>().SingleOrDefault();
				Assert.IsNotNull(roleError);

				var roleDuplicateNameError = roleError as RoleDuplicateNameError;
				Assert.IsNotNull(roleDuplicateNameError);

				var role = rolesToCreate.Single(c => c.Id == roleDuplicateNameError.Id);
				Assert.IsNotNull(role);

				Assert.AreEqual(role.Name, roleDuplicateNameError.Name);
				Assert.AreEqual($"Role '{role.Name}' has a duplicate name.", roleDuplicateNameError.ErrorMessage);
			}
		}

		[TestMethod]
		public void UpdateToSameNameThrowsException()
		{
			var prefix = Guid.NewGuid();

			var role1 = new Role
			{
				Name = $"{prefix}_Role1",
			};
			var role2 = new Role
			{
				Name = $"{prefix}_Role2",
			};

			var createdRoles = objectCreator.CreateRoles([role1, role2]);
			var toUpdate = createdRoles.Single(x => x.Id == role2.Id);
			toUpdate.Name = role1.Name;

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				TestContext.Api.Roles.Update(toUpdate);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			var errorMessage = "Name is already in use.";
			Assert.AreEqual(errorMessage, expectedException.Message);

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleNameExistsError = roleError as RoleNameExistsError;
			Assert.IsNotNull(roleNameExistsError);
			Assert.AreEqual(toUpdate.Id, roleNameExistsError.Id);
			Assert.AreEqual(toUpdate.Name, roleNameExistsError.Name);
			Assert.AreEqual(errorMessage, roleNameExistsError.ErrorMessage);
		}

		[TestMethod]
		public void UpdateUnmodifiedRole()
		{
			var role = new Role
			{
				Name = $"{Guid.NewGuid()}_Role",
			};

			role = objectCreator.CreateRole(role);

			var originalRole = TestContext.Api.Roles.Read(role.Id);
			var updatedRole = TestContext.Api.Roles.Update(originalRole);

			Assert.AreEqual(originalRole, updatedRole);
		}

		[TestMethod]
		public void BulkUpdateWithChangedAndUnchangedRoleReturnsTwoRoles()
		{
			var prefix = Guid.NewGuid();

			var changedRole = new Role { Name = $"{prefix}_Changed" };
			var unchangedRole = new Role { Name = $"{prefix}_Unchanged" };

			objectCreator.CreateRoles([changedRole, unchangedRole]);

			var changedToUpdate = TestContext.Api.Roles.Read(changedRole.Id);
			var unchangedToUpdate = TestContext.Api.Roles.Read(unchangedRole.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";

			var updatedRoles = TestContext.Api.Roles.Update(new[] { changedToUpdate, unchangedToUpdate });

			Assert.AreEqual(2, updatedRoles.Count);
			Assert.IsTrue(updatedRoles.Any(x => x.Id == changedRole.Id));
			Assert.IsTrue(updatedRoles.Any(x => x.Id == unchangedRole.Id));

			var changedAfterUpdate = TestContext.Api.Roles.Read(changedRole.Id);
			var unchangedAfterUpdate = TestContext.Api.Roles.Read(unchangedRole.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(unchangedRole.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void BulkUpdateWithChangedInvalidAndUnchangedRoleReturnsTwoSuccessfulIds()
		{
			var prefix = Guid.NewGuid();

			var changedRole = new Role { Name = $"{prefix}_Changed" };
			var invalidRole = new Role { Name = $"{prefix}_Invalid" };
			var unchangedRole = new Role { Name = $"{prefix}_Unchanged" };

			objectCreator.CreateRoles([changedRole, invalidRole, unchangedRole]);

			var changedToUpdate = TestContext.Api.Roles.Read(changedRole.Id);
			var invalidToUpdate = TestContext.Api.Roles.Read(invalidRole.Id);
			var unchangedToUpdate = TestContext.Api.Roles.Read(unchangedRole.Id);

			changedToUpdate.Name = $"{prefix}_Changed_Updated";
			invalidToUpdate.Name = string.Empty;

			PeopleAndOrganizationsBulkException<Guid>? expectedException = null;
			try
			{
				TestContext.Api.Roles.Update(new[] { changedToUpdate, invalidToUpdate, unchangedToUpdate });
			}
			catch (PeopleAndOrganizationsBulkException<Guid> ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(2, expectedException.Result.SuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(changedRole.Id));
			Assert.IsTrue(expectedException.Result.SuccessfulIds.Contains(unchangedRole.Id));
			Assert.AreEqual(1, expectedException.Result.UnsuccessfulIds.Count);
			Assert.IsTrue(expectedException.Result.UnsuccessfulIds.Contains(invalidRole.Id));

			var changedAfterUpdate = TestContext.Api.Roles.Read(changedRole.Id);
			var invalidAfterUpdate = TestContext.Api.Roles.Read(invalidRole.Id);
			var unchangedAfterUpdate = TestContext.Api.Roles.Read(unchangedRole.Id);

			Assert.AreEqual(changedToUpdate.Name, changedAfterUpdate.Name);
			Assert.AreEqual(invalidRole.Name, invalidAfterUpdate.Name);
			Assert.AreEqual(unchangedRole.Name, unchangedAfterUpdate.Name);
		}

		[TestMethod]
		public void ReadWithEmptyListReturnsEmptyList()
		{
			var roles = TestContext.Api.Roles.Read(new List<Guid>());
			Assert.IsNotNull(roles);
			Assert.AreEqual(0, roles.Count());
		}

		[TestMethod]
		public void ReadWithEmptyFilterReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Role>(idsToRetrieve.Select(x => RoleExposers.Id.Equal(x)).ToArray());

			var roles = TestContext.Api.Roles.Read(emptyFilter);
			Assert.IsNotNull(roles);
			Assert.AreEqual(0, roles.Count());
		}

		[TestMethod]
		public void CountWithEmptyFilterReturnsZero()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Role>(idsToRetrieve.Select(x => RoleExposers.Id.Equal(x)).ToArray());

			var count = TestContext.Api.Roles.Count(emptyFilter);
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void ReadWithEmptyQueryReturnsEmptyList()
		{
			var idsToRetrieve = new Guid[0];
			var emptyFilter = new ORFilterElement<Role>(idsToRetrieve.Select(x => RoleExposers.Id.Equal(x)).ToArray());
			var queryWithEmptyFilter = emptyFilter.ToQuery();

			var roles = TestContext.Api.Roles.Read(queryWithEmptyFilter);
			Assert.IsNotNull(roles);
			Assert.AreEqual(0, roles.Count());
		}

		[TestMethod]
		public void CreateWithNullNameThrowsException()
		{
			var role = new Role
			{
				Name = null,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateRole(role);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleInvalidNameError = roleError as RoleInvalidNameError;
			Assert.IsNotNull(roleInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", roleInvalidNameError.ErrorMessage);
		}

		[TestMethod]
		public void CreateWithEmptyNameThrowsException()
		{
			var role = new Role
			{
				Name = string.Empty,
			};

			PeopleAndOrganizationsException? expectedException = null;
			try
			{
				objectCreator.CreateRole(role);
			}
			catch (PeopleAndOrganizationsException ex)
			{
				expectedException = ex;
			}

			Assert.IsNotNull(expectedException, "Expected exception was not thrown.");

			Assert.AreEqual(1, expectedException.TraceData.ErrorData.Count);
			var roleError = expectedException.TraceData.ErrorData.OfType<RoleError>().SingleOrDefault();
			Assert.IsNotNull(roleError);

			var roleInvalidNameError = roleError as RoleInvalidNameError;
			Assert.IsNotNull(roleInvalidNameError);
			Assert.AreEqual($"Name cannot be empty.", roleInvalidNameError.ErrorMessage);
		}
	}
}
