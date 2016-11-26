using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace FullSerializer.Tests {
    public class ErrorRecoveryTests {
        public struct Model {
            public int a, b, c;

            public override string ToString() {
                return string.Format("A: {0}, B: {1}, C: {2}", a, b, c);
            }
        }

        [Test]
        public void TestObjectDeserializeNotAllData() {
            var data = fsData.CreateDictionary();
            data.AsDictionary["a"] = new fsData(5);

            var model = default(Model);
            Assert.IsTrue((new fsSerializer()).TryDeserialize(data, ref model).Succeeded);

            Assert.AreEqual(5, model.a);
            Assert.AreEqual(0, model.b);
            Assert.AreEqual(0, model.b);
        }

        [Test]
        public void TestObjectDeserializeBadDataType() {
            var data = fsData.CreateDictionary();
            data.AsDictionary["a"] = new fsData(5);
            data.AsDictionary["b"] = fsData.CreateDictionary();
            data.AsDictionary["c"] = fsData.CreateList();

            var model = default(Model);
            var result = (new fsSerializer()).TryDeserialize(data, ref model).AssertSuccess();

            Assert.AreEqual(2, result.RawMessages.Count());
            Assert.AreEqual("fsPrimitiveConverter expected number but got Object in {}", result.RawMessages.ElementAt(0));
            Assert.AreEqual("fsPrimitiveConverter expected number but got Array in []", result.RawMessages.ElementAt(1));

            Assert.AreEqual(5, model.a);
            Assert.AreEqual(0, model.b);
            Assert.AreEqual(0, model.b);
        }

        [Test]
        public void TestDeserializeInvalidTypeInfo() {
            Action<fsData> AssertSuccess = typeData => {
                var data = fsData.CreateDictionary();
                data.AsDictionary["$type"] = typeData;
                data.AsDictionary["a"] = new fsData(1);

                var model = default(Model);
                var result = (new fsSerializer()).TryDeserialize(data, ref model).AssertSuccess();
                Debug.Log(result.FormattedMessages);

                Assert.AreEqual(1, model.a);
            };

            AssertSuccess(fsData.CreateDictionary());
            AssertSuccess(fsData.CreateList());
            AssertSuccess(new fsData("invalid type name"));
            AssertSuccess(new fsData("System.Object")); // the wrong type
        }

        [Test]
        public void TestCollectionDeserializeBadCollectionMember() {
            var data = fsJsonParser.Parse(@"
            [
                [],
                { ""a"": {}, ""b"": 1, ""c"": 1 },
                [],
                { ""a"": 1, ""b"": {}, ""c"": 1 },
                [],
                { ""a"": 1, ""b"": 1, ""c"": {} },
                [],
                { ""a"": {}, ""b"": {}, ""c"": {} },
                [],
                { ""a"": 1, ""b"": 1, ""c"": 1 },
                []
            ]");

            TestCollectionDeserializeBadCollectionMemberHelper<Model, Model[]>(data);
            TestCollectionDeserializeBadCollectionMemberHelper<Model, List<Model>>(data);
        }

        private static void TestCollectionDeserializeBadCollectionMemberHelper<TElement, TCollection>(fsData data)
            where TCollection : IList<TElement> {

            var coll = default(TCollection);
            (new fsSerializer()).TryDeserialize(data, ref coll).AssertSuccess();

            Assert.AreEqual(5, coll.Count);
            Assert.AreEqual(new Model { a = 0, b = 1, c = 1 }, coll[0]);
            Assert.AreEqual(new Model { a = 1, b = 0, c = 1 }, coll[1]);
            Assert.AreEqual(new Model { a = 1, b = 1, c = 0 }, coll[2]);
            Assert.AreEqual(new Model { a = 0, b = 0, c = 0 }, coll[3]);
            Assert.AreEqual(new Model { a = 1, b = 1, c = 1 }, coll[4]);
        }


    }
}