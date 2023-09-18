namespace Credfeto.Database.Source.Generation.Helpers;

public static class VersionInformation
{
    public static string Version()
    {
        return ThisAssembly.Info.FileVersion;
    }
}