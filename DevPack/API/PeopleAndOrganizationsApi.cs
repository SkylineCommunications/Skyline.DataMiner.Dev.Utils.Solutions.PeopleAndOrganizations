namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
    using System;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Logging;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.Tools;

    /// <summary>
    /// Provides the main entry point for interacting with the People and Organizations API.
    /// </summary>
    public class PeopleAndOrganizationsApi : IPeopleAndOrganizationsApi
    {
        private readonly IConnection connection;

        private readonly InstalledAppPackageCache installedAppPackages;

        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeopleAndOrganizationsApi"/> class.
        /// </summary>
        /// <param name="connection">The connection to use for API operations.</param>
        public PeopleAndOrganizationsApi(IConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.logger = new NullLogger();

            installedAppPackages = new InstalledAppPackageCache(connection);
        }

        /// <inheritdoc/>
        public bool IsInstalled(out string version)
        {
            var isInstalled = installedAppPackages.IsInstalled("SLC-S-MediaOps", out var installedAppInfo);
            version = isInstalled ? installedAppInfo?.AppInfo?.Version : null;
            return isInstalled;
        }

        /// <inheritdoc/>
        public bool IsInstalled()
        {
            return IsInstalled(out _);
        }

        /// <inheritdoc/>
        public void SetLogger(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
