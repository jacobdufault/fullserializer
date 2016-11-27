using System.Collections.Generic;
using NUnit.Framework;

namespace FullSerializer.Tests {
    public class AbstractTypeRemappingTests {
        public string Serialize<T>(T value) {
            fsData data;
            (new fsSerializer()).TrySerialize(value, out data).AssertSuccessWithoutWarnings();
            return fsJsonPrinter.PrettyJson(data);
        }

        public T Deserialize<T>(string content) {
            fsData data;
            fsJsonParser.Parse(content, out data).AssertSuccessWithoutWarnings();
            var result = default(T);
            (new fsSerializer()).TryDeserialize(data, ref result).AssertSuccessWithoutWarnings();
            return result;
        }

        [Test]
        public void IListDeserializedAsList() {
            var value = new List<int> { 1, 2, 3 };
            string serialized = Serialize<List<int>>(value);
            IList<int> deserialized = Deserialize<IList<int>>(serialized);
            Assert.IsInstanceOf<List<int>>(deserialized);
            CollectionAssert.AreEquivalent(value, deserialized);
        }

        [Test]
        public void ICollectionDeserializedAsList() {
            var value = new List<int> { 1, 2, 3 };
            string serialized = Serialize<List<int>>(value);
            ICollection<int> deserialized = Deserialize<ICollection<int>>(serialized);
            Assert.IsInstanceOf<List<int>>(deserialized);
            CollectionAssert.AreEquivalent(value, deserialized);
        }

        [Test]
        public void IDictionaryDeserializedAsDictionary() {
            var value = new Dictionary<string, int> { { "1", 1 }, { "2", 2 }, { "3", 3 } };
            string serialized = Serialize<Dictionary<string, int>>(value);
            IDictionary<string, int> deserialized = Deserialize<IDictionary<string, int>>(serialized);
            Assert.IsInstanceOf<Dictionary<string, int>>(deserialized);
            CollectionAssert.AreEquivalent(value, deserialized);
        }
    }
}