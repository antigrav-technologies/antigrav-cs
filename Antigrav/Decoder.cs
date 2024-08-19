using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using static Antigrav.Regexs;

namespace Antigrav;

internal static partial class Extensions {
    public static int End(this Match match) => match.Index + match.Length;
    public static bool IsWhitespace(this char? c) => " \t\n\r".Contains(c ?? 'ъ');
    public static string SubstringSafe(this string s, int startIndex, int length) => startIndex + length <= s.Length ? s.Substring(startIndex, length) : s[startIndex..];
    public static char? CharAt(this string s, int index) => index <= s.Length ? s[index] : null;
}

public class Decoder {
    private static readonly Dictionary<char, char> BACKSLASH = new() {
        {'"', '"'},
        {'\\', '\\'},
        {'/', '/'},
        {'b', '\b'},
        {'f', '\f'},
        {'n', '\n'},
        {'r', '\r'},
        {'t', '\t'}
    };

    private class ConvertButNotReally {
        public static Int128 ToInt128(object value) {
            if (value is string str) return Int128.Parse(str);
            if (value is Int128 int128Value) return int128Value;
            if (value is UInt128 uint128Value) return (Int128)uint128Value;
            if (value is sbyte sbyteValue) return (Int128)sbyteValue;
            if (value is byte byteValue) return (Int128)byteValue;
            if (value is short shortValue) return (Int128)shortValue;
            if (value is ushort ushortValue) return (Int128)ushortValue;
            if (value is int intValue) return (Int128)intValue;
            if (value is uint uintValue) return (Int128)uintValue;
            if (value is long longValue) return (Int128)longValue;
            if (value is ulong ulongValue) return (Int128)ulongValue;
            throw new ArgumentException("Cannot convert to Int128");
        }

        public static UInt128 ToUInt128(object value) {
            if (value is string str) return UInt128.Parse(str);
            if (value is Int128 int128Value) return (UInt128)int128Value;
            if (value is UInt128 uint128Value) return uint128Value;
            if (value is sbyte sbyteValue) return (UInt128)sbyteValue;
            if (value is byte byteValue) return (UInt128)byteValue;
            if (value is short shortValue) return (UInt128)shortValue;
            if (value is ushort ushortValue) return (UInt128)ushortValue;
            if (value is int intValue) return (UInt128)intValue;
            if (value is uint uintValue) return (UInt128)uintValue;
            if (value is long longValue) return (UInt128)longValue;
            if (value is ulong ulongValue) return (UInt128)ulongValue;
            throw new ArgumentException("Cannot convert to UInt128");
        }

        public static object? DecodeObject(object o, Type type) {
            if (o is not Dictionary<object, object>) return null;
            object? target;
            try {
                target = Activator.CreateInstance(type);
            } catch (MissingMethodException) { throw new MissingMethodException($"Type {type} does not have parameterless constructor and cannot be created"); }
            bool converted = false;
            var dictionary = (Dictionary<string, object?>)ChangeType(o, typeof(Dictionary<string, object?>))!;
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                Main.AntigravProperty? antigravProperty = property.GetCustomAttribute<Main.AntigravProperty>();
                if (antigravProperty != null) {
                    string name = antigravProperty.Name ?? property.Name;
                    var value = antigravProperty.DefaultValue;
                    if (dictionary.TryGetValue(name, out var v)) value = v;
                    property.SetValue(target, ChangeType(value, property.PropertyType));
                    converted = true;
                }
            }
            return converted ? target : null;
        }

        public static object? ChangeType(object? o, Type type) {
            if (o == null) return null;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(object)) return o;
            Type[] args = type.GetGenericArguments();
            if (type.IsArray) {
                Type elementType = type.GetElementType()!;
                Array array = Array.CreateInstance(elementType, ((ICollection)o).Count);
                int i = 0;
                foreach (object item in (IEnumerable)o) {
                    array.SetValue(ChangeType(item, elementType), i++);
                }
                return array;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) || type == typeof(IList)) {
                Type elementType = args[0];
                Type listType = typeof(List<>).MakeGenericType(elementType);
                object list = Activator.CreateInstance(listType)!;
                foreach (object item in (IEnumerable)o) {
                    listType.GetMethod("Add")!.Invoke(list, [ChangeType(item, elementType)]);
                }
                return list;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                Type keyType = args[0];
                Type valueType = args[1];
                var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dict = Activator.CreateInstance(dictType);
                foreach (KeyValuePair<object, object?> entry in (IEnumerable)o) {
                    dictType.GetMethod("Add")!.Invoke(dict, [ChangeType(entry.Key, keyType), ChangeType(entry.Value, valueType)]);
                }
                return dict;
            }
            if (type == typeof(Int128)) return ToInt128(o);
            if (type == typeof(UInt128)) return ToUInt128(o);
            if (type == typeof(Complex)) return (Complex)o;
            if (type.IsEnum) return Enum.ToObject(type, o);
            if (type == typeof(string)) return (string)o;
            object? o_ = DecodeObject(o, type);
            if (o_ != null) return o_;
            return Convert.ChangeType(o, type);
        }
    }

    private class StopIteration : Exception {
        public int Value { get; }

        public StopIteration() : base("Stop iteration") { }

        public StopIteration(int value) : base("Stop iteration") {
            Value = value;
        }
    }

    public class ANTIGRAVDecodeError(string msg, string doc, int pos) : Exception(
        $"{msg}: " +
            $"line {doc[..pos].Count(c => c == '\n') + 1} " +
            $"column {(doc.LastIndexOf('\n', pos) == -1 ? pos + 1 : pos - doc.LastIndexOf('\n', pos))} " +
            $"(char {pos})"
        ) {}

    public static T? Decode<T>(string s) {
        string _decode_string(ref int end) {
            char _decode_uXXXX(ref int end) {
                string uni = s.SubstringSafe(end, 4);
                if (uni.Length == 4) {
                    try {
                        return Convert.ToChar(Convert.ToInt16(uni, 16));
                    }
                    catch (FormatException) { }
                }
                throw new ANTIGRAVDecodeError("Invalid \\uXXXX escape", uni, end);
            }
            string _decode_uXXXXXXXX(ref int end) {
                string uni = s.SubstringSafe(end, 8);
                if (uni.Length == 8) {
                    int codePoint = Convert.ToInt32(uni, 16);
                    if (codePoint <= 0x10FFFF) return char.ConvertFromUtf32(codePoint);
                }
                throw new ANTIGRAVDecodeError("Invalid \\uXXXXXXXX escape", uni, end);
            }
            string buf = "";
            int begin = end - 1;
            while (true) {
                Match chunk = STRINGCHUNK().Match(s, end);
                if (!chunk.Success) throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                end = chunk.End();
                buf += chunk.Groups[1].Value;
                string terminator = chunk.Groups[2].Value;
                if (terminator == "\"") break;
                if (terminator != "\\") throw new ANTIGRAVDecodeError($"Invalid control character {Regex.Escape(terminator)} at", s, end);
                char esc = s.CharAt(end++) ?? throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                if (!"Uu".Contains(esc)) {
                    if (BACKSLASH.TryGetValue(esc, out char value)) {
                        buf += value;
                    }
                    else {
                        throw new ANTIGRAVDecodeError($"Invalid \\escape: {esc}", s, end);
                    }
                    end++;
                }
                if (esc == 'u') {
                    buf += _decode_uXXXX(ref end);
                    end += 4;
                }
                if (esc == 'U') {
                    buf += _decode_uXXXXXXXX(ref end);
                    end += 8;
                }
            }
            return buf;
        }

        void _expect_char(ref int end, char c) {
            _expect_whitespace(ref end);
            if (s[end] != c) {
                throw new ANTIGRAVDecodeError($"Expecting '{c}' delimiter", s, end);
            }
            end++;
        }

        void _expect_whitespace(ref int end) {
            end = WHITESPACE().Match(s, end).End();
        }

        Dictionary<object, object?> _decode_dict(ref int end) {
            Dictionary<object, object?> pairs = [];

            _expect_whitespace(ref end);
            char? nextchar = s.CharAt(end);
            if (nextchar == '}') {
                end++;
                return pairs;
            }

            while (true) {
                _expect_whitespace(ref end);
                object key = _decode(ref end) ?? throw new ArgumentException("Dictionaries keys can't be null");

                _expect_char(ref end, ':');
                _expect_whitespace(ref end);

                try {
                    pairs.Add(key, _decode(ref end));
                }
                catch (StopIteration err) {
                    throw new ANTIGRAVDecodeError("Expecting value", s, err.Value);
                }

                _expect_whitespace(ref end);
                if (s.CharAt(end) == '}') {
                    end++;
                    return pairs;
                }
                _expect_char(ref end, ',');
            }
        }

        List<object?> _decode_list(ref int end) {
            List<object?> values = [];
            _expect_whitespace(ref end);
            char? nextchar = s.CharAt(end);
            if (nextchar == ']') {
                end++;
                return values;
            }
            while (true) {
                _expect_whitespace(ref end);
                try {
                    values.Add(_decode(ref end));
                }
                catch (StopIteration err) {
                    throw new ANTIGRAVDecodeError("Expecting value", s, err.Value);
                }
                _expect_whitespace(ref end);
                nextchar = s.CharAt(end++);
                if (nextchar == ']') return values;
                if (nextchar != ',') throw new ANTIGRAVDecodeError("Expecting ',' delimiter", s, end);
            }
        }

        object? _decode(ref int end) {
            char nextchar;
            try {
                nextchar = s[end];
            }
            catch (IndexOutOfRangeException) {
                throw new StopIteration(end);
            }

            if (nextchar == '"') {
                end++;
                return _decode_string(ref end);
            }
            if (nextchar == '{') {
                end++;
                return _decode_dict(ref end);
            }
            if (nextchar == '[') {
                end++;
                return _decode_list(ref end);
            }
            if (nextchar == 'n' && s.SubstringSafe(end, 4) == "null") {
                end += 4;
                return null;
            }
            if (nextchar == 't' && s.SubstringSafe(end, 4) == "true") {
                end += 4;
                return true;
            }
            if (nextchar == 'f' && s.SubstringSafe(end, 5) == "false") {
                end += 5;
                return false;
            }
            Match match;
            match = COMPLEX().Match(s, end);
            if (match.Success && match.Index == end) {
                // миша☘️go to нахуй 😾собирайся в садик🏡идидиди 😭misha get up quickly 🥺ДА ИДЕ НАХУУУУУ 🐀😅
                var realSign = match.Groups[1].Value;
                var realRest = match.Groups[2].Value;
                var imagSign = match.Groups[4].Value;
                var imagRest = match.Groups[5].Value;

                double real;
                double imag;

                if (realRest == "inf") real = double.PositiveInfinity;
                else if (realRest == "nan") real = double.NaN;
                else real = double.Parse(realRest);
                if (realSign == "-") real *= -1;

                if (imagRest == "inf") imag = double.PositiveInfinity;
                else if (imagRest == "nan") imag = double.NaN;
                else imag = double.Parse(imagRest);
                if (imagSign == "-") imag *= -1;

                end = match.End();
                return new Complex(real, imag);
            }
            match = DECIMAL().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return decimal.Parse(match.Groups[1].Value);
            }
            match = FLOAT().Match(s, end);
            if (match.Success && match.Index == end) {
                var sign = match.Groups[1].Value;
                var rest = match.Groups[2].Value;
                if (rest != null) {
                    float value;

                    if (rest == "inf") value = float.PositiveInfinity;
                    else if (rest == "nan") value = float.NaN;
                    else value = float.Parse(rest);
                    if (sign == "-") value *= -1;

                    end = match.End();
                    return value;
                }
            }
            match = DOUBLE().Match(s, end);
            if (match.Success && match.Index == end) {
                var sign = match.Groups[1].Value;
                var rest = match.Groups[2].Value;
                if (rest != null) {
                    double value;

                    if (rest == "inf") value = double.PositiveInfinity;
                    else if (rest == "nan") value = double.NaN;
                    else value = double.Parse(rest);
                    if (sign == "-") value *= -1;

                    end = match.End();
                    return value;
                }
            }
            // i will probably have to change the order of them
            // УКРАЛИ СУКА ВСЁ ОТМЕНА
            // update i finally changed the order
            match = LONGLONG().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return Int128.Parse(match.Groups[1].Value);
            }
            match = ULONGLONG().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return UInt128.Parse(match.Groups[1].Value);
            }
            match = LONG().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return long.Parse(match.Groups[1].Value);
            }
            match = ULONG().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return ulong.Parse(match.Groups[1].Value);
            }
            match = SBYTE().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return sbyte.Parse(match.Groups[1].Value);
            }
            match = BYTE().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return byte.Parse(match.Groups[1].Value);
            }
            match = SHORT().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return short.Parse(match.Groups[1].Value);
            }
            match = USHORT().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return ushort.Parse(match.Groups[1].Value);
            }
            match = UINT().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return uint.Parse(match.Groups[1].Value);
            }
            match = INT().Match(s, end);
            if (match.Success && match.Index == end) {
                end = match.End();
                return int.Parse(match.Groups[1].Value);
            }
            throw new StopIteration(end);
        }

        int idx = 0;
        return (T?)ConvertButNotReally.ChangeType(_decode(ref idx), typeof(T?));
    }
}
