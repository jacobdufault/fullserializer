using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace FullSerializer.Tests {
    public struct Model {
        public int aaaAAA;
        public int bbbBBB;

        public override string ToString() {
            return "Model [aaaAAA = " + aaaAAA + ", bbbBBB = " + bbbBBB + "]";
        }
    }

    public class CaseUnsensitiveDeserializationTests {
        [Test]
        public void TestCaseUnsensitiveDeserialization() {
            try {
                fsGlobalConfig.IsCaseSensitive = false;

                DoTest(new Model());
                DoTest(new Model {
                    aaaAAA = 1,
                    bbbBBB = 2
                });
            }
            finally {
                fsGlobalConfig.IsCaseSensitive = true;
            }
        }

        private void DoTest<T>(T expected) {
            fsData serializedData;
            new fsSerializer().TrySerialize(expected, out serializedData).AssertSuccessWithoutWarnings();

            foreach (var entry in serializedData.AsDictionary.ToArray()) {
                serializedData.AsDictionary.Remove(entry.Key);
                serializedData.AsDictionary[entry.Key.ToLower()] = entry.Value;
            }

            var actual = default(T);
            new fsSerializer().TryDeserialize(serializedData, ref actual);

            Debug.Log("Lowercase was " + fsJsonPrinter.PrettyJson(serializedData));
            Assert.AreEqual(expected, actual);
        }
    }
}