using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Diagnostics;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class SummaryNode : ContentNode
{
    public List<ParaNode>? Paras { get; set; }
    public List<RemarksNode>? Remarks { get; set; }

    public override void Read(XmlNode node)
    {
        if (!node.HasChildNodes) return;

        foreach (XmlNode item in node.ChildNodes)
        {
            if (item.Name == "para")
            {
                var para = new ParaNode();
                para.Read(item);
                Paras ??= [];
                Paras.Add(para);
            }
            else if (item.Name == "#text")
            {
                Content = item.InnerText?.Trim();
            }
            else if (item.Name == "remarks")
            {
                Remarks ??= [];
                var remarks = new RemarksNode();
                remarks.Read(item);
                Remarks.Add(remarks);
            }
            else
            {
                Debug.WriteLine($"SummaryNode.Read() failed with name:{item.Name}");
            }
        }
    }
}
