namespace SharpLibMarkdownDocGenerator.Helpers;

public static class BelongDirectoryAttributeHelper
{
    public static string? GetBelongDirectory(this Type type)
    {
        var current = type;
        while (current is not null)
        {
            var directoryAttributeData = current.GetCustomAttributesData().FirstOrDefault(x => x.AttributeType.Name == "BelongDirectoryAttribute");
            if (directoryAttributeData is not null) return directoryAttributeData.ConstructorArguments[0].Value?.ToString();
            current = current.BaseType;
        }
        return null;
    }
}
