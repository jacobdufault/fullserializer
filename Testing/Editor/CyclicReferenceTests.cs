using System.Collections.Generic;
using NUnit.Framework;

namespace FullSerializer.Tests.CyclicReference {
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



    public class CyclicReferenceTests {
        private static T Clone<T>(T obj) {
            fsData data;
            (new fsSerializer()).TrySerialize(obj, out data).AssertSuccessWithoutWarnings();
            (new fsSerializer()).TryDeserialize(data, ref obj).AssertSuccessWithoutWarnings();
            return obj;
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