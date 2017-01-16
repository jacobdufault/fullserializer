using System.Collections.Generic;
using NUnit.Framework;

namespace FullSerializer.Tests {
    public class IgnoreTypeTests {
        [fsIgnore]
        private class IgnoredClass {
        }
        private class NotIgnoredClass {
        }
        [fsIgnore]
        private struct IgnoredStruct {
        }
        private struct NotIgnoredStruct {
        }
        private class Model {
            public IgnoredClass IgnoredClass;
            public NotIgnoredClass NotIgnoredClass;
            public IgnoredStruct IgnoredStruct;
            public NotIgnoredStruct NotIgnoredStruct;
        }

        [Test]
        public void TestSerializeReadOnlyProperty() {
            var model = new Model();

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model, out data).Succeeded);

            var expected = fsData.CreateDictionary();
            expected.AsDictionary["NotIgnoredClass"] = new fsData();
            expected.AsDictionary["NotIgnoredStruct"] = fsData.CreateDictionary();
            Assert.AreEqual(expected, data);
        }
    }
}