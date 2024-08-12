using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class FieldMetadata : BaseMetadata<TypeMetadata>
{
    public FieldMetadata(FieldInfo fieldInfo, TypeMetadata parent) : base(parent)
    {
        FieldInfo = fieldInfo;
        Name = fieldInfo.Name;
        var xmlKey = $"F:{fieldInfo.DeclaringType!.Namespace}.{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}";
        MemberNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == xmlKey);
        DocSummary = MemberNode?.Summary?.Content ?? "-";
        DocSummaryParas = MemberNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        Signature = GetSignature();
    }

    public FieldInfo FieldInfo { get; }
    public MemberNode? MemberNode { get; }
    public string Name { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public string Signature { get; }

    List<string> GetAttributes() => FieldInfo.CustomAttributes.Where(x => x.AttributeType.IsPublic).Select(x => x.ToString()).ToList();

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));
        builder.Append("public ");
        if (FieldInfo.IsStatic) builder.Append("static ");
        builder.Append(FieldInfo.FieldType.GetTypeDefinitionName()).Append(' ');
        builder.Append(Name);
        builder.Append(';');
        return builder.ToString();
    }
}