namespace SharpLibMarkdownDocGenerator.Metadatas;

internal abstract class BaseMetadata
{
    protected BaseMetadata()
    {
        Root = GetMetadata<RootMetadata>()!;
        Assembly = GetMetadata<AssemblyMetadata>();
    }

    public RootMetadata Root { get; }

    public AssemblyMetadata? Assembly { get; }

    public T? GetMetadata<T>() where T : class
    {
        object? current = this;
        while (current is not null)
        {
            if (current is T t) return t;
            var parentPropertyInfo = current.GetType().GetProperty("Parent");
            current = parentPropertyInfo?.GetValue(current);
        }
        return null;
    }
}

internal abstract class BaseMetadata<TParent>(TParent parent) : BaseMetadata
{
    public TParent Parent { get; } = parent;
}