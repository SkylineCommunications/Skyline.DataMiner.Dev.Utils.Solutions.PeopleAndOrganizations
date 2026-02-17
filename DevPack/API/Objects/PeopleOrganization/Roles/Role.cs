namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

	/// <summary>
	/// Represents a role in People and Organizations.
	/// </summary>
	public class Role : ApiObject
	{
		private StoragePeopleAndOrganizations.RoleInstance originalInstance;
		private StoragePeopleAndOrganizations.RoleInstance updatedInstance;

		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class.
		/// </summary>
		public Role() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Role"/> class with a specific role ID.
		/// </summary>
		/// <param name="roleId">The unique identifier of the role.</param>
		public Role(Guid roleId) : base(roleId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

		internal Role(StoragePeopleAndOrganizations.RoleInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}

		/// <summary>
		/// The name of the role that a person has in a team.
		/// </summary>
		public override string Name { get; set; }

		internal StoragePeopleAndOrganizations.RoleInstance OriginalInstance => originalInstance;

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + Id.GetHashCode();
				hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);

				return hash;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current Role instance.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is not Role other)
			{
				return false;
			}

			return Id == other.Id &&
				   Name == other.Name;
		}

		internal StoragePeopleAndOrganizations.RoleInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.RoleInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.RoleInformation.Role = Name;

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.RoleInstance instance)
		{
			this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.RoleInformation.Role;
		}
	}
}
