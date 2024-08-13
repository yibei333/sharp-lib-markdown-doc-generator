using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class NamespaceWriter(NamespaceMetadata metadata, AssemblyWriter assemblyWriter) : BaseWriter<NamespaceMetadata, AssemblyWriter>(metadata, assemblyWriter)
{
    public override string MarkdownPath => GetNamespaceMarkdownPath(Metadata.Name);

    protected override void WriteInternal()
    {
        WriteNamespace();
        if (Metadata.Types.Count > 0) Builder.AppendLine().Append(MarkdownHelper.Header(4)).Append(' ').AppendLine("类型").AppendLine();
        WriteDirectoryTypes();
        WriteTypes();
        WriteDelegates();
    }

    void WriteNamespace()
    {
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').AppendLine($"{Metadata.Name} 命名空间").AppendLine();
        Builder.Append(MarkdownHelper.Bold("程序集:")).AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(Parent.MarkdownPath))).AppendLine();
    }

    void WriteDirectoryTypes()
    {
        var directoryTemp = new List<string>();
        Metadata.Types.Where(x => x.BelongDirectory.NotNullOrWhiteSpace()).OrderBy(x => x.BelongDirectory).ToList().ForEach(type =>
        {
            var array = type.BelongDirectory.SplitToList('/');
            var level = 1;
            foreach (var item in array)
            {
                if (!directoryTemp.Contains(item))
                {
                    directoryTemp.Add(item);
                    Builder.Append(MarkdownHelper.UnOrderedList(level)).Append(' ').AppendLine(item);
                }
                level++;
            }

            var typeWriter = new TypeWriter(type, this);
            typeWriter.Write();

            Builder.Append(MarkdownHelper.UnOrderedList(level)).Append(' ').AppendLine(MarkdownHelper.HyperLink(type.Type.GetTypeDefinitionName(), MarkdownPath.GetUrlRelativePath(typeWriter.MarkdownPath))).AppendLine();
        });
    }

    void WriteTypes()
    {
        Metadata.Types.Where(x => x.BelongDirectory.IsNullOrWhiteSpace()).ToList().ForEach(type =>
        {
            var typeWriter = new TypeWriter(type, this);
            typeWriter.Write();

            Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(MarkdownHelper.HyperLink(type.TypeDefinitionName, MarkdownPath.GetUrlRelativePath(typeWriter.MarkdownPath))).AppendLine();
        });
    }

    void WriteDelegates()
    {
        if (Metadata.Delegates.Count <= 0) return;
        if (Metadata.Delegates.Any(x => x.Type.DeclaringType is null)) Builder.AppendLine().Append(MarkdownHelper.Header(4)).Append(' ').AppendLine("委托").AppendLine();
        Metadata.Delegates.Where(x => x.BelongDirectory.IsNullOrWhiteSpace()).ToList().ForEach(type =>
        {
            var typeWriter = new DelegateWriter(type, this);
            typeWriter.Write();

            if (type.Type.DeclaringType is null) Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(MarkdownHelper.HyperLink(type.TypeDefinitionName, MarkdownPath.GetUrlRelativePath(typeWriter.MarkdownPath))).AppendLine();
        });
    }
}