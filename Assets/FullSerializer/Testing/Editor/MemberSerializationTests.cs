using NUnit.Framework;
using UnityEngine;

namespace FullSerializer.Tests {
    public class MemberSerializationTests {
        public class Base {
            public int Serialized0;
        }

        [fsObject(MemberSerialization = fsMemberSerialization.OptIn)]
        public class SimpleModel : Base {
            public int NotSerialized0;

            [fsProperty]
            public int Serialized1;
            [SerializeField]
            public int Serialized2;
        }

        [Test]
        public void TestOptIn() {
            var model1 = new SimpleModel {
                Serialized0 = 1,
                Serialized1 = 1,
                Serialized2 = 1,
                NotSerialized0 = 1
            };

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model1, out data).Succeeded);

            SimpleModel model2 = null;
            Assert.IsTrue(serializer.TryDeserialize(data, ref model2).Succeeded);

            Debug.Log(fsJsonPrinter.PrettyJson(data));

            Assert.AreEqual(model1.Serialized0, model2.Serialized0);
            Assert.AreEqual(model1.Serialized1, model2.Serialized1);
            Assert.AreEqual(model1.Serialized2, model2.Serialized2);
            Assert.AreEqual(0, model2.NotSerialized0);
        }
    }
}