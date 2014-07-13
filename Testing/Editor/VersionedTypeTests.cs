using NUnit.Framework;

namespace FullSerializer.Tests {
    [fsObject(VersionString = "1")]
    public struct VersionedModel_v1 {
        public int A;
    }

    [fsObject(
        VersionString = "1",
        PreviousModels = new[] { typeof(VersionedModel_v1) })
    ]
    public struct VersionedModelDuplicateVersionString {
        public VersionedModelDuplicateVersionString(VersionedModel_v1 model) { }
    }

    [fsObject(
        VersionString = "2",
        PreviousModels = new[] { typeof(VersionedModel_v1) })
    ]
    public struct VersionedModelMissingConstructor {
    }

    [fsObject(
        VersionString = "2",
        PreviousModels = new[] { typeof(VersionedModel_v1) })
    ]
    public struct VersionedModel_v2 {
        //public VersionedModel_v2() { }
        public VersionedModel_v2(VersionedModel_v1 model) {
            B = model.A;
        }

        public int B;
    }


    public class VersionedTypeTests {
        [Test]
        public void DuplicateVersionString() {
            fsVersionedImport.GetVersionedType(typeof(VersionedModelDuplicateVersionString));
        }

        [Test]
        public void MissingConstructor() {
            fsVersionedImport.GetVersionedType(typeof(VersionedModelMissingConstructor));
        }

        [Test]
        public void VerifyGraphHistory() {
            Assert.AreEqual(
                fsVersionedImport.GetVersionedType(typeof(VersionedModel_v1)).Value,
                fsVersionedImport.GetVersionedType(typeof(VersionedModel_v2)).Value.Ancestors[0]);
        }

        [Test]
        public void MultistageMigration() {
            var serializer = new fsSerializer();

            var model_v1 = new VersionedModel_v1 {
                A = 3
            };
            fsData serialized;
            Assert.IsTrue(serializer.TrySerialize(model_v1, out serialized).Succeeded);

            var model_v2 = new VersionedModel_v2();
            Assert.IsTrue(serializer.TryDeserialize(serialized, ref model_v2).Succeeded);
            Assert.AreEqual(model_v1.A, model_v2.B);
        }
    }
}