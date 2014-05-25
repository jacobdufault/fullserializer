using FullInspector;
using FullInspector.Internal;
using FullJson;
using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FullJsonSerializer : BaseSerializer {
    public override string Serialize(MemberInfo storageType, object value,
        ISerializationOperator serializationOperator) {

        JsonConverter converter = new JsonConverter();
        JsonData data;
        var fail = converter.TrySerialize(GetStorageType(storageType), value, out data);
        if (fail.Failed) {
            throw new Exception(fail.FailureReason);
        }

        return data.CompressedJson;
    }

    public override object Deserialize(MemberInfo storageType, string serializedState,
        ISerializationOperator serializationOperator) {

        JsonData data = JsonParser.Parse(serializedState);
        JsonConverter converter = new JsonConverter();

        object deserialized = null;
        var fail = converter.TryDeserialize(data, GetStorageType(storageType), ref deserialized);
        if (fail.Failed) {
            throw new Exception(fail.FailureReason);
        }

        return deserialized;
    }
}

public struct TestItem {
    public object Item;
    public Func<object, object, bool> Comparer;
}

public interface ITestProvider {
    IEnumerable<TestItem> GetValues();
}

public abstract class BaseProvider<T> : ITestProvider {
    public abstract IEnumerable<T> GetValues();
    public bool Compare(T original, T deserialized) {
        var customCompare = deserialized as ICustomCompareRequested;
        if (customCompare != null) {
            return customCompare.AreEqual(original);
        }

        CompareLogic compare = new CompareLogic(true);
        ComparisonResult result = compare.Compare(original, deserialized);
        return result.AreEqual;
    }
    
    private bool CompareObjects(object a, object b) {
        if (typeof(T).IsAssignableFrom(a.GetType()) == false ||
            typeof(T).IsAssignableFrom(b.GetType()) == false) {
            return false;
        }

        if (a.GetType() != b.GetType()) {
            return false;
        }

        return Compare((T)a, (T)b);
    }

    IEnumerable<TestItem> ITestProvider.GetValues() {
        foreach (T value in GetValues()) {
            yield return new TestItem {
                Item = value,
                Comparer = CompareObjects
            };
        }
    }
}

public interface IAfterDeserializeCallback {
    void VerifyDeserialize();
}

public interface ICustomCompareRequested {
    bool AreEqual(object original);
}

public class TestRunner : BaseBehavior<FullJsonSerializer> {
    public string Serialize(Type type, object value) {
        JsonConverter converter = new JsonConverter();
        JsonData data;
        var fail = converter.TrySerialize(type, value, out data);
        if (fail.Failed) {
            throw new Exception(fail.FailureReason);
        }

        return data.CompressedJson;
    }

    public object Deserialize(Type type, string serializedState) {

        JsonData data = JsonParser.Parse(serializedState);
        JsonConverter converter = new JsonConverter();

        object deserialized = null;
        var fail = converter.TryDeserialize(data, type, ref deserialized);
        if (fail.Failed) {
            throw new Exception(fail.FailureReason);
        }

        return deserialized;
    }


    [InspectorOrder(0)]
    [InspectorButton]
    public void PopulateProviders() {
        TestProviders = new List<ITestProvider>();

        foreach (var type in RuntimeReflectionUtilities.AllSimpleTypesDerivingFrom(typeof(ITestProvider))) {
            var provider = (ITestProvider)Activator.CreateInstance(type);
            TestProviders.Add(provider);
        }
    }

    [InspectorOrder(.5)]
    [InspectorButton]
    public void PopulateValues() {
        Failed = new List<TestObject>();
        TestValues = new List<TestObject>();

        foreach (ITestProvider provider in TestProviders) {
            foreach (var value in provider.GetValues()) {
                TestValues.Add(new TestObject {
                    Original = value.Item,
                    EqualityComparer = value.Comparer
                });
            }
        }
    }

    [InspectorOrder(1)]
    [InspectorButton]
    public void Verify() {
        Failed = new List<TestObject>();

        for (int i = 0; i < TestValues.Count; ++i) {
            TestObject testObj = TestValues[i];

            testObj.Serialized = Serialize(testObj.Original.GetType(), testObj.Original);
            testObj.Deserialized = Deserialize(testObj.Original.GetType(), testObj.Serialized);

            TestValues[i] = testObj;

            try {
                var afterDeserialize = testObj.Deserialized as IAfterDeserializeCallback;
                if (afterDeserialize != null) {
                    afterDeserialize.VerifyDeserialize();
                }

                if (testObj.EqualityComparer(testObj.Original, testObj.Deserialized) == false) {
                    throw new Exception("Item " + i + " with type " + testObj.Original.GetType() +
                        " is not equal to it's deserialized object");
                }


            }
            catch (Exception e) {
                Failed.Add(testObj);
                LogError(e);
            }

        }

        if (Failed.Count == 0) Log("Verified all values");
        else LogError("Failed " + Failed.Count + " values");
    }

    private void LogError(object msg) {
        if (Application.isPlaying) {
            var txt = GetComponent<TextMesh>();
            if (txt != null) {
                txt.text += "ERROR: " + msg.ToString() + Environment.NewLine;
            }
        }

        Debug.LogError(msg);
    }
    private void Log(object msg) {
        if (Application.isPlaying) {
            var txt = GetComponent<TextMesh>();
            if (txt != null) {
                txt.text += msg.ToString() + Environment.NewLine;
            }
        }

        Debug.Log(msg);
    }

    public struct TestObject {
        public Func<object, object, bool> EqualityComparer;
        public object Original;
        public string Serialized;
        public object Deserialized;
    }

    public List<ITestProvider> TestProviders;
    [NonSerialized]
    public List<TestObject> Failed;
    [NotSerialized]
    public List<TestObject> TestValues;

    public void OnEnable() {
        if (Application.isPlaying) {
            var txt = GetComponent<TextMesh>();
            if (txt != null) {
                txt.text += Environment.NewLine;
            }
        }

        if (TestProviders == null) {
            LogError("Providers is null");
            TestProviders = new List<ITestProvider>();
        }

        Log("Populating values; have " + TestProviders.Count + " providers");
        PopulateValues();
        Verify();
    }
}
