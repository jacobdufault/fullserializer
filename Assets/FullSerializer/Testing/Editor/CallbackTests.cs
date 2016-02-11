using System;
using NUnit.Framework;

namespace FullSerializer.Tests.CallbackTests {
    public class ClassModel : fsISerializationCallbacks {
        [fsIgnore]
        public int beforeSerialize;
        [fsIgnore]
        public int afterSerialize;
        [fsIgnore]
        public int beforeDeserialize;
        [fsIgnore]
        public int afterDeserialize;

        void fsISerializationCallbacks.OnBeforeSerialize(Type storageType) { ++beforeSerialize; }
        void fsISerializationCallbacks.OnAfterSerialize(Type storageType, ref fsData data) { ++afterSerialize; }
        void fsISerializationCallbacks.OnBeforeDeserialize(Type storageType, ref fsData data) { ++beforeDeserialize; }
        void fsISerializationCallbacks.OnAfterDeserialize(Type storageType) { ++afterDeserialize; }
    }

    public struct StructModel : fsISerializationCallbacks {
        [fsIgnore]
        public int beforeSerialize;
        [fsIgnore]
        public int afterSerialize;
        [fsIgnore]
        public int beforeDeserialize;
        [fsIgnore]
        public int afterDeserialize;

        void fsISerializationCallbacks.OnBeforeSerialize(Type storageType) { ++beforeSerialize; }
        void fsISerializationCallbacks.OnAfterSerialize(Type storageType, ref fsData data) { ++afterSerialize; }
        void fsISerializationCallbacks.OnBeforeDeserialize(Type storageType, ref fsData data) { ++beforeDeserialize; }
        void fsISerializationCallbacks.OnAfterDeserialize(Type storageType) { ++afterDeserialize; }
    }

    public class CallbackTests {
        [Test]
        public void TestSerializationCallbacksOnStruct() {
            var original = new StructModel();
            var dup = Clone(original);
            // not possible since we don't box original
            //Assert.AreEqual(1, original.beforeSerialize);
            //Assert.AreEqual(1, original.afterSerialize);
            Assert.AreEqual(1, dup.beforeDeserialize);
            Assert.AreEqual(1, dup.afterDeserialize);
        }

        [Test]
        public void TestSerializationCallbacksOnClass() {
            var original = new ClassModel();
            var dup = Clone(original);
            Assert.AreEqual(1, original.beforeSerialize);
            Assert.AreEqual(1, original.afterSerialize);
            Assert.AreEqual(1, dup.beforeDeserialize);
            Assert.AreEqual(1, dup.afterDeserialize);
        }

        [Test]
        public void TestSerializationCallbacksOnNullInstances() {
            ClassModel original = null;
            var dup = Clone( original );
            Assert.AreEqual( original, dup );
        }

        private T Clone<T>(T expected) {
            fsData serializedData;
            new fsSerializer().TrySerialize(expected, out serializedData).AssertSuccessWithoutWarnings();
            var actual = default(T);
            new fsSerializer().TryDeserialize(serializedData, ref actual);
            return actual;
        }
    }
}