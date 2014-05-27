using System;
using System.Text;

namespace FullSerializer {
    public static class fsJsonPrinter {
        /// <summary>
        /// Inserts the given number of indents into the builder.
        /// </summary>
        private static void InsertSpacing(StringBuilder builder, int count) {
            for (int i = 0; i < count; ++i) {
                builder.Append("    ");
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
                    case '\\': result.Append(@"\"); continue;
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

        private static void BuildCompressedString(fsData data, StringBuilder builder) {
            switch (data.Type) {
                case fsDataType.Null:
                    builder.Append("null");
                    break;

                case fsDataType.Boolean:
                    if (data.AsBool) builder.Append("true");
                    else builder.Append("false");
                    break;

                case fsDataType.Number:
                    builder.Append(data.AsFloat);
                    break;

                case fsDataType.String:
                    builder.Append('"');
                    builder.Append(EscapeString(data.AsString));
                    builder.Append('"');
                    break;

                case fsDataType.Object: {
                        builder.Append('{');
                        bool comma = false;
                        foreach (var entry in data.AsDictionary) {
                            if (comma) builder.Append(',');
                            comma = true;
                            builder.Append('"');
                            builder.Append(entry.Key);
                            builder.Append('"');
                            builder.Append(":");
                            BuildCompressedString(entry.Value, builder);
                        }
                        builder.Append('}');
                        break;
                    }

                case fsDataType.Array: {
                        builder.Append('[');
                        bool comma = false;
                        foreach (var entry in data.AsList) {
                            if (comma) builder.Append(',');
                            comma = true;
                            BuildCompressedString(entry, builder);
                        }
                        builder.Append(']');
                        break;
                    }
            }
        }

        /// <summary>
        /// Formats this data into the given builder.
        /// </summary>
        private static void BuildPrettyString(fsData data, StringBuilder builder, int depth) {
            switch (data.Type) {
                case fsDataType.Null:
                    builder.Append("null");
                    break;

                case fsDataType.Boolean:
                    if (data.AsBool) builder.Append("true");
                    else builder.Append("false");
                    break;

                case fsDataType.Number:
                    builder.Append(data.AsFloat);
                    break;

                case fsDataType.String:
                    builder.Append('"');
                    builder.Append(EscapeString(data.AsString));
                    builder.Append('"');
                    break;

                case fsDataType.Object: {
                        builder.Append('{');
                        builder.AppendLine();
                        bool comma = false;
                        foreach (var entry in data.AsDictionary) {
                            if (comma) {
                                builder.Append(',');
                                builder.AppendLine();
                            }
                            comma = true;
                            InsertSpacing(builder, depth + 1);
                            builder.Append('"');
                            builder.Append(entry.Key);
                            builder.Append('"');
                            builder.Append(": ");
                            BuildPrettyString(entry.Value, builder, depth + 1);
                        }
                        builder.AppendLine();
                        InsertSpacing(builder, depth);
                        builder.Append('}');
                        break;
                    }

                case fsDataType.Array:
                    // special case for empty lists; we don't put an empty line between the brackets
                    if (data.AsList.Count == 0) {
                        builder.Append("[]");
                    }

                    else {
                        bool comma = false;

                        builder.Append('[');
                        builder.AppendLine();
                        foreach (var entry in data.AsList) {
                            if (comma) {
                                builder.Append(',');
                                builder.AppendLine();
                            }
                            comma = true;
                            InsertSpacing(builder, depth + 1);
                            BuildPrettyString(entry, builder, depth + 1);
                        }
                        builder.AppendLine();
                        InsertSpacing(builder, depth);
                        builder.Append(']');
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns the data in a pretty printed JSON format.
        /// </summary>
        public static string PrettyJson(fsData data) {
            StringBuilder builder = new StringBuilder();
            BuildPrettyString(data, builder, 0);
            return builder.ToString();
        }

        /// <summary>
        /// Returns the data in a relatively compressed JSON format.
        /// </summary>
        public static string CompressedJson(fsData data) {
            var builder = new StringBuilder();
            BuildCompressedString(data, builder);
            return builder.ToString();
        }
    }
}