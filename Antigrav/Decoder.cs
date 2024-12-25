using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Antigrav;

file static class Extensions {
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

internal static partial class Decoder {
    private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    private const RegexOptions FLAGS = RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled;

    [GeneratedRegex(@"-?\d+", FLAGS)]
    private static partial Regex INT();

    [GeneratedRegex(@"([-+]?)((\d*(?:\.\d+|[eE][-+]?\d+))|inf|nan)", FLAGS)]
    private static partial Regex FLOAT();

    [GeneratedRegex(@"([-+]?)((\d+(?:\.\d+|[eE][-+]?\d+))|inf|nan)" +
                    @"([+-])((\d+(?:\.\d+|[eE][-+]?\d+))|inf|nan)i", FLAGS)]
    private static partial Regex COMPLEX();

    private static readonly Dictionary<char, char> BACKSLASH = new() {
        {'\\', '\\'},
        {'"', '"'},
        {'0', '\0'},
        {'a', '\a'},
        {'b', '\b'},
        {'t', '\t'},
        {'n', '\n'},
        {'v', '\v'},
        {'f', '\f'},
        {'r', '\r'}
    };
    
    private static class ConvertButNotReally {
        private static Array DecodeArray(object o, Type type) {
            Type elementType = type.GetElementType()!;
            Array array = Array.CreateInstance(elementType, ((ICollection)o).Count);
            int i = 0;
            foreach (object item in (IEnumerable)o) {
                array.SetValue(ChangeType(item, elementType), i++);
            }
            return array;
        }

        private static object DecodeDictionary(object o, Type type) {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];
            Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            object dict = Activator.CreateInstance(dictType)!;
            MethodInfo method = dictType.GetMethod("Add")!;
            foreach (KeyValuePair<object, object?> entry in (IEnumerable)o) {
                method.Invoke(dict, [ChangeType(entry.Key, keyType), ChangeType(entry.Value, valueType)]);
            }
            return dict;
        }

        private static object DecodeList(object o, Type type) {
            Type elementType = type.GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(elementType);
            object list = Activator.CreateInstance(listType)!;
            MethodInfo method = listType.GetMethod("Add")!;
            foreach (object item in (IEnumerable)o) {
                method.Invoke(list, [ChangeType(item, elementType)]);
            }
            return list;
        }

        private static object TryCreateObject(Type targetType) {
            try {
                return Activator.CreateInstance(targetType)!;
            }
            catch (MissingMethodException) {
#pragma warning disable SYSLIB0050
                return FormatterServices.GetSafeUninitializedObject(targetType);
            }
        }

        private static object DecodeObject(object o, Type type) {
            if (o is not Dictionary<object, object>) throw new AntigravCastingError(o.GetType(), type);
            object target = TryCreateObject(type);
            var dictionary = (Dictionary<object, object?>)ChangeType(o, typeof(Dictionary<object, object?>))!;
            foreach (var member in type.GetMembers(BINDING_FLAGS).Where(m => m.MemberType is MemberTypes.Property or MemberTypes.Field)) {
                AntigravSerializable? antigravSerializable = member.GetCustomAttribute<AntigravSerializable>();
                string name = antigravSerializable == null ? member.Name : antigravSerializable.Name ?? member.Name;
                if (antigravSerializable != null) {
                    member.SetValue(target, ChangeType(dictionary.TryGetValue(name, out var v) ? v : antigravSerializable.LoadAsNull ? antigravSerializable.DefaultValue : antigravSerializable.DefaultValue ?? TryCreateObject(member.Type()), member.Type()));
                }
                else if (dictionary.ContainsKey(member.Name)) {
                    member.SetValue(target, ChangeType(dictionary[name], member.Type()));
                }
                dictionary.Remove(name);
            }
            MemberInfo? extensionsMember = type.GetMembers(BINDING_FLAGS).Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field).FirstOrDefault(member => member.GetCustomAttribute<AntigravExtensionData>() != null);
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
            if (type == typeof(bool)) return (bool)o;
            if (type.IsEnum) return Enum.ToObject(type, o);
            if (type.IsArray) return DecodeArray(o, type);
            if (type.IsGenericType) {
                Type type_ = type.GetGenericTypeDefinition();
                
                switch (type_) {
                    case var _ when type_.IsAssignableTo(typeof(Tuple<>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,,,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,,,,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,,,,,>)):
                    case var _ when type_.IsAssignableTo(typeof(Tuple<,,,,,,,>)):
                        return Activator.CreateInstance(type, ((IList)o).Cast<object>().ToArray());
                    case var _ when type_.IsAssignableTo(typeof(Dictionary<,>)):
                        return DecodeDictionary(o, type);
                    case var _ when type_.IsAssignableTo(typeof(List<>)):
                        return DecodeList(o, type);
                }
            }
            return DecodeObject(o, type);
        }
    }

    private class StopIteration(int value) : Exception("Stop iteration") {
        public int Value { get; } = value;
    }

    public static T? Decode<T>(string s) {
        int idx = 0;
        
        object? o;
        try {
            o = DecodeAny();
        }
        catch (StopIteration err) {
            throw new AntigravDecodeError("Expecting value", s, err.Value);
        }
        if (idx != s.Length) throw new AntigravDecodeError("Extra data", s, idx);
        return (T?)ConvertButNotReally.ChangeType(o, typeof(T?));
        
        string DecodeString() {
            StringBuilder builder = new();

            int begin = idx - 1;
            while (true) {
                if (idx >= s.Length || s[idx] < ' ') {
                    throw new AntigravDecodeError("Unterminated string starting at", s, begin);
                }
                char nextchar = s[idx++];
                if (nextchar == '"') break;
                if (nextchar == '\\') {
                    if (idx >= s.Length) throw new AntigravDecodeError("Unterminated string starting at", s, idx);
                    char esc = s[idx++];
                    switch (esc) {
                        case 'x':
                            _decode_xXX();
                            break;
                        case 'u':
                            _decode_uXXXX();
                            break;
                        case 'U':
                            _decode_uXXXXXXXX();
                            break;
                        default:
                            if (BACKSLASH.TryGetValue(esc, out char value)) {
                                builder.Append(value);
                            }
                            else {
                                throw new AntigravDecodeError($"Invalid \\ escape: {esc}", s, idx);
                            }
                            break;
                    }
                }
                else {
                    builder.Append(nextchar);
                    if (!char.IsSurrogate(nextchar)) continue;
                    idx++;
                    if (idx >= s.Length) throw new AntigravDecodeError("Unterminated string starting at", s, idx);
                    builder.Append(s[idx]);
                }
            }
            return builder.ToString();
            
            void _decode_xXX() {
                string uni = s.SubstringSafe(idx, 2);
                if (uni.Length == 2 && short.TryParse(uni, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out short codePoint)) {
                    builder.Append((char)codePoint);
                    idx += 2;
                }
                else throw new AntigravDecodeError("Invalid \\xXX escape", uni, idx);
            }
            void _decode_uXXXX() {
                string uni = s.SubstringSafe(idx, 4);
                if (uni.Length == 4 && short.TryParse(uni, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out short codePoint)) {
                    builder.Append((char)codePoint);
                    idx += 4;
                }
                else throw new AntigravDecodeError("Invalid \\uXXXX escape", uni, idx);
            }
            void _decode_uXXXXXXXX() {
                string uni = s.SubstringSafe(idx, 8);
                if (uni.Length == 8 && int.TryParse(uni, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int codePoint)) {
                    builder.Append(char.ConvertFromUtf32(codePoint));
                    idx += 8;
                }
                else throw new AntigravDecodeError("Invalid \\uXXXXXXXX escape", uni, idx);
            }
        }

        void ExpectChar(char c) {
            SkipWhitespace();
            if (s[idx] != c) {
                throw new AntigravDecodeError($"Expecting '{c}' delimiter", s, idx);
            }
            idx++;
        }

        void SkipWhitespace() {
            while (idx < s.Length && " \t\n\r".Contains(s[idx])) {
                unchecked { idx++; }
            }
        }

        Dictionary<object, object?> DecodeDict() {
            Dictionary<object, object?> pairs = [];

            SkipWhitespace();
            if (s.CharAt(idx) == '}') {
                idx++;
                return pairs;
            }

            while (true) {
                SkipWhitespace();
                object key = DecodeAny() ?? throw new ArgumentException("Dictionaries keys can't be null");
                ExpectChar(':');

                try {
                    SkipWhitespace();
                    pairs.Add(key, DecodeAny());
                }
                catch (StopIteration err) {
                    throw new AntigravDecodeError("Expecting value", s, err.Value);
                }

                SkipWhitespace();
                if (s.CharAt(idx) == '}') {
                    idx++;
                    return pairs;
                }
                ExpectChar(',');
            }
        }

        List<object?> DecodeList() {
            List<object?> values = [];
            SkipWhitespace();
            char? nextchar = s.CharAt(idx);
            if (nextchar == ']') {
                idx++;
                return values;
            }
            while (true) {
                SkipWhitespace();
                try {
                    values.Add(DecodeAny());
                }
                catch (StopIteration err) {
                    throw new AntigravDecodeError("Expecting value", s, err.Value);
                }
                SkipWhitespace();
                if (s.CharAt(idx) == ']') {
                    idx++;
                    return values;
                }
                ExpectChar(',');
            }
        }

        object? DecodeAny() {
            char nextchar;
            try {
                nextchar = s[idx];
            }
            catch (IndexOutOfRangeException) {
                throw new StopIteration(idx);
            }

            switch (nextchar) {
                case '"':
                    idx++;
                    return DecodeString();
                case '{':
                    idx++;
                    return DecodeDict();
                case '[':
                    idx++;
                    return DecodeList();
                case 'n' when s.SubstringSafe(idx, 4) == "null":
                    idx += 4;
                    return null;
                case 't' when s.SubstringSafe(idx, 4) == "true":
                    idx += 4;
                    return true;
                case 'f' when s.SubstringSafe(idx, 5) == "false":
                    idx += 5;
                    return false;
            }

            Match match = COMPLEX().Match(s, idx);
            if (match.Success && match.Index == idx) {
                // миша☘️go to нахуй 😾собирайся в садик🏡идидиди 😭misha get up quickly 🥺ДА ИДЕ НАХУУУУУ 🐀😅
                idx = match.End();
                return new Complex(
                    match.Groups[2].Value switch {
                        "inf" => double.PositiveInfinity,
                        "nan" => double.NaN,
                        _ => double.Parse(match.Groups[2].Value)
                    } * (match.Groups[1].Value == "-" ? -1 : 1),
                    match.Groups[5].Value switch {
                        "inf" => double.PositiveInfinity,
                        "nan" => double.NaN,
                        _ => double.Parse(match.Groups[5].Value)
                    } * (match.Groups[4].Value == "-" ? -1 : 1)
                );
            }
            match = FLOAT().Match(s, idx);
            if (match.Success && match.Index == idx) {
                string rest = match.Groups[2].Value;
                string sign = match.Groups[1].Value;
                idx = match.End();
                switch (char.ToUpper(s.CharAt(idx) ?? 'ъ')) {
                    case 'F':
                        idx++;
                        return rest switch {
                            "inf" => float.PositiveInfinity,
                            "nan" => float.NaN,
                            _ => float.Parse(rest)
                        } * (sign == "-" ? -1 : 1);
                    case 'M':
                        idx++;
                        return decimal.Parse(rest) * (sign == "-" ? -1 : 1);
                    default:
                        return rest switch {
                            "inf" => double.PositiveInfinity,
                            "nan" => double.NaN,
                            _ => double.Parse(rest)
                        } * (sign == "-" ? -1 : 1);
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
    }
}
