using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class AssemblyWriter(AssemblyMetadata metadata, RootWriter parent) : BaseWriter<AssemblyMetadata, RootWriter>(metadata, parent)
{
    public override string MarkdownPath => GetAssemblyMarkdownPath(Metadata.AssemblyInfo);

    protected override void WriteInternal()
    {
        //assembly
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').AppendLine($"{Metadata.AssemblyInfo.Name} 程序集").AppendLine();
        Builder.Append(MarkdownHelper.Header(4)).Append(' ').AppendLine("命名空间").AppendLine();

        //namespaces
        Metadata.Namespaces.ForEach(x =>
        {
            var namespaceWriter = new NamespaceWriter(x, this);
            namespaceWriter.Write();

            Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(namespaceWriter.MarkdownPath))).AppendLine();
        });
    }
}
