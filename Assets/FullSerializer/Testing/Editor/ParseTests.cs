using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

// It may not look like it, but at last check there were 17915 test cases. Combinatorics is used
// to test a huge number of different variants, mainly dealing with whitespace.

namespace FullSerializer.Tests {
    public class ParseTests {
        private static IEnumerable<string> Permutations(List<fsData> items, int depth) {
            if (items.Count == 0) {
                yield return "";
                yield break;
            }

            if (depth == items.Count - 1) {
                foreach (string item in Print(items[depth])) {
                    yield return item;
                }
                yield break;
            }

            foreach (string version in Print(items[depth])) {
                foreach (string subversion in Permutations(items, depth + 1)) {
                    yield return version + ", " + subversion;
                }
            }
        }

        private static IEnumerable<string> Permutations(List<KeyValuePair<string, fsData>> items, int depth) {
            if (items.Count == 0) {
                yield return "";
                yield break;
            }

            if (depth == items.Count - 1) {
                foreach (string item in Print(items[depth].Value)) {
                    yield return "\"" + items[depth].Key + "\":" + item;
                }
                yield break;
            }

            foreach (string version in Print(items[depth].Value)) {
                foreach (string subversion in Permutations(items, depth + 1)) {
                    yield return "\"" + items[depth].Key + "\":" + version + ", " + subversion;
                }
            }
        }

        /// <summary>
        /// Utility method that converts a double to a string.
        /// </summary>
        private static String ConvertDoubleToString(double d) {
            if (Double.IsInfinity(d) || Double.IsNaN(d)) return d.ToString();
            String doubledString = d.ToString();
            if (doubledString.Contains(".") == false) doubledString += ".0";
            return doubledString;
        }


        private static IEnumerable<string> Print(fsData data) {
            if (data.IsBool) {
                yield return "" + data.AsBool.ToString().ToLower() + "";
                yield return "  " + data.AsBool.ToString().ToLower() + "";
                yield return " " + data.AsBool.ToString().ToLower() + "   ";
                yield return " \n" + data.AsBool.ToString().ToLower() + "\n   ";
            }

            else if (data.IsDouble) {
                yield return "" + ConvertDoubleToString(data.AsDouble) + "";
                yield return "  " + ConvertDoubleToString(data.AsDouble) + "";
                yield return " " + ConvertDoubleToString(data.AsDouble) + "   ";
                yield return " \n" + ConvertDoubleToString(data.AsDouble) + "\n   ";
            }

            else if (data.IsInt64) {
                yield return "" + data.AsInt64 + "";
                yield return "  " + data.AsInt64 + "";
                yield return " " + data.AsInt64 + "   ";
                yield return " \n" + data.AsInt64 + "\n   ";
            }

            else if (data.IsNull) {
                yield return "null";
                yield return "  null";
                yield return " null  ";
                yield return " \nnull\n   ";
            }

            else if (data.IsString) {
                yield return "\"" + data.AsString + "\"";
                yield return " \"" + data.AsString + "\"";
                yield return "\"" + data.AsString + "\" ";
                yield return "  \"" + data.AsString + "\"  ";
                yield return "\n\"" + data.AsString + "\" \n ";
            }

            else if (data.IsList) {
                foreach (string permutation in Permutations(data.AsList, 0)) {
                    yield return "[" + permutation + "]";
                    yield return " [" + permutation + "]";
                    yield return "[ " + permutation + "]";
                    yield return "[" + permutation + " ]";
                    yield return "[" + permutation + "] ";
                    yield return " \n[\n" + permutation + "\n] \n";
                }
            }

            else if (data.IsDictionary) {
                foreach (string permutation in Permutations(data.AsDictionary.ToList(), 0)) {
                    yield return "{" + permutation + "}";
                    yield return " {" + permutation + "}";
                    yield return "{ " + permutation + "}";
                    yield return "{" + permutation + " }";
                    yield return "{" + permutation + "} ";
                    yield return " \n{\n" + permutation + "\n} \n";
                }
            }
        }

        private static fsData Parse(string json) {
            fsData data;
            fsResult fail = fsJsonParser.Parse(json, out data);
            if (fail.Failed) {
                Assert.Fail(fail.FormattedMessages);
            }
            return data;
        }

        private static void VerifyData(fsData data) {
            foreach (string permutation in Print(data)) {
                var parsedData = Parse(permutation);
                Assert.AreEqual(data, parsedData);
            }
        }

        [Test]
        public void ParseNumbers() {
            VerifyData(new fsData(0f));
            VerifyData(new fsData(3f));
            VerifyData(new fsData(-3f));
            VerifyData(new fsData(3.5f));
            VerifyData(new fsData(-3.5f));
            //VerifyData(new fsData(Single.MinValue)); // mono has a bug where it cannot handle Double.Parse(Double.MinValue.ToString())
            //VerifyData(new fsData(Single.MaxValue)); // mono has a bug where it cannot handle Double.Parse(Double.MinValue.ToString())
            VerifyData(new fsData(Double.NegativeInfinity));
            VerifyData(new fsData(Double.PositiveInfinity));
            //VerifyData(new fsData(Double.NaN));
            VerifyData(new fsData(Double.Epsilon));

            VerifyData(new fsData(Int64.MaxValue));
            VerifyData(new fsData(Int64.MinValue));
            VerifyData(new fsData(3));
            VerifyData(new fsData(-3));

        }


        [Test]
        public void ParseObjects() {
            VerifyData(fsData.CreateDictionary());

            VerifyData(new fsData(new Dictionary<string, fsData> {
                { "ok", new fsData(1) },
                { "null", new fsData() },
                { " yes ", fsData.CreateList() },
                { 
                    "something",
                    new fsData(new Dictionary<string, fsData> {
                        { "nested", new fsData("yes") }
                    })
                }
            }));
        }

        [Test]
        public void ParseLists() {
            VerifyData(fsData.CreateList());

            VerifyData(new fsData(new List<fsData>() {
                new fsData(1),
                new fsData(5),
                fsData.CreateDictionary()
            }));
        }

        [Test]
        public void ParseBooleans() {
            VerifyData(new fsData(true));
            VerifyData(new fsData(false));
        }

        [Test]
        public void ParseNull() {
            VerifyData(new fsData());
        }

        [Test]
        public void ParseStrings() {
            VerifyData(new fsData(string.Empty));
            VerifyData(new fsData("ok"));
            VerifyData(new fsData("yes one two three"));
        }

        [Test]
        public void TestEscaping() {
            fsData data = new fsData("ok");
            fsData escapeRequired = new fsData(fsJsonPrinter.PrettyJson(data));
            Assert.AreEqual(escapeRequired, Parse(fsJsonPrinter.PrettyJson(escapeRequired)));
        }

        [Test]
        public void TestComment() {
            string jsonString = @"
                /* 
                * comment
                */
                // comment
                { 
                    // comment
                    /* comment */
                    ""ls"": [
                        1,
                        // comment
                        2,
                        /* comment */
                        3
                    ],
                    // comment
                    ""obj"" : {
                    // comment
                    ""a"" : ""b"",
                    // comment
                    ""c"" : /*  comment  */
                    ""d""
                    // comment
                    }  /* comment */
                    // comment
                }
                /* comment */ // comment
                ";
            fsData data = new fsData(new Dictionary<string, fsData>() {
                    {"ls", new fsData(new List<fsData> {new fsData(1), new fsData(2), new fsData(3)})},
                    {"obj", new fsData(new Dictionary<string, fsData>() {
                        {"a", new fsData("b")},
                        {"c", new fsData("d")}
                    })}
                });
            fsData parsedData = Parse(jsonString);
            Assert.AreEqual(data, parsedData);
        }
    }
}