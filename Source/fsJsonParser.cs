using System;
using System.Collections.Generic;
using System.Text;

namespace FullSerializer {
    /// <summary>
    /// A simple recursive descent parser for JSON.
    /// </summary>
    public class fsJsonParser {
        private int _start;
        private string _input;

        private fsFailure MakeFailure(string message) {
            int start = Math.Max(0, _start - 10);
            int length = Math.Min(20, _input.Length - start);

            string error = "Error while parsing: " + message + "; context = <" +
                _input.Substring(start, length) + ">";
            return fsFailure.Fail(error);
        }

        private bool TryMoveNext() {
            if (_start < _input.Length) {
                ++_start;
                return true;
            }

            return false;
        }

        private bool HasValue(int offset = 0) {
            return (_start + offset) >= 0 && (_start + offset) < _input.Length;
        }

        private char Character(int offset = 0) {
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

                // comment? they begin with //
                if (HasValue(1) &&
                    (Character(0) == '/' && Character(1) == '/')) {

                    // skip the rest of the line
                    while (HasValue() && Environment.NewLine.Contains("" + Character()) == false) {
                        TryMoveNext();
                    }

                    // we still need to skip whitespace on the next line
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

        private fsFailure TryUnescapeChar(out char escaped) {
            // skip leading backslash '\'
            TryMoveNext();
            if (HasValue() == false) {
                escaped = ' ';
                return MakeFailure("Unexpected end of input after \\");
            }

            switch (Character()) {
                case '\\': TryMoveNext(); escaped = '\\'; return fsFailure.Success;
                case '"': TryMoveNext(); escaped = '\"'; return fsFailure.Success;
                case 'a': TryMoveNext(); escaped = '\a'; return fsFailure.Success;
                case 'b': TryMoveNext(); escaped = '\b'; return fsFailure.Success;
                case 'f': TryMoveNext(); escaped = '\f'; return fsFailure.Success;
                case 'n': TryMoveNext(); escaped = '\n'; return fsFailure.Success;
                case 'r': TryMoveNext(); escaped = '\r'; return fsFailure.Success;
                case 't': TryMoveNext(); escaped = '\t'; return fsFailure.Success;
                case '0': TryMoveNext(); escaped = '\0'; return fsFailure.Success;
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
                        return fsFailure.Success;
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

        private fsFailure TryParseExact(string content) {
            for (int i = 0; i < content.Length; ++i) {
                if (Character() != content[i]) {
                    return MakeFailure("Expected " + content[i]);
                }

                if (TryMoveNext() == false) {
                    return MakeFailure("Unexpected end of content when parsing " + content);
                }
            }

            return fsFailure.Success;
        }

        private fsFailure TryParseTrue(out fsData data) {
            var fail = TryParseExact("true");

            if (fail.Succeeded) {
                data = new fsData(true);
                return fsFailure.Success;
            }

            data = null;
            return fail;
        }

        private fsFailure TryParseFalse(out fsData data) {
            var fail = TryParseExact("false");

            if (fail.Succeeded) {
                data = new fsData(false);
                return fsFailure.Success;
            }

            data = null;
            return fail;
        }

        private fsFailure TryParseNull(out fsData data) {
            var fail = TryParseExact("null");

            if (fail.Succeeded) {
                data = new fsData();
                return fsFailure.Success;
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
        private fsFailure TryParseNumber(out fsData data) {
            int start = _start;

            // read until we get to a separator
            while (
                TryMoveNext() &&
                (HasValue() && IsSeparator(Character()) == false)) {
            }

            // try to parse the value
            float floatValue;
            if (float.TryParse(_input.Substring(start, _start - start), out floatValue) == false) {
                data = null;
                return MakeFailure("Bad float format with " + _input.Substring(start, _start - start));
            }

            data = new fsData(floatValue);
            return fsFailure.Success;
        }

        /// <summary>
        /// Parses a string
        /// </summary>
        private fsFailure TryParseString(out string str) {
            var result = new StringBuilder();

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

                    result.Append(unescaped);
                }

                // no escaping necessary
                else {
                    result.Append(c);

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

            str = result.ToString();
            return fsFailure.Success;
        }

        /// <summary>
        /// Parses an array
        /// </summary>
        private fsFailure TryParseArray(out fsData arr) {
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
            return fsFailure.Success;
        }

        private fsFailure TryParseObject(out fsData obj) {
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

            var result = new Dictionary<string, fsData>();

            while (HasValue() && Character() != '}') {
                fsFailure failure;

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
            return fsFailure.Success;
        }

        private fsFailure RunParse(out fsData data) {
            SkipSpace();

            switch (Character()) {
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
                        fsFailure fail = TryParseString(out str);
                        if (fail.Failed) {
                            data = null;
                            return fail;
                        }
                        data = new fsData(str);
                        return fsFailure.Success;
                    }
                case '[': return TryParseArray(out data);
                case '{': return TryParseObject(out data);
                case 't': return TryParseTrue(out data);
                case 'f': return TryParseFalse(out data);
                case 'n': return TryParseNull(out data);
                default:
                    data = null;
                    return MakeFailure("unable to parse; invalid initial token \"" + Character() + "\"");
            }
        }

        /// <summary>
        /// Parses the specified input. Throws a ParseException if parsing failed.
        /// </summary>
        /// <param name="input">The input to parse.</param>
        /// <returns>The parsed input.</returns>
        public static fsFailure Parse(string input, out fsData data) {
            var context = new fsJsonParser(input);
            return context.RunParse(out data);
        }

        private fsJsonParser(string input) {
            _input = input;
            _start = 0;
        }
    }
}