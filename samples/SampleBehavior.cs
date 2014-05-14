using FullInspector;
using FullJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SampleBehavior : BaseBehavior<FullJsonSerializer> {
    [Serializable]
    public struct MyStruct<T1, T2> {
        public T1 Field1;
        public T2 Field2;
    }

    [Serializable]
    public struct SimpleStruct {
        public int IntField;

        public float[] FloatArray;
        public HashSet<int> HashSetTest;
        public List<double> DoubleList;
    }

    public enum EnumValue {
        Key1, Key2, Key3, Key4
    }
    //public Dictionary<EnumValue, List<float>> EnumDict;

    [Serializable]
    public struct SimpleStruct22 {
        public int Key;
        public int Value;
    }
    //public Dictionary<SimpleStruct22, SimpleStruct22> Dict2;

    public interface IInterface<T> { T Field { get; set; } }
    [Serializable]
    public class DerivedA : IInterface<int> { public int A; public int Field { get; set; } }
    [Serializable]
    public class DerivedB<T> : IInterface<T> { public float B; public T Field { get; set; } }

    /*
    [Serializable]
    public class Cyclic {
        public Cyclic Other;
        public int Value;
    }

    [InspectorButton]
    public void MakeCyclic() {
        CyclicRoot = new Cyclic();
        CyclicRoot.Other = new Cyclic();
        CyclicRoot.Other.Other = CyclicRoot;
    }
    */

    //public Cyclic CyclicRoot;
    //[Margin(500)]
    //public SimpleStruct Simple;
    //public MyStruct<int, string> MyStructValue;
    //public IInterface<int> MyInterface;

    //public DateTime DateTime;
    public SortedDictionary<int, string> SortedDict;
    //public DictionaryEntry Entry;

    //public SortedList<int, string> SortedList;
    //public KeyValuePair<int, float> KeyValuePair;
}

public class FullJsonSerializer : BaseSerializer {

    internal override string Serialize(MemberInfo storageType, object value, ISerializationOperator serializationOperator) {
        JsonConverter converter = new JsonConverter();
        JsonData data;
        converter.TrySerialize(GetStorageType(storageType), value, out data);
        return data.CompressedJson;
    }

    internal override object Deserialize(MemberInfo storageType, string serializedState, ISerializationOperator serializationOperator) {
        JsonData data = JsonParser.Parse(serializedState);
        JsonConverter converter = new JsonConverter();

        object deserialized = null;
        converter.TryDeserialize(data, GetStorageType(storageType), ref deserialized);
        return deserialized;
    }
}