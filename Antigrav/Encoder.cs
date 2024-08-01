using System.Collections;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Antigrav;

internal partial class Encoder {

    [GeneratedRegex("[\\x00-\\x1f\\\\\"\\b\\f\\n\\r\\t]")]
    private static partial Regex ESCAPE();

    [GeneratedRegex("([\\\\\"]|[^\\ -~])")]
    private static partial Regex ESCAPE_ASCII();
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
        return '"' + ESCAPE().Replace(s, m => ESCAPE_DICT[m.Value[0]]) + '"';
    }
    private static string EncodeStringASCII(string s) {
        static string replace(Match match) {
            char c = match.Value[0];
            if (ESCAPE_DICT.TryGetValue(c, out string? value)) return value;

            int n = Convert.ToInt32(c);
            if (n < 0x10000) return $"\\u{n:x4}";

            n -= 0x10000;
            int s1 = 0xd800 | ((n >> 10) & 0x3ff);
            int s2 = 0xdc00 | (n & 0x3ff);
            return $"\\u{s1:x4}\\u{s2:x4}";
        }
        return '"' + ESCAPE_ASCII().Replace(s, m => replace(m)) + '"';
    }
    public class ANTIGRAVEncoder(
        bool SortKeys,
        uint? indent,
        bool ensure_ascii,
        bool allow_nan,
        bool skipKeys
    ) {
        private readonly bool sortKeys = SortKeys;
        private readonly uint? indent = indent;
        private readonly bool ensureASCII = ensure_ascii;
        private readonly bool allowNaN = allow_nan;
        private readonly bool skipKeys = skipKeys;

        public string Encode(object? o) {
            Func<string, string> _encoder = ensureASCII ? EncodeStringASCII : EncodeString;

            string _encode_integer(object o, bool prefix = true) => o switch {
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
            };

            string _format_float(object o) => o switch {
                float f => f.ToString(),
                double d => d.ToString(),
                decimal m => m.ToString(),
                _ => "idk bruvver it should never call"
            };

            bool _is_inf(object o) => o switch {
                float f => float.IsInfinity(f),
                double d => double.IsInfinity(d),
                _ => false,
            };

            bool _is_ninf(object o) => o switch {
                float f => float.IsNegativeInfinity(f),
                double d => double.IsNegativeInfinity(d),
                _ => false,
            };

            string _encode_float(object o) {
                string prefix = "";
                if (o is float) prefix = "F";
                // if (o is double)
                if (o is decimal) prefix = "M";

                string? text = null;
#pragma warning disable CS1718 // Выполнено сравнение с той же переменной
                if (o != o) text = "NaN";
                else if (_is_inf(o)) text = "inf";
                else if (_is_ninf(o)) text = "-inf";

                if (!allowNaN && text != null) {
                    throw new ArgumentException($"Out of range float values are not JSON compliant: {o}");
                }

                text ??= _format_float(o);

                return text + prefix;
            }

            string _encode(object? o, uint _current_indent_level) => o switch {
                string s => _encoder(s),
                null => "null",
                true => "true",
                false => "false",
                { } when o is sbyte or byte or short or ushort or int or uint or long or ulong or Int128 or UInt128 => _encode_integer(o),
                { } when o is float or double or decimal => _encode_float(o),
                Complex c => $"{_format_float(c.Real)}+{_format_float(c.Imaginary)}i",
                { } when o is IList e => _encode_list(e.Cast<object>().ToList(), _current_indent_level),
                { } when o is IDictionary d => _encode_dict(d.Keys.Cast<object>().Zip(d.Values.Cast<object?>(), (k, v) => new KeyValuePair<object, object?>(k, v)).ToDictionary(x => x.Key, x => x.Value), _current_indent_level),
                _ => throw new ArgumentException($"Type is not ANTIGRAV Serializable: {o.GetType()}")
            };

            string _encode_list(List<object> list, uint _current_indent_level) {
                if (list.Count == 0) return "[]";
                string buf = "[";
                string separator = ", ";
                string? _newline_indent = null;
                if (indent != null) {
                    _current_indent_level++;
                    _newline_indent = '\n' + new string(' ', (int)(indent * _current_indent_level));
                    separator = "," + _newline_indent;
                    buf += _newline_indent;
                }
                bool first = true;
                foreach (object value in list) {
                    if (first) first = false;
                    else buf += separator;
                    buf += _encode(value, _current_indent_level);
                }
                if (indent != null) {
                    _current_indent_level--;
                    buf += '\n' + new string(' ', (int)(indent * _current_indent_level));
                }
                buf += ']';
                return buf;
            }

            string _encode_dict(Dictionary<object, object?> dict, uint _current_indent_level) {
                if (dict.Count == 0) return "{}";
                string buf = "{";
                string separator = ", ";
                string? _newline_indent = null;
                if (indent != null) {
                    _current_indent_level++;
                    _newline_indent = '\n' + new string(' ', (int)(indent * _current_indent_level));
                    separator = "," + _newline_indent;
                    buf += _newline_indent;
                }

                Dictionary<string, object?> newDict = [];

                foreach (var keyValue in dict) {
                    object k = keyValue.Key;
                    object? v = keyValue.Value;

                    if (k is null) {
                        newDict.Add("null", v);
                    }
                    else if (k is string s) {
                        newDict.Add(s, v);
                    }
                    else if (k is true) {
                        newDict.Add("true", v);
                    }
                    else if (k is false) {
                        newDict.Add("false", v);
                    }
                    else if (k is sbyte or byte or ushort or short or int or uint or long or ulong or Int128 or UInt128) {
                        newDict.Add(_encode_integer(k, prefix: false), v);
                    }
                    else if (k is float || k is double || k is decimal) {
                        newDict.Add(_encode_float(k), v);
                    }
                    else if (skipKeys) continue;
                    else throw new ArgumentException($"keys must be string, sbyte, byte, ushort, short, int, uint, long, float, double, bool or null, not {nameof(k)}");
                }

                if (sortKeys) newDict = newDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                bool first = true;
                foreach (var keyValue in newDict) {
                    if (first) first = false;
                    else buf += separator;

                    buf += _encoder(keyValue.Key);
                    buf += ": ";
                    buf += _encode(keyValue.Value, _current_indent_level);
                }
                if (indent != null) {
                    _current_indent_level--;
                    buf += '\n' + new string(' ', (int)(indent * _current_indent_level));
                }
                buf += '}';
                return buf;
            }

            return _encode(o, 0);
        }
    }
}
