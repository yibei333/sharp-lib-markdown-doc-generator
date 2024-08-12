using SharpLibMarkdownDocGenerator.Metadatas;
using SharpLibMarkdownDocGenerator.Writers;

namespace SharpLibMarkdownDocGenerator;

public class Generator
{
    public static void Generate(List<GenerateRequest> requests)
    {
        requests.ForEach(Generate);
    }

    public static void Generate(GenerateRequest request)
    {
        var metadata = new RootMetadata(request);
        var generator = new RootWriter(metadata);
        generator.Write();
    }
}