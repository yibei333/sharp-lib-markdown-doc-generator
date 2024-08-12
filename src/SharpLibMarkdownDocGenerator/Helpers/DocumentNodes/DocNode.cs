using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Diagnostics;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class DocNode : Node
{
    public AssemblyNode? AssemblyNode { get; set; }
    public MembersNode? MembersNode { get; set; }

    public override void Read(XmlNode node)
    {
        if (!node.HasChildNodes) return;

        foreach (XmlNode item in node.ChildNodes)
        {
            if (item.Name == "assembly")
            {
                AssemblyNode = new AssemblyNode();
                AssemblyNode.Read(item);
            }
            else if (item.Name == "members")
            {
                MembersNode = new MembersNode();
                MembersNode.Read(item);
            }
            else
            {
                Debug.WriteLine($"{item.Name} type not supported yet");
            }
        }
    }
}
