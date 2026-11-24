# Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations

This package provides the People & Organizations API for managing people, organizations, teams, and related records. It targets .NET Framework 4.8.

## Installing the DOM schema

On a client created by the DevPack, call the `InstallDomModules()` extension in `Skyline.DataMiner.Solutions.PeopleAndOrganizations.API` to reconcile `(slc)people_organizations`. For MediaOps Plan deployments, run this before the Plan installer. It does not deploy scripts, initialize instances, or register the solution.

See [Installation and Setup](https://github.com/SkylineCommunications/Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations/blob/main/Documentation/Advanced%20Topics.md#installation-and-setup) for permissions, upgrade behavior, and retry guidance.
