using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static Antigrav.Main;

static class Extensions {
    public static int End(this Match match) => match.Index + match.Length;
    public static string SubstringSafe(this string s, int startIndex, int length) => startIndex + length <= s.Length ? s.Substring(startIndex, length) : s[startIndex..];
    public static char? CharAt(this string s, int index) => index < s.Length ? s[index] : null;
    public static Type Type(this MemberInfo memberInfo) => memberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.PropertyType,
        FieldInfo fieldInfo => fieldInfo.FieldType,
        _ => typeof(object)
    };

    public static void SetValue(this MemberInfo memberInfo, object? obj, object? value) {
        switch (memberInfo) {
            case PropertyInfo propertyInfo:
                propertyInfo.SetValue(obj, value);
                break;
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(obj, value);
                break;
        }
    }
}

namespace Antigrav {
    public partial class Decoder {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const RegexOptions FLAGS = RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled;

        [GeneratedRegex("(.*?)([\"\\\\\\x00-\\x1f])", FLAGS)]
        private static partial Regex STRINGCHUNK();

        [GeneratedRegex("[ \\t\\n\\r]*", FLAGS)]
        private static partial Regex WHITESPACE();
        [GeneratedRegex("-?\\d+", FLAGS)]
        private static partial Regex INT();

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
            public static Array DecodeArray(object o, Type type) {
                Type elementType = type.GetElementType()!;
                Array array = Array.CreateInstance(elementType, ((ICollection)o).Count);
                int i = 0;
                foreach (object item in (IEnumerable)o) {
                    array.SetValue(ChangeType(item, elementType), i++);
                }
                return array;
            }

            public static object DecodeDictionary(object o, Type type) {
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];
                var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                object dict = Activator.CreateInstance(dictType)!;
                var method = dictType.GetMethod("Add")!;
                foreach (KeyValuePair<object, object?> entry in (IEnumerable)o) {
                    method.Invoke(dict, [ChangeType(entry.Key, keyType), ChangeType(entry.Value, valueType)]);
                }
                return dict;
            }

            public static object DecodeList(object o, Type type) {
                Type elementType = type.GetGenericArguments()[0];
                Type listType = typeof(List<>).MakeGenericType(elementType);
                object list = Activator.CreateInstance(listType)!;
                var method = listType.GetMethod("Add")!;
                foreach (object item in (IEnumerable)o) {
                    method.Invoke(list, [ChangeType(item, elementType)]);
                }
                return list;
            }

            public static object TryCreateObject(Type targetType) {
                try {
                    return Activator.CreateInstance(targetType)!;
                }
                catch (MissingMethodException) {
#pragma warning disable SYSLIB0050
                    return FormatterServices.GetSafeUninitializedObject(targetType);
                }
            }

            public static object DecodeObject(object o, Type type) {
                if (o is not Dictionary<object, object>) throw new AntigravCastingError(o.GetType(), type);
                object target = TryCreateObject(type);
                var dictionary = (Dictionary<object, object?>)ChangeType(o, typeof(Dictionary<object, object?>))!;
                foreach (var member in type.GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)) {
                    AntigravSerializable? antigravSerializable = member.GetCustomAttribute<AntigravSerializable>();
                    if (antigravSerializable == null) continue;
                    string name = antigravSerializable.Name ?? member.Name;
                    member.SetValue(target, ChangeType(dictionary.TryGetValue(name, out var v) ? v : (antigravSerializable.LoadAsNull ? antigravSerializable.DefaultValue : antigravSerializable.DefaultValue ?? TryCreateObject(member.Type())), member.Type()));
                    dictionary.Remove(name);
                }
                MemberInfo? extensionsMember = type.GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field).FirstOrDefault(member => member.GetCustomAttribute<AntigravExtensionData>() != null);
                if (extensionsMember != null) {
                    IDictionary extensionData = (IDictionary)TryCreateObject(extensionsMember.Type());
                    MethodInfo method = extensionsMember.Type().GetMethod("Add")!;
                    foreach (var kvp in dictionary) {
                        method.Invoke(extensionData, [kvp.Key, kvp.Value]);
                    }
                    extensionsMember.SetValue(target, extensionData);
                }
                return target;
            }

            public static object? ChangeType(object? o, Type type) {
                if (o == null) return null;
                type = Nullable.GetUnderlyingType(type) ?? type;
                if (type == typeof(object)) return o;
                if (type == typeof(string)) return (string)o;
                if (type == typeof(sbyte)) return (sbyte)o;
                if (type == typeof(byte)) return (byte)o;
                if (type == typeof(short)) return (short)o;
                if (type == typeof(ushort)) return (ushort)o;
                if (type == typeof(int)) return (int)o;
                if (type == typeof(uint)) return (uint)o;
                if (type == typeof(long)) return (long)o;
                if (type == typeof(ulong)) return (ulong)o;
                if (type == typeof(Int128)) return (Int128)o;
                if (type == typeof(UInt128)) return (UInt128)o;
                if (type == typeof(float)) return (float)o;
                if (type == typeof(double)) return (double)o;
                if (type == typeof(decimal)) return (decimal)o;
                if (type == typeof(Complex)) return (Complex)o;
                if (o is bool @bool && type == typeof(bool)) return @bool;
                if (type.IsEnum) return Enum.ToObject(type, o);
                if (type.IsArray) return DecodeArray(o, type);
                if (type.IsGenericType) {
                    var type_ = type.GetGenericTypeDefinition();
                    if (type_ == typeof(Tuple<>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Tuple<,,,,,,,>)) return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    if (type_ == typeof(Dictionary<,>)) return DecodeDictionary(o, type);
                    if (type_ == typeof(List<>)) return DecodeList(o, type);
                }
                return DecodeObject(o, type);
            }
        }

        private class StopIteration(int? value = null) : Exception("Stop iteration") {
            public int? Value { get; } = value;
        }

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
                    throw new AntigravDecodeError("Invalid \\uXXXX escape", uni, idx);
                }
                string _decode_uXXXXXXXX() {
                    string uni = s.SubstringSafe(idx, 8);
                    if (uni.Length == 8) {
                        int codePoint = Convert.ToInt32(uni, 16);
                        if (codePoint <= 0x10FFFF) return char.ConvertFromUtf32(codePoint);
                    }
                    throw new AntigravDecodeError("Invalid \\uXXXXXXXX escape", uni, idx);
                }
                string buf = "";
                int begin = idx - 1;
                while (true) {
                    Match chunk = STRINGCHUNK().Match(s, idx);
                    if (!chunk.Success) throw new AntigravDecodeError("Unterminated string starting at", s, begin);
                    idx = chunk.End();
                    buf += chunk.Groups[1].Value;
                    string terminator = chunk.Groups[2].Value;
                    if (terminator == "\"") break;
                    if (terminator != "\\") throw new AntigravDecodeError($"Invalid control character {Regex.Escape(terminator)} at", s, idx);
                    char esc = s.CharAt(idx++) ?? throw new AntigravDecodeError("Unterminated string starting at", s, begin);
                    if (!"Uu".Contains(esc)) {
                        if (BACKSLASH.TryGetValue(esc, out char value)) {
                            buf += value;
                        }
                        else {
                            throw new AntigravDecodeError($"Invalid \\escape: {esc}", s, idx);
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
                    throw new AntigravDecodeError($"Expecting '{c}' delimiter", s, idx);
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
                        throw new AntigravDecodeError("Expecting value", s, (int)err.Value!);
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
                        throw new AntigravDecodeError("Expecting value", s, (int)err.Value!);
                    }
                    _expect_whitespace();
                    nextchar = s.CharAt(idx++);
                    if (nextchar == ']') return values;
                    if (nextchar != ',') throw new AntigravDecodeError("Expecting ',' delimiter", s, idx);
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
                match = INT().Match(s, idx);
                if (match.Success && match.Index == idx) {
                    idx = match.End();
                    switch (s.SubstringSafe(idx, 2)) {
                        case "ll":
                            idx += 2;
                            return Int128.Parse(match.Value);
                        case "LL":
                            idx += 2;
                            return UInt128.Parse(match.Value);
                        default:
                            switch (s.CharAt(idx)) {
                                case 'l':
                                    idx++;
                                    return long.Parse(match.Value);
                                case 'L':
                                    idx++;
                                    return ulong.Parse(match.Value);
                                case 'I':
                                    idx++;
                                    return uint.Parse(match.Value);
                                case 's':
                                    idx++;
                                    return short.Parse(match.Value);
                                case 'S':
                                    idx++;
                                    return ushort.Parse(match.Value);
                                case 'b':
                                    idx++;
                                    return sbyte.Parse(match.Value);
                                case 'B':
                                    idx++;
                                    return byte.Parse(match.Value);
                            }
                            return int.Parse(match.Value);
                    }
                }
                throw new StopIteration(idx);
            }

            object? o;
            try {
                o = _decode();
            }
            catch (StopIteration err) {
                throw new AntigravDecodeError("Expecting value", s, (int)err.Value!);
            }
            if (idx != s.Length) throw new AntigravDecodeError("Extra data", s, idx);
            return (T?)ConvertButNotReally.ChangeType(o, typeof(T?));
        }
    }
}
