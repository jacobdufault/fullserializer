using System;
using System.Collections.Generic;
using System.Text;

namespace FullJson {
    /// <summary>
    /// Exception thrown when a parsing error has occurred.
    /// </summary>
    public sealed class ParseException : Exception {
        /// <summary>
        /// Helper method to create a parsing exception message
        /// </summary>
        private static string CreateMessage(string message, JsonParser context) {
            int start = Math.Max(0, context._start - 10);
            int length = Math.Min(20, context._input.Length - start);

            return "Error while parsing: " + message + "; context = \"" +
                context._input.Substring(start, length) + "\"";
        }

        /// <summary>
        /// Initializes a new instance of the ParseException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="context">The context that the error occurred</param>
        public ParseException(string message, JsonParser context)
            : base(CreateMessage(message, context)) {
        }
    }

    /// <summary>
    /// Parses serialized data into instances of SerializedData.
    /// </summary>
    public sealed class JsonParser {
        internal string _input;
        internal int _start;

        private char CurrentCharacter(int offset = 0) {
            return _input[_start + offset];
        }

        private void MoveNext() {
            ++_start;

            if (_start > _input.Length) {
                throw new ParseException("Unexpected end of input", this);
            }
        }

        private bool HasNext() {
            return _start < _input.Length - 1;
        }

        private bool HasValue(int offset = 0) {
            return (_start + offset) >= 0 && (_start + offset) < _input.Length;
        }

        private void SkipSpace() {
            while (HasValue()) {
                char c = CurrentCharacter();

                if (char.IsWhiteSpace(c)) {
                    MoveNext();
                }
                else if (c == '#') {
                    while (HasValue() && Environment.NewLine.Contains("" + CurrentCharacter()) == false) {
                        MoveNext();
                    }
                }
                else {
                    break;
                }
            }
        }

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
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private char UnescapeChar() {
            /* skip leading backslash '\' */
            switch (CurrentCharacter()) {
                case '\\': MoveNext(); return '\\';
                case '/': MoveNext(); return '/';
                case '\'': MoveNext(); return '\'';
                case '"': MoveNext(); return '\"';
                case 'a': MoveNext(); return '\a';
                case 'b': MoveNext(); return '\b';
                case 'f': MoveNext(); return '\f';
                case 'n': MoveNext(); return '\n';
                case 'r': MoveNext(); return '\r';
                case 't': MoveNext(); return '\t';
                case '0': MoveNext(); return '\0';
                case 'u':
                    MoveNext();
                    if (IsHex(CurrentCharacter(0))
                     && IsHex(CurrentCharacter(1))
                     && IsHex(CurrentCharacter(2))
                     && IsHex(CurrentCharacter(3))) {
                        MoveNext();
                        MoveNext();
                        MoveNext();
                        MoveNext();
                        uint codePoint = ParseUnicode(CurrentCharacter(0), CurrentCharacter(1), CurrentCharacter(2), CurrentCharacter(3));
                        return (char)codePoint;
                    }

                    /* invalid hex escape sequence */
                    throw new ParseException(string.Format("invalid escape sequence '\\u{0}{1}{2}{3}'\n",
                            CurrentCharacter(0),
                            CurrentCharacter(1),
                            CurrentCharacter(2),
                            CurrentCharacter(3)), this);
                default:
                    throw new ParseException(string.Format("Invalid escape sequence \\{0}", CurrentCharacter()), this);
            }
        }

        private JsonData ParseTrue() {
            if (CurrentCharacter() != 't') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'r') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'u') throw new ParseException("expected true", this);
            MoveNext();
            if (CurrentCharacter() != 'e') throw new ParseException("expected true", this);
            MoveNext();

            return new JsonData(true);
        }

        private JsonData ParseFalse() {
            if (CurrentCharacter() != 'f') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'a') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 's') throw new ParseException("expected false", this);
            MoveNext();
            if (CurrentCharacter() != 'e') throw new ParseException("expected false", this);
            MoveNext();

            return new JsonData(false);
        }

        private JsonData ParseNull() {
            if (CurrentCharacter() != 'n') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'u') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected null", this);
            MoveNext();
            if (CurrentCharacter() != 'l') throw new ParseException("expected null", this);
            MoveNext();

            return new JsonData();
        }

        private long ParseSubstring(string baseString, int start, int end) {
            if (start == end) {
                return 0;
            }

            return long.Parse(baseString.Substring(start, end - start));
        }

        /// <summary>
        /// Parses numbers that follow the regular expression [-+](\d+|\d*\.\d*)
        /// </summary>
        /// <returns></returns>
        private JsonData ParseNumber() {
            int start = _start;

            // read until whitespace
            while (HasNext() && CurrentCharacter() != ' ') {
                MoveNext();
            }

            float floatValue;
            if (float.TryParse(_input.Substring(start, _start - start), out floatValue) == false) {
                throw new ParseException("Bad float format", this);
            }

            return new JsonData(floatValue);
        }

        private string ParseKey() {
            var result = new StringBuilder();

            while (CurrentCharacter() != ':' && CurrentCharacter() != '`' &&
                char.IsWhiteSpace(CurrentCharacter()) == false) {
                char c = CurrentCharacter();

                if (c == '\\') {
                    char unescaped = UnescapeChar();
                    result.Append(unescaped);
                }
                else {
                    result.Append(c);
                }

                MoveNext();
            }

            SkipSpace();

            return result.ToString();
        }

        private JsonData ParseString() {
            if (CurrentCharacter() != '"') {
                throw new ParseException("Attempt to parse string without leading \"", this);
            }

            // skip '"'
            MoveNext();

            StringBuilder result = new StringBuilder();

            while (CurrentCharacter() != '"') {
                char c = CurrentCharacter();

                if (c == '\\') {
                    char unescaped = UnescapeChar();
                    result.Append(unescaped);
                }
                else {
                    result.Append(c);
                }

                MoveNext();
            }

            // skip '"'
            MoveNext();

            return new JsonData(result.ToString());
        }

        private JsonData ParseArray() {
            // skip '['
            MoveNext();
            SkipSpace();

            var result = new List<JsonData>();

            while (CurrentCharacter() != ']') {
                JsonData element = RunParse();
                result.Add(element);

                SkipSpace();
            }

            // skip ']'
            MoveNext();

            return new JsonData(result);
        }

        private JsonData ParseObject() {
            // skip '{'
            SkipSpace();
            MoveNext();
            SkipSpace();

            var result = new Dictionary<string, JsonData>();

            while (CurrentCharacter() != '}') {
                SkipSpace();
                string key = ParseKey();
                SkipSpace();

                if (CurrentCharacter() != ':') {
                    throw new ParseException("Expected : after object key " + key, this);
                }

                // skip ':'
                MoveNext();
                SkipSpace();

                JsonData value = RunParse();
                result.Add(key, value);

                SkipSpace();
            }

            /* skip '}' */
            MoveNext();
            return new JsonData(result);
        }

        /// <summary>
        /// Parses the specified input. Throws a ParseException if parsing failed.
        /// </summary>
        /// <param name="input">The input to parse.</param>
        /// <returns>The parsed input.</returns>
        public static JsonData Parse(string input) {
            var context = new JsonParser(input);
            return context.RunParse();
        }

        private JsonParser(string input) {
            _input = input;
            _start = 0;
        }

        private JsonData RunParse() {
            SkipSpace();

            switch (CurrentCharacter()) {
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
                case '9': return ParseNumber();
                case '"': return ParseString();
                case '[': return ParseArray();
                case '{': return ParseObject();
                case 't': return ParseTrue();
                case 'f': return ParseFalse();
                case 'n': return ParseNull();
                default: throw new ParseException("unable to parse", this);
            }
        }

    }

}