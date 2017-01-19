using NUnit.Framework;

namespace FullSerializer.Tests {
    public class ConstructorTests {
        private class ClassWithNoPublicDefaultConstructor {
            public int a = 1;

            [fsIgnore]
            public bool constructorCalled;

            public ClassWithNoPublicDefaultConstructor(int dummy) {
                a = 2;
                constructorCalled = true;
            }
        }

        [Test]
        public void TestClassWithNoPublicDefaultConstructor() {
            var serialized = fsData.CreateDictionary();
            serialized.AsDictionary["a"] = new fsData(3);

            var serializer = new fsSerializer();

            ClassWithNoPublicDefaultConstructor result = null;
            Assert.IsTrue(serializer.TryDeserialize(serialized, ref result).Succeeded);

            // We expect the original value, but not for the constructor to have been called.
            Assert.AreEqual(3, result.a);
            Assert.IsFalse(result.constructorCalled);
        }

        private class ClassWithNoPublicDefaultConstructorButImplicitStatic {
            public int a = 1;
            public static int b = 2;

            [fsIgnore]
            public bool constructorCalled;

            public ClassWithNoPublicDefaultConstructorButImplicitStatic(int dummy) {
                a = 2;
                constructorCalled = true;
            }
        }

        [Test]
        public void TestClassWithNoPublicDefaultConstructorButImplicitStatic() {
            var serialized = fsData.CreateDictionary();
            serialized.AsDictionary["a"] = new fsData(3);

            var serializer = new fsSerializer();

            ClassWithNoPublicDefaultConstructorButImplicitStatic result = null;
            Assert.IsTrue(serializer.TryDeserialize(serialized, ref result).Succeeded);

            // We expect the original value, but not for the constructor to have been called,
            // DESPITE the presence of an implicit public static parameterless constructor.
            Assert.AreEqual(3, result.a);
            Assert.IsFalse(result.constructorCalled);
        }
    }
}