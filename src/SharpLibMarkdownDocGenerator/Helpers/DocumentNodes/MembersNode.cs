using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class MembersNode : Node
{
    public List<MemberNode>? Members { get; set; }

    public override void Read(XmlNode node)
    {
        if (!node.HasChildNodes) return;

        Members = [];
        foreach (XmlNode item in node.ChildNodes)
        {
            var memberNode = new MemberNode();
            memberNode.Read(item);
            Members.Add(memberNode);
        }
    }
}
