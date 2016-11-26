using System.Collections.Generic;
using NUnit.Framework;

namespace FullSerializer.Tests {

    public class DictionaryTests {
        [Test]
        public void TestEmitStringDictionary() {
            var model = new Dictionary<string, object>();
            model["t"] = true;
            model["0"] = 0;

            fsData actual;
            (new fsSerializer().TrySerialize(model, out actual)).AssertSuccessWithoutWarnings();

            var expected = fsData.CreateDictionary();
            expected.AsDictionary["t"] = fsData.CreateDictionary();
            expected.AsDictionary["t"].AsDictionary["$type"] = new fsData("System.Boolean");
            expected.AsDictionary["t"].AsDictionary["$content"] = fsData.True;
            expected.AsDictionary["0"] = fsData.CreateDictionary();
            expected.AsDictionary["0"].AsDictionary["$type"] = new fsData("System.Int32");
            expected.AsDictionary["0"].AsDictionary["$content"] = new fsData(0);

            Assert.AreEqual(expected, actual);
        }
    }
}