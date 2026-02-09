namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Logging;

    /// <summary>
    /// Defines the contract for the People and Organizations API.
    /// </summary>
    public interface IPeopleAndOrganizationsApi
    {
        /// <summary>
        /// Determines whether the People and Organizations application is installed on the DataMiner System.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the application is installed; otherwise, <c>false</c>.
        /// </returns>
        bool IsInstalled();

        /// <summary>
        /// Determines whether the People and Organizations application is installed on the DataMiner System.
        /// </summary>
        /// <param name="version">
        /// When this method returns <c>true</c>, contains the version of the installed application;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the application is installed; otherwise, <c>false</c>.
        /// </returns>
        bool IsInstalled(out string version);

        /// <summary>
        /// Sets the logger to be used by the People and Organizations API.
        /// </summary>
        /// <param name="logger">The logger instance to use for logging operations.</param>
        void SetLogger(ILogger logger);
    }
}
