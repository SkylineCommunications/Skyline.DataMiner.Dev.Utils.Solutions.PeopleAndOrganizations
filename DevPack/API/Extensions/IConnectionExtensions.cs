namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;

    using Skyline.DataMiner.Net;

    /// <summary>
    /// Defines extension methods on the <see cref="IConnection"/> class.
    /// </summary>
    public static class IConnectionExtensions
    {
        /// <summary>
        /// Retrieves an instance of the <see cref="IPeopleAndOrganizationsApi"/> interface."/>
        /// </summary>
        /// <param name="connection">The <see cref="IConnection"/> instance.</param>
        /// <returns>Instance of the <see cref="IPeopleAndOrganizationsApi"/> interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
        public static IPeopleAndOrganizationsApi GetPeopleAndOrganizationsApi(this IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return new PeopleAndOrganizationsApi(connection);
        }
    }
}
