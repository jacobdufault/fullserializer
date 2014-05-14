using FullInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullJson.Test {
    public abstract class TestBase : BaseBehavior {
        public abstract void Run();

        protected void AssertTrue(bool value) {
            if (value == false) throw new InvalidOperationException("Expected " + value + " to be true");
        }
        protected void AssertNotNull(object value) {
            if (value == null) throw new InvalidOperationException("Expected " + value + " to not be null");
        }
        protected void AssertEquals<T>(T expected, T actual) {
            if (EqualityComparer<T>.Default.Equals(expected, actual) == false) {
                throw new InvalidOperationException("Expected " + actual + " to be equal to " + expected);
            }
        }

        [InspectorButton]
        public void InvokeAll() {
            TestBase[] objects = UnityObject.FindObjectsOfType<TestBase>();
            foreach (TestBase test in objects) {
                test.Run();
                Debug.Log("success on " + test);
            }
        }
    }
}