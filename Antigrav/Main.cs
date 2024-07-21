namespace Antigrav {
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
            Func<object, string>? customEncoder = null,
            bool skipKeys = false
        ) {
            Encoder.ANTIGRAVEncoder encoder = new Encoder.ANTIGRAVEncoder(
                sortKeys, indent, ensureASCII, allowNaN, skipKeys, customEncoder ?? (o => throw new ArgumentException($"Type is not ANTIGRAV Serializable: {o.GetType()}"))
            );
            return encoder.Encode(o);
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
            System.IO.Stream stream,
            bool sortKeys = false,
            uint? indent = null,
            bool ensureASCII = true,
            bool allowNaN = true,
            Func<object, string>? customEncoder = null,
            bool skipKeys = false
        ) {
            stream.Write(System.Text.Encoding.UTF8.GetBytes(DumpToString(
                o,
                sortKeys,
                indent,
                ensureASCII,
                allowNaN,
                customEncoder,
                skipKeys
            )));
        }
    }
}
