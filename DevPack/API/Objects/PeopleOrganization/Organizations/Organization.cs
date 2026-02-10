namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;

    using StoragePeopleAndOrganizations = Storage.DOM.SlcPeople_Organizations;

    /// <summary>
    /// Represents an organization in People and Organizations.
    /// </summary>
    public class Organization : ApiObject
    {
        private StoragePeopleAndOrganizations.OrganizationsInstance originalInstance;

        internal Organization(StoragePeopleAndOrganizations.OrganizationsInstance instance) : base(instance.ID.Id)
        {
            ParseInstance(instance);
            InitTracking();
        }

        /// <summary>
        /// Gets or sets the name of the organization.
        /// </summary>
        public override string Name { get; set; }

        private void ParseInstance(StoragePeopleAndOrganizations.OrganizationsInstance instance)
        {
            this.originalInstance = instance ?? throw new ArgumentNullException(nameof(instance));

            Name = instance.OrganizationInformation.OrganizationName;
        }
    }
}
