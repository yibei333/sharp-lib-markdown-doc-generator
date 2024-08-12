using System.Diagnostics.CodeAnalysis;

namespace SharpLibMarkdownDocGenerator.Tests;

/// <summary>
/// generate tests
/// </summary>
[TestClass]
public class GenerateTests
{
    /// <summary>
    /// generate test
    /// </summary>
    [TestMethod]
    public void GenerateTest()
    {
        var dllPath = AppDomain.CurrentDomain.BaseDirectory.CombinePath("SharpLibMarkdownDocGenerator.Tests.dll");
        var outputDirectory = AppDomain.CurrentDomain.BaseDirectory.CombinePath("doc");
        var addtionalMarkdownDirecotry = AppDomain.CurrentDomain.BaseDirectory.CombinePath("Addtional");
        var request = new GenerateRequest(dllPath, outputDirectory)
        {
            AddtionalMarkdownDirecotry = addtionalMarkdownDirecotry,
            ExternalUrlResolver = (type) =>
            {
                if (type.Name.Equals("User")) return "https://www.google.com";
                return GenerateRequest.ResolveMicrosoftDoc(type);
            }
        };

        Generator.Generate(request);
    }
}

internal static class FileExtension
{
    /// <summary>
    /// 合并路径
    /// </summary>
    /// <param name="leftPath">左边路径</param>
    /// <param name="rightPath">右边路径</param>
    /// <returns>路径</returns>
    public static string CombinePath(this string leftPath, string rightPath) => Path.Combine(leftPath.Trim(), rightPath.Trim().TrimStart('/').TrimStart('\\')).FormatPath();

    /// <summary>
    /// 格式化路径
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>格式化字符串</returns>
    /// <exception cref="ArgumentNullException">当path参数为空时引发异常</exception>
    public static string FormatPath([NotNull] this string path) => path?.Trim().Replace("\\", "/") ?? throw new ArgumentNullException(nameof(path));
}
