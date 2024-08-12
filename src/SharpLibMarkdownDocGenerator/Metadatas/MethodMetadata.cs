using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class MethodMetadata : BaseMetadata<TypeMetadata>
{
    public MethodMetadata(MethodInfo methodInfo, TypeMetadata parent) : base(parent)
    {
        MethodInfo = methodInfo;
        DeclareType = methodInfo.DeclaringType!;
        Name = methodInfo.GetMethodDefinitionName(true);
        var xmlKey = GetConstructorXmlKey();
        MemberNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == xmlKey);
        DocSummary = MemberNode?.Summary?.Content ?? "-";
        DocSummaryParas = MemberNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        GenericParameters = GetGenericParameters();
        Accssor = GetAccessor();
        Signature = GetSignature();
    }

    public MethodInfo MethodInfo { get; }
    Type DeclareType { get; }
    public MemberNode? MemberNode { get; }
    public string Name { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public List<GenericParameter> GenericParameters { get; }
    public string Accssor { get; }
    public string Signature { get; }

    static readonly List<Type> _ignoredAttributeTypes = [typeof(ExtensionAttribute)];
    List<string> GetAttributes() => MethodInfo.CustomAttributes.Where(x => x.AttributeType.IsPublic && !x.AttributeType.IsSpecialName && !_ignoredAttributeTypes.Any(y => x.AttributeType == y)).Select(x => x.ToString()).ToList();

    List<GenericParameter> GetGenericParameters()
    {
        if (!MethodInfo.IsGenericMethod) return [];
        var result = new List<GenericParameter>();
        var docs = MemberNode?.TypeParams ?? [];
        foreach (var argument in MethodInfo.GetGenericArguments())
        {
            var name = argument.Name;
            var doc = docs.FirstOrDefault(x => x.Name == name)?.Content;
            var parameter = new GenericParameter(name, doc);
            var constraints = argument.GetGenericParameterConstraints();
            var isValueType = false;
            foreach (var constraint in constraints)
            {
                if (constraint == typeof(ValueType))
                {
                    isValueType = true;
                    parameter.Constraints.Add(new GenericParameterConstraint("struct", null));
                }
                else
                {
                    parameter.Constraints.Add(new GenericParameterConstraint(constraint.GetTypeDefinitionName(), constraint));
                }
            }

            if (!isValueType)
            {
                var attributes = argument.GenericParameterAttributes;
                if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.ReferenceTypeConstraint) parameter.Constraints.Add(new GenericParameterConstraint("class", null));
                if ((attributes & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint) parameter.Constraints.Add(new GenericParameterConstraint("new()", null));
            }

            result.Add(parameter);
        }

        return result;
    }

    string GetConstructorXmlKey()
    {
        var builder = new StringBuilder();
        builder.Append("M:");
        builder.Append(DeclareType.Namespace);
        builder.Append('.');
        builder.Append(DeclareType.Name);
        builder.Append('.');
        var methodName = MethodInfo.Name;
        if (MethodInfo.IsGenericMethod)
        {
            methodName += $"``{MethodInfo.GetGenericArguments().Length}";
        }
        builder.Append(methodName);
        var definitionName = MethodInfo.GetMethodDefinitionName(false, true).Replace(", ", ",").Replace("(this ", "(");
        definitionName = definitionName.Substring(definitionName.IndexOf('('));
        definitionName = definitionName.Replace('<', '{').Replace('>', '}');
        if (MethodInfo.IsGenericMethod)
        {
            var index = 0;
            MethodInfo.GetGenericArguments().ToList().ForEach(x =>
            {
                definitionName = definitionName.Replace("(" + x.Name + ")", "(``" + index + ")");
                definitionName = definitionName.Replace("(" + x.Name + ",", "(``" + index + ",");
                definitionName = definitionName.Replace("," + x.Name + ")", ",``" + index + ")");
                definitionName = definitionName.Replace("," + x.Name + ",", ",``" + index + ",");
                definitionName = definitionName.Replace("{" + x.Name + "}", "{``" + index + "}");
                index++;
            });
        }
        if (DeclareType.IsGenericType)
        {
            var index = 0;
            DeclareType.GetGenericArguments().ToList().ForEach(x =>
            {
                definitionName = definitionName.Replace("(" + x.Name + ")", "(`" + index + ")");
                definitionName = definitionName.Replace("(" + x.Name + ",", "(`" + index + ",");
                definitionName = definitionName.Replace("," + x.Name + ")", ",`" + index + ")");
                definitionName = definitionName.Replace("," + x.Name + ",", ",`" + index + ",");
                definitionName = definitionName.Replace("{" + x.Name + "}", "{`" + index + "}");
                index++;
            });
        }
        builder.Append(definitionName);
        return builder.ToString();
    }

    string GetAccessor()
    {
        if (MethodInfo.IsPublic) return "public";
        else if (MethodInfo.IsFamily) return "protected";
        else if (MethodInfo.IsPrivate) return "private";
        throw new NotSupportedException();
    }

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));
        builder.Append(Accssor);
        builder.Append(' ');
        if (MethodInfo.IsStatic) builder.Append("static ");
        if (MethodInfo.IsVirtual) builder.Append("virtual ");
        if (MethodInfo.IsAbstract) builder.Append("abstract ");
        builder.Append(MethodInfo.ReturnType.GetTypeDefinitionName()).Append(' ');
        builder.Append(Name);

        GenericParameters.Where(x => x.Constraints.Count > 0).ToList().ForEach(x =>
        {
            builder.AppendLine();
            builder.Append($"    where {x.Name} : {string.Join(", ", x.Constraints.Select(y => y.Text))}");
        });
        return builder.ToString();
    }
}