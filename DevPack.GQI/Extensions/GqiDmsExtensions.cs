namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Defines extension methods on the <see cref="GqiDmsExtensions"/> class.
    /// </summary>
    public static class GqiDmsExtensions
    {
        /// <summary>
        /// Retrieves an instance of the <see cref="IPeopleAndOrganizationsApi"/> interface."/>
        /// </summary>
        /// <param name="dms">The <see cref="GQIDMS"/> instance.</param>
        /// <returns>Instance of the <see cref="IPeopleAndOrganizationsApi"/> interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dms"/> is <see langword="null" />.</exception>
        public static IPeopleAndOrganizationsApi GetPeopleAndOrganizationsApi(this GQIDMS dms)
        {
            if (dms == null)
            {
                throw new ArgumentNullException(nameof(dms));
            }

            return new PeopleAndOrganizationsApi(dms.GetConnection());
        }
    }
}
