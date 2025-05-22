using System.Reflection;
using System.Runtime.Loader;

namespace Evently.Api.Extensions;

internal static class AssemblyExtensions
{
    public static Assembly GetModuleAssembly(this Assembly source, string moduleName, string layerName)
    {
        string assemblyName = $"Evently.Modules.{moduleName}.{layerName}";
        string executingAssemblyPath = source.Location;
        string? directory = Path.GetDirectoryName(executingAssemblyPath);

        if (directory == null)
        {
            throw new InvalidOperationException($"Assembly {assemblyName} not found.");
        }

        string dllPath = Path.Combine(directory, $"{assemblyName}.dll");
        if (!File.Exists(dllPath))
        {
            throw new InvalidOperationException($"Assembly {assemblyName} not found.");
        }

        try
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load assembly {assemblyName} from {dllPath}: {ex.Message}");
        }
    }
}
