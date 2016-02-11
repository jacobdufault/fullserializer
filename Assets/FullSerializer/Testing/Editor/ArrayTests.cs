using System;
using NUnit.Framework;

namespace FullSerializer.Tests.ArrayTest {
    public class ArrayTests {
        [Test]
        public void TestArray1D()
        {
            int[] before = new int[ 10 ];
            DoTest( before );
        }

        [Test]
        public void TestArray2D()
        {
            int[,] before = new int[ 10,20 ];
            DoTest( before );
        }


        private void DoTest<T>( T expected ) {
            fsData serializedData;
            new fsSerializer().TrySerialize( expected, out serializedData ).AssertSuccessWithoutWarnings();

            var actual = default( T );
            new fsSerializer().TryDeserialize( serializedData, ref actual );

            Assert.AreEqual( expected, actual );
        }
    }
}
