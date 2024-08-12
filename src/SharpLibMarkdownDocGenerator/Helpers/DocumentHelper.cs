using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Xml;

namespace SharpLibMarkdownDocGenerator.Helpers;

internal class DocumentHelper
{
    public DocumentHelper(string xmlPath)
    {
        xmlPath.ThrowIfFileNotExist();
        var doc = new XmlDocument();
        doc.Load(xmlPath);

        Doc = new DocNode();
        if (doc.HasChildNodes && doc.LastChild is not null && doc.LastChild.Name == "doc")
        {
            Doc.Read(doc.LastChild);
        }
    }

    public DocNode Doc { get; private set; }
}
