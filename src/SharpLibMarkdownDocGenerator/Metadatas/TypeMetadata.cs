using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Helpers;
using SharpLibMarkdownDocGenerator.Helpers.DocumentNodes;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class TypeMetadata : BaseMetadata<NamespaceMetadata>
{
    public TypeMetadata(Type type, NamespaceMetadata parent) : base(parent)
    {
        Type = type;
        BelongDirectory = type.GetBelongDirectory();
        Category = type.GetTypeCategory();
        DocNode = Assembly?.AssemblyInfo?.Doc?.MembersNode?.Members?.FirstOrDefault(x => x.Name == $"T:{Type.Namespace}.{type.Name}");
        DocSummary = DocNode?.Summary?.Content ?? string.Empty;
        DocSummaryParas = DocNode?.Summary?.Paras?.Select(x => x.Content ?? string.Empty)?.ToList() ?? [];
        Attributes = GetAttributes();
        BaseTypeChain = GetBaseTypeChain();
        BaseType = BaseTypeChain.LastOrDefault();
        Implementions = GetImplementions();
        DerivedTypes = GetDerivedTypes();
        GenericParameters = GetGenericParameters();
        Signature = GetSignature();
        type.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToList().ForEach(x => Constructors.Add(new ConstructorMetadata(x, this)));

        if (Category == TypeCategory.Enum) type.GetFields(BindingFlags.Public | BindingFlags.Static).ToList().ForEach(x => EnumFields.Add(new EnumFieldMetadata(x, this)));
        else type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToList().ForEach(x => Fields.Add(new FieldMetadata(x, this)));

        type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToList().ForEach(x => Properties.Add(new PropertyMetadata(x, this)));

        type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(x => !x.IsSpecialName && (x.IsPublic || x.IsFamily)).ToList().ForEach(x => Methods.Add(new MethodMetadata(x, this)));

        type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x =>
        {
            var addMethod = x.GetAddMethod(true);
            return addMethod is not null && (addMethod.IsPublic || addMethod.IsFamily);
        }).ToList().ForEach(x => Events.Add(new EventMetadata(x, this)));
    }

    public Type Type { get; }
    public string TypeDefinitionName => Type.GetTypeDefinitionName();
    public string? BelongDirectory { get; }
    public TypeCategory Category { get; }
    string CategoryDefinition
    {
        get
        {
            if (Category == TypeCategory.Enum) return "enum";
            else if (Category == TypeCategory.Struct) return "struct";
            else if (Category == TypeCategory.Class) return "class";
            else if (Category == TypeCategory.Interface) return "interface";
            else return "error";
        }
    }
    public string CategoryName
    {
        get
        {
            if (Category == TypeCategory.Enum) return "枚举";
            else if (Category == TypeCategory.Struct) return "结构体";
            else if (Category == TypeCategory.Class) return "类";
            else if (Category == TypeCategory.Interface) return "接口";
            else return "未知";
        }
    }
    public MemberNode? DocNode { get; }
    public string DocSummary { get; }
    public List<string> DocSummaryParas { get; }
    List<string> Attributes { get; }
    public Type? BaseType { get; }
    public List<Type> BaseTypeChain { get; }
    public List<Type> Implementions { get; }
    public List<Type> DerivedTypes { get; }
    public string Signature { get; }
    public List<GenericParameter> GenericParameters { get; }

    public List<ConstructorMetadata> Constructors { get; } = [];
    public List<EnumFieldMetadata> EnumFields { get; } = [];
    public List<FieldMetadata> Fields { get; } = [];
    public List<PropertyMetadata> Properties { get; } = [];
    public List<MethodMetadata> Methods { get; } = [];
    public List<EventMetadata> Events { get; } = [];

    static readonly List<Type> _ignoredAttributeTypes = [typeof(ExtensionAttribute)];
    List<string> GetAttributes() => Type.CustomAttributes.Where(x => x.AttributeType.IsPublic && !x.AttributeType.IsSpecialName && !_ignoredAttributeTypes.Any(y => x.AttributeType == y)).Select(x => x.ToString()).ToList();

    List<Type> GetBaseTypeChain()
    {
        var baseType = Type.BaseType;
        var baseTypeChain = new List<Type>();
        while (baseType is not null)
        {
            baseTypeChain.Add(baseType);
            baseType = baseType.BaseType;
        }
        baseTypeChain.Reverse();
        return baseTypeChain;
    }

    List<Type> GetImplementions() => Category == TypeCategory.Enum || Category == TypeCategory.Struct ? [] : [.. Type.GetInterfaces()];

    List<Type> GetDerivedTypes() => Type.Assembly.GetTypes().Where(x => x.IsPublic && (x.BaseType?.Name == Type.Name || x.GetInterface(Type.Name) is not null)).ToList();

    List<GenericParameter> GetGenericParameters()
    {
        if (!Type.IsGenericType) return [];
        var result = new List<GenericParameter>();
        var docs = DocNode?.TypeParams ?? [];
        foreach (var argument in Type.GetGenericArguments())
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

    string GetSignature()
    {
        var builder = new StringBuilder();
        Attributes.ForEach(x => builder.AppendLine(x));

        builder.Append(Type.IsNestedFamily ? "protected " : "public ");
        if (Type.IsClass)
        {
            if (Type.IsAbstract && Type.IsSealed) builder.Append("static ");
            else if (Type.IsSealed) builder.Append("sealed ");
            else if (Type.IsAbstract) builder.Append("abstract ");
        }
        builder.Append($"{CategoryDefinition} ");
        builder.Append(TypeDefinitionName);

        if (BaseType is not null || Implementions.Count > 0)
        {
            builder.Append(" : ");
        }

        if (BaseType is not null) builder.Append(BaseType.GetTypeDefinitionName());
        if (Implementions.Count > 0)
        {
            if (BaseType is not null) builder.Append(", ");
            builder.Append(string.Join(", ", Implementions.Select(x => x.GetTypeDefinitionName())));
        }

        GenericParameters.Where(x => x.Constraints.Count > 0).ToList().ForEach(x =>
        {
            builder.AppendLine();
            builder.Append($"    where {x.Name} : {string.Join(", ", x.Constraints.Select(y => y.Text))}");
        });

        return builder.ToString();
    }
}

internal class GenericParameter(string name, string? doc)
{
    public string Name { get; } = name;
    public string? Doc { get; } = doc;
    public List<GenericParameterConstraint> Constraints { get; } = [];
}

internal class GenericParameterConstraint(string text, Type? type)
{
    public string Text { get; } = text;
    public Type? Type { get; } = type;
}