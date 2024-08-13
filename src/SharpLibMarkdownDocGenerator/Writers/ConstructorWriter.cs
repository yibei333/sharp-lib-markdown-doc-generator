﻿using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class ConstructorWriter(ConstructorMetadata metadata, TypeWriter parent) : BaseWriter<ConstructorMetadata, TypeWriter>(metadata, parent)
{
    public override string MarkdownPath => GetConstructorMarkdownPath(Metadata.ConstructorInfo);

    public string ParameterDoc { get; private set; } = "-";

    protected override void WriteInternal()
    {
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(4)).Append(' ').Append(Metadata.Name.MarkdownEscaping('<', '>')).Append(' ').AppendLine("构造函数").AppendLine();
        Builder.Append(MarkdownHelper.Bold("程序集")).Append(" : ").AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(GetWriter<AssemblyWriter>()!.MarkdownPath))).AppendLine();
        Builder.Append(MarkdownHelper.Bold("命名空间")).Append(" : ").AppendLine(MarkdownHelper.HyperLink(Parent.Parent.Metadata.Name, MarkdownPath.GetUrlRelativePath(GetNamespaceMarkdownPath(Parent.Parent.Metadata.Name)))).AppendLine();
        Builder.Append(MarkdownHelper.Bold("所属类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Parent.Metadata.Type)).AppendLine();

        //签名
        Builder.AppendLine(MarkdownHelper.Code(Metadata.Signature));

        //注释
        if (Metadata.DocSummary.NotNullOrWhiteSpace())
        {
            Builder.AppendLine(MarkdownHelper.Bold("注释")).AppendLine();
            Builder.AppendLine(MarkdownHelper.Italic(Metadata.DocSummary)).AppendLine();
            Metadata.DocSummaryParas.ForEach(x =>
            {
                Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').AppendLine(x).AppendLine();
            });
            Builder.AppendLine();
        }

        //参数
        if (Metadata.ConstructorInfo.GetParameters().Length != 0)
        {
            Builder.AppendLine(MarkdownHelper.Bold("参数")).AppendLine();
            var columns = new string[] { "名称", "类型", "注释" };
            var data = new List<string[]>();
            Metadata.ConstructorInfo.GetParameters().ToList().ForEach(x =>
            {
                var array = new string[3];
                array[0] = x.Name ?? string.Empty;
                array[1] = MarkdownHelper.TypeReference(this, MarkdownPath, x.ParameterType);
                var doc = Metadata.MemberNode?.Params?.FirstOrDefault(y => x.Name == y.Name)?.Content;
                if (doc.IsNullOrWhiteSpace()) doc = "-";
                array[2] = doc;
                data.Add(array);
            });
            Builder.AppendLine(MarkdownHelper.Table(columns, data)).AppendLine();
            ParameterDoc = string.Join("<br>", data.Select(x => $"{x[0]}:{x[2]}"));
        }
    }
}
