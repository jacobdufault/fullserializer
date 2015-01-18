using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public struct TestObject {
    public Func<object, object, bool> EqualityComparer;
    public object Original;
    public string Serialized;
    public object Deserialized;
}


public struct TestItem {
    public object Item;
    public Func<object, object, bool> Comparer;
}

public interface ITestProvider {
    IEnumerable<TestItem> GetValues();
}


public abstract class TestProvider<T> : ITestProvider {
    public abstract bool Compare(T before, T after);
    public abstract IEnumerable<T> GetValues();

    IEnumerable<TestItem> ITestProvider.GetValues() {
        foreach (T value in GetValues()) {
            yield return new TestItem() {
                Item = value,
                Comparer = (a, b) => Compare((T)a, (T)b)
            };
        }
    }
}

public class RuntimeTestRunner : MonoBehaviour {
    public Text MessageOutput;

    public string Serialize(Type type, object value) {
        fsData data;
        (new fsSerializer()).TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();
        return fsJsonPrinter.CompressedJson(data);
    }

    public object Deserialize(Type type, string serializedState) {
        fsData data = fsJsonParser.Parse(serializedState);

        object deserialized = null;
        (new fsSerializer()).TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }

    private void ExecuteTests<TProvider>() where TProvider : ITestProvider, new() {
        Log("Executing test provider " + typeof(TProvider).Name);

        var TestValues = new List<TestObject>();
        foreach (var value in (new TProvider()).GetValues()) {
            TestValues.Add(new TestObject {
                Original = value.Item,
                EqualityComparer = value.Comparer
            });
        }

        var Failed = new List<TestObject>();

        for (int i = 0; i < TestValues.Count; ++i) {
            TestObject testObj = TestValues[i];
            try {

                testObj.Serialized = Serialize(testObj.Original.GetType(), testObj.Original);
                testObj.Deserialized = Deserialize(testObj.Original.GetType(), testObj.Serialized);

                TestValues[i] = testObj;

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

        if (Failed.Count == 0) Log("- Verified all values");
        else LogError("!!! Failed " + Failed.Count + " values");
    }

    private void LogError(object msg) {
        if (Application.isPlaying) {
            if (MessageOutput != null) {
                MessageOutput.text += "<color=red><b>ERROR</b>: " + msg.ToString() + "</color>" + Environment.NewLine;
            }
        }

        Debug.LogError(msg);
    }
    private void Log(object msg) {
        if (Application.isPlaying) {
            if (MessageOutput != null) {
                MessageOutput.text += msg.ToString() + Environment.NewLine;
            }
        }

        Debug.Log(msg);
    }

    private void ExecuteAllTests() {
        ExecuteTests<CustomIEnumerableProvider>();
        ExecuteTests<CyclesProvider>();
        ExecuteTests<DateTimeOffsetProvider>();
        ExecuteTests<DateTimeProvider>();
        ExecuteTests<FlagsEnumProvider>();
        ExecuteTests<GuidProvider>();
        ExecuteTests<IDictionaryIntIntProvider>();
        ExecuteTests<IDictionaryStringIntProvider>();
        ExecuteTests<IDictionaryStringStringProvider>();
        ExecuteTests<KeyValuePairProvider>();
        ExecuteTests<NullableDateTimeOffsetProvider>();
        ExecuteTests<NullableDateTimeProvider>();
        ExecuteTests<NullableTimeSpanProvider>();
        ExecuteTests<NumberProvider>();
        ExecuteTests<OptOutProvider>();
        ExecuteTests<PrivateFieldsProvider>();
        ExecuteTests<PropertiesProvider>();
        ExecuteTests<QueueProvider>();
        ExecuteTests<SimpleEnumProvider>();
        ExecuteTests<SortedDictionaryProvider>();
        ExecuteTests<StackProvider>();
        ExecuteTests<TimeSpanProvider>();
        ExecuteTests<TypeListProvider>();
        ExecuteTests<TypeProvider>();
    }

    protected void OnEnable() {
        ExecuteAllTests();
    }
}
