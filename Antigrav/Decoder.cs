using System.Text.RegularExpressions;

namespace Antigrav;
public static class MatchEndExtension {
    public static int End(this Match match) => match.Index + match.Length;
    public static bool IsWhitespace(this char s) => " \t\n\r".Contains(s);
    public static bool IsWhitespace(this char? s) => s != null && ((char)s).IsWhitespace();
}

internal partial class Decoder {
    private const RegexOptions FLAGS = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Singleline;
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

    [GeneratedRegex("(.*?)([\"\\\\\\x00-\\x1f])", FLAGS)]
    private static partial Regex STRINGCHUNK();

    [GeneratedRegex("[ \\t\\n\\r]*", FLAGS)]
    private static partial Regex WHITESPACE();

    [GeneratedRegex("(-?(?:0|[1-9]\\d*))(\\.\\d+)?([eE][-+]?\\d+)?([Ff])", FLAGS)]
    private static partial Regex FLOAT();

    [GeneratedRegex("(-?(?:0|[1-9]\\d*))(\\.\\d+)?([eE][-+]?\\d+)?", FLAGS)]
    private static partial Regex DOUBLE();

    public class ANTIGRAVDecodeError(string msg, string doc, int pos) : Exception(
        $"{msg}: " +
            $"line {doc[..pos].Count(c => c == '\n') + 1} " +
            $"column {(doc.LastIndexOf('\n', pos) == -1 ? pos + 1 : pos - doc.LastIndexOf('\n', pos))} " +
            $"(char {pos})"
        ) {
    }

    public class ANTIGRAVDecoder {
        public object Decode(string s) {
            int _decode_uXXXX(string s, int pos) {
                string esc = s.Substring(pos + 1, 4);
                if (esc.Length == 4 && 'x' == Char.ToLower(esc[1])) {
                    try {
                        return Convert.ToInt32(esc, 16);
                    }
                    catch (FormatException) {
                        // do nothing
                    }
                }
                throw new ANTIGRAVDecodeError("Invalid \\uXXXX escape", esc, pos);
            }

            string _decode_string(string s, ref int end, bool strict = true) {
                string buf = "";
                int begin = end - 1;
                while (true) {
                    Match chunk = STRINGCHUNK().Match(s, end);
                    if (!chunk.Success) {
                        throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                    }
                    end = chunk.End();
                    GroupCollection group = chunk.Groups;
                    string content = group[1].Value;
                    string terminator = group[2].Value;
                    if (!string.IsNullOrEmpty(content)) {
                        buf += content;
                    }
                    if (terminator == "\"") {
                        break;
                    }
                    else if (terminator != "\\") {
                        if (strict) {
                            throw new ANTIGRAVDecodeError($"Invalid control character {Regex.Escape(terminator)} at", s, end);
                        }
                        else {
                            buf += terminator;
                            continue;
                        }
                    }
                    char esc;
                    try {
                        esc = s[end];
                    }
                    catch (IndexOutOfRangeException) {
                        throw new ANTIGRAVDecodeError("Unterminated string starting at", s, begin);
                    }
                    char c;
                    if (esc != 'u') {
                        if (BACKSLASH.TryGetValue(esc, out char value)) {
                            c = value;
                        }
                        else {
                            throw new ANTIGRAVDecodeError($"Invalid \\escape: {esc}", s, end);
                        }
                        end++;
                    }
                    else {
                        int uni = _decode_uXXXX(s, end);
                        end += 5;
                        if (0xd800 <= uni && uni <= 0xdbff && s.Substring(end, 2) == "\\u") {
                            int uni2 = _decode_uXXXX(s, end + 1);
                            if (0xdc00 <= uni2 && uni2 <= 0xdfff) {
                                uni = 0x10000 + (((uni - 0xd800) << 10) | (uni2 - 0xdc00));
                                end += 6;
                            }
                        }
                        c = (char)uni;
                    }
                    buf += c;
                }
                return buf;
            }

            Dictionary<string, object> _decode_dict(string s, ref int end, bool strict) {
                Dictionary<string, string> memo = [];
                Dictionary<string, object> pairs = [];

                char? nextchar = end <= s.Length ? s[end] : null;
                if (nextchar != null && nextchar != '"') {
                    if (nextchar.IsWhitespace()) {
                        end = WHITESPACE().Match(s, end).End();
                        nextchar = s[end];
                    }
                    if (nextchar == '}') {
                        end++;
                        return [];
                    }
                    else if (nextchar != '"') {
                        throw new ANTIGRAVDecodeError("Expecting property name enclosed in double quotes", s, end);
                    }
                }
                end++;
                while (true) {
                    string key = _decode_string(s, ref end, strict);
                    if (memo.TryGetValue(key, out string? v)) {
                        key = v;
                    }
                    else {
                        memo.Add(key, key);
                    }
                    if ((end <= s.Length ? s[end] : null) != ':') {
                        end = WHITESPACE().Match(s, end).End();
                        if ((end <= s.Length ? s[end] : null) != ':') {
                            throw new ANTIGRAVDecodeError("Expecting ':' delimiter", s, end);
                        }
                    }
                    end++;

                    try {
                        if (s[end].IsWhitespace()) {
                            end++;
                            if (s[end].IsWhitespace()) {
                                end = WHITESPACE().Match(s, end).End();
                            }
                        }
                    }
                    catch (IndexOutOfRangeException) { }

                    object value;
                    try {
                        value = _decode(s, ref end);
                    }
                    catch (StopIteration err) {
                        throw new ANTIGRAVDecodeError("Expecting value", s, err.value);
                    }
                    pairs.Add(key, value);
                    try {
                        nextchar = s[end];
                        if (nextchar.IsWhitespace()) {
                            end = WHITESPACE().Match(s, end + 1).End();
                            nextchar = s[end];
                        }
                    }
                    catch (ArgumentOutOfRangeException) {
                        nextchar = null;
                    }
                    end++;

                    if (nextchar == '}') break;
                    if (nextchar != ',') throw new ANTIGRAVDecodeError("Expecting ',' delimiter", s, end - 1);

                    end = WHITESPACE().Match(s, end).End();
                    nextchar = end <= s.Length ? s[end] : null;
                    end++;

                    if (nextchar != '"') throw new ANTIGRAVDecodeError("Expecting property name enclosed in double quotes", s, end - 1);
                }
                return pairs;
            }

            List<object> _decode_list(string s, ref int end) {
                List<object> values = [];
                char? nextchar = end <= s.Length ? s[end] : null;
                if (nextchar.IsWhitespace()) {
                    end = WHITESPACE().Match(s, end + 1).End();
                    nextchar = end <= s.Length ? s[end] : null;
                    if (nextchar == ']') {
                        end++;
                        return values;
                    }
                }
                while (true) {
                    try {
                        values.Add(_decode(s, ref end));
                    }
                    catch (StopIteration err) {
                        throw new ANTIGRAVDecodeError("Expecting value", s, err.value);
                    }
                    nextchar = end <= s.Length ? s[end] : null;
                    if (nextchar.IsWhitespace()) {
                        end = WHITESPACE().Match(s, end + 1).End();
                        nextchar = end <= s.Length ? s[end] : null;
                    }
                    end++;
                    if (nextchar == ']') break;
                    if (nextchar != ',') throw new ANTIGRAVDecodeError("Expecting ',' delimiter", s, end - 1);

                    try {
                        if (s[end].IsWhitespace()) {
                            end++;
                            if (s[end].IsWhitespace()) {
                                end = WHITESPACE().Match(s, end).End();
                            }
                        }
                    }
                    catch (IndexOutOfRangeException) { }
                }
                return values;
            }

            object _decode(string s, ref int end) {
                throw new NotImplementedException();
            }

            int idx = 0;
            return _decode(s, ref idx);
        }
    }
}
