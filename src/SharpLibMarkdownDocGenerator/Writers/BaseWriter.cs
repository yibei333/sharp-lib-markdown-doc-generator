using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.Encode;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;
using System.Reflection;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Writers;

internal abstract class BaseWriter(BaseMetadata metadata)
{
    public RootMetadata RootMetadata { get; } = metadata.Root;

    public void Write()
    {
        WriteInternal();
        if (RootMetadata.Request.AddtionalMarkdownDirecotry.NotNullOrWhiteSpace())
        {
            var additionalMarkdown = RootMetadata.Request.AddtionalMarkdownDirecotry.CombinePath(MarkdownPath.GetFileName());
            if (File.Exists(additionalMarkdown))
            {
                Builder.AppendLine();
                Builder.AppendLine(File.ReadAllText(additionalMarkdown));
            }
        }
        Builder.ToString().Utf8Decode().SaveToFile(MarkdownPath);
    }

    protected abstract void WriteInternal();

    public abstract string MarkdownPath { get; }

    protected StringBuilder Builder { get; } = new StringBuilder();

    public TWriter? GetWriter<TWriter>() where TWriter : class
    {
        object current = this;
        while (current is not null)
        {
            var propertyInfo = current.GetType().GetProperty("Parent");
            if (propertyInfo is null) return null;
            if (current is TWriter writer) return writer;
            current = propertyInfo.GetValue(current);
        }
        return null;
    }

    #region MarkdownPath
    readonly Dictionary<string, string> _mdPaths = [];

    static string FormatMarkdownName(string name)
    {
        var result = name.Replace('`', '.').Replace(',', '.').Replace('<', '.').Replace('>', '.').Replace('(', '.').Replace(')', '.').Replace(" ", "").Replace("[", "").Replace("]", "").Trim();
        while (result.Contains(".."))
        {
            result = result.Replace("..", ".");
        }
        return result.TrimEnd('.');
    }

    string GetMarkdownPath(string key)
    {
        key = FormatMarkdownName(key);
        if (!_mdPaths.TryGetValue(key, out string value))
        {
            var path = RootMetadata.Request.OutputPath.CombinePath($"{RootMetadata.Name}/{key}.md");
            value = path;
            _mdPaths.Add(key, value);
        }
        return value;
    }

    public string GetRootMarkdownPath() => GetMarkdownPath("Index");

    public string GetAssemblyMarkdownPath(AssemblyInfo assemblyInfo) => GetMarkdownPath($"{assemblyInfo.Name}.assembly");

    public string GetNamespaceMarkdownPath(string @namespace) => GetMarkdownPath($"{@namespace}.namespace");

    public string GetTypeMarkdownPath(Type type)
    {
        var typeName = $"{type.Namespace}.{type.Name}";
        if (RootMetadata.Types.Any(x => x.Namespace == type.Namespace && x.Name == type.Name)) return GetMarkdownPath(typeName);
        return string.Empty;
    }

    public string GetConstructorMarkdownPath(ConstructorInfo constructorInfo)
    {
        var name = constructorInfo.GetConstructorDefinitionName(false);
        var declareName = constructorInfo.DeclaringType!.Name;
        if (declareName.Contains('`')) declareName = declareName.Substring(declareName.IndexOf('`'));
        name = name.TrimStart(declareName).TrimEnd("()").TrimStart('(').TrimEnd(')').Trim();
        return GetMarkdownPath($"{constructorInfo.DeclaringType!.Namespace}.{constructorInfo.DeclaringType.Name}.ctor{(name.IsNullOrWhiteSpace() ? "" : $".{name}")}");
    }

    public string GetMethodMarkdownPath(MethodInfo methodInfo) => GetMarkdownPath($"{methodInfo.DeclaringType!.Namespace}.{methodInfo.DeclaringType.Name}.{methodInfo.GetMethodDefinitionName(false)}");

    public string GetFieldMarkdownPath(FieldInfo fieldInfo) => GetMarkdownPath($"{fieldInfo.DeclaringType!.Namespace}.{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}");

    public string GetPropertyMarkdownPath(PropertyInfo propertyInfo) => GetMarkdownPath($"{propertyInfo.DeclaringType!.Namespace}.{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}");

    public string GetEventMarkdownPath(EventInfo eventInfo) => GetMarkdownPath($"{eventInfo.DeclaringType!.Namespace}.{eventInfo.DeclaringType.Name}.{eventInfo.Name}");

    public string GetExternalTypeUrl(Type type)
    {
        return RootMetadata.Request.ExternalUrlResolver(type);
    }
    #endregion
}

internal abstract class BaseWriter<TMetadata, TParent>(TMetadata metadata, TParent parent) : BaseWriter(metadata) where TParent : BaseWriter where TMetadata : BaseMetadata
{
    public TMetadata Metadata { get; } = metadata;

    public TParent Parent { get; } = parent;
}