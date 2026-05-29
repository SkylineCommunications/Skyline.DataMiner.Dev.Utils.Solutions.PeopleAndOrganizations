namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

#if !ABSTRACTIONS
	using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;
#endif

	/// <summary>
	/// Represents an experience in People and Organizations.
	/// </summary>
	public class Experience : ApiObject
	{
#if !ABSTRACTIONS
		private StoragePeopleAndOrganizations.ExperienceInstance originalInstance;
		private StoragePeopleAndOrganizations.ExperienceInstance updatedInstance;
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="Experience"/> class.
		/// </summary>
		public Experience() : base()
		{
			IsNew = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Experience"/> class with a specific experience ID.
		/// </summary>
		/// <param name="roleId">The unique identifier of the experience.</param>
		public Experience(Guid roleId) : base(roleId)
		{
			IsNew = true;
			HasUserDefinedId = true;
		}

#if !ABSTRACTIONS
		internal Experience(StoragePeopleAndOrganizations.ExperienceInstance instance) : base(instance.ID.Id)
		{
			ParseInstance(instance);
			InitTracking();
		}
#endif

		/// <summary>
		/// Gets or sets the name of the experience that a person has.
		/// </summary>
		public override string Name { get; set; }

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.ExperienceInstance OriginalInstance => originalInstance;
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
		/// Determines whether the specified object is equal to the current Experience instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (obj is not Experience other)
			{
				return false;
			}

			return Id == other.Id &&
				   Name == other.Name;
		}

#if !ABSTRACTIONS
		internal StoragePeopleAndOrganizations.ExperienceInstance GetInstanceWithChanges()
		{
			if (updatedInstance == null)
			{
				updatedInstance = IsNew ? new StoragePeopleAndOrganizations.ExperienceInstance(Id) : originalInstance.Clone();
			}

			updatedInstance.ExperienceInformation.Experience = Name;

			return updatedInstance;
		}

		private void ParseInstance(StoragePeopleAndOrganizations.ExperienceInstance instance)
		{
			this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Name = instance.ExperienceInformation.Experience;
		}
#endif
	}
}
