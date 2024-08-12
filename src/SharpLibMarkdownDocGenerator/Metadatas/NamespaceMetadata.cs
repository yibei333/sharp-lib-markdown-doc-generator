namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class NamespaceMetadata : BaseMetadata<AssemblyMetadata>
{
    public NamespaceMetadata(string name, List<Type> types, AssemblyMetadata parent) : base(parent)
    {
        Name = name;
        types.Where(x => x.IsPublic && x.BaseType != typeof(MulticastDelegate)).ToList().ForEach(type => Types.Add(new TypeMetadata(type, this)));
        types.Where(x => x.BaseType == typeof(MulticastDelegate)).ToList().ForEach(type => Delegates.Add(new DelegateMetadata(type, this)));
    }

    public List<TypeMetadata> Types { get; } = [];
    public List<DelegateMetadata> Delegates { get; } = [];
    public string Name { get; }
}