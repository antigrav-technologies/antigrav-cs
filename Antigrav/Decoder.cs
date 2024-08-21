using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

static class Extensions {
    public static int End(this Match match) => match.Index + match.Length;
    public static bool IsWhitespace(this char? c) => " \t\n\r".Contains(c ?? 'ъ');
    public static string SubstringSafe(this string s, int startIndex, int length) => startIndex + length <= s.Length ? s.Substring(startIndex, length) : s[startIndex..];
    public static char? CharAt(this string s, int index) => index <= s.Length ? s[index] : null;
}

namespace Antigrav {
    public partial class Decoder {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public;
        private const RegexOptions FLAGS = RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled;

        [GeneratedRegex("(.*?)([\"\\\\\\x00-\\x1f])", FLAGS)]
        private static partial Regex STRINGCHUNK();

        [GeneratedRegex("[ \\t\\n\\r]*", FLAGS)]
        private static partial Regex WHITESPACE();

        [GeneratedRegex("(-?\\d+)b", FLAGS)]
        private static partial Regex SBYTE();

        [GeneratedRegex("(-?\\d+)B", FLAGS)]
        private static partial Regex BYTE();

        [GeneratedRegex("(-?\\d+)s", FLAGS)]
        private static partial Regex SHORT();

        [GeneratedRegex("(-?\\d+)S", FLAGS)]
        private static partial Regex USHORT();

        [GeneratedRegex("(-?\\d+)", FLAGS)]
        private static partial Regex INT();

        [GeneratedRegex("(-?\\d+)I", FLAGS)]
        private static partial Regex UINT();

        [GeneratedRegex("(-?\\d+)l", FLAGS)]
        private static partial Regex LONG();

        [GeneratedRegex("(-?\\d+)L", FLAGS)]
        private static partial Regex ULONG();

        [GeneratedRegex("(-?\\d+)ll", FLAGS)]
        private static partial Regex LONGLONG();

        [GeneratedRegex("(-?\\d+)LL", FLAGS)]
        private static partial Regex ULONGLONG();

        [GeneratedRegex("([-+]?)((\\d*(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)[Ff]", FLAGS)]
        private static partial Regex FLOAT();

        [GeneratedRegex("([-+]?)((\\d*(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)", FLAGS)]
        private static partial Regex DOUBLE();

        [GeneratedRegex("([-+]?\\d+\\.\\d+)[Mm]", FLAGS)]
        private static partial Regex DECIMAL();

        [GeneratedRegex("([-+]?)((\\d+(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)" +
                        "([+-])((\\d+(?:\\.\\d+|[eE][-+]?\\d+))|inf|nan)i", FLAGS)]
        private static partial Regex COMPLEX();

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
                }
                catch (MissingMethodException) { throw new MissingMethodException($"Type {type} does not have parameterless constructor and cannot be created"); }
                bool converted = false;
                var dictionary = (Dictionary<string, object?>)ChangeType(o, typeof(Dictionary<string, object?>))!;
                foreach (var property in type.GetProperties(BINDING_FLAGS)) {
                    Main.AntigravProperty? antigravProperty = property.GetCustomAttribute<Main.AntigravProperty>();
                    if (antigravProperty != null) {
                        string name = antigravProperty.Name ?? property.Name;
                        var value = antigravProperty.DefaultValue;
                        if (dictionary.TryGetValue(name, out var v)) {
                            value = v;
                            dictionary.Remove(name);
                        }
                        property.SetValue(target, ChangeType(value, property.PropertyType));
                        converted = true;
                    }
                }
                foreach (var field in type.GetFields(BINDING_FLAGS)) {
                    Main.AntigravProperty? antigravField = field.GetCustomAttribute<Main.AntigravProperty>();
                    if (antigravField != null) {
                        string name = antigravField.Name ?? field.Name;
                        var value = antigravField.DefaultValue;
                        if (dictionary.TryGetValue(name, out var v)) {
                            value = v;
                            dictionary.Remove(name);
                        }
                        field.SetValue(target, ChangeType(value, field.FieldType));
                        converted = true;
                    }
                }
                MemberInfo? extensionsMember = type.GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field).FirstOrDefault(member => member.GetCustomAttribute<Main.AntigravExtensionData>() != null);
                if (extensionsMember != null) {
                    if (extensionsMember is PropertyInfo propertyInfo) {
                        if (typeof(IDictionary).IsAssignableFrom(propertyInfo.PropertyType)) {
                            IDictionary extensionData = (IDictionary)Activator.CreateInstance(propertyInfo.PropertyType)!;
                            foreach (var kvp in dictionary) {
                                extensionData.GetType().GetMethod("Add", [kvp.Key.GetType(), (kvp.Value ?? typeof(object)).GetType()])?.Invoke(extensionData, [kvp.Key, kvp.Value]);
                            }
                            propertyInfo.SetValue(target, extensionData);
                            converted = true;
                        }
                    }
                    if (extensionsMember is FieldInfo fieldInfo) {
                        if (typeof(IDictionary).IsAssignableFrom(fieldInfo.FieldType)) {
                            IDictionary extensionData = (IDictionary)Activator.CreateInstance(fieldInfo.FieldType)!;
                            foreach (var kvp in dictionary) {
                                extensionData.GetType().GetMethod("Add", [kvp.Key.GetType(), (kvp.Value ?? typeof(object)).GetType()])?.Invoke(extensionData, [kvp.Key, kvp.Value]);
                            }
                            fieldInfo.SetValue(target, extensionData);
                            converted = true;
                        }
                    }
                }
                return converted ? target : null;
            }

            public static object? ChangeType(object? o, Type type) {
                if (o == null) return null;
                type = Nullable.GetUnderlyingType(type) ?? type;
                if (type == typeof(object)) return o;
                Type[] args = type.GetGenericArguments();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                if (type.IsArray) {
                    Type elementType = type.GetElementType()!;
                    Array array = Array.CreateInstance(elementType, ((ICollection)o).Count);
                    int i = 0;
                    foreach (object item in (IEnumerable)o) {
                        array.SetValue(ChangeType(item, elementType), i++);
                    }
                    return array;
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
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) || type == typeof(ICollection)) {
                    Type elementType = args[0];
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    object list = Activator.CreateInstance(listType)!;
                    foreach (object item in (IEnumerable)o) {
                        listType.GetMethod("Add")!.Invoke(list, [ChangeType(item, elementType)]);
                    }
                    return list;
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

        private class StopIteration(int? value = null) : Exception("Stop iteration") {
            public int? Value { get; } = value;
        }

        public class ANTIGRAVDecodeError(string msg, string doc, int pos) : Exception(
            $"{msg}: " +
                $"line {doc[..pos].Count(c => c == '\n') + 1} " +
                $"column {(doc.LastIndexOf('\n', pos) == -1 ? pos + 1 : pos - doc.LastIndexOf('\n', pos))} " +
                $"(char {pos})"
            ) { }

        public static T? Decode<T>(string s) {
            int idx = 0;
            string _decode_string() {
                char _decode_uXXXX() {
                    string uni = s.SubstringSafe(idx, 4);
                    if (uni.Length == 4) {
                        try {
                            return Convert.ToChar(Convert.ToInt16(uni, 16));
                        }
                        catch (FormatException) { }
                    }
                    throw new ANTIGRAVDecodeError("Invalid \\uXXXX escape", uni, idx);
                }
                string _decode_uXXXXXXXX() {
                    string uni = s.SubstringSafe(idx, 8);
                    if (uni.Length == 8) {
                        int codePoint = Convert.ToInt32(uni, 16);
                        if (codePoint <= 0x10FFFF) return char.ConvertFromUtf32(codePoint);
                    }
                    throw new ANTIGRAVDecodeError("Invalid \\uXXXXXXXX escape", uni, idx);
                }
                string buf = "";
                int begin = idx - 1;
                while (true) {
                    Match chunk = STRINGCHUNK().Match(s, idx);
                    if (!chunk.Success) throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                    idx = chunk.End();
                    buf += chunk.Groups[1].Value;
                    string terminator = chunk.Groups[2].Value;
                    if (terminator == "\"") break;
                    if (terminator != "\\") throw new ANTIGRAVDecodeError($"Invalid control character {Regex.Escape(terminator)} at", s, idx);
                    char esc = s.CharAt(idx++) ?? throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                    if (!"Uu".Contains(esc)) {
                        if (BACKSLASH.TryGetValue(esc, out char value)) {
                            buf += value;
                        }
                        else {
                            throw new ANTIGRAVDecodeError($"Invalid \\escape: {esc}", s, idx);
                        }
                        idx++;
                    }
                    if (esc == 'u') {
                        buf += _decode_uXXXX();
                        idx += 4;
                    }
                    if (esc == 'U') {
                        buf += _decode_uXXXXXXXX();
                        idx += 8;
                    }
                }
                return buf;
            }

            void _expect_char(char c) {
                _expect_whitespace();
                if (s[idx] != c) {
                    throw new ANTIGRAVDecodeError($"Expecting '{c}' delimiter", s, idx);
                }
                idx++;
            }

            void _expect_whitespace() => idx = WHITESPACE().Match(s, idx).End();

            Dictionary<object, object?> _decode_dict() {
                Dictionary<object, object?> pairs = [];

                _expect_whitespace();
                char? nextchar = s.CharAt(idx);
                if (nextchar == '}') {
                    idx++;
                    return pairs;
                }

                while (true) {
                    _expect_whitespace();
                    object key = _decode() ?? throw new ArgumentException("Dictionaries keys can't be null");

                    _expect_char(':');
                    _expect_whitespace();

                    try {
                        pairs.Add(key, _decode());
                    }
                    catch (StopIteration err) {
                        throw new ANTIGRAVDecodeError("Expecting value", s, (int)err.Value!);
                    }

                    _expect_whitespace();
                    if (s.CharAt(idx) == '}') {
                        idx++;
                        return pairs;
                    }
                    _expect_char(',');
                }
            }

            List<object?> _decode_list() {
                List<object?> values = [];
                _expect_whitespace();
                char? nextchar = s.CharAt(idx);
                if (nextchar == ']') {
                    idx++;
                    return values;
                }
                while (true) {
                    _expect_whitespace();
                    try {
                        values.Add(_decode());
                    }
                    catch (StopIteration err) {
                        throw new ANTIGRAVDecodeError("Expecting value", s, (int)err.Value!);
                    }
                    _expect_whitespace();
                    nextchar = s.CharAt(idx++);
                    if (nextchar == ']') return values;
                    if (nextchar != ',') throw new ANTIGRAVDecodeError("Expecting ',' delimiter", s, idx);
                }
            }

            object? _decode() {
                char nextchar;
                try {
                    nextchar = s[idx];
                }
                catch (IndexOutOfRangeException) {
                    throw new StopIteration(idx);
                }

                if (nextchar == '"') {
                    idx++;
                    return _decode_string();
                }
                if (nextchar == '{') {
                    idx++;
                    return _decode_dict();
                }
                if (nextchar == '[') {
                    idx++;
                    return _decode_list();
                }
                if (nextchar == 'n' && s.SubstringSafe(idx, 4) == "null") {
                    idx += 4;
                    return null;
                }
                if (nextchar == 't' && s.SubstringSafe(idx, 4) == "true") {
                    idx += 4;
                    return true;
                }
                if (nextchar == 'f' && s.SubstringSafe(idx, 5) == "false") {
                    idx += 5;
                    return false;
                }
                Match match;
                match = COMPLEX().Match(s, idx);
                if (match.Success && match.Index == idx) {
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

                    idx = match.End();
                    return new Complex(real, imag);
                }
                match = DECIMAL().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return decimal.Parse(match.Groups[1].Value);
                }
                match = FLOAT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    var sign = match.Groups[1].Value;
                    var rest = match.Groups[2].Value;
                    if (rest != null) {
                        float value;

                        if (rest == "inf") value = float.PositiveInfinity;
                        else if (rest == "nan") value = float.NaN;
                        else value = float.Parse(rest);
                        if (sign == "-") value *= -1;

                        idx = match.End();
                        return value;
                    }
                }
                match = DOUBLE().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    var sign = match.Groups[1].Value;
                    var rest = match.Groups[2].Value;
                    if (rest != null) {
                        double value;

                        if (rest == "inf") value = double.PositiveInfinity;
                        else if (rest == "nan") value = double.NaN;
                        else value = double.Parse(rest);
                        if (sign == "-") value *= -1;

                        idx = match.End();
                        return value;
                    }
                }
                // i will probably have to change the order of them
                // УКРАЛИ СУКА ВСЁ ОТМЕНА
                // update i finally changed the order
                match = LONGLONG().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return Int128.Parse(match.Groups[1].Value);
                }
                match = ULONGLONG().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return UInt128.Parse(match.Groups[1].Value);
                }
                match = LONG().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return long.Parse(match.Groups[1].Value);
                }
                match = ULONG().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return ulong.Parse(match.Groups[1].Value);
                }
                match = SBYTE().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return sbyte.Parse(match.Groups[1].Value);
                }
                match = BYTE().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return byte.Parse(match.Groups[1].Value);
                }
                match = SHORT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return short.Parse(match.Groups[1].Value);
                }
                match = USHORT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return ushort.Parse(match.Groups[1].Value);
                }
                match = UINT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return uint.Parse(match.Groups[1].Value);
                }
                match = INT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    return int.Parse(match.Groups[1].Value);
                }
                throw new StopIteration(idx);
            }

            object? o;
            try {
                o = _decode();
            }
            catch (StopIteration err) {
                throw new ANTIGRAVDecodeError("Expecting value", s, (int)err.Value!);
            }
            if (idx != s.Length) throw new ANTIGRAVDecodeError("Extra data", s, idx);
            return (T?)ConvertButNotReally.ChangeType(o, typeof(T?));
        }
    }
}
