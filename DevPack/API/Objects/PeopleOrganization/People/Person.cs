namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;

    using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

    /// <summary>
    /// Represents a person in People and Organizations.
    /// </summary>
    public class Person : ApiObject
    {
        private StoragePeopleAndOrganizations.PeopleInstance originalInstance;

        internal Person(StoragePeopleAndOrganizations.PeopleInstance instance) : base(instance.ID.Id)
        {
            ParseInstance(instance);
            InitTracking();
        }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public override string Name { get; set; }

		/// <summary>
		/// Gets or sets the Experience ID of the person.
		/// </summary>
		public Guid ExperienceId { get; set; }

        /// <summary>
        /// Gets or sets the Organization ID of the person.
        /// </summary>
        public Guid OrganizationId { get; set; }

        private void ParseInstance(StoragePeopleAndOrganizations.PeopleInstance instance)
        {
            this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

            Name = instance.PeopleInformation.FullName;
			ExperienceId = instance.PeopleInformation.ExperienceLevel ?? Guid.Empty;
            OrganizationId = instance.Organization.OrganizationId;
        }
    }
}
