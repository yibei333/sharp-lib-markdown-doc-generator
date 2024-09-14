using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using System.Diagnostics;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;

internal abstract class ContentNode : Node
{
    public string? Content { get; set; }

    List<ParaNode>? Paras { get; set; }

    public override void Read(XmlNode node)
    {
        Content = node.InnerText?.Trim();

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
            else
            {
                Debug.WriteLine($"ContentNode.Read() failed with name:{item.Name}");
            }
        }

        if (Paras.NotNullOrEmpty())
        {
            Content += "<br>";
            Content += string.Join("<br>",Paras.Select(x=>x.Content));
        }
    }
}
