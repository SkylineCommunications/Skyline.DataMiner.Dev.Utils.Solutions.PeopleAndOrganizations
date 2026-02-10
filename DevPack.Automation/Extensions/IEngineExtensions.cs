namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Defines extension methods on the <see cref="IEngine"/> interface.
    /// </summary>
    public static class IEngineExtensions
    {
        /// <summary>
        /// Retrieves an instance of the <see cref="IPeopleAndOrganizationsApi"/> interface."/>
        /// </summary>
        /// <param name="engine">The <see cref="IEngine"/> implementation.</param>
        /// <returns>Instance of the <see cref="IPeopleAndOrganizationsApi"/> interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null" />.</exception>
        public static IPeopleAndOrganizationsApi GetPeopleAndOrganizationsApi(this IEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            return engine.GetUserConnection().GetPeopleAndOrganizationsApi();
        }
    }
}
