namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
using System;
using System.Reflection;
using Skyline.DataMiner.Net;

public static class PeopleAndOrganizationsApiFactory
{
private static readonly object _syncRoot = new object();
private const string MainAssemblyName = "Skyline.DataMiner.Dev.Utils.Solutions.PeopleAndOrganizations";
private const string FactoryRegistrationTypeName = "Skyline.DataMiner.Solutions.PeopleAndOrganizations.API.PeopleAndOrganizationsApiFactoryRegistration";
private const string EnsureRegisteredMethodName = "EnsureRegistered";
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
TryEnsureRegistered();
implementation = createImplementation;
}

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

private static void TryEnsureRegistered()
{
lock (_syncRoot)
{
if (createImplementation != null)
{
return;
}

Assembly mainAssembly;
try
{
mainAssembly = Assembly.Load(MainAssemblyName);
}
catch
{
return;
}

var registrationType = mainAssembly.GetType(FactoryRegistrationTypeName, false);
var ensureRegisteredMethod = registrationType?.GetMethod(EnsureRegisteredMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
ensureRegisteredMethod?.Invoke(null, null);
}
}
}
}
