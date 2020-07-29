using System;
using NUnit.Framework;

namespace FullSerializer.Tests {
    public class DateTimeTests {
        [Test]
        public void StrangeFormatTests() {
            var serializer = new fsSerializer();
            DateTime time = DateTime.Now;
            serializer.TryDeserialize(new fsData("2016-01-22T12:06:57.503005Z"), ref time).AssertSuccessWithoutWarnings();

            Assert.AreEqual(Convert.ToDateTime("2016-01-22T12:06:57.503005Z"), time);
        }

        [Test]
        public void TestDateTimeAsIntIsInt() {
            var serializer = new fsSerializer {Config = {SerializeDateTimeAsInteger = true}};

            var original = new DateTime(1985, 8, 22, 4, 19, 01, 123, DateTimeKind.Utc);
            fsData serializedData;
            serializer.TrySerialize(original, out serializedData).AssertSuccessWithoutWarnings();

            Assert.That(serializedData.IsInt64);
        }

        [Test]
        public void TestDateTimeAsIntRoundTrips() {
            var serializer = new fsSerializer {Config = {SerializeDateTimeAsInteger = true}};

            var original = new DateTime(1985, 8, 22, 4, 19, 01, 123, DateTimeKind.Utc);
            var deserialized = Clone(original, serializer, serializer);

            Assert.AreEqual(original, deserialized);
        }

        public static T Clone<T>(T original, fsSerializer serializer, fsSerializer deserializer) {
            fsData serializedData;
            serializer.TrySerialize(original, out serializedData).AssertSuccessWithoutWarnings();
            var actual = default(T);
            deserializer.TryDeserialize(serializedData, ref actual);
            return actual;
        }
    }
}
