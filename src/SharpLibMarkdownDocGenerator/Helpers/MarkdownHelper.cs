using SharpLibMarkdownDocGenerator.Extensions;
using SharpLibMarkdownDocGenerator.Extensions.NullCheck;
using SharpLibMarkdownDocGenerator.Writers;
using System.Text;

namespace SharpLibMarkdownDocGenerator.Helpers;

internal static class MarkdownHelper
{
    static readonly Dictionary<int, string> _headers = [];
    static readonly Dictionary<int, string> _blockquotes = [];
    static readonly Dictionary<int, string> _unOrderedList = [];
    static readonly List<char> _symbols =
    [
        '\\','`','*','-','{','}','[',']','<','>','(',')','#','+','-','.','!','|'
    ];

    public static string MarkdownEscaping(this string text, params char[] symbols)
    {
        if (symbols.NotNullOrEmpty())
        {
            symbols.ToList().ForEach(x =>
            {
                text = text.Replace(x.ToString(), $"\\{x}");
            });
        }
        else
        {
            _symbols.ForEach(x =>
            {
                text = text.Replace(x.ToString(), $"\\{x}");
            });
        }
        return text;
    }

    public static string LineBreak() => "<br>";

    public static string Header(int number)
    {
        number = Math.Max(1, Math.Min(6, number));
        if (_headers.TryGetValue(number, out var header)) return header;
        var result = "";
        for (int i = 0; i < number; i++)
        {
            result += "#";
        }
        _headers[number] = $"{result}";
        return _headers[number];
    }

    public static string Bold(string text) => $"**{text}**";

    public static string Italic(string text) => $"*{text}*";

    public static string Blockquote(int count = 1)
    {
        if (_blockquotes.TryGetValue(count, out var blockquote)) return blockquote;
        var result = "";
        for (int i = 0; i < count; i++)
        {
            result += ">";
        }
        _blockquotes[count] = $"{result}";
        return _blockquotes[count];
    }

    public static string HyperLink(string text, string url, string? title = null) => $"[{text.MarkdownEscaping('<', '>')}]({url} \"{title ?? text}\")";

    public static string Url(string url) => $"<{url}>";

    public static string Keyword(string text) => $"`{text}`";

    public static string UnOrderedList(int level)
    {
        level = Math.Max(1, Math.Min(8, level));
        if (_unOrderedList.TryGetValue(level, out var unOrderedList)) return unOrderedList;
        var result = "";
        for (int i = 1; i < level; i++)
        {
            result += "    ";
        }
        _unOrderedList[level] = $"{result}*";
        return _unOrderedList[level];
    }

    public static string HorizontalLine() => "---";

    public static string Image(string name, string imageUrl) => $"![{name.MarkdownEscaping('<', '>')}]({imageUrl} \"{name}\")";

    public static string ImageHyperLink(string name, string imageUrl, string linkUrl) => HyperLink(Image(name, imageUrl), linkUrl, name);

    public static string Code(string code, string language = "csharp")
    {
        var builder = new StringBuilder();
        builder.AppendLine($"``` {language}");
        builder.AppendLine(code);
        builder.Append($"```");
        return builder.ToString();
    }

    public static string TypeReference(BaseWriter writer, string mdPath, Type type)
    {
        if (type.IsGenericType)
        {
            var str = string.Empty;
            var referenceMdPath = writer.GetTypeMarkdownPath(type);
            if (referenceMdPath.IsNullOrWhiteSpace())
            {
                var externalUrl = writer.GetExternalTypeUrl(type);
                if (externalUrl.IsNullOrWhiteSpace()) str += type.Name.Split('`')[0];
                else str += HyperLink(type.Name.Split('`')[0], externalUrl, null);
            }
            else
            {
                str += HyperLink(type.Name.Split('`')[0], mdPath.GetUrlRelativePath(referenceMdPath), null);
            }

            str += "\\<";
            var index = 0;
            type.GetGenericArguments().ToList().ForEach(x =>
            {
                if (index != 0) str += ", ";
                str += TypeReference(writer, mdPath, x);
                index++;
            });
            str += "\\>";
            return str;
        }
        else
        {
            if (type.IsGenericParameter) return type.Name;
            var referenceMdPath = writer.GetTypeMarkdownPath(type);
            if (referenceMdPath.IsNullOrWhiteSpace())
            {
                var externalUrl = writer.GetExternalTypeUrl(type);
                if (externalUrl.IsNullOrWhiteSpace()) return type.GetTypeDefinitionName().MarkdownEscaping();
                else return HyperLink(type.GetTypeDefinitionName().MarkdownEscaping(), externalUrl, null);
            }
            else return HyperLink(type.GetTypeDefinitionName().MarkdownEscaping(), mdPath.GetUrlRelativePath(referenceMdPath), null);
        }
    }

    public static string Table(string[] columns, List<string[]> data)
    {
        var builder = new StringBuilder();
        builder.Append('|');
        foreach (var column in columns)
        {
            builder.Append(column);
            builder.Append('|');
        }
        builder.AppendLine();

        builder.Append('|');
        for (int i = 0; i < columns.Length; i++)
        {
            builder.Append("---");
            builder.Append('|');
        }
        builder.AppendLine();


        foreach (var item in data)
        {
            builder.Append('|');
            foreach (var value in item)
            {
                builder.Append(value);
                builder.Append('|');
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
}