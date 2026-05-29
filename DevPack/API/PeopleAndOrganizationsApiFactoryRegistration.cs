namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
internal static class PeopleAndOrganizationsApiFactoryRegistration
{
private static readonly object _syncRoot = new object();
private static bool _isRegistered;

internal static void EnsureRegistered()
{
lock (_syncRoot)
{
if (_isRegistered)
{
return;
}

PeopleAndOrganizationsApiFactory.Register(connection => new PeopleAndOrganizationsApi(connection));
_isRegistered = true;
}
}
}
}
