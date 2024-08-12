namespace SharpLibMarkdownDocGenerator;

public class GenerateRequest(string dllPath, string outputPath)
{
    public string DllPath { get; set; } = dllPath;
    public string OutputPath { get; set; } = outputPath;
    public string? AddtionalMarkdownDirecotry { get; set; }
    public Func<Type, string> ExternalUrlResolver { get; set; } = ResolveMicrosoftDoc;

    public static string ResolveMicrosoftDoc(Type type)
    {
        var fullName = $"{type.Namespace}.{type.Name.Replace('`', '-')}".ToLower();
        return $"https://learn.microsoft.com/en-us/dotnet/api/{fullName}";
    }
}
