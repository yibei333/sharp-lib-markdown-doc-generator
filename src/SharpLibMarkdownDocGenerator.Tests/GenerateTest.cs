using SharpLibMarkdownDocGenerator.Extensions;

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