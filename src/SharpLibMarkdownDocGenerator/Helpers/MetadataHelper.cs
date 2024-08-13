using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json.Nodes;

namespace SharpLibMarkdownDocGenerator.Helpers;

internal class MetadataHelper
{
    public MetadataHelper(string dllPath)
    {
        dllPath.ThrowIfFileNotExist();
        Name = dllPath.GetFileName(false);
        DllPath = dllPath;
        var depPath = new FileInfo(dllPath).Directory?.FullName.CombinePath($"{dllPath.GetFileName(false)}.deps.json");
        depPath.ThrowIfFileNotExist();
        DepPath = depPath;
        ResolveDependencyPaths();
        LoadAssemblies();
    }

    public string Name { get; }

    string DllPath { get; }

    string DepPath { get; }

    Dictionary<string, string> DependencyPaths { get; } = [];

    List<string> AssemblyPaths { get; } = [];

    public List<AssemblyInfo> Assemblies { get; } = [];

    void ResolveDependencyPaths()
    {
        var fileInfo = new FileInfo(DllPath);
        var directory = fileInfo.Directory;
        directory.ThrowIfDirectoryNotExist();
        var depJson = JsonNode.Parse(File.ReadAllText(DepPath)) ?? throw new InvalidDataException();

        var runtimeTarget = depJson["runtimeTarget"]?["name"]?.ToString();
        if (runtimeTarget.IsNullOrWhiteSpace()) throw new InvalidDataException();
        var dependencies = depJson["libraries"]?.AsObject() ?? throw new InvalidDataException();
        var targets = depJson["targets"]?[runtimeTarget]?.AsObject() ?? throw new InvalidDataException();

        foreach (var item in targets)
        {
            var type = dependencies[item.Key]?["type"]?.ToString();
            if (type == "project")
            {
                var path = item.Value?["runtime"]?.AsObject().Select(x => x.Key).FirstOrDefault();
                var fullPath = path.IsNullOrWhiteSpace() ? string.Empty : directory.FullName.CombinePath(path);
                if (fullPath.IsNullOrWhiteSpace()) continue;
                var name = new FileInfo(fullPath).Name.GetFileName(false);
                DependencyPaths.Add(name, fullPath);
                AssemblyPaths.Add(fullPath);
            }
            else if (type == "package")
            {
                var path = item.Value?["runtime"]?.AsObject().Select(x => x.Key).FirstOrDefault();
                var fullPath = path.IsNullOrWhiteSpace() ? string.Empty : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).CombinePath($".nuget/packages/{item.Key}/{path}");
                if (fullPath.IsNullOrWhiteSpace()) continue;
                var name = new FileInfo(fullPath).Name.GetFileName(false);
                DependencyPaths.Add(name, fullPath);
            }
        }
    }

    void LoadAssemblies()
    {
        AssemblyPaths.ForEach(x =>
        {
            var assemblyContext = new AssemblyLoadContext(x.GetFileName(false));
            assemblyContext.Resolving += Context_Resolving;
            var assembly = assemblyContext.LoadFromAssemblyPath(x);
            Assemblies.Add(new AssemblyInfo(x, assembly));
        });
    }

    Assembly Context_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        if (arg2.Name.IsNullOrWhiteSpace()) return null!;
        var path = DependencyPaths[arg2.Name];
        path.ThrowIfFileNotExist();
        var assemblyContext = new AssemblyLoadContext(arg2.Name);
        assemblyContext.Resolving += Context_Resolving;
        var assembly = assemblyContext.LoadFromAssemblyPath(path);
        return assembly;
    }
}

internal class AssemblyInfo
{
    internal AssemblyInfo(string dllPath, Assembly assembly)
    {
        DllPath = dllPath;
        XmlPath = new FileInfo(dllPath).Directory?.FullName.CombinePath($"{dllPath.GetFileName(false)}.xml");
        Assembly = assembly;
        Name = assembly.GetName().Name!;
        Types = Assembly.GetTypes().Where(x => x.IsPublic || x.IsNestedFamily || x.IsNestedPublic).ToList();
        Doc = XmlPath.NotNullOrWhiteSpace() && File.Exists(XmlPath) ? new DocumentHelper(XmlPath).Doc : null;
    }

    public Assembly Assembly { get; }
    public string Name { get; }
    public string DllPath { get; }
    public string? XmlPath { get; }
    public DocNode? Doc { get; }
    public List<Type> Types { get; }
}