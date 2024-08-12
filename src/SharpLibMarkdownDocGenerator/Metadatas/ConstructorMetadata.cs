using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class ConstructorMetadata : BaseMetadata<TypeMetadata>
{
    public ConstructorMetadata(ConstructorInfo constructorInfo, TypeMetadata parent) : base(parent)
    {
        Name = constructorInfo.GetConstructorDefinitionName(true);
        ConstructorInfo = constructorInfo;
        DeclareType = ConstructorInfo.DeclaringType!;
        var xmlKey = GetConstructorXmlKey();
        MemberNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == xmlKey);
        DocSummary = MemberNode?.Summary?.Content ?? "-";
        if (DocSummary == "-" && ConstructorInfo.GetParameters().Length == 0) DocSummary = "默认构造函数";
        DocSummaryParas = MemberNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        Signature = GetSignature();
    }

    Type DeclareType { get; }

    public MemberNode? MemberNode { get; }

    public string Name { get; }

    public string DocSummary { get; }

    public List<string> DocSummaryParas { get; }

    List<string> Attributes { get; }

    public string Signature { get; }

    public ConstructorInfo ConstructorInfo { get; }

    string GetConstructorXmlKey()
    {
        var builder = new StringBuilder();
        builder.Append("M:");
        builder.Append(DeclareType.Namespace);
        builder.Append('.');
        builder.Append(DeclareType.Name);
        builder.Append(".#ctor");
        var definitionName = ConstructorInfo.GetConstructorDefinitionName(false, true).Replace(", ", ",");
        definitionName = definitionName.Substring(definitionName.IndexOf('('));
        definitionName = definitionName.Replace('<', '{').Replace('>', '}');
        if (DeclareType.IsGenericType)
        {
            var index = 0;
            DeclareType.GetGenericArguments().ToList().ForEach(x =>
            {
                definitionName = definitionName.Replace("(" + x.Name + ")", "(`" + index + ")");
                definitionName = definitionName.Replace("(" + x.Name + ",", "(`" + index + ",");
                definitionName = definitionName.Replace("," + x.Name + ")", ",`" + index + ")");
                definitionName = definitionName.Replace("," + x.Name + ",", ",`" + index + ",");
                definitionName = definitionName.Replace("{" + x.Name + "}", "{`" + index + "}");
                index++;
            });
        }
        builder.Append(definitionName);
        var result = builder.ToString();
        result = result.TrimEnd("()");
        return result;
    }

    List<string> GetAttributes() => ConstructorInfo.CustomAttributes.Where(x => x.AttributeType.IsPublic).Select(x => x.ToString()).ToList();

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));

        builder.Append("public ");
        builder.Append(Name);
        return builder.ToString();
    }
}