using NUnit.Framework;

namespace FullSerializer.Tests {
    public class PropertiesTests {
        public class Model {
            [fsProperty]
            public int Getter { get { return 3; } }

            [fsIgnore]
            public int _setValue;

            [fsProperty]
            public int Setter { set { _setValue = value; } }
        }

        [Test]
        public void TestSerializeReadOnlyProperty() {
            var model = new Model();

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model, out data).Succeeded);

            var expected = fsData.CreateDictionary();
            expected.AsDictionary["Getter"] = new fsData(model.Getter);
            Assert.AreEqual(expected, data);
        }

        [Test]
        public void TestDeserializeWriteOnlyProperty() {
            var data = fsData.CreateDictionary();
            data.AsDictionary["Getter"] = new fsData(111); // not used, but somewhat verifies that we do not try to deserialize into a R/O property
            data.AsDictionary["Setter"] = new fsData(222);

            var model = default(Model);
            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TryDeserialize(data, ref model).Succeeded);

            Assert.AreEqual(222, model._setValue);
        }

        [Test]
        public void TestOptOutOfProperties() {
            var model = new Model();

            fsData data;

            var serializer = new fsSerializer();
            serializer.Config.EnablePropertySerialization = false;
            Assert.IsTrue( serializer.TrySerialize( model, out data ).Succeeded );

            var expected = fsData.CreateDictionary(); // Should just be empty dictionary.
            Assert.AreEqual( expected, data );
        }
    }
}