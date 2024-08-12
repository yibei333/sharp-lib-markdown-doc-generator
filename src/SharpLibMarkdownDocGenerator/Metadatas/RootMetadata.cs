using SharpLibMarkdownDocGenerator.Helpers;

namespace SharpLibMarkdownDocGenerator.Metadatas;

internal class RootMetadata : BaseMetadata<object>
{
    public RootMetadata(GenerateRequest request) : base(null!)
    {
        Request = request;
        var helper = new MetadataHelper(Request.DllPath);
        Name = helper.Name;
        Assemblies = helper.Assemblies.Select(x => new AssemblyMetadata(x, this)).ToList();
        Types = Assemblies.SelectMany(x => x.AssemblyInfo.Types).ToList();
    }

    public string Name { get; }
    public List<AssemblyMetadata> Assemblies { get; }
    public List<Type> Types { get; }
    public GenerateRequest Request { get; }
}