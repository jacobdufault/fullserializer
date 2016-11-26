using NUnit.Framework;

namespace FullSerializer.Tests {
    public class LegacyDataTests {
        [Test]
        public void ImportLegacyInheritance() {
            fsData data = fsData.CreateDictionary();
            data.AsDictionary["Type"] = new fsData("System.Int32");
            data.AsDictionary["Data"] = new fsData(32);

            object o = null;
            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TryDeserialize(data, ref o).Succeeded);

            Assert.IsTrue(o.GetType() == typeof(int));
            Assert.IsTrue((int)o == 32);
        }

        public class Cycle {
            public Cycle Ref;
            public int Value;
        }

        [Test]
        public void ImportLegacyCycle() {
            fsData data = fsData.CreateDictionary();
            data.AsDictionary["SourceId"] = new fsData("0");
            data.AsDictionary["Data"] = fsData.CreateDictionary();
            data.AsDictionary["Data"].AsDictionary["Value"] = new fsData(3);
            data.AsDictionary["Data"].AsDictionary["Ref"] = fsData.CreateDictionary();
            data.AsDictionary["Data"].AsDictionary["Ref"].AsDictionary["ReferenceId"] = new fsData("0");

            UnityEngine.Debug.Log(fsJsonPrinter.PrettyJson(data));

            Cycle c = null;
            var serializer = new fsSerializer();
            Assert.IsTrue(serializer.TryDeserialize(data, ref c).Succeeded);
            Assert.AreEqual(3, c.Value);
            Assert.AreEqual(c, c.Ref);
        }
    }
}