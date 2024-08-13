using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class MethodWriter(MethodMetadata metadata, TypeWriter parent) : BaseWriter<MethodMetadata, TypeWriter>(metadata, parent)
{
    public override string MarkdownPath => GetMethodMarkdownPath(Metadata.MethodInfo);

    public string ParameterDoc { get; private set; } = "-";

    protected override void WriteInternal()
    {
        if (Metadata.MethodInfo.DeclaringType != Parent.Metadata.Type) return;

        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath()))).AppendLine();
        Builder.Append(MarkdownHelper.Header(4)).Append(' ').Append(Metadata.Name.MarkdownEscaping('<', '>')).Append(' ').AppendLine("方法").AppendLine();
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

        //泛型参数
        if (Metadata.GenericParameters.NotNullOrEmpty())
        {
            Builder.AppendLine(MarkdownHelper.Bold("泛型参数")).AppendLine();
            var columns = new string[] { "名称", "注释", "约束" };
            var data = new List<string[]>();

            Metadata.GenericParameters.ForEach(x =>
            {
                var constraints = x.Constraints.Select(y => y.Type is null ? MarkdownHelper.Keyword(y.Text) : MarkdownHelper.TypeReference(this, MarkdownPath, y.Type));
                var array = new string[]
                {
                    x.Name,
                    x.Doc ?? "-",
                    constraints.Any()? string.Join(", ", constraints):"-"
                };
                data.Add(array);
            });
            Builder.AppendLine(MarkdownHelper.Table(columns, data)).AppendLine();
            Builder.AppendLine().AppendLine();
        }

        //返回类型
        Builder.Append(MarkdownHelper.Bold("返回类型")).Append(" : ").AppendLine(MarkdownHelper.TypeReference(this, MarkdownPath, Metadata.MethodInfo.ReturnType)).AppendLine().AppendLine();

        //参数
        if (Metadata.MethodInfo.GetParameters().Length != 0)
        {
            Builder.AppendLine(MarkdownHelper.Bold("参数")).AppendLine();
            var columns = new string[] { "名称", "类型", "注释" };
            var data = new List<string[]>();
            Metadata.MethodInfo.GetParameters().ToList().ForEach(x =>
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
