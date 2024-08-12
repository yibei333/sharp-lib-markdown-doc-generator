using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers.DocumentNodes.Base;

internal abstract class Node
{
    public abstract void Read(XmlNode node);
}
