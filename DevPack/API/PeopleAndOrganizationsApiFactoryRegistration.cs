namespace Skyline.DataMiner.Solutions.PeopleAndOrganizations.API
{
using System.Runtime.CompilerServices;

internal static class PeopleAndOrganizationsApiFactoryRegistration
{
[ModuleInitializer]
internal static void Register()
{
PeopleAndOrganizationsApiFactory.Register(connection => new PeopleAndOrganizationsApi(connection));
}
}
}

namespace System.Runtime.CompilerServices
{
#if !NET5_0_OR_GREATER
using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute
{
}
#endif
}
