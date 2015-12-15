using System;
using NUnit.Framework;

namespace FullSerializer.Tests.EnumTest {

    [Flags]
    public enum DefinedFlags {
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 5
    }

    [Flags]
    public enum RegularFlags {
        A,
        B,
        C
    }


    public enum NotFlags {
        A = 10,
        B = 0,
        C = 1,
        D = 20
    }

    public class EnumTests {
        [Test]
        public void TestDefinedFlagsEnum() {
            DoTest(DefinedFlags.A);
            DoTest(DefinedFlags.B);
            DoTest(DefinedFlags.C);
            DoTest(DefinedFlags.A | DefinedFlags.B);
            DoTest(DefinedFlags.A | DefinedFlags.C);
            DoTest(DefinedFlags.A | DefinedFlags.B | DefinedFlags.C);
        }

        [Test]
        public void TestRegularFlagsEnum() {
            DoTest(RegularFlags.A);
            DoTest(RegularFlags.B);
            DoTest(RegularFlags.C);
            DoTest(RegularFlags.A | RegularFlags.B);
            DoTest(RegularFlags.A | RegularFlags.C);
            DoTest(RegularFlags.A | RegularFlags.B | RegularFlags.C);
        }

        [Test]
        public void TestEnum() {
            DoTest(NotFlags.A);
            DoTest(NotFlags.B);
            DoTest(NotFlags.C);
            DoTest(NotFlags.D);
            DoTest(NotFlags.A & NotFlags.B);
        }

        private void DoTest<T>(T expected) {
            fsData serializedData;
            new fsSerializer().TrySerialize(expected, out serializedData).AssertSuccessWithoutWarnings();

            var actual = default(T);
            new fsSerializer().TryDeserialize(serializedData, ref actual);

            Assert.AreEqual(expected, actual);
        }
    }
}