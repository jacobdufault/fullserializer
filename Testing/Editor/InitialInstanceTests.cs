using NUnit.Framework;

namespace FullSerializer.Tests.InitialInstance {
    public class SimpleModel {
        public int A;
    }

    public class InitialInstanceTests {
        [Test]
        public void TestPopulateObject() {
            // This test verifies that when we pass in an existing object
            // instance that same instance is used to deserialize into, ie,
            // we can do the equivalent of Json.NET's PopulateObject

            SimpleModel model1 = new SimpleModel { A = 3 };

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model1, out data).Succeeded);

            model1.A = 1;
            SimpleModel model2 = model1;
            Assert.AreEqual(1, model1.A);
            Assert.AreEqual(1, model2.A);

            Assert.IsTrue(serializer.TryDeserialize(data, ref model2).Succeeded);

            Assert.AreEqual(3, model1.A);
            Assert.AreEqual(3, model2.A);
            Assert.IsTrue(ReferenceEquals(model1, model2));
        }
    }
}