namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Defines extension methods on the <see cref="Engine"/> class.
    /// </summary>
    public static class EngineExtensions
    {
        /// <summary>
        /// Retrieves an instance of the <see cref="IPeopleAndOrganizationsApi"/> interface."/>
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> instance.</param>
        /// <returns>Instance of the <see cref="IPeopleAndOrganizationsApi"/> interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null" />.</exception>
        public static IPeopleAndOrganizationsApi GetPeopleAndOrganizationsApi(this Engine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            return engine.GetUserConnection().GetPeopleAndOrganizationsApi();
        }
    }
}
