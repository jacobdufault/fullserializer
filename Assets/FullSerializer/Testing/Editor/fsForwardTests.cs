using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FullSerializer.Tests {
    [fsForward("a")]
    struct ForwardInt {
        public int a;
    }
    [fsForward("a")]
    struct ForwardArray {
        public int[] a;

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override bool Equals(object obj) {
            int[] oa = ((ForwardArray)obj).a;
            return Enumerable.SequenceEqual(a, oa);
        }
    }
    [fsForward("a")]
    struct ForwardList {
        public List<int> a;

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public override bool Equals(object obj) {
            List<int> oa = ((ForwardList)obj).a;
            return Enumerable.SequenceEqual(a, oa);
        }
    }
    [fsForward("ok")]
    struct ForwardBadRef {
    }

    public class fsForwardTests {
        [Test]
        public void TestForwarding() {
            DoTest(new ForwardInt { a = 1 }, "1");
            DoTest(new ForwardArray { a = new[] { 1, 2, 3 } }, "[1,2,3]");
            DoTest(new ForwardList { a = new List<int>() { 1, 2, 3 } }, "[1,2,3]");

            Assert.Throws(typeof(Exception), () => {
                DoTest(new ForwardBadRef(), "{}");
            });
        }

        private void DoTest<T>(T instance, string expectedJson) {
            fsData expected = fsJsonParser.Parse(expectedJson);

            fsData serializedData;
            new fsSerializer().TrySerialize(instance, out serializedData).AssertSuccessWithoutWarnings();
            Assert.AreEqual(expected, serializedData);

            var actual = default(T);
            new fsSerializer().TryDeserialize(serializedData, ref actual);

            Assert.AreEqual(instance, actual);
        }
    }
}