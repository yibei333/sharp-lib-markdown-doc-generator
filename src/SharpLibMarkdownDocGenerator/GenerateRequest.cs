namespace SharpLibMarkdownDocGenerator;

/// <summary>
/// 生成请求
/// </summary>
/// <param name="dllPath">dll文件路径</param>
/// <param name="outputPath">文档输出路径</param>
public class GenerateRequest(string dllPath, string outputPath)
{
    /// <summary>
    /// dll路径
    /// </summary>
    public string DllPath { get; set; } = dllPath;
    /// <summary>
    /// 文档输出路径
    /// </summary>
    public string OutputPath { get; set; } = outputPath;
    /// <summary>
    /// 补充文档路径
    /// </summary>
    public string? AddtionalMarkdownDirecotry { get; set; }
    /// <summary>
    /// 外部连接解析器
    /// </summary>
    public Func<Type, string> ExternalUrlResolver { get; set; } = ResolveMicrosoftDoc;
    /// <summary>
    /// 解析微软文档
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>外部地址</returns>
    public static string ResolveMicrosoftDoc(Type type)
    {
        var fullName = $"{type.Namespace}.{type.Name.Replace('`', '-')}".ToLower();
        return $"https://learn.microsoft.com/en-us/dotnet/api/{fullName}";
    }
}
