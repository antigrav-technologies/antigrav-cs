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
        {'\x00', "\\u0000"},
        {'\x01', "\\u0001"},
        {'\x02', "\\u0002"},
        {'\x03', "\\u0003"},
        {'\x04', "\\u0004"},
        {'\x05', "\\u0005"},
        {'\x06', "\\u0006"},
        {'\x07', "\\u0007"},
        {'\b', "\\b"},
        {'\t', "\\t"},
        {'\n', "\\n"},
        {'\x0b', "\\u000b"},
        {'\f', "\\f"},
        {'\r', "\\r"},
        {'\x0e', "\\u000e"},
        {'\x0f', "\\u000f"},
        {'\x10', "\\u0010"},
        {'\x11', "\\u0011"},
        {'\x12', "\\u0012"},
        {'\x13', "\\u0013"},
        {'\x14', "\\u0014"},
        {'\x15', "\\u0015"},
        {'\x16', "\\u0016"},
        {'\x17', "\\u0017"},
        {'\x18', "\\u0018"},
        {'\x19', "\\u0019"},
        {'\x1a', "\\u001a"},
        {'\x1b', "\\u001b"},
        {'\x1c', "\\u001c"},
        {'\x1d', "\\u001d"},
        {'\x1e', "\\u001e"},
        {'\x1f', "\\u001f"}
    };
    private static string EncodeString(string s) {
        return '"' + s.Select(x => ESCAPE_DICT.TryGetValue(x, out string? value) ? value : char.ToString(x)).ToString() + '"';
    }
    private static string EncodeStringASCII(string s) => '"' + string.Join("",
        s.EnumerateRunes().Select(rune =>
            rune.Value <= 0x7f ?
            char.ToString((char)rune.Value) :
            ESCAPE_DICT.TryGetValue((char)rune.Value, out string? value) ? value :
            rune.Value < 0x10000 ? $"\\u{rune.Value:x4}" : $"\\U{rune.Value:x8}")
    ) + '"';

    // i hope i dont forget why i called a class so in the feature
    private class NoneOfYourGoddamnBusinees : IComparer<object?> {
        private static bool IsNumber(object? o) => o is sbyte or byte or ushort or short or int or uint or long or ulong or Int128 or UInt128 or float or double or decimal or Complex;

        public int Compare(object? x, object? y) {
            if (x is bool boolX) return y is bool boolY ? boolX ? boolY ? 0 : 1 : -1 : -1;

            if (IsNumber(x)) {
                if (!IsNumber(y)) return -1;
                return Comparer<object>.Default.Compare(x is Complex complexX ? complexX.Real : x, y is Complex complexY ? complexY.Real : y);
            }
            if (x is string stringX && y is string stringY) return String.Compare(stringX, stringY);
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

        void EncodeInteger(object o, bool prefix = true) => builder.Append(o switch {
            sbyte b => b.ToString() + (prefix ? "b" : ""),
            byte B => B.ToString() + (prefix ? "B" : ""),
            short s => s.ToString() + (prefix ? "s" : ""),
            ushort S => S.ToString() + (prefix ? "S" : ""),
            int i => i.ToString(),
            uint I => I.ToString() + (prefix ? "I" : ""),
            long l => l.ToString() + (prefix ? "l" : ""),
            ulong L => L.ToString() + (prefix ? "L" : ""),
            Int128 ll => ll.ToString() + (prefix ? "ll" : ""),
            UInt128 LL => LL.ToString() + (prefix ? "LL" : ""),
            _ => "at this point i dont know this should never run"
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
                float f => 1e-4f < MathF.Abs(f) && MathF.Abs(f) < 1e7f ? f.ToString("0.0#####") : f.ToString(),
                double d => 1e-4 < Math.Abs(d) && Math.Abs(d) < 1e15 ? d.ToString("0.0#############") : d.ToString(),
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

        void Encode(object? o, uint _current_indent_level) {
            if (o is null) {
                builder.Append("null");
                return;
            }
            if (o is string s) {
                builder.Append(stringEncoder(s));
                return;
            }
            if (o is true) {
                builder.Append("true");
                return;
            }
            if (o is false) {
                builder.Append("false");
                return;
            }
            if (o is sbyte or byte or short or ushort or int or uint or long or ulong or Int128 or UInt128) {
                EncodeInteger(o);
                return;
            }
            if (o is Enum @enum) {
                EncodeInteger(Convert.ChangeType(@enum, Enum.GetUnderlyingType(@enum.GetType())));
                return;
            }
            if (o is float or double or decimal) {
                EncodeFloat(o);
                return;
            }
            if (o is Complex c) {
                EncodeComplex(c);
                return;
            }
            if (o is IDictionary d) {
                EncodeDict(d.Keys.Cast<object>().Zip(d.Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v)).ToDictionary(x => x.Key, x => x.Value), _current_indent_level);
                return;
            }
            if (o is ITuple t) {
                EncodeList(t.GetType().GetProperties().Select(p => p.GetValue(t)).ToList(), _current_indent_level);
                return;
            }
            if (o is ICollection l) {
                EncodeList(l.Cast<object?>().ToList(), _current_indent_level);
                return;
            }
            EncodeDict(ObjectToDict(o), _current_indent_level);
        }

        static Dictionary<object, object?> ObjectToDict(object o) {
            Dictionary<object, object?> dictionary = [];
            bool converted = false;
            foreach (MemberInfo member in o.GetType().GetMembers(BINDING_FLAGS).Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)) {
                AntigravSerializable? antigravSerializable = member.GetCustomAttribute<AntigravSerializable>();
                AntigravExtensionData? antigravExtensionData = member.GetCustomAttribute<AntigravExtensionData>();
                if (antigravSerializable != null || antigravExtensionData != null) converted = true;
                if (member is PropertyInfo property) {
                    if (antigravSerializable != null && ((o is not IConditionalAntigravSerializable) || (o is IConditionalAntigravSerializable conditionalSerializable && conditionalSerializable.SerializeIt(antigravSerializable, property)))) dictionary.Add(antigravSerializable.Name ?? property.Name, property.GetValue(o));
                    if (antigravExtensionData != null) dictionary = dictionary.Concat(((IDictionary)property.GetValue(o)!).Keys.Cast<object>().Zip(((IDictionary)property.GetValue(o)!).Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v))).ToDictionary(x => x.Key, x => x.Value);
                }
                if (member is FieldInfo field) {
                    if (antigravSerializable != null && ((o is not IConditionalAntigravSerializable) || (o is IConditionalAntigravSerializable conditionalSerializable && conditionalSerializable.SerializeIt(antigravSerializable, field)))) dictionary.Add(antigravSerializable.Name ?? field.Name, field.GetValue(o));
                    if (antigravExtensionData != null) dictionary = dictionary.Concat(((IDictionary)field.GetValue(o)!).Keys.Cast<object>().Zip(((IDictionary)field.GetValue(o)!).Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v))).ToDictionary(x => x.Key, x => x.Value);
                }
            }
            if (!converted) throw new ArgumentException($"Type is not Antigrav Serializable: {o.GetType()}");
            return dictionary;
        }

        void EncodeList(List<object?> list, uint _current_indent_level) {
            builder.Append('[');
            if (list.Count == 0) {
                builder.Append(']');
                return;
            }
            string separator = ", ";
            string? _newline_indent = null;
            if (indent != null) {
                _current_indent_level++;
                _newline_indent = '\n' + new string(' ', (int)(indent * _current_indent_level));
                separator = "," + _newline_indent;
                builder.Append(_newline_indent);
            }
            bool first = true;
            foreach (object? value in list) {
                if (first) first = false;
                else builder.Append(separator);
                Encode(value, _current_indent_level);
            }
            if (indent != null) {
                _current_indent_level--;
                builder.Append('\n' + new string(' ', (int)(indent * _current_indent_level)));
            }
            builder.Append(']');
        }

        void EncodeDict(Dictionary<object, object?> dict, uint _current_indent_level) {
            builder.Append('{');
            if (dict.Count == 0) {
                builder.Append('}');
                return;
            }
            string separator = ", ";
            string? _newline_indent = null;
            if (indent != null) {
                _current_indent_level++;
                _newline_indent = '\n' + new string(' ', (int)(indent * _current_indent_level));
                separator = "," + _newline_indent;
                builder.Append(_newline_indent);
            }

            if (sortKeys) dict = dict.OrderBy(x => x.Key, new NoneOfYourGoddamnBusinees()).ToDictionary(x => x.Key, x => x.Value);
            bool first = true;
            foreach (var keyValue in dict) {
                if (first) first = false;
                else builder.Append(separator);

                Encode(keyValue.Key, _current_indent_level);
                builder.Append(": ");
                Encode(keyValue.Value, _current_indent_level);
            }
            if (indent != null) {
                _current_indent_level--;
                builder.Append('\n' + new string(' ', (int)(indent * _current_indent_level)));
            }
            builder.Append('}');
        }

        Encode(o, 0);

        return builder.ToString();
    }
}
