using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FullSerializer {
    // TODO: properly propagate warnings/etc for fsResult states

    /// <summary>
    /// A simple recursive descent parser for JSON.
    /// </summary>
    public class fsJsonParser {
        private int _start;
        private string _input;

        private fsResult MakeFailure(string message) {
            int start = Math.Max(0, _start - 20);
            int length = Math.Min(50, _input.Length - start);

            string error = "Error while parsing: " + message + "; context = <" +
                _input.Substring(start, length) + ">";
            return fsResult.Fail(error);
        }

        private bool TryMoveNext() {
            if (_start < _input.Length) {
                ++_start;
                return true;
            }

            return false;
        }

        private bool HasValue() {
            return HasValue(0);
        }

        private bool HasValue(int offset) {
            return (_start + offset) >= 0 && (_start + offset) < _input.Length;
        }

        private char Character() {
            return Character(0);
        }

        private char Character(int offset) {
            return _input[_start + offset];
        }

        /// <summary>
        /// Skips input such that Character() will return a non-whitespace character
        /// </summary>
        private void SkipSpace() {
            while (HasValue()) {
                char c = Character();

                // whitespace; fine to skip
                if (char.IsWhiteSpace(c)) {
                    TryMoveNext();
                    continue;
                }

                // comment?
                if (HasValue(1) && Character(0) == '/') {
                    if (Character(1) == '/') {
                        // skip the rest of the line
                        while (HasValue() && Environment.NewLine.Contains("" + Character()) == false) {
                            TryMoveNext();
                        }
                        continue;
                    } else if (Character(1) == '*') {
                        // skip to comment close
                        TryMoveNext();
                        TryMoveNext();
                        while (HasValue(1)) {
                            if (Character(0) == '*' && Character(1) == '/') {
                                TryMoveNext();
                                TryMoveNext();
                                TryMoveNext();
                                break;
                            } else {
                                TryMoveNext();
                            }
                        }
                    }
                    // let other checks to check fail
                    continue;
                }

                break;
            }
        }

        #region Escaping
        private bool IsHex(char c) {
            return ((c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F'));
        }

        private uint ParseSingleChar(char c1, uint multipliyer) {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4) {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 0x1);

            return p1 + p2 + p3 + p4;
        }

        private fsResult TryUnescapeChar(out char escaped) {
            // skip leading backslash '\'
            TryMoveNext();
            if (HasValue() == false) {
                escaped = ' ';
                return MakeFailure("Unexpected end of input after \\");
            }

            switch (Character()) {
                case '\\': TryMoveNext(); escaped = '\\'; return fsResult.Success;
                case '/': TryMoveNext(); escaped = '/'; return fsResult.Success;
                case '"': TryMoveNext(); escaped = '\"'; return fsResult.Success;
                case 'a': TryMoveNext(); escaped = '\a'; return fsResult.Success;
                case 'b': TryMoveNext(); escaped = '\b'; return fsResult.Success;
                case 'f': TryMoveNext(); escaped = '\f'; return fsResult.Success;
                case 'n': TryMoveNext(); escaped = '\n'; return fsResult.Success;
                case 'r': TryMoveNext(); escaped = '\r'; return fsResult.Success;
                case 't': TryMoveNext(); escaped = '\t'; return fsResult.Success;
                case '0': TryMoveNext(); escaped = '\0'; return fsResult.Success;
                case 'u':
                    TryMoveNext();
                    if (IsHex(Character(0))
                     && IsHex(Character(1))
                     && IsHex(Character(2))
                     && IsHex(Character(3))) {

                        uint codePoint = ParseUnicode(Character(0), Character(1), Character(2), Character(3));

                        TryMoveNext();
                        TryMoveNext();
                        TryMoveNext();
                        TryMoveNext();

                        escaped = (char)codePoint;
                        return fsResult.Success;
                    }

                    // invalid escape sequence
                    escaped = (char)0;
                    return MakeFailure(
                        string.Format("invalid escape sequence '\\u{0}{1}{2}{3}'\n",
                            Character(0),
                            Character(1),
                            Character(2),
                            Character(3)));
                default:
                    escaped = (char)0;
                    return MakeFailure(string.Format("Invalid escape sequence \\{0}", Character()));
            }
        }
        #endregion

        private fsResult TryParseExact(string content) {
            for (int i = 0; i < content.Length; ++i) {
                if (Character() != content[i]) {
                    return MakeFailure("Expected " + content[i]);
                }

                if (TryMoveNext() == false) {
                    return MakeFailure("Unexpected end of content when parsing " + content);
                }
            }

            return fsResult.Success;
        }

        private fsResult TryParseTrue(out fsData data) {
            var fail = TryParseExact("true");

            if (fail.Succeeded) {
                data = new fsData(true);
                return fsResult.Success;
            }

            data = null;
            return fail;
        }

        private fsResult TryParseFalse(out fsData data) {
            var fail = TryParseExact("false");

            if (fail.Succeeded) {
                data = new fsData(false);
                return fsResult.Success;
            }

            data = null;
            return fail;
        }

        private fsResult TryParseNull(out fsData data) {
            var fail = TryParseExact("null");

            if (fail.Succeeded) {
                data = new fsData();
                return fsResult.Success;
            }

            data = null;
            return fail;
        }


        private bool IsSeparator(char c) {
            return char.IsWhiteSpace(c) || c == ',' || c == '}' || c == ']';
        }

        /// <summary>
        /// Parses numbers that follow the regular expression [-+](\d+|\d*\.\d*)
        /// </summary>
        private fsResult TryParseNumber(out fsData data) {
            int start = _start;

            // read until we get to a separator
            while (
                TryMoveNext() &&
                (HasValue() && IsSeparator(Character()) == false)) {
            }

            // try to parse the value
            string numberString = _input.Substring(start, _start - start);

            // double -- includes a .
            if (numberString.Contains(".") || numberString == "Infinity" || numberString == "-Infinity" || numberString == "NaN") {
                double doubleValue;
                if (double.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue) == false) {
                    data = null;
                    return MakeFailure("Bad double format with " + numberString);
                }

                data = new fsData(doubleValue);
                return fsResult.Success;
            }
            else {
                Int64 intValue;
                if (Int64.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out intValue) == false) {
                    data = null;
                    return MakeFailure("Bad Int64 format with " + numberString);
                }

                data = new fsData(intValue);
                return fsResult.Success;
            }
        }

        private readonly StringBuilder _cachedStringBuilder = new StringBuilder(256);
        /// <summary>
        /// Parses a string
        /// </summary>
        private fsResult TryParseString(out string str) {
            _cachedStringBuilder.Length = 0;

            // skip the first "
            if (Character() != '"' || TryMoveNext() == false) {
                str = string.Empty;
                return MakeFailure("Expected initial \" when parsing a string");
            }

            // read until the next "
            while (HasValue() && Character() != '\"') {
                char c = Character();

                // escape if necessary
                if (c == '\\') {
                    char unescaped;
                    var fail = TryUnescapeChar(out unescaped);
                    if (fail.Failed) {
                        str = string.Empty;
                        return fail;
                    }

                    _cachedStringBuilder.Append(unescaped);
                }

                // no escaping necessary
                else {
                    _cachedStringBuilder.Append(c);

                    // get the next character
                    if (TryMoveNext() == false) {
                        str = string.Empty;
                        return MakeFailure("Unexpected end of input when reading a string");
                    }
                }
            }

            // skip the first "
            if (HasValue() == false || Character() != '"' || TryMoveNext() == false) {
                str = string.Empty;
                return MakeFailure("No closing \" when parsing a string");
            }

            str = _cachedStringBuilder.ToString();
            return fsResult.Success;
        }

        /// <summary>
        /// Parses an array
        /// </summary>
        private fsResult TryParseArray(out fsData arr) {
            if (Character() != '[') {
                arr = null;
                return MakeFailure("Expected initial [ when parsing an array");
            }

            // skip '['
            if (TryMoveNext() == false) {
                arr = null;
                return MakeFailure("Unexpected end of input when parsing an array");
            }
            SkipSpace();

            var result = new List<fsData>();

            while (HasValue() && Character() != ']') {
                // parse the element
                fsData element;
                var fail = RunParse(out element);
                if (fail.Failed) {
                    arr = null;
                    return fail;
                }

                result.Add(element);

                // parse the comma
                SkipSpace();
                if (HasValue() && Character() == ',') {
                    if (TryMoveNext() == false) break;
                    SkipSpace();
                }
            }

            // skip the final ]
            if (HasValue() == false || Character() != ']' || TryMoveNext() == false) {
                arr = null;
                return MakeFailure("No closing ] for array");
            }

            arr = new fsData(result);
            return fsResult.Success;
        }

        private fsResult TryParseObject(out fsData obj) {
            if (Character() != '{') {
                obj = null;
                return MakeFailure("Expected initial { when parsing an object");
            }

            // skip '{'
            if (TryMoveNext() == false) {
                obj = null;
                return MakeFailure("Unexpected end of input when parsing an object");
            }
            SkipSpace();

            var result = new Dictionary<string, fsData>(
                fsConfig.IsCaseSensitive ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase);

            while (HasValue() && Character() != '}') {
                fsResult failure;

                // parse the key
                SkipSpace();
                string key;
                failure = TryParseString(out key);
                if (failure.Failed) {
                    obj = null;
                    return failure;
                }
                SkipSpace();

                // parse the ':' after the key
                if (HasValue() == false || Character() != ':' || TryMoveNext() == false) {
                    obj = null;
                    return MakeFailure("Expected : after key \"" + key + "\"");
                }
                SkipSpace();

                // parse the value
                fsData value;
                failure = RunParse(out value);
                if (failure.Failed) {
                    obj = null;
                    return failure;
                }

                result.Add(key, value);

                // parse the comma
                SkipSpace();
                if (HasValue() && Character() == ',') {
                    if (TryMoveNext() == false) break;
                    SkipSpace();
                }
            }

            // skip the final }
            if (HasValue() == false || Character() != '}' || TryMoveNext() == false) {
                obj = null;
                return MakeFailure("No closing } for object");
            }

            obj = new fsData(result);
            return fsResult.Success;
        }

        private fsResult RunParse(out fsData data) {
            SkipSpace();

            if (HasValue() == false) {
                data = default(fsData);
                return MakeFailure("Unexpected end of input");
            }

            switch (Character()) {
                case 'I': // Infinity
                case 'N': // NaN
                case '.':
                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return TryParseNumber(out data);
                case '"': {
                        string str;
                        fsResult fail = TryParseString(out str);
                        if (fail.Failed) {
                            data = null;
                            return fail;
                        }
                        data = new fsData(str);
                        return fsResult.Success;
                    }
                case '[': return TryParseArray(out data);
                case '{': return TryParseObject(out data);
                case 't': return TryParseTrue(out data);
                case 'f': return TryParseFalse(out data);
                case 'n': return TryParseNull(out data);
                default:
                    data = null;
                    return MakeFailure("unable to parse; invalid token \"" + Character() + "\"");
            }
        }

        /// <summary>
        /// Parses the specified input. Returns a failure state if parsing failed.
        /// </summary>
        /// <param name="input">The input to parse.</param>
        /// <param name="data">The parsed data. This is undefined if parsing fails.</param>
        /// <returns>The parsed input.</returns>
        public static fsResult Parse(string input, out fsData data) {
            if (string.IsNullOrEmpty(input)) {
                data = default(fsData);
                return fsResult.Fail("No input");
            }

            var context = new fsJsonParser(input);
            return context.RunParse(out data);
        }

        /// <summary>
        /// Helper method for Parse that does not allow the error information
        /// to be recovered.
        /// </summary>
        public static fsData Parse(string input) {
            fsData data;
            Parse(input, out data).AssertSuccess();
            return data;
        }

        private fsJsonParser(string input) {
            _input = input;
            _start = 0;
        }
    }
}