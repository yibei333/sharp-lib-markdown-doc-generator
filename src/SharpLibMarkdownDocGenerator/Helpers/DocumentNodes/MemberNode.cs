using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;
using System.Diagnostics;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;

internal class MemberNode : Node
{
    public string? Name { get; set; }
    public SummaryNode? Summary { get; set; }
    public List<TypeParamNode>? TypeParams { get; set; }
    public List<ParamNode>? Params { get; set; }
    public ReturnNode? Return { get; set; }
    public List<ExceptionNode>? Exceptions { get; set; }

    public override void Read(XmlNode node)
    {
        Name = node.Attributes?["name"]?.Value?.Trim();
        if (!node.HasChildNodes) return;

        foreach (XmlNode item in node.ChildNodes)
        {
            if (item.Name == "summary")
            {
                Summary = new SummaryNode();
                Summary.Read(item);
            }
            else if (item.Name == "#text")
            {
                Summary = new SummaryNode { Content = item.InnerText.Trim() };
            }
            else if (item.Name == "param")
            {
                var param = new ParamNode();
                param.Read(item);
                Params ??= [];
                Params.Add(param);
            }
            else if (item.Name == "typeparam")
            {
                var typeParam = new TypeParamNode();
                typeParam.Read(item);
                TypeParams ??= [];
                TypeParams.Add(typeParam);
            }
            else if (item.Name == "returns")
            {
                Return = new ReturnNode();
                Return.Read(item);
            }
            else if (item.Name == "exception")
            {
                var exception = new ExceptionNode();
                exception.Read(item);
                Exceptions ??= [];
                Exceptions.Add(exception);
            }
            else
            {
                Debug.WriteLine($"MemberNode.Read() failed with name:{item.Name}");
            }
        }
    }
}
