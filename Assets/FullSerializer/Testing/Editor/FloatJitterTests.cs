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
    }
}