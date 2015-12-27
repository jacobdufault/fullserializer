using System;
using NUnit.Framework;

namespace FullSerializer.Tests.ProcessorTests {
    public class OrderedProcessor : fsObjectProcessor {
        public override bool CanProcess(Type type) {
            return true;
        }

        public static int NextId;
        public int onBeforeSerialize, onAfterSerialize, onBeforeDeserialize, onAfterDeserialize;

        public override void OnBeforeSerialize(Type storageType, object instance) {
            onBeforeSerialize = ++NextId;
        }

        public override void OnAfterSerialize(Type storageType, object instance, ref fsData data) {
            onAfterSerialize = ++NextId;
        }

        public override void OnBeforeDeserialize(Type storageType, ref fsData data) {
            onBeforeDeserialize = ++NextId;
        }

        public override void OnAfterDeserialize(Type storageType, object instance) {
            onAfterDeserialize = ++NextId;
        }
    }

    public class ProcessorTests {
        [Test]
        public void TestProcessorOrdering() {
            DoTest<object>(null);
            DoTest(3);
            DoTest(new fsData());
        }

        private static void DoTest<T>(T obj) {
            var serializer = new fsSerializer();

            var processor1 = new OrderedProcessor();
            var processor2 = new OrderedProcessor();

            serializer.AddProcessor(processor1);
            serializer.AddProcessor(processor2);

            int id = 0;
            fsData data;
            OrderedProcessor.NextId = 0;

            serializer.TrySerialize(obj, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual(++id, processor1.onBeforeSerialize);
            Assert.AreEqual(++id, processor2.onBeforeSerialize);
            Assert.AreEqual(++id, processor2.onAfterSerialize);
            Assert.AreEqual(++id, processor1.onAfterSerialize);


            id = 0;
            var deserialized = default(T);
            OrderedProcessor.NextId = 0;

            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(++id, processor1.onBeforeDeserialize);
            Assert.AreEqual(++id, processor2.onBeforeDeserialize);
            Assert.AreEqual(++id, processor2.onAfterDeserialize);
            Assert.AreEqual(++id, processor1.onAfterDeserialize);
        }
    }
}