using System.Collections.Generic;
using NUnit.Framework;

namespace FullSerializer.Tests {
    public class SimpleModel {
        public int A;
        public List<int> B;
    }

    public class InitialInstanceTests {
        [Test]
        public void TestPopulateObject() {
            // This test verifies that when we pass in an existing object
            // instance that same instance is used to deserialize into, ie,
            // we can do the equivalent of Json.NET's PopulateObject

            SimpleModel model1 = new SimpleModel { A = 3, B = new List<int> { 1, 2, 3 } };

            fsData data;

            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TrySerialize(model1, out data).Succeeded);

            model1.A = 1;
            model1.B = new List<int> { 1 };
            SimpleModel model2 = model1;
            Assert.AreEqual(1, model1.A);
            Assert.AreEqual(1, model2.A);
            CollectionAssert.AreEqual(new List<int> { 1 }, model1.B);
            CollectionAssert.AreEqual(new List<int> { 1 }, model2.B);

            Assert.IsTrue(serializer.TryDeserialize(data, ref model2).Succeeded);

            // If the same instance was not used, then model2.A will equal 1
            Assert.AreEqual(3, model1.A);
            Assert.AreEqual(3, model2.A);
            CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, model1.B);
            CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, model2.B);
            Assert.IsTrue(ReferenceEquals(model1, model2));
        }
    }
}