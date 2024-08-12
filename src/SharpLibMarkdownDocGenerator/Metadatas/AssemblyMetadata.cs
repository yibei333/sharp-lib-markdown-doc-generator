using SharpLibMarkdownDocGenerator.Helpers;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class AssemblyMetadata : BaseMetadata<RootMetadata>
{
    public AssemblyMetadata(AssemblyInfo assemblyInfo, RootMetadata parent) : base(parent)
    {

        AssemblyInfo = assemblyInfo;
        foreach (var item in assemblyInfo.Types.GroupBy(x => x.Namespace ?? string.Empty))
        {
            Namespaces.Add(new NamespaceMetadata(item.Key, assemblyInfo.Types.Where(x => x.Namespace == item.Key).ToList(), this));
        }
    }

    public List<NamespaceMetadata> Namespaces { get; } = [];
    public AssemblyInfo AssemblyInfo { get; }
}