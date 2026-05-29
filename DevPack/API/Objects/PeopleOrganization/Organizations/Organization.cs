namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Extensions;

#if !ABSTRACTIONS
	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;
#endif

	/// <summary>
	/// Represents an organization in People and Organizations.
	/// </summary>
	public class Organization : ApiObject
	{
#if !ABSTRACTIONS
		private StoragePeopleAndOrganizations.OrganizationsInstance originalInstance;
		private StoragePeopleAndOrganizations.OrganizationsInstance updatedInstance;
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="Organization"/> class.
		/// </summary>
		public Organization() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Organization"/> class with a specific organization ID.
		/// </summary>
		/// <param name="organizationId">The unique identifier of the organization.</param>
		public Organization(Guid organizationId) : base(organizationId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

#if !ABSTRACTIONS
		internal Organization(StoragePeopleAndOrganizations.OrganizationsInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}
#endif

		/// <summary>
		/// Gets or sets the name of the organization.
		/// </summary>
		public override string Name { get; set; }

		/// <summary>
		/// Gets or sets the ID of the category associated with the organization.
		/// </summary>
		public Guid CategoryId { get; set; }

		/// <summary>
		/// Gets the state of the organization.
		/// </summary>
		public OrganizationState State { get; private set; }

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.OrganizationsInstance OriginalInstance => originalInstance;
#endif

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + Id.GetHashCode();
				hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
				hash = (hash * 23) + CategoryId.GetHashCode();
				hash = (hash * 23) + State.GetHashCode();

				return hash;
			}
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current Organization instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current Organization instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Organization other)
			{
				return false;
			}

			return Id == other.Id &&
				   Name == other.Name &&
				   CategoryId == other.CategoryId &&
				   State == other.State;
		}

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.OrganizationsInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.OrganizationsInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.OrganizationInformation.OrganizationName = Name;
			updatedInstance.OrganizationInformation.Category = CategoryId != Guid.Empty ? CategoryId : null;

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.OrganizationsInstance instance)
		{
			this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.OrganizationInformation.OrganizationName;
			CategoryId = instance.OrganizationInformation.Category ?? Guid.Empty;

			State = EnumExtensions.MapEnum<StoragePeopleAndOrganizations.SlcPeople_OrganizationsIds.Behaviors.Organizations_Behavior.StatusesEnum, OrganizationState>(instance.Status);
		}
#endif
	}
}
