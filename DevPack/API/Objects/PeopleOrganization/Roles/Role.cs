namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

#if !ABSTRACTIONS
	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;
#endif

	/// <summary>
	/// Represents a role in People and Organizations.
	/// </summary>
	public class Role : ApiObject
	{
#if !ABSTRACTIONS
		private StoragePeopleAndOrganizations.RoleInstance originalInstance;
		private StoragePeopleAndOrganizations.RoleInstance updatedInstance;
#endif

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

#if !ABSTRACTIONS
		internal Role(StoragePeopleAndOrganizations.RoleInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}
#endif

		/// <summary>
		/// Gets or sets the name of the role that a person has in a team.
		/// </summary>
		public override string Name { get; set; }

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.RoleInstance OriginalInstance => originalInstance;
#endif

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
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Role other)
			{
				return false;
			}

			return Id == other.Id &&
				   Name == other.Name;
		}

#if !ABSTRACTIONS
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
#endif
	}
}
