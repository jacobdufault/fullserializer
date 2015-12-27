using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace FullSerializer.Tests.MinimalMetadataTests {
    public class MinimalMetadata {
        [fsObject(MemberSerialization = fsMemberSerialization.OptIn)]
        public class TestContents {
            [fsProperty]
            public int Number { get; set; }

            public override string ToString() {
                return string.Format("Number={0}", Number);
            }
        }

        [fsObject(MemberSerialization = fsMemberSerialization.OptIn)]
        public class TestContainer {

            [fsProperty]
            public string Id { get; set; }

            [fsProperty]
            public List<TestContents> Contents { get; set; }

            public TestContainer() {
                Id = "C1";
                Contents = new List<TestContents>();
            }

            public override string ToString() {
                return string.Format("Id={0}, Contents.Count={1}", Id, Contents.Count);
            }
        }

        [Test]
        public void JsonSerializerDoesNotEmitMetadata() {
            var container = new TestContainer() { Id = "C7" };
            fsData data;
            (new fsSerializer()).TrySerialize(container, out data);
            var json = fsJsonPrinter.CompressedJson(data);

            // {"Id":"C7","Contents":{"$content":[]}}
            Assert.True(Regex.IsMatch(json, @"C7"));
            Assert.False(Regex.IsMatch(json, @"\$content"));
        }
    }
}