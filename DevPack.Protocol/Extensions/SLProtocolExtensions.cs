namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Protocol
{
    using System;

    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Defines extension methods on the <see cref="SLProtocolExtensions"/> class.
    /// </summary>
    public static class SLProtocolExtensions
    {
        /// <summary>
        /// Retrieves an instance of the <see cref="IPeopleAndOrganizationsApi"/> interface."/>
        /// </summary>
        /// <param name="protocol">The <see cref="SLProtocol"/> instance.</param>
        /// <returns>Instance of the <see cref="IPeopleAndOrganizationsApi"/> interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="protocol"/> is <see langword="null" />.</exception>
        public static IPeopleAndOrganizationsApi GetPeopleAndOrganizationsApi(this SLProtocol protocol)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException(nameof(protocol));
            }

            return protocol.GetUserConnection().GetPeopleAndOrganizationsApi();
        }
    }
}
