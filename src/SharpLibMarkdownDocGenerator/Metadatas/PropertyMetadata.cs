using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class PropertyMetadata : BaseMetadata<TypeMetadata>
{
    public PropertyMetadata(PropertyInfo propertyInfo, TypeMetadata parent) : base(parent)
    {
        PropertyInfo = propertyInfo;
        Name = propertyInfo.Name;
        var xmlKey = $"P:{propertyInfo.DeclaringType!.Namespace}.{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}";
        MemberNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == xmlKey);
        DocSummary = MemberNode?.Summary?.Content ?? "-";
        DocSummaryParas = MemberNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        Signature = GetSignature();
    }
    public PropertyInfo PropertyInfo { get; }
    public MemberNode? MemberNode { get; }
    public string Name { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public string Signature { get; }
    public bool IsStatic => PropertyInfo.GetAccessors(true)[0].IsStatic;

    List<string> GetAttributes() => PropertyInfo.CustomAttributes.Where(x => x.AttributeType.IsPublic).Select(x => x.ToString()).ToList();

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));
        builder.Append("public ");
        var accessor = PropertyInfo.GetAccessors(true)[0];
        if (accessor.IsStatic) builder.Append("static ");
        else if (accessor.IsAbstract) builder.Append("abstract ");
        else if (accessor.IsVirtual) builder.Append("virtual ");
        builder.Append(PropertyInfo.PropertyType.GetTypeDefinitionName()).Append(' ');
        builder.Append(Name).Append(" { ");

        //get
        var getMethod = PropertyInfo.GetGetMethod(true);
        if (getMethod is not null)
        {
            if (getMethod.IsPrivate) builder.Append("private ");
            else if (getMethod.IsFamily) builder.Append("proptected ");
            else if (getMethod.IsPublic) builder.Append("");
            else builder.Append("internal ");
            builder.Append("get; ");
        }

        //set
        var setMethod = PropertyInfo.GetSetMethod(true);
        if (setMethod is not null)
        {
            if (setMethod.IsPrivate) builder.Append("private ");
            else if (setMethod.IsFamily) builder.Append("proptected ");
            else if (setMethod.IsPublic) builder.Append("");
            else builder.Append("internal ");
            builder.Append("set; ");
        }

        builder.Append('}');
        return builder.ToString();
    }
}