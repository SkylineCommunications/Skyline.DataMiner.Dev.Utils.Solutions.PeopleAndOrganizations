namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
internal static class PeopleAndOrganizationsApiFactoryRegistration
{
private static readonly object _syncRoot = new object();
private static bool isRegistered;

internal static void EnsureRegistered()
{
lock (_syncRoot)
{
if (isRegistered)
{
return;
}

PeopleAndOrganizationsApiFactory.Register(connection => new PeopleAndOrganizationsApi(connection));
isRegistered = true;
}
}
}
}
