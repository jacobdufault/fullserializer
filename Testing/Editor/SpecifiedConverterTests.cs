using System;
using FullSerializer.Internal;
using NUnit.Framework;
using System.Collections.Generic;

namespace FullSerializer.Tests {
    [fsObject(Converter = typeof(MyConverter))]
    public class MyModel {
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

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            DidSerialize = true;
            serialized = new fsData();
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            DidDeserialize = true;
            return fsFailure.Success;
        }
    }

    public class SpecifiedConverterTests {
        [Test]
        public void VerifyConversion() {
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