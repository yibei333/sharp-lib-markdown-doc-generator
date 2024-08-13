using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class RootWriter(RootMetadata metadata) : BaseWriter<RootMetadata, BaseWriter>(metadata, null!)
{
    public override string MarkdownPath => GetRootMarkdownPath();

    protected override void WriteInternal()
    {
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').AppendLine("此文档包含如下程序集").AppendLine();

        Metadata.Assemblies.ForEach(x =>
        {
            var assemblyWriter = new AssemblyWriter(x, this);
            assemblyWriter.Write();

            Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(MarkdownHelper.HyperLink($"{x.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(assemblyWriter.MarkdownPath))).AppendLine();
        });
    }
}
