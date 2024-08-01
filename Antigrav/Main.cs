using System.Text;

namespace Antigrav;

public static class Main {
    /// <summary>
    /// Serialize object as an ANTIGRAV string
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <param name="customEncoder">Make your encoding logic here if you need, throws ArgumentException by default</param>
    /// <param name="skipKeys">If false then when not convertable to string keys pass in dictionaries throw ArgumentException. Otherwise just skip</param>
    /// <returns>ANTIGRAV serialized string</returns>
    /// <exception>ArgumentException, see <param>skipKeys</param>, <param>ensureASCII</param> and <param>allowNaN</param></exception>
    public static string DumpToString(
        object? o,
        bool sortKeys = false,
        uint? indent = null,
        bool ensureASCII = true,
        bool allowNaN = true,
        bool skipKeys = false
    ) {
        return new Encoder.ANTIGRAVEncoder(
            sortKeys, indent, ensureASCII, allowNaN, skipKeys
        ).Encode(o);
    }

    /// <summary>
    /// Write object serialized as an ANTIGRAV string to stream
    /// </summary>
    /// <param name="o">Object to serialze</param>
    /// <param name="stream"Stream to write in</param>
    /// <param name="sortKeys">Sort keys in dictionaries</param>
    /// <param name="indent">Amount of spaces to indent, no indent if null</param>
    /// <param name="ensureASCII">If true then escapes all non-ASCII symbols</param>
    /// <param name="allowNaN">Allow not a number values (includes infinity), if false will throw ArgumentException</param>
    /// <param name="customEncoder">Make your encoding logic here if you need, throws ArgumentException by default</param>
    /// <param name="skipKeys">If false then when not convertable to string keys pass in dictionaries throw ArgumentException. Otherwise just skip</param>
    /// <returns>ANTIGRAV serialized string</returns>
    /// <exception>ArgumentException, see <param>skipKeys</param>, <param>ensureASCII</param> and <param>allowNaN</param></exception>
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
        bool allowNaN = true,
        bool skipKeys = false
    ) {
        stream.Write(System.Text.Encoding.UTF8.GetBytes(DumpToString(
            o,
            sortKeys,
            indent,
            ensureASCII,
            allowNaN,
            skipKeys
        )));
    }

    public static string DetectEncoding(byte[] bytes) {
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

    public static object Load(
        Stream stream,
        int offset,
        int bytes
    ) {
        byte[] buffer = new byte[bytes];
        stream.Read(buffer, offset, bytes);
        return LoadFromString(
            Encoding.GetEncoding(DetectEncoding(buffer)).GetString(buffer)
        );
    }

    public static object LoadFromString(string s) {
        return new Decoder.ANTIGRAVDecoder().Decode(s);
    }
}
