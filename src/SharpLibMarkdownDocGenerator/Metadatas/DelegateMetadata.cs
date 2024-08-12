using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class DelegateMetadata : BaseMetadata<NamespaceMetadata>
{
    public DelegateMetadata(Type type, NamespaceMetadata parent) : base(parent)
    {
        Type = type;
        BelongDirectory = type.GetBelongDirectory();
        DocNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == $"T:{Type.Namespace}.{Type.Name}");
        DocSummary = DocNode?.Summary?.Content ?? string.Empty;
        DocSummaryParas = DocNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        BaseTypeChain = GetBaseTypeChain();
        BaseType = BaseTypeChain.LastOrDefault();
        Implementions = GetImplementions();
        DerivedTypes = GetDerivedTypes();
        Signature = GetSignature();
    }

    public Type Type { get; }
    public string TypeDefinitionName => Type.GetTypeDefinitionName();
    public string? BelongDirectory { get; }
    public MemberNode? DocNode { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public Type? BaseType { get; }
    public List<Type> BaseTypeChain { get; }
    public List<Type> Implementions { get; }
    public List<Type> DerivedTypes { get; }
    public string Signature { get; }

    static readonly List<Type> _ignoredAttributeTypes = [typeof(ExtensionAttribute)];
    List<string> GetAttributes() => Type.CustomAttributes.Where(x => x.AttributeType.IsPublic && !x.AttributeType.IsSpecialName && !_ignoredAttributeTypes.Any(y => x.AttributeType == y)).Select(x => x.ToString()).ToList();

    List<Type> GetBaseTypeChain()
    {
        var baseType = Type.BaseType;
        var baseTypeChain = new List<Type>();
        while (baseType is not null)
        {
            baseTypeChain.Add(baseType);
            baseType = baseType.BaseType;
        }
        baseTypeChain.Reverse();
        return baseTypeChain;
    }

    List<Type> GetImplementions() => [.. Type.GetInterfaces()];

    List<Type> GetDerivedTypes() => Type.Assembly.GetTypes().Where(x => x.IsPublic && (x.BaseType?.Name == Type.Name || x.GetInterface(Type.Name) is not null)).ToList();

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));

        builder.Append(Type.IsNestedFamily ? "protected " : "public ");
        builder.Append("delegate ");
        var methodInfo = Type.GetMethod("Invoke")!;
        builder.Append(methodInfo.ReturnType.GetTypeDefinitionName()).Append(' ').Append(Type.Name);
        builder.Append(methodInfo.GetMethodDefinitionName(true).TrimStart("Invoke"));
        return builder.ToString();
    }
}