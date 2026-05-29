namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
using System;
using Skyline.DataMiner.Net;

public static class PeopleAndOrganizationsApiFactory
{
private static readonly object _syncRoot = new object();
private static Func<IConnection, IPeopleAndOrganizationsApi> createImplementation;

public static IPeopleAndOrganizationsApi Create(IConnection connection)
{
if (connection == null)
{
throw new ArgumentNullException(nameof(connection));
}

var implementation = createImplementation;
if (implementation == null)
{
throw new InvalidOperationException("No PeopleAndOrganizations API implementation has been registered. Ensure Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations is loaded before calling PeopleAndOrganizationsApiFactory.Create.");
}

return implementation(connection);
}

internal static void Register(Func<IConnection, IPeopleAndOrganizationsApi> factory)
{
if (factory == null)
{
throw new ArgumentNullException(nameof(factory));
}

lock (_syncRoot)
{
createImplementation = factory;
}
}
}
}
