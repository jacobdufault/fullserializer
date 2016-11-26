using NUnit.Framework;

namespace FullSerializer.Tests {
    public class Base {
        public int id;
        public Base reference;

        public override bool Equals(object obj) {
            return ((Base)obj).id == id;
        }

        public override int GetHashCode() {
            return id;
        }
    }

    public class Derived1 : Base {
    }

    public class Derived2 : Base {
    }

    public struct Holder {
        public object value;
    }


    public class CyclicReferenceTests {
        private static T Clone<T>(T obj) {
            fsData data;
            (new fsSerializer()).TrySerialize(obj, out data).AssertSuccessWithoutWarnings();
            (new fsSerializer()).TryDeserialize(data, ref obj).AssertSuccessWithoutWarnings();
            return obj;
        }

        [Test]
        public void SharedReferenceAcrossDifferentSerializationsAreNotKept() {
            var obj = new object();
            var holder = new Holder { value = obj };

            fsData data;
            var serializer = new fsSerializer();

            // Try serializing once.
            serializer.TrySerialize(holder, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("{\"value\":{}}", fsJsonPrinter.CompressedJson(data));

            // Serialize the same thing again to verify we don't preseve the reference.
            serializer.TrySerialize(holder, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("{\"value\":{}}", fsJsonPrinter.CompressedJson(data));

            // Serialize an array of Holders to verify references are maintained across an array.
            var arrayOfHolders = new Holder[] {
                new Holder { value = obj },
                new Holder { value = obj }
            };
            serializer.TrySerialize(arrayOfHolders, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("[{\"value\":{\"$id\":\"0\"}},{\"value\":{\"$ref\":\"0\"}}]", fsJsonPrinter.CompressedJson(data));
        }

        [Test]
        public void TestCyclicReferenceWithDifferentTypes() {
            // this verifies that cyclic reference detection will not
            // only treat objects of the same type as part of a cycle;
            // this supports some (broken) equals implementations

            var original = new Derived1 {
                reference = new Derived2()
            };
            var cloned = Clone(original);

            Assert.IsInstanceOf<Derived1>(cloned);
            Assert.IsInstanceOf<Derived2>(cloned.reference);
        }
    }
}