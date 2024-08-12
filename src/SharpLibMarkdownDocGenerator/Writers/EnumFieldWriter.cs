using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class EnumFieldWriter(EnumFieldMetadata metadata, TypeWriter parent) : BaseWriter<EnumFieldMetadata, TypeWriter>(metadata, parent)
{
    public override string MarkdownPath => GetFieldMarkdownPath(Metadata.FieldInfo);

    protected override void WriteInternal()
    {
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath())));
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').Append(Metadata.Name.MarkdownEscaping('<', '>')).Append(' ').AppendLine("字段");
        Builder.Append(MarkdownHelper.Bold("程序集")).Append(" : ").AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(GetWriter<AssemblyWriter>()!.MarkdownPath)));
        Builder.Append(MarkdownHelper.Bold("命名空间")).Append(" : ").AppendLine(MarkdownHelper.HyperLink(Parent.Parent.Metadata.Name, MarkdownPath.GetUrlRelativePath(GetNamespaceMarkdownPath(Parent.Parent.Metadata.Name))));
        Builder.Append(MarkdownHelper.Bold("所属类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Parent.Metadata.Type));

        //签名
        Builder.AppendLine(MarkdownHelper.Code(Metadata.Signature));

        //注释
        if (Metadata.DocSummary.NotNullOrWhiteSpace())
        {
            Builder.AppendLine(MarkdownHelper.Bold("注释"));
            Builder.AppendLine(MarkdownHelper.Italic(Metadata.DocSummary));
            Metadata.DocSummaryParas.ForEach(x =>
            {
                Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(x);
            });
            Builder.AppendLine();
        }
    }
}
