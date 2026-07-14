using System.Reflection;

namespace OnibusExpress.Api.Common;

/// <summary>
/// Application version exposed by GET /version. Fed by the APP_VERSION build-arg (from
/// scripts/version.sh) in Docker; falls back to the assembly informational version.
/// </summary>
public static class AppVersion
{
    public static string Current { get; } =
        Environment.GetEnvironmentVariable("APP_VERSION")
        ?? Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0-dev";
}
