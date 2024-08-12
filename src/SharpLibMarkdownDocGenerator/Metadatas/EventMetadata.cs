using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class EventMetadata : BaseMetadata<TypeMetadata>
{
    public EventMetadata(EventInfo eventInfo, TypeMetadata parent) : base(parent)
    {
        EventInfo = eventInfo;
        Name = eventInfo.Name;
        var xmlKey = $"E:{eventInfo.DeclaringType!.Namespace}.{eventInfo.DeclaringType.Name}.{eventInfo.Name}";
        MemberNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == xmlKey);
        DocSummary = MemberNode?.Summary?.Content ?? "-";
        DocSummaryParas = MemberNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        Accssor = GetAccessor();
        Signature = GetSignature();
    }

    public EventInfo EventInfo { get; }
    public MemberNode? MemberNode { get; }
    public string Name { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public string Accssor { get; }
    public string Signature { get; }

    List<string> GetAttributes() => EventInfo.CustomAttributes.Where(x => x.AttributeType.IsPublic).Select(x => x.ToString()).ToList();

    string GetAccessor()
    {
        var addMethod = EventInfo.GetAddMethod(true)!;
        if (addMethod.IsFamily) return "protected";
        return "public";
    }

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));
        builder.Append(Accssor).Append(" event ");
        builder.Append(EventInfo.EventHandlerType?.GetTypeDefinitionName());
        builder.Append(' ');
        builder.Append(Name);
        builder.Append(';');
        return builder.ToString();
    }
}