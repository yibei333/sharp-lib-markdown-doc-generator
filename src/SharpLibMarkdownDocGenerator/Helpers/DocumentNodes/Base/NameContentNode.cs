using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;

internal abstract class NameContentNode : ContentNode
{
    public string? Name { get; set; }

    public override void Read(XmlNode node)
    {
        base.Read(node);
        Name = node.Attributes?["name"]?.Value?.Trim();
    }
}
