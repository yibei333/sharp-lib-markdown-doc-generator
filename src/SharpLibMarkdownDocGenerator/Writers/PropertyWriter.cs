using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class PropertyWriter(PropertyMetadata metadata, TypeWriter parent) : BaseWriter<PropertyMetadata, TypeWriter>(metadata, parent)
{
    public override string MarkdownPath => GetPropertyMarkdownPath(Metadata.PropertyInfo);

    protected override void WriteInternal()
    {
        if (Metadata.PropertyInfo.DeclaringType != Parent.Metadata.Type) return;

        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').Append(Metadata.Name).Append(' ').AppendLine("属性").AppendLine();
        Builder.Append(MarkdownHelper.Bold("程序集")).Append(" : ").AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(GetWriter<AssemblyWriter>()!.MarkdownPath))).AppendLine();
        Builder.Append(MarkdownHelper.Bold("命名空间")).Append(" : ").AppendLine(MarkdownHelper.HyperLink(Parent.Parent.Metadata.Name, MarkdownPath.GetUrlRelativePath(GetNamespaceMarkdownPath(Parent.Parent.Metadata.Name)))).AppendLine();
        Builder.Append(MarkdownHelper.Bold("所属类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Parent.Metadata.Type)).AppendLine();

        //签名
        Builder.AppendLine(MarkdownHelper.Code(Metadata.Signature)).AppendLine();

        //注释
        if (Metadata.DocSummary.NotNullOrWhiteSpace())
        {
            Builder.AppendLine(MarkdownHelper.Bold("注释")).AppendLine();
            Builder.AppendLine(MarkdownHelper.Italic(Metadata.DocSummary)).AppendLine();
            Metadata.DocSummaryParas.ForEach(x =>
            {
                Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(x).AppendLine();
            });
            Builder.AppendLine().AppendLine();
        }
    }
}
