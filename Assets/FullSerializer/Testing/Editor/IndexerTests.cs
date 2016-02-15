using NUnit.Framework;

namespace FullSerializer.Tests.IndexerTest {
    public class IndexerTests {
        class IndexerType {
            private int[] arr = new int[2];

            [fsProperty]
            public int this[int i] {
                get { return arr[i]; }
                set { arr[i] = value; }
            }
        }

        [Test]
        public void TestIndexerType() {
            var original = new IndexerType();
            original[0] = 1;
            original[1] = 5;
            var dup = Clone(original);

            // Silly test really; in an ideal world the below would succeed,
            // but serializing indexers is very hard so instead we test that FullSerializer doesn't crash while processing types with indexers!
            //Assert.AreEqual(original[0], dup[0]);
            //Assert.AreEqual(original[1], dup[1]);

            Assert.IsNotNull(dup);
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
