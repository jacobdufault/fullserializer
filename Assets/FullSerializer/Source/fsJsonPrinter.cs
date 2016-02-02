using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace FullSerializer {
    public static class fsJsonPrinter {
        /// <summary>
        /// Inserts the given number of indents into the builder.
        /// </summary>
        private static void InsertSpacing(TextWriter stream, int count) {
            for (int i = 0; i < count; ++i) {
                stream.Write("    ");
            }
        }

        /// <summary>
        /// Escapes a string.
        /// </summary>
        private static string EscapeString(string str) {
            // Escaping a string is pretty allocation heavy, so we try hard to not do it.

            bool needsEscape = false;
            for (int i = 0; i < str.Length; ++i) {
                char c = str[i];

                // unicode code point
                int intChar = Convert.ToInt32(c);
                if (intChar < 0 || intChar > 127) {
                    needsEscape = true;
                    break;
                }

                // standard escape character
                switch (c) {
                    case '"':
                    case '\\':
                    case '\a':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                    case '\0':
                        needsEscape = true;
                        break;
                }

                if (needsEscape) {
                    break;
                }
            }

            if (needsEscape == false) {
                return str;
            }


            StringBuilder result = new StringBuilder();

            for (int i = 0; i < str.Length; ++i) {
                char c = str[i];

                // unicode code point
                int intChar = Convert.ToInt32(c);
                if (intChar < 0 || intChar > 127) {
                    result.Append(string.Format("\\u{0:x4} ", intChar).Trim());
                    continue;
                }

                // standard escape character
                switch (c) {
                    case '"': result.Append("\\\""); continue;
                    case '\\': result.Append(@"\\"); continue;
                    case '\a': result.Append(@"\a"); continue;
                    case '\b': result.Append(@"\b"); continue;
                    case '\f': result.Append(@"\f"); continue;
                    case '\n': result.Append(@"\n"); continue;
                    case '\r': result.Append(@"\r"); continue;
                    case '\t': result.Append(@"\t"); continue;
                    case '\0': result.Append(@"\0"); continue;
                }

                // no escaping needed
                result.Append(c);
            }
            return result.ToString();
        }

        private static void BuildCompressedString(fsData data, TextWriter stream) {
            switch (data.Type) {
                case fsDataType.Null:
                    stream.Write("null");
                    break;

                case fsDataType.Boolean:
                    if (data.AsBool) stream.Write("true");
                    else stream.Write("false");
                    break;

                case fsDataType.Double:
                    // doubles must *always* include a decimal
                    stream.Write(ConvertDoubleToString(data.AsDouble));
                    break;

                case fsDataType.Int64:
                    stream.Write(data.AsInt64);
                    break;

                case fsDataType.String:
                    stream.Write('"');
                    stream.Write(EscapeString(data.AsString));
                    stream.Write('"');
                    break;

                case fsDataType.Object: {
                        stream.Write('{');
                        bool comma = false;
                        foreach (var entry in data.AsDictionary) {
                            if (comma) stream.Write(',');
                            comma = true;
                            stream.Write('"');
                            stream.Write(entry.Key);
                            stream.Write('"');
                            stream.Write(":");
                            BuildCompressedString(entry.Value, stream);
                        }
                        stream.Write('}');
                        break;
                    }

                case fsDataType.Array: {
                        stream.Write('[');
                        bool comma = false;
                        foreach (var entry in data.AsList) {
                            if (comma) stream.Write(',');
                            comma = true;
                            BuildCompressedString(entry, stream);
                        }
                        stream.Write(']');
                        break;
                    }
            }
        }

        /// <summary>
        /// Formats this data into the given builder.
        /// </summary>
        private static void BuildPrettyString(fsData data, TextWriter stream, int depth) {
            switch (data.Type) {
                case fsDataType.Null:
                    stream.Write("null");
                    break;

                case fsDataType.Boolean:
                    if (data.AsBool) stream.Write("true");
                    else stream.Write("false");
                    break;

                case fsDataType.Double:
                    stream.Write(ConvertDoubleToString(data.AsDouble));
                    break;

                case fsDataType.Int64:
                    stream.Write(data.AsInt64);
                    break;


                case fsDataType.String:
                    stream.Write('"');
                    stream.Write(EscapeString(data.AsString));
                    stream.Write('"');
                    break;

                case fsDataType.Object: {
                        stream.Write('{');
                        stream.WriteLine();
                        bool comma = false;
                        foreach (var entry in data.AsDictionary) {
                            if (comma) {
                                stream.Write(',');
                                stream.WriteLine();
                            }
                            comma = true;
                            InsertSpacing(stream, depth + 1);
                            stream.Write('"');
                            stream.Write(entry.Key);
                            stream.Write('"');
                            stream.Write(": ");
                            BuildPrettyString(entry.Value, stream, depth + 1);
                        }
                        stream.WriteLine();
                        InsertSpacing(stream, depth);
                        stream.Write('}');
                        break;
                    }

                case fsDataType.Array:
                    // special case for empty lists; we don't put an empty line between the brackets
                    if (data.AsList.Count == 0) {
                        stream.Write("[]");
                    }

                    else {
                        bool comma = false;

                        stream.Write('[');
                        stream.WriteLine();
                        foreach (var entry in data.AsList) {
                            if (comma) {
                                stream.Write(',');
                                stream.WriteLine();
                            }
                            comma = true;
                            InsertSpacing(stream, depth + 1);
                            BuildPrettyString(entry, stream, depth + 1);
                        }
                        stream.WriteLine();
                        InsertSpacing(stream, depth);
                        stream.Write(']');
                    }
                    break;
            }
        }

        /// <summary>
        /// Writes the pretty JSON output data to the given stream.
        /// </summary>
        /// <param name="data">The data to print.</param>
        /// <param name="outputStream">Where to write the printed data.</param>
        public static void PrettyJson(fsData data, TextWriter outputStream) {
            BuildPrettyString(data, outputStream, 0);
        }

        /// <summary>
        /// Returns the data in a pretty printed JSON format.
        /// </summary>
        public static string PrettyJson(fsData data) {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                BuildPrettyString(data, writer, 0);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Writes the compressed JSON output data to the given stream.
        /// </summary>
        /// <param name="data">The data to print.</param>
        /// <param name="outputStream">Where to write the printed data.</param>
        public static void CompressedJson(fsData data, StreamWriter outputStream) {
            BuildCompressedString(data, outputStream);
        }

        /// <summary>
        /// Returns the data in a relatively compressed JSON format.
        /// </summary>
        public static string CompressedJson(fsData data) {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                BuildCompressedString(data, writer);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Utility method that converts a double to a string.
        /// </summary>
        private static string ConvertDoubleToString(double d) {
            if (Double.IsInfinity(d) || Double.IsNaN(d))
                return d.ToString(CultureInfo.InvariantCulture);

            string doubledString = d.ToString(CultureInfo.InvariantCulture);

            // NOTE/HACK: If we don't serialize with a period, then the number will be deserialized as an Int64, not a double.
            if (doubledString.Contains(".") == false)
                doubledString += ".0";

            return doubledString;
        }

    }
}