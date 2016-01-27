using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FullSerializer.Tests {
    [fsObject(Converter = typeof(ModelDirectConverter))]
    public class WarningModel {}

    public class ModelDirectConverter : fsDirectConverter<WarningModel> {
        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref WarningModel model) {
            return fsResult.Warn("Warning");
        }

        protected override fsResult DoSerialize(WarningModel model, Dictionary<string, fsData> serialized) {
            return fsResult.Warn("Warning");
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new WarningModel();
        }
    }

    public class WarningPropagationTests {
        [Test]
        public void TestWarningsFromDirectConverters() {
            fsData data;
            var serializer = new fsSerializer();

            fsResult result = serializer.TrySerialize(new WarningModel(), out data);
            Assert.AreEqual(1, result.RawMessages.Count());
            Assert.AreEqual("Warning", result.RawMessages.First());

            WarningModel model = null;
            result = serializer.TryDeserialize(data, ref model);
            Assert.AreEqual(1, result.RawMessages.Count());
            Assert.AreEqual("Warning", result.RawMessages.First());
        }
    }
}