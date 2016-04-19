using NUnit.Framework;

namespace FullSerializer.Tests {
    public class FloatJitterTests {
        [Test]
        public void VerifyFloatSerializationDoesNotHaveJitter() {
            var serializer = new fsSerializer();

            // We serialize w/o jitter
            fsData data;
            serializer.TrySerialize(0.1f, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("0.1", fsJsonPrinter.PrettyJson(data));

            // We deserialize w/o jitter.
            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(0.1f, deserialized);
        }

        [Test]
        public void VerifyNaNRoundTrips() {
            var serializer = new fsSerializer();

            // todo: could definitely reduce duplication of tests in this file!
            fsData data;
            serializer.TrySerialize(float.NaN, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("NaN", fsJsonPrinter.PrettyJson(data));

            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(float.NaN, deserialized);
        }

        [Test]
        public void VerifyPositiveInfinityRoundTrips() {
            var serializer = new fsSerializer();

            fsData data;
            serializer.TrySerialize(float.PositiveInfinity, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("Infinity", fsJsonPrinter.PrettyJson(data));

            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(float.PositiveInfinity, deserialized);
        }

        [Test]
        public void VerifyNegativeInfinityRoundTrips() {
            var serializer = new fsSerializer();

            fsData data;
            serializer.TrySerialize(float.NegativeInfinity, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual("-Infinity", fsJsonPrinter.PrettyJson(data));

            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(float.NegativeInfinity, deserialized);
        }

        [Test]
        public void VerifyLargeDoubleRoundTrips() {
            double valueToTest = 500000000000000000.0;

            var serializer = new fsSerializer();

            fsData data;
            serializer.TrySerialize(valueToTest, out data).AssertSuccessWithoutWarnings();

            Assert.AreEqual(valueToTest.ToString(System.Globalization.CultureInfo.InvariantCulture), fsJsonPrinter.PrettyJson(data));

            double deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(valueToTest, deserialized);
        }

        [Test]
        public void VerifyMaxValueRoundTrips() {
            var serializer = new fsSerializer();

            fsData data;
            serializer.TrySerialize(float.MaxValue, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual(((double)float.MaxValue).ToString(System.Globalization.CultureInfo.InvariantCulture), fsJsonPrinter.PrettyJson(data));

            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(float.MaxValue, deserialized);
        }

        [Test]
        public void VerifyMinValueRoundTrips() {
            var serializer = new fsSerializer();

            fsData data;
            serializer.TrySerialize(float.MinValue, out data).AssertSuccessWithoutWarnings();
            Assert.AreEqual(((double)float.MinValue).ToString(System.Globalization.CultureInfo.InvariantCulture), fsJsonPrinter.PrettyJson(data));

            float deserialized = 0f;
            serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            Assert.AreEqual(float.MinValue, deserialized);
        }
    }
}