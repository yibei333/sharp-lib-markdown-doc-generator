using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class DelegateWriter(DelegateMetadata metadata, NamespaceWriter namespaceWriter) : BaseWriter<DelegateMetadata, NamespaceWriter>(metadata, namespaceWriter)
{
    public override string MarkdownPath => GetTypeMarkdownPath(Metadata.Type);

    protected override void WriteInternal()
    {
        //名称
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(1)).Append(' ').Append(Metadata.TypeDefinitionName.MarkdownEscaping('<', '>')).Append(' ').AppendLine("委托").AppendLine();
        Builder.Append(MarkdownHelper.Header(2)).Append(' ').AppendLine("定义").AppendLine();
        Builder.Append(MarkdownHelper.Bold("程序集")).Append(" : ").AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(GetWriter<AssemblyWriter>()!.MarkdownPath))).AppendLine();
        Builder.Append(MarkdownHelper.Bold("命名空间")).Append(" : ").AppendLine(MarkdownHelper.HyperLink(Parent.Metadata.Name, MarkdownPath.GetUrlRelativePath(GetNamespaceMarkdownPath(Parent.Metadata.Name)))).AppendLine();
        if (Metadata.Type.DeclaringType is not null) Builder.Append(MarkdownHelper.Bold("所属类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Metadata.Type.DeclaringType)).AppendLine();

        //继承
        if (Metadata.BaseTypeChain.Count > 0)
        {
            Builder.Append(MarkdownHelper.Bold("继承")).Append(" : ");
            var baseIndex = 0;
            Metadata.BaseTypeChain.ForEach(x =>
            {
                if (baseIndex != 0) Builder.Append(" ↣ ");
                Builder.Append(MarkdownHelper.TypeReference(this, MarkdownPath, x));
                baseIndex++;
            });
            Builder.AppendLine().AppendLine();
        }

        //实现
        if (Metadata.Implementions.Count > 0)
        {
            Builder.Append(MarkdownHelper.Bold("实现")).Append(" : ");
            var baseIndex = 0;
            Metadata.Implementions.ForEach(x =>
            {
                if (baseIndex != 0) Builder.Append(", ");
                Builder.Append(MarkdownHelper.TypeReference(this, MarkdownPath, x));
                baseIndex++;
            });
            Builder.AppendLine().AppendLine();
        }

        //派生
        if (Metadata.DerivedTypes.Count > 0)
        {
            Builder.Append(MarkdownHelper.Bold("派生")).Append(" : ");
            var baseIndex = 0;
            Metadata.DerivedTypes.ForEach(x =>
            {
                if (baseIndex != 0) Builder.Append(", ");
                Builder.Append(MarkdownHelper.TypeReference(this, MarkdownPath, x));
                baseIndex++;
            });
            Builder.AppendLine().AppendLine();
        }
        if (Metadata.Type.DeclaringType is not null) Builder.Append(MarkdownHelper.Bold("所属类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Metadata.Type.DeclaringType)).AppendLine();

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
