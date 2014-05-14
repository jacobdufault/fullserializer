using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullJson.Test {
    public class PrimitiveTest : TestBase {

        public override void Run() {
            JsonConverter converter = new JsonConverter();

            for (int i = -50; i < 50; ++i) {
                object result = null;

                var fail = converter.TryDeserialize(new JsonData(i), typeof(int), ref result);
                AssertTrue(fail.Succeeded);

                JsonData serialized;
                fail = converter.TrySerialize(typeof(int), i, out serialized);
                AssertTrue(fail.Succeeded);
                AssertEquals(new JsonData(i), serialized);

                AssertEquals(i, result);
            }
        }
    }
}