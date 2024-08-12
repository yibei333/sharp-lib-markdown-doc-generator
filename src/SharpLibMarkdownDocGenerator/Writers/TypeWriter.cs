using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Metadatas;

namespace SharpLibMarkdownDocGenerator.Writers;

internal class TypeWriter(TypeMetadata metadata, NamespaceWriter namespaceWriter) : BaseWriter<TypeMetadata, NamespaceWriter>(metadata, namespaceWriter)
{
    public override string MarkdownPath => GetTypeMarkdownPath(Metadata.Type);

    protected override void WriteInternal()
    {
        WriteType();
        WriteConstructors();
        WriteEnumFields();
        WriteFields();
        WriteProperties();
        WriteMethods();
        WriteEvents();
        WriteDelegates();
    }

    void WriteType()
    {
        //名称
        Builder.Append(MarkdownHelper.Header(6)).Append(' ').AppendLine(MarkdownHelper.HyperLink("主页", MarkdownPath.GetUrlRelativePath(GetRootMarkdownPath())));
        Builder.Append(MarkdownHelper.Header(2)).Append(' ').Append(Metadata.TypeDefinitionName.MarkdownEscaping('<', '>')).Append(' ').AppendLine(Metadata.CategoryName);
        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("定义");
        Builder.Append(MarkdownHelper.Bold("程序集")).Append(" : ").AppendLine(MarkdownHelper.HyperLink($"{Metadata.Assembly?.AssemblyInfo.Name}.dll", MarkdownPath.GetUrlRelativePath(GetWriter<AssemblyWriter>()!.MarkdownPath)));
        Builder.Append(MarkdownHelper.Bold("命名空间")).Append(" : ").AppendLine(MarkdownHelper.HyperLink(Parent.Metadata.Name, MarkdownPath.GetUrlRelativePath(GetNamespaceMarkdownPath(Parent.Metadata.Name))));

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
            Builder.AppendLine();
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
            Builder.AppendLine();
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
            Builder.AppendLine();
        }

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

        //泛型参数
        if (Metadata.GenericParameters.NotNullOrEmpty())
        {
            Builder.AppendLine(MarkdownHelper.Bold("泛型参数"));
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
            Builder.AppendLine(MarkdownHelper.Table(columns, data));
            Builder.AppendLine();
        }
    }

    void WriteConstructors()
    {
        if (Metadata.Constructors.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("构造函数");

        var columns = new string[] { "方法", "注释", "参数" };
        var data = new List<string[]>();
        Metadata.Constructors.ForEach(x =>
        {
            var constructorWriter = new ConstructorWriter(x, this);
            constructorWriter.Write();

            var array = new string[]
            {
                MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(constructorWriter.MarkdownPath)),
                x.DocSummary,
                constructorWriter.ParameterDoc
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }

    void WriteEnumFields()
    {
        if (Metadata.EnumFields.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("字段");

        var columns = new string[] { "名称", "值", "说明" };
        var data = new List<string[]>();
        Metadata.EnumFields.ForEach(x =>
        {
            var fieldWriter = new EnumFieldWriter(x, this);
            fieldWriter.Write();

            var array = new string[]
            {
                MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(fieldWriter.MarkdownPath)),
                x.Value.ToString(),
                x.DocSummary
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }

    void WriteFields()
    {
        if (Metadata.Fields.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("字段");

        var columns = new string[] { "名称", "类型", "是否静态", "注释" };
        var data = new List<string[]>();
        Metadata.Fields.ForEach(x =>
        {
            FieldWriter? fieldWriter = null;
            if (GetTypeMarkdownPath(x.FieldInfo.DeclaringType!).NotNullOrWhiteSpace())
            {
                fieldWriter = new FieldWriter(x, this);
                fieldWriter.Write();
            }

            var nameReference = fieldWriter is null ? x.Name : MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(fieldWriter.MarkdownPath));
            var inheritText = x.FieldInfo.DeclaringType == Metadata.Type ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;" + MarkdownHelper.Italic($"(继承自{MarkdownHelper.TypeReference(this, MarkdownPath, x.FieldInfo.DeclaringType!)})");

            var array = new string[]
            {
                nameReference + inheritText,
                MarkdownHelper.TypeReference(this, MarkdownPath, x.FieldInfo.FieldType),
                MarkdownHelper.Keyword(x.FieldInfo.IsStatic ? "是" : "否"),
                x.DocSummary??"-"
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }

    void WriteProperties()
    {
        if (Metadata.Properties.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("属性");

        var columns = new string[] { "名称", "类型", "是否静态", "注释" };
        var data = new List<string[]>();
        Metadata.Properties.ForEach(x =>
        {
            PropertyWriter? propertyWriter = null;
            if (GetTypeMarkdownPath(x.PropertyInfo.DeclaringType!).NotNullOrWhiteSpace())
            {
                propertyWriter = new PropertyWriter(x, this);
                propertyWriter.Write();
            }

            var nameReference = propertyWriter is null ? x.Name : MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(propertyWriter.MarkdownPath));
            var inheritText = x.PropertyInfo.DeclaringType == Metadata.Type ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;" + MarkdownHelper.Italic($"(继承自{MarkdownHelper.TypeReference(this, MarkdownPath, x.PropertyInfo.DeclaringType!)})");

            var array = new string[]
            {
                nameReference + inheritText,
                MarkdownHelper.TypeReference(this, MarkdownPath, x.PropertyInfo.PropertyType),
                MarkdownHelper.Keyword(x.IsStatic ? "是" : "否"),
                x.DocSummary??"-"
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }

    void WriteMethods()
    {
        if (Metadata.Category == TypeCategory.Enum || Metadata.Methods.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("方法");

        var columns = new string[] { "方法", "返回类型", "Accessor", "是否静态", "参数" };
        var data = new List<string[]>();
        Metadata.Methods.ForEach(x =>
        {
            MethodWriter? methodWriter = null;
            if (GetTypeMarkdownPath(x.MethodInfo.DeclaringType!).NotNullOrWhiteSpace())
            {
                methodWriter = new MethodWriter(x, this);
                methodWriter.Write();
            }

            var nameReference = methodWriter is null ? x.Name : MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(methodWriter.MarkdownPath));
            var inheritText = x.MethodInfo.DeclaringType == Metadata.Type ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;" + MarkdownHelper.Italic($"(继承自{MarkdownHelper.TypeReference(this, MarkdownPath, x.MethodInfo.DeclaringType!)})");

            var array = new string[]
            {
                nameReference + inheritText,
                MarkdownHelper.TypeReference(this, MarkdownPath, x.MethodInfo.ReturnType),
                MarkdownHelper.Keyword(x.Accssor),
                MarkdownHelper.Keyword(x.MethodInfo.IsStatic ? "是" : "否"),
                methodWriter?.ParameterDoc??"-"
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }

    void WriteDelegates()
    {
        var innerTypes = Metadata.Type.GetNestedTypes().Where(x => x.BaseType == typeof(MulticastDelegate) && (x.IsPublic || x.IsNestedFamily || x.IsNestedPublic)).ToList();
        if (innerTypes.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("委托");
        innerTypes.ForEach(x =>
        {
            Builder.Append(MarkdownHelper.UnOrderedList(1)).Append(' ').Append(MarkdownHelper.TypeReference(this, MarkdownPath, x));
        });
        Builder.AppendLine();
    }

    void WriteEvents()
    {
        if (Metadata.Events.Count <= 0) return;

        Builder.Append(MarkdownHelper.Header(3)).Append(' ').AppendLine("事件");

        var columns = new string[] { "名称", "事件处理类型", "Accessor", "注释" };
        var data = new List<string[]>();
        Metadata.Events.ForEach(x =>
        {
            EventWriter? eventWriter = null;
            if (GetTypeMarkdownPath(x.EventInfo.DeclaringType!).NotNullOrWhiteSpace())
            {
                eventWriter = new EventWriter(x, this);
                eventWriter.Write();
            }

            var nameReference = eventWriter is null ? x.Name : MarkdownHelper.HyperLink(x.Name, MarkdownPath.GetUrlRelativePath(eventWriter.MarkdownPath));
            var inheritText = x.EventInfo.DeclaringType == Metadata.Type ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;" + MarkdownHelper.Italic($"(继承自{MarkdownHelper.TypeReference(this, MarkdownPath, x.EventInfo.DeclaringType!)})");

            var array = new string[]
            {
                nameReference + inheritText,
                MarkdownHelper.TypeReference(this, MarkdownPath, x.EventInfo.EventHandlerType!),
                MarkdownHelper.Keyword(x.Accssor),
                x.DocSummary ?? "-"
            };
            data.Add(array);
        });
        Builder.AppendLine(MarkdownHelper.Table(columns, data));
    }
}
