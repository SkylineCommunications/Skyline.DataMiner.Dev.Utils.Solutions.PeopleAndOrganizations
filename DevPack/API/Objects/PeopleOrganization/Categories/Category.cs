namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

#if !ABSTRACTIONS
	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;
#endif

	/// <summary>
	/// Represents a category in People and Organizations.
	/// </summary>
	public class Category : ApiObject
	{
#if !ABSTRACTIONS
		private StoragePeopleAndOrganizations.CategoryInstance originalInstance;
		private StoragePeopleAndOrganizations.CategoryInstance updatedInstance;
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="Category"/> class.
		/// </summary>
		public Category() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Category"/> class with a specific category ID.
		/// </summary>
		/// <param name="roleId">The unique identifier of the category.</param>
		public Category(Guid roleId) : base(roleId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

#if !ABSTRACTIONS
		internal Category(StoragePeopleAndOrganizations.CategoryInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}
#endif

		/// <summary>
		/// Gets or sets the name of the category.
		/// </summary>
		public override string Name { get; set; }

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.CategoryInstance OriginalInstance => originalInstance;
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
		/// Determines whether the specified object is equal to the current Category instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>True if the specified object is equal to the current instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Category other)
			{
				return false;
			}

			return Id == other.Id &&
				   Name == other.Name;
		}

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.CategoryInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.CategoryInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.CategoryInformation.Category = Name;

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.CategoryInstance instance)
		{
			this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.CategoryInformation.Category;
		}
#endif
	}
}
