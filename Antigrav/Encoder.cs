using System.Collections;
using System.Data;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Antigrav;

internal static class Encoder {
    private static bool IsUserDefined(this MemberInfo memberInfo) => memberInfo.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length == 0;
    private static string Name(this MemberInfo memberInfo) => memberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.Name,
        FieldInfo fieldInfo => fieldInfo.Name,
        _ => "ъ"
    };
    private static object? GetValue(this MemberInfo memberInfo, object? o) => memberInfo switch {
        PropertyInfo propertyInfo => (propertyInfo.GetGetMethod() ?? throw new InvalidOperationException($"Property {propertyInfo.Name} does not have a getter method.")).Invoke(o, []),
        FieldInfo fieldInfo => fieldInfo.GetValue(o),
        _ => throw new Exception("instant death of instant death of the universe")
    };
    private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
    private static readonly Dictionary<int, string> ESCAPE_DICT = new() {
        {'\\', "\\\\"},
        {'"', "\\\""},
        {'\x00', "\\0"},
        {'\x01', "\\x01"},
        {'\x02', "\\x02"},
        {'\x03', "\\x03"},
        {'\x04', "\\x04"},
        {'\x05', "\\x05"},
        {'\x06', "\\x06"},
        {'\x07', "\\a"},
        {'\x08', "\\b"},
        {'\x09', "\\t"},
        {'\x0a', "\\n"},
        {'\x0b', "\\v"},
        {'\x0c', "\\f"},
        {'\x0d', "\\r"},
        {'\x0e', "\\x0e"},
        {'\x0f', "\\x0f"},
        {'\x10', "\\x10"},
        {'\x11', "\\x11"},
        {'\x12', "\\x12"},
        {'\x13', "\\x13"},
        {'\x14', "\\x14"},
        {'\x15', "\\x15"},
        {'\x16', "\\x16"},
        {'\x17', "\\x17"},
        {'\x18', "\\x18"},
        {'\x19', "\\x19"},
        {'\x1a', "\\x1a"},
        {'\x1b', "\\x1b"},
        {'\x1c', "\\x1c"},
        {'\x1d', "\\x1d"},
        {'\x1e', "\\x1e"},
        {'\x1f', "\\x1f"}
    };
    private static string EncodeString(string s) => string.Join("", s.EnumerateRunes().Select(x => ESCAPE_DICT.TryGetValue(x.Value, out string? value) ? value : x.ToString()));
    private static string EncodeStringASCII(string s) => string.Join("",
        s.EnumerateRunes().Select(rune =>
            0x20 <= rune.Value && rune.Value <= 0x7f ?
            char.ToString((char)rune.Value) :
            ESCAPE_DICT.TryGetValue((char)rune.Value, out string? value) ? value :
            rune.Value < 0x10000 ? $"\\u{rune.Value:x4}" : $"\\U{rune.Value:x8}"
        )
    );

    // i hope i dont forget why i called a class so in the future
    private class NoneOfYourGoddamnBusinees : IComparer<object?> {
        private static bool IsNumber(object? o) => o is sbyte or byte or ushort or short or int or uint or long or ulong or Int128 or UInt128 or float or double or decimal or Complex;

        public int Compare(object? x, object? y) {
            if (x is bool boolX) return y is bool boolY ? boolX ? boolY ? 0 : 1 : -1 : -1;

            if (x is string stringX) {
                if (y is string stringY) return string.Compare(stringX, stringY);
                return -1;
            }

            if (x!.GetType().IsEnum) x = Convert.ChangeType((Enum)x, Enum.GetUnderlyingType(x.GetType()));
            if (y!.GetType().IsEnum) y = Convert.ChangeType((Enum)y, Enum.GetUnderlyingType(y.GetType()));

            if (IsNumber(x)) {
                if (IsNumber(y)) return Comparer<object>.Default.Compare(x is Complex complexX ? complexX.Real : x, y is Complex complexY ? complexY.Real : y);
                return -1;
            }
            return 0;
        }
    }

    public static string Encode(
        object? o,
        bool sortKeys,
        uint? indent,
        bool ensureASCII,
        bool allowNaN,
        bool forceSave
    ) {
        StringBuilder builder = new();
        Func<string, string> stringEncoder = ensureASCII ? EncodeStringASCII : EncodeString;

        void EncodeInteger(object o) => builder.Append(o.ToString() + o switch {
            sbyte => "b",
            byte => "B",
            short => "s",
            ushort => "S",
            uint => "I",
            long => "l",
            ulong => "L",
            Int128 => "ll",
            UInt128 => "LL",
            _ => ""
        });

        void EncodeFloat(object o) {
            string prefix = "";
            if (o is float) prefix = "F";
            if (o is decimal) prefix = "M";

            string? text = null;
            if ((o is float f1 && float.IsNaN(f1)) || (o is double d1 && double.IsNaN(d1))) text = "NaN";
            else if ((o is float f2 && float.IsPositiveInfinity(f2)) || (o is double d2 && double.IsPositiveInfinity(d2))) text = "inf";
            else if ((o is float f3 && float.IsNegativeInfinity(f3)) || (o is double d3 && double.IsNegativeInfinity(d3))) text = "-inf";

            if (!allowNaN && text != null) {
                throw new ArgumentException($"Out of range float values are not Antigrav compliant: {o}");
            }

            text ??= o switch {
                float f => (f == 0) || (1e-4f < MathF.Abs(f) && MathF.Abs(f) < 1e7f) ? f.ToString("0.0#####") : f.ToString("e"),
                double d => (d == 0) || (1e-4 < Math.Abs(d) && Math.Abs(d) < 1e15) ? d.ToString("0.0#############") : d.ToString("e"),
                decimal m => m.ToString("0.0#############################"),
                _ => "idk bruvver it should never call"
            };

            builder.Append(text + prefix);
        }

        void EncodeComplex(Complex c) {
            EncodeFloat(c.Real);
            builder.Append(c.Imaginary < 0 ? '-' : '+');
            EncodeFloat(Math.Abs(c.Imaginary));
            builder.Append('i');
        }

        void EncodeAny(object? o, uint currentIndentLevel) {
            switch (o) {
                case null:
                    builder.Append("null");
                    return;
                case char @char:
                    builder.Append("'" + stringEncoder(char.ToString(@char)) + "'");
                    return;
                case string s:
                    builder.Append('"' + stringEncoder(s) + '"');
                    return;
                case bool b:
                    builder.Append(b ? "true" : "false");
                    return;
                case sbyte or byte or short or ushort or int or uint or long or ulong or Int128 or UInt128:
                    EncodeInteger(o);
                    return;
                case Enum @enum:
                    EncodeInteger(Convert.ChangeType(@enum, Enum.GetUnderlyingType(@enum.GetType())));
                    return;
                case float or double or decimal:
                    EncodeFloat(o);
                    return;
                case Complex c:
                    EncodeComplex(c);
                    return;
                case IDictionary d:
                    EncodeDict(d.Keys.Cast<object>().Zip(d.Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v)).ToDictionary(x => x.Key, x => x.Value), currentIndentLevel);
                    return;
                case ITuple t:
                    EncodeList(t.GetType().GetProperties().Select(p => p.GetValue(t)).ToList(), currentIndentLevel);
                    return;
                case ICollection l:
                    EncodeList(l.Cast<object?>().ToList(), currentIndentLevel);
                    return;
                default:
                    EncodeDict(ObjectToDict(o), currentIndentLevel);
                    return;
            }
        }
        
        Dictionary<object, object?> ObjectToDict(object o) {
            Dictionary<object, object?> dictionary = [];
            foreach (MemberInfo member in o.GetType().GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)) {
                AntigravSerializable? antigravSerializable = member.GetCustomAttribute<AntigravSerializable>();
                AntigravExtensionData? antigravExtensionData = member.GetCustomAttribute<AntigravExtensionData>();
                string name = antigravSerializable == null ? member.Name() : antigravSerializable.Name ?? member.Name();
                object? value = member.GetValue(o);
                if ((forceSave && member.IsUserDefined()) || (antigravSerializable != null))
                    dictionary.Add(name, value);
                else if (antigravExtensionData != null) {
                    IDictionary? extensionData = (IDictionary?)value;
                    if (extensionData == null) continue;
                    dictionary = dictionary.Concat(
                        extensionData.Keys.Cast<object>().Zip(extensionData.Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v))
                    ).ToDictionary(x => x.Key, x => x.Value);
                }
            }
            return dictionary;
        }

        void EncodeList(List<object?> list, uint currentIndentLevel) {
            builder.Append('[');
            if (list.Count == 0) {
                builder.Append(']');
                return;
            }
            string separator = ", ";
            string? _newline_indent = null;
            if (indent != null) {
                currentIndentLevel++;
                _newline_indent = '\n' + new string(' ', (int)(indent * currentIndentLevel));
                separator = "," + _newline_indent;
                builder.Append(_newline_indent);
            }
            bool first = true;
            foreach (object? value in list) {
                if (first) first = false;
                else builder.Append(separator);
                EncodeAny(value, currentIndentLevel);
            }
            if (indent != null) {
                currentIndentLevel--;
                builder.Append('\n' + new string(' ', (int)(indent * currentIndentLevel)));
            }
            builder.Append(']');
        }

        void EncodeDict(Dictionary<object, object?> dict, uint currentIndentLevel) {
            builder.Append('{');
            if (dict.Count == 0) {
                builder.Append('}');
                return;
            }
            string separator = ", ";
            string? _newline_indent = null;
            if (indent != null) {
                currentIndentLevel++;
                _newline_indent = '\n' + new string(' ', (int)(indent * currentIndentLevel));
                separator = "," + _newline_indent;
                builder.Append(_newline_indent);
            }

            if (sortKeys) dict = dict.OrderBy(x => x.Key, new NoneOfYourGoddamnBusinees()).ToDictionary(x => x.Key, x => x.Value);
            bool first = true;
            foreach (var keyValue in dict) {
                if (first) first = false;
                else builder.Append(separator);

                EncodeAny(keyValue.Key, currentIndentLevel);
                builder.Append(": ");
                EncodeAny(keyValue.Value, currentIndentLevel);
            }
            if (indent != null) {
                currentIndentLevel--;
                builder.Append('\n' + new string(' ', (int)(indent * currentIndentLevel)));
            }
            builder.Append('}');
        }

        EncodeAny(o, 0);

        return builder.ToString();
    }
}
