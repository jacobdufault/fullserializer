using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        private static IEnumerable<string> Print(fsData data) {
            if (data.IsBool) {
                yield return "" + data.AsBool.ToString().ToLower() + "";
                yield return "  " + data.AsBool.ToString().ToLower() + "";
                yield return " " + data.AsBool.ToString().ToLower() + "   ";
                yield return " \n" + data.AsBool.ToString().ToLower() + "\n   ";
            }

            else if (data.IsFloat) {
                yield return "" + data.AsFloat + "";
                yield return "  " + data.AsFloat + "";
                yield return " " + data.AsFloat + "   ";
                yield return " \n" + data.AsFloat + "\n   ";
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
        private static int ParseCount = 0;
        private static fsData Parse(string json) {
            fsData data;
            fsFailure fail = fsJsonParser.Parse(json, out data);
            if (fail.Failed) {
                Assert.Fail(fail.FailureReason);
            }
            ++ParseCount;
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
            VerifyData(new fsData(0));
            VerifyData(new fsData(3f));
            VerifyData(new fsData(-3f));
            VerifyData(new fsData(3.5f));
            VerifyData(new fsData(-3.5f));
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
    }
}