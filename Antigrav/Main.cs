using System.Text.Json;

namespace Antigrav;

public static class Main {
    /// <summary>
    /// Provides metadata to make property serializable
    /// </summary>
    /// <param name="name">The name of the property in the serialized output. If not specified then original name is used</param>
    /// <param name="defaultValue">Default value for the property if it's missing</param>
    [AttributeUsage(AttributeTargets.Property)]
    public class AntigravProperty : Attribute {
        public string? Name { get; }
        public object? DefaultValue { get; }
        public AntigravProperty(string? name = null, object? defaultValue = null) {
            Name = name;
            DefaultValue = defaultValue;
        }
        public AntigravProperty(string? name) {
            Name = name;
            DefaultValue = null;
        }
    }

    /// <summary>
    /// Serialize object as an Antigrav string
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <returns>Antigrav serialized string</returns>
    /// <exception>ArgumentException, see <param>ensureASCII</param> and <param>allowNaN</param></exception>
    public static string DumpToString(
        object? o,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true
    ) => Encoder.Encode(o, sortKeys, indent, ensureASCII, allowNaN);

    /// <summary>
    /// Write object serialized as an Antigrav string to stream
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <returns>Antigrav serialized string</returns>
    /// <exception>ArgumentException, see <param>ensureASCII</param> and <param>allowNaN</param></exception>
    /// <example><code>
    /// Dictionary<string, int> value = new Dictionary<string, int> { {"1", 2}, {"3", 4} };
    /// using (System.IO.StreamWriter writer = new System.IO.StreamWriter("D:\\toaster oven.txt")) {
    ///     Antigrav.Main.Dump(value, writer.BaseStream, indent: 4);
    /// }
    /// /*
    /// should write this to file:
    /// {
    ///     "1": 2
    ///     "3": 4
    /// }
    /// */
    /// </code></example>
    public static void Dump(
        object? o,
        Stream stream,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true
    ) => stream.Write(System.Text.Encoding.UTF8.GetBytes(DumpToString(o, sortKeys, indent, ensureASCII, allowNaN)));

    private static string DetectEncoding(byte[] bytes) {
        if (bytes.Length >= 2 && (bytes[0] == 0xFE && bytes[1] == 0xFF || // UTF-32 BE
                                  bytes[0] == 0xFF && bytes[1] == 0xFE)) // UTF-32 LE
            return "utf-32";
        if (bytes.Length >= 2 && (bytes[0] == 0xFF && bytes[1] == 0xFE || // UTF-16 BE
                                  bytes[0] == 0xFE && bytes[1] == 0xFF)) // UTF-16 LE
            return "utf-16";
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // UTF-8 with BOM
            return "utf-8-sig";

        if (bytes.Length >= 4) {
            if (bytes[0] == 0) {
                // 00 00 -- -- - utf-32-be
                // 00 XX -- -- - utf-16-be
                return bytes[1] == 0 ? "utf-32-be" : "utf-16-be";
            }
            if (bytes[1] == 0) {
                // XX 00 00 00 - utf-32-le
                // XX 00 00 XX - utf-16-le
                // XX 00 XX -- - utf-16-le
                return bytes[2] == 0 && bytes[3] == 0 ? "utf-32-le" : "utf-16-le";
            }
        }
        else if (bytes.Length == 2) {
            if (bytes[0] == 0) {
                // 00 XX - utf-16-be
                return "utf-16-be";
            }
            if (bytes[1] == 0) {
                // XX 00 - utf-16-le
                return "utf-16-le";
            }
        }

        // default
        return "utf-8";
    }

    public static T? Load<T>(
        Stream stream,
        int offset,
        int bytes
    ) {
        byte[] buffer = new byte[bytes];
        stream.Read(buffer, offset, bytes);
        return LoadFromString<T>(
            System.Text.Encoding.GetEncoding(DetectEncoding(buffer)).GetString(buffer)
        );
    }

    public static T? LoadFromString<T>(string s) => Decoder.Decode<T>(s);
}
