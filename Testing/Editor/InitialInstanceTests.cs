using NUnit.Framework;

namespace FullSerializer.Tests {
    public class SimpleModel {
        public int A;
    }

    public class InitialInstanceTests {
        [Test]
        public void TestInitialInstance() {
            SimpleModel model1 = new SimpleModel { A = 3 };

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model1, out data).Succeeded);

            model1.A = 1;
            SimpleModel model2 = model1;
            Assert.IsTrue(serializer.TryDeserialize(data, ref model2).Succeeded);

            Assert.AreEqual(model1.A, 3);
            Assert.AreEqual(model2.A, 3);
            Assert.IsTrue(ReferenceEquals(model1, model2));
        }
    }
}