using System.Text.RegularExpressions;

namespace Antigrav;

static partial class Regexs {
    private const RegexOptions FLAGS = RegexOptions.Multiline | RegexOptions.Singleline;

    [GeneratedRegex("[\\x00-\\x1f\\\\\"\\b\\f\\n\\r\\t]")]
    public static partial Regex ESCAPE();

    [GeneratedRegex("(.*?)([\"\\\\\\x00-\\x1f])", FLAGS)]
    public static partial Regex STRINGCHUNK();

    [GeneratedRegex("[ \\t\\n\\r]*", FLAGS)]
    public static partial Regex WHITESPACE();

    [GeneratedRegex("(-?\\d+)b", FLAGS)]
    public static partial Regex SBYTE();

    [GeneratedRegex("(-?\\d+)B", FLAGS)]
    public static partial Regex BYTE();

    [GeneratedRegex("(-?\\d+)s", FLAGS)]
    public static partial Regex SHORT();

    [GeneratedRegex("(-?\\d+)S", FLAGS)]
    public static partial Regex USHORT();

    [GeneratedRegex("(-?\\d+)", FLAGS)]
    public static partial Regex INT();

    [GeneratedRegex("(-?\\d+)I", FLAGS)]
    public static partial Regex UINT();

    [GeneratedRegex("(-?\\d+)l", FLAGS)]
    public static partial Regex LONG();

    [GeneratedRegex("(-?\\d+)L", FLAGS)]
    public static partial Regex ULONG();

    [GeneratedRegex("(-?\\d+)ll", FLAGS)]
    public static partial Regex LONGLONG();

    [GeneratedRegex("(-?\\d+)LL", FLAGS)]
    public static partial Regex ULONGLONG();

    [GeneratedRegex("([-+]?)((\\d*(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)[Ff]", FLAGS)]
    public static partial Regex FLOAT();

    [GeneratedRegex("([-+]?)((\\d*(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)", FLAGS)]
    public static partial Regex DOUBLE();

    [GeneratedRegex("([-+]?\\d+\\.\\d+)[Mm]", FLAGS)]
    public static partial Regex DECIMAL();

    [GeneratedRegex("([-+]?)((\\d+(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)" +
                    "([+-])((\\d+(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)i", FLAGS)]
    public static partial Regex COMPLEX();
}
