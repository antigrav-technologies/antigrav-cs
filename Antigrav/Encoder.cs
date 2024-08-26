using System.Collections;
using System.Data;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static Antigrav.Main;

namespace Antigrav;

internal static class Encoder {
    private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    private static readonly Dictionary<char, string> ESCAPE_DICT = new() {
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
    private static string EncodeString(string s) => s.Select(x => ESCAPE_DICT.TryGetValue(x, out string? value) ? value : char.ToString(x)).ToString()!;
    private static string EncodeStringASCII(string s) => string.Join("",
        s.EnumerateRunes().Select(rune =>
            0x20 <= rune.Value && rune.Value <= 0x7f ?
            char.ToString((char)rune.Value) :
            ESCAPE_DICT.TryGetValue((char)rune.Value, out string? value) ? value :
            rune.Value < 0x10000 ? $"\\u{rune.Value:x4}" : $"\\U{rune.Value:x8}"
        )
    );

    // i hope i dont forget why i called a class so in the feature
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

            if (IsNumber(x) && IsNumber(y)) {
                return Comparer<object>.Default.Compare(x is Complex complexX ? complexX.Real : x, y is Complex complexY ? complexY.Real : y);
            }
            throw new ArgumentException($"Keys must be string, sbyte, byte, ushort, short, int, uint, long, ulong, Int128, UInt128, float, double, decimal, Complex or bool, not {nameof(x)} and/or {nameof(y)}");
        }
    }

    public static string Encode(
        object? o,
        bool sortKeys,
        uint? indent,
        bool ensureASCII,
        bool allowNaN
    ) {
        StringBuilder builder = new();
        Func<string, string> stringEncoder = ensureASCII ? EncodeStringASCII : EncodeString;

        void EncodeInteger(object o, bool prefix = true) {
            builder.Append(o.ToString());
            if (prefix) {
                builder.Append(o switch {
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
            }
        }

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

        static Dictionary<object, object?> ObjectToDict(object o) {
            Dictionary<object, object?> dictionary = [];
            foreach (MemberInfo member in o.GetType().GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)) {
                AntigravSerializable? antigravSerializable = member.GetCustomAttribute<AntigravSerializable>();
                AntigravExtensionData? antigravExtensionData = member.GetCustomAttribute<AntigravExtensionData>();
                if (member is PropertyInfo property) {
                    if (antigravSerializable != null && ((o is not IConditionalAntigravSerializable) || (o is IConditionalAntigravSerializable conditionalSerializable && conditionalSerializable.SerializeIt(antigravSerializable, property)))) dictionary.Add(antigravSerializable.Name ?? property.Name, property.GetValue(o));
                    if (antigravExtensionData != null) dictionary = dictionary.Concat(((IDictionary)property.GetValue(o)!).Keys.Cast<object>().Zip(((IDictionary)property.GetValue(o)!).Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v))).ToDictionary(x => x.Key, x => x.Value);
                }
                if (member is FieldInfo field) {
                    if (antigravSerializable != null && ((o is not IConditionalAntigravSerializable) || (o is IConditionalAntigravSerializable conditionalSerializable && conditionalSerializable.SerializeIt(antigravSerializable, field)))) dictionary.Add(antigravSerializable.Name ?? field.Name, field.GetValue(o));
                    if (antigravExtensionData != null) dictionary = dictionary.Concat(((IDictionary)field.GetValue(o)!).Keys.Cast<object>().Zip(((IDictionary)field.GetValue(o)!).Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v))).ToDictionary(x => x.Key, x => x.Value);
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
