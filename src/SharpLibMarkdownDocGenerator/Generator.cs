using SharpLibMarkdownDocGenerator.Metadatas;
using SharpLibMarkdownDocGenerator.Writers;

namespace SharpLibMarkdownDocGenerator;

/// <summary>
/// 生成器
/// </summary>
public class Generator
{
    /// <summary>
    /// 批量生成
    /// </summary>
    /// <param name="requests">请求集合</param>
    public static void Generate(List<GenerateRequest> requests)
    {
        requests.ForEach(Generate);
    }

    /// <summary>
    /// 单个生成
    /// </summary>
    /// <param name="request">请求</param>
    public static void Generate(GenerateRequest request)
    {
        var metadata = new RootMetadata(request);
        var generator = new RootWriter(metadata);
        generator.Write();
    }
}