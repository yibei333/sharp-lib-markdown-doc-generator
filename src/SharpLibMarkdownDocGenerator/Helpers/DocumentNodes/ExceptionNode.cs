using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class ExceptionNode : ContentNode
{
    public string? Cref { get; set; }

    public override void Read(XmlNode node)
    {
        base.Read(node);
        Cref = node.Attributes?["cref"]?.Value;
    }
}
