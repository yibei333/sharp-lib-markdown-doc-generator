using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;

internal abstract class ContentNode : Node
{
    public string? Content { get; set; }

    public override void Read(XmlNode node)
    {
        Content = node.InnerText?.Trim();
    }
}
