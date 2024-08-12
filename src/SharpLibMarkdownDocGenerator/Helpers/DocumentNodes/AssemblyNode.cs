using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Diagnostics;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class AssemblyNode : Node
{
    public string? Name { get; set; }

    public override void Read(XmlNode node)
    {
        if (!node.HasChildNodes) return;

        foreach (XmlNode item in node.ChildNodes)
        {
            if (item.Name == "name")
            {
                Name = item.InnerText?.Trim();
            }
            else
            {
                Debug.WriteLine($"AssemblyNode.Read() failed with name '{item.Name}'");
            }
        }
    }
}
