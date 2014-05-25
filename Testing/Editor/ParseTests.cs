using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// It may not look like it, but at last check there were 17915 test cases. Combinatorics is used
// to test a huge number of different variants, mainly dealing with whitespace.

namespace FullJson {
    public class ParseTests {
        private static IEnumerable<string> Permutations(List<JsonData> items, int depth) {
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

        private static IEnumerable<string> Permutations(List<KeyValuePair<string, JsonData>> items, int depth) {
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

        private static IEnumerable<string> Print(JsonData data) {
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
        private static JsonData Parse(string json) {
            JsonData data;
            JsonFailure fail = JsonParser.Parse(json, out data);
            if (fail.Failed) {
                Assert.Fail("When parsing " + json + ", failed because of " + fail.FailureReason);
            }
            ++ParseCount;
            return data;
        }

        private static void VerifyData(JsonData data) {
            foreach (string permutation in Print(data)) {
                var parsedData = Parse(permutation);
                Assert.AreEqual(data, parsedData);
            }
        }

        [Test]
        public void ParseNumbers() {
            VerifyData(new JsonData(0));
            VerifyData(new JsonData(3f));
            VerifyData(new JsonData(-3f));
            VerifyData(new JsonData(3.5f));
            VerifyData(new JsonData(-3.5f));
        }


        [Test]
        public void ParseObjects() {
            VerifyData(JsonData.CreateDictionary());

            VerifyData(new JsonData(new Dictionary<string, JsonData> {
                { "ok", new JsonData(1) },
                { "null", new JsonData() },
                { " yes ", JsonData.CreateList() },
                { 
                    "something",
                    new JsonData(new Dictionary<string, JsonData> {
                        { "nested", new JsonData("yes") }
                    })
                }
            }));
        }

        [Test]
        public void ParseLists() {
            VerifyData(JsonData.CreateList());

            VerifyData(new JsonData(new List<JsonData>() {
                new JsonData(1),
                new JsonData(5),
                JsonData.CreateDictionary()
            }));
        }

        [Test]
        public void ParseBooleans() {
            VerifyData(new JsonData(true));
            VerifyData(new JsonData(false));
        }

        [Test]
        public void ParseNull() {
            VerifyData(new JsonData());
        }

        [Test]
        public void ParseStrings() {
            VerifyData(new JsonData(string.Empty));
            VerifyData(new JsonData("ok"));
            VerifyData(new JsonData("yes one two three"));
        }
    }
}