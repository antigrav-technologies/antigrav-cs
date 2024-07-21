namespace Antigrav {
    public class Main {
        private static string AbandonAllHope(object o) {
            throw new ArgumentException($"Type is not ANTIGRAV Serializable: {o.GetType()}");
        }
        /// <summary>
        /// Serialize object as an ANTIGRAV string
        /// </summary>
        /// <param name="o">Object to serialze</param>
        /// <param name="SortKeys">Sort keys in dictionaries</param>
        /// <param name="indent">Amount of spaces to indent, no indent if null</param>
        /// <param name="ensure_ascii">If true then escapes all non-ASCII symbols</param>
        /// <param name="allow_nan">Allow not a number values (includes infinity)</param>
        /// <param name="custom_encoder">Make your encoding logic here if you need</param>
        /// <param name="skipKeys">If false then when non-string/int/etc. keys pass in dictionaries throw ArgumentException. Otherwise just skip</param>
        /// <returns>ANTIGRAV serialized string</returns>
        public static string Dump(
            object o,
            bool SortKeys = false,
            uint? indent = null,
            bool ensure_ascii = true,
            bool allow_nan = true,
            Func<object, string>? custom_encoder = null,
            bool skipKeys = false
        ) {
            Encoder.ANTIGRAVEncoder encoder = new Encoder.ANTIGRAVEncoder(
                SortKeys, indent, ensure_ascii, allow_nan, skipKeys, custom_encoder ?? AbandonAllHope
            );
            return encoder.Encode(o);
        }
        public static void Main(string[] args) {
            // ...
        }
    }
}
