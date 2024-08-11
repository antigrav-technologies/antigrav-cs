using System;
using System.Text.RegularExpressions;
using static Antigrav.Regexs;

namespace Antigrav;
public static class MatchEndExtension {
    public static int End(this Match match) => match.Index + match.Length;
    public static bool IsWhitespace(this char c) => " \t\n\r".Contains(c);
    public static bool IsWhitespace(this char? c) => c != null && ((char)c).IsWhitespace(); // "01234" 
    public static string? SubstringSafe(this string s, int startIndex, int length) => startIndex + length <= s.Length ? s.Substring(startIndex, length) : null;
    public static char? CharAt(this string s, int index) => index <= s.Length ? s[index] : null;
}

internal partial class Decoder {
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

    public static object? Decode(string s) {
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

        string _decode_string(string s, ref int end) {
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
                    throw new ANTIGRAVDecodeError($"Invalid control character {Regex.Escape(terminator)} at", s, end);
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

        Dictionary<string, object?> _decode_dict(string s, ref int end) {
            Dictionary<string, string> memo = [];
            Dictionary<string, object?> pairs = [];

            char? nextchar = s.CharAt(end);
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
                string key = _decode_string(s, ref end);
                if (memo.TryGetValue(key, out string? v)) {
                    key = v;
                }
                else {
                    memo.Add(key, key);
                }
                if (s.CharAt(end) != ':') {
                    end = WHITESPACE().Match(s, end).End();
                    if (s.CharAt(end) != ':') {
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

                object? value;
                try {
                    value = _decode(s, ref end);
                }
                catch (StopIteration err) {
                    throw new ANTIGRAVDecodeError("Expecting value", s, err.Value);
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
                nextchar = s.CharAt(end);
                end++;

                if (nextchar != '"') throw new ANTIGRAVDecodeError("Expecting property name enclosed in double quotes", s, end - 1);
            }
            return pairs;
        }

        List<object?> _decode_list(string s, ref int end) {
            List<object?> values = [];
            char? nextchar = s.CharAt(end);
            if (nextchar.IsWhitespace()) {
                end = WHITESPACE().Match(s, end + 1).End();
                nextchar = s.CharAt(end);
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
                    throw new ANTIGRAVDecodeError("Expecting value", s, err.Value);
                }
                nextchar = s.CharAt(end);
                if (nextchar.IsWhitespace()) {
                    end = WHITESPACE().Match(s, end + 1).End();
                    nextchar = s.CharAt(end);
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

        object? _decode(string s, ref int end) {
            char nextchar;
            try {
                nextchar = s[end];
            }
            catch (IndexOutOfRangeException) {
                throw new StopIteration(end);
            }

            if (nextchar == '"') {
                end++;
                return _decode_string(s, ref end);
            }
            if (nextchar == '{') {
                end++;
                return _decode_dict(s, ref end);
            }
            if (nextchar == '[') {
                end++;
                return _decode_list(s, ref end);
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
            Match match = COMPLEX().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = DECIMAL().Match(s, end);
            if (match.Success) {
                // also yes
            }
            match = FLOAT().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = DOUBLE().Match(s, end);
            if (match.Success) {
                var value = match.Value;

                if (value.Equals("inf", StringComparison.OrdinalIgnoreCase)) {
                    return double.PositiveInfinity;
                }
                else if (value.Equals("nan", StringComparison.OrdinalIgnoreCase)) {
                    return double.NaN;
                }
                else {
                    // i am gonna type an extremely long comment instead of doing anything useful because why not
                }
            }
            // i will probably have to change the order of them
            match = SBYTE().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = BYTE().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = SHORT().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = USHORT().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = INT().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = UINT().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = LONG().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = ULONG().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = LONGLONG().Match(s, end);
            if (match.Success) {
                // yes
            }
            match = ULONGLONG().Match(s, end);
            if (match.Success) {
                // yes
            }
            throw new StopIteration(end);
        }

        int idx = 0;
        return _decode(s, ref idx);
    }
}
