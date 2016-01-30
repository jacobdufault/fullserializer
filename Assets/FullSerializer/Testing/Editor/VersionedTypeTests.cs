using System.Collections.Generic;
using FullSerializer.Internal;
using NUnit.Framework;

namespace FullSerializer.Tests {
    [fsObject("1")]
    public struct VersionedModel_v1 {
        public int A;
    }

    [fsObject("1", typeof(VersionedModel_v1))]
    public struct VersionedModelDuplicateVersionString {
        public VersionedModelDuplicateVersionString(VersionedModel_v1 model) { }
    }

    [fsObject("2", typeof(VersionedModel_v1))]
    public struct VersionedModelMissingConstructor {
    }

    [fsObject("2", typeof(VersionedModel_v1))]
    public struct VersionedModel_v2 {
        //public VersionedModel_v2() { }
        public VersionedModel_v2(VersionedModel_v1 model) {
            B = model.A;
        }

        public int B;
    }


    // Type graph hierarchy
    //
    //      1_2abc_3
    //
    //        /|\
    //       / | \
    //      /  |  \
    //     /   |   \
    //    /    |    \
    //   /     |     \
    //
    //  1_2a  1_2b  1_2c
    //
    //   \     |     /
    //    \    |    /
    //     \   |   /
    //      \  |  /
    //       \ | /
    //        \|/
    //
    //         1
    //
    // Valid migration paths:
    //   1 -> 1_2a -> 1_2abc_3
    //   1 -> 1_2b -> 1_2abc_3
    //   1 -> 1_2c -> 1_2abc_3

    [fsObject("1")]
    struct ComplexModel_1 { }

    [fsObject("1_2a", typeof(ComplexModel_1))]
    struct ComplexModel_1_2a { ComplexModel_1_2a(ComplexModel_1 m) { } }
    [fsObject("1_2b", typeof(ComplexModel_1))]
    struct ComplexModel_1_2b { ComplexModel_1_2b(ComplexModel_1 m) { } }
    [fsObject("1_2c", typeof(ComplexModel_1))]
    struct ComplexModel_1_2c { ComplexModel_1_2c(ComplexModel_1 m) { } }

    [fsObject("1_2abc_3", typeof(ComplexModel_1_2a), typeof(ComplexModel_1_2b), typeof(ComplexModel_1_2c))]
    struct ComplexModel_1_2abc_3 {
        ComplexModel_1_2abc_3(ComplexModel_1_2a m) { }
        ComplexModel_1_2abc_3(ComplexModel_1_2b m) { }
        ComplexModel_1_2abc_3(ComplexModel_1_2c m) { }
    }




    public class VersionedTypeTests {
        [Test]
        [ExpectedException(typeof(fsDuplicateVersionNameException))]
        public void DuplicateVersionString() {
            fsVersionManager.GetVersionedType(typeof(VersionedModelDuplicateVersionString));
        }

        [Test]
        [ExpectedException(typeof(fsMissingVersionConstructorException))]
        public void MissingConstructor() {
            fsVersionManager.GetVersionedType(typeof(VersionedModelMissingConstructor));
        }

        [Test]
        public void VerifyGraphHistory() {
            Assert.AreEqual(
                fsVersionManager.GetVersionedType(typeof(VersionedModel_v1)).Value,
                fsVersionManager.GetVersionedType(typeof(VersionedModel_v2)).Value.Ancestors[0]);

            List<fsVersionedType> path;


            Assert.IsTrue(fsVersionManager.GetVersionImportPath("1", fsVersionManager.GetVersionedType(typeof(VersionedModel_v2)).Value, out path).Succeeded);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(fsVersionManager.GetVersionedType(typeof(VersionedModel_v1)).Value, path[0]);
            Assert.AreEqual(fsVersionManager.GetVersionedType(typeof(VersionedModel_v2)).Value, path[1]);


            Assert.IsTrue(fsVersionManager.GetVersionImportPath("1", fsVersionManager.GetVersionedType(typeof(ComplexModel_1_2abc_3)).Value, out path).Succeeded);
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(fsVersionManager.GetVersionedType(typeof(ComplexModel_1)).Value, path[0]);
            Assert.IsTrue(
                (fsVersionManager.GetVersionedType(typeof(ComplexModel_1_2a)).Value == path[1]) ||
                (fsVersionManager.GetVersionedType(typeof(ComplexModel_1_2b)).Value == path[1]) ||
                (fsVersionManager.GetVersionedType(typeof(ComplexModel_1_2c)).Value == path[1]));
            Assert.AreEqual(fsVersionManager.GetVersionedType(typeof(ComplexModel_1_2abc_3)).Value, path[2]);
        }

        [Test]
        public void MultistageMigration() {
            var serializer = new fsSerializer();

            var model_v1 = new VersionedModel_v1 {
                A = 3
            };
            fsData serialized;
            serializer.TrySerialize(model_v1, out serialized).AssertSuccessWithoutWarnings();

            var model_v2 = new VersionedModel_v2();
            serializer.TryDeserialize(serialized, ref model_v2).AssertSuccessWithoutWarnings();
            Assert.AreEqual(model_v1.A, model_v2.B);
        }
    }
}