namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
	using System;

	/// <summary>
	/// Represents a skill in People and Organizations.
	/// </summary>
	public class Skill : TrackableObject
	{
		public Skill()
		{
			IsNew = true;
		}

		internal Skill(string name)
		{
			Parse(name);
			InitTracking();
		}

		/// <summary>
		/// Gets or sets the name of the skill.
		/// </summary>
		public string Name { get; set; }

		internal string OriginalName { get; private set; }

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
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

			return Name == other.Name;
		}

		private void Parse(string name)
		{
			OriginalName = name ?? throw new ArgumentNullException(nameof(name));

			Name = name;
		}
	}
}
