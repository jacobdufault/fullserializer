using System;
using NUnit.Framework;

namespace FullSerializer.Tests {
    [fsObject(Converter = typeof(MyConverter))]
    public class MyModel {
    }

    public class ModelWithPropertyConverter {
        [fsProperty(Converter = typeof(MyConverter))]
        public object a;
    }

    public class MyConverter : fsConverter {
        public static bool DidSerialize = false;
        public static bool DidDeserialize = false;

        public override bool CanProcess(Type type) {
            throw new NotSupportedException();
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new MyModel();
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            DidSerialize = true;
            serialized = fsData.CreateDictionary();
            return fsResult.Success;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            DidDeserialize = true;
            return fsResult.Success;
        }
    }

    public class SpecifiedConverterTests {
        [Test]
        public void VerifyPropertyConverter() {
            MyConverter.DidDeserialize = false;
            MyConverter.DidSerialize = false;

            var serializer = new fsSerializer();

            // Make sure to set |a| to some value, otherwise we will short-circuit serialize it to null.
            fsData result;
            serializer.TrySerialize(new ModelWithPropertyConverter { a = 3 }, out result);
            Assert.IsTrue(MyConverter.DidSerialize);
            Assert.IsFalse(MyConverter.DidDeserialize);

            MyConverter.DidSerialize = false;
            object resultObj = null;
            serializer.TryDeserialize(result, typeof(ModelWithPropertyConverter), ref resultObj);
            Assert.IsFalse(MyConverter.DidSerialize);
            Assert.IsTrue(MyConverter.DidDeserialize);
        }

        [Test]
        public void VerifyConversion() {
            MyConverter.DidDeserialize = false;
            MyConverter.DidSerialize = false;

            var serializer = new fsSerializer();

            fsData result;
            serializer.TrySerialize(new MyModel(), out result);
            Assert.IsTrue(MyConverter.DidSerialize);
            Assert.IsFalse(MyConverter.DidDeserialize);

            MyConverter.DidSerialize = false;
            object resultObj = null;
            serializer.TryDeserialize(result, typeof (MyModel), ref resultObj);
            Assert.IsFalse(MyConverter.DidSerialize);
            Assert.IsTrue(MyConverter.DidDeserialize);
        }
    }
}