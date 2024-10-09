namespace Antigrav;


/// <summary>
/// Provides metadata to make property or field serializable.
/// </summary>
/// <param name="name">The name of the property or field in the serialized output. If not specified then original name is used</param>
/// <param name="defaultValue">Default value for the property or field if it's missing</param>
/// <param name="serializeIf">Serializes if true, skips property otherwise</param>
/// <param name="loadAsNull">If <paramref name="defaultValue"/>defaultValue</paramref> is null, then if this argument is false, this field/property will be loaded as new instance (0, '\x0', new List<int> { }, null, etc.), keeps null otherwise (do not use on strings!)</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class AntigravSerializable(string? name = null, object? defaultValue = null, bool loadAsNull = false) : Attribute {
    public string? Name { get; } = name;
    public object? DefaultValue { get; } = defaultValue;
    public bool LoadAsNull { get; } = loadAsNull;
}

/// <summary>
/// When placed on a property or field of type System.Collections.Generic.IDictionary`2, any
/// properties that do not have a matching member are added to that dictionary during
/// deserialization and written during serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AntigravExtensionData : Attribute { }

public class AntigravDecodeError(string msg, string doc, int pos) : Exception( // you do not need to create instance of it!!!!!!!!!1111111111
$"{msg}: " +
    $"line {doc[..pos].Count(c => c == '\n') + 1} " +
    $"column {(doc.LastIndexOf('\n', pos) == -1 ? pos + 1 : pos - doc.LastIndexOf('\n', pos))} " +
    $"(char {pos})"
) { }

public class AntigravCastingError(Type fromType, Type targetType) : Exception($"Cant cast {fromType} to {targetType}") { }

public static class AntigravConvert {
    /// <summary>
    /// Write object serialized as an Antigrav string to stream
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="filePath"></param>
    /// <param name="sortKeys"Sort keys in dictionaries></param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <param name="forceSave">Saves every single field/property even if its not AntigravSerializable</param>
    /// <exception cref="ArgumentException"></exception>
    public static void DumpToFile(
        object? o,
        string filePath,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true,
        bool forceSave = false
    ) => File.WriteAllText(filePath, DumpToString(o, sortKeys, indent, ensureASCII, allowNaN, forceSave));
    /// <summary>
    /// Serialize object as an Antigrav string
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <param name="forceSave">Saves every single field/property even if its not AntigravSerializable</param>
    /// <returns>Antigrav serialized string</returns>
    /// <exception cref="ArgumentException"></exception>
    public static string DumpToString(
        object? o,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true,
        bool forceSave = false
    ) => Encoder.Encode(o, sortKeys, indent, ensureASCII, allowNaN, forceSave);

    /// <summary>
    /// Write object serialized as an Antigrav string to stream
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <param name="forceSave">Saves every single field/property even if its not AntigravSerializable</param>
    /// <example><code>
    /// Dictionary<string, int> value = new Dictionary<string, int> { {"1", 2}, {"3", 4} };
    /// using (System.IO.StreamWriter writer = new System.IO.StreamWriter("D:\\toaster oven.txt")) {
    ///     Antigrav.Main.Dump(value, writer.BaseStream, indent: 4);
    /// }
    /// /*
    /// should write this to stream:
    /// {
    ///     "1": 2
    ///     "3": 4
    /// }
    /// */
    /// </code></example>
    /// <exception cref="ArgumentException"></exception>
    public static void Dump(
        object? o,
        Stream stream,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true,
        bool forceSave = false
    ) => stream.Write(System.Text.Encoding.UTF8.GetBytes(DumpToString(o, sortKeys, indent, ensureASCII, allowNaN, forceSave)));

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

    /// <summary>
    /// Deserialize stream contating Antigrav serialized object to a C# object
    /// </summary>
    /// <typeparam name="T">Object type to deserialize</typeparam>
    /// <param name="stream">Stream to read</param>
    /// <returns>Deserialized object</returns>
    /// <exception cref="AntigravCastingError"></exception>
    /// <exception cref="AntigravDecodeError"></exception>
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
    /// <summary>
    /// Deserialize Antigrav serialized string to a C# object
    /// </summary>
    /// <typeparam name="T">Object type to deserialize</typeparam>
    /// <param name="s">String to deserialize</param>
    /// <returns>Deserialized object</returns>
    /// <exception cref="AntigravCastingError"></exception>
    /// <exception cref="AntigravDecodeError"></exception>
    public static T? LoadFromString<T>(string s) => Decoder.Decode<T>(s);

    /// <summary>
    /// Deserialize file containing Antigrav serialized object to a C# object
    /// </summary>
    /// <typeparam name="T">Object type to deserialize</typeparam>
    /// <param name="filePath">File to read</param>
    /// <returns>Deserialized object</returns>
    /// <exception cref="AntigravCastingError"></exception>
    /// <exception cref="AntigravDecodeError"></exception>
    public static T? LoadFromFile<T>(string filePath) => Decoder.Decode<T>(File.ReadAllText(filePath));
}
