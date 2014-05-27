using FullJson;
using FullInspector;
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

    [Serializable]
    public class Cyclic {
        public Cyclic Other;
        public int Value;
    }

    public void MakeCyclic() {
        CyclicRoot = new Cyclic();
        CyclicRoot.Other = new Cyclic();
        CyclicRoot.Other.Other = CyclicRoot;
    }

    public Cyclic CyclicRoot;

    public SimpleStruct Simple;
    public MyStruct<int, string> MyStructValue;
    public IInterface<int> MyInterface;

    public DateTime DateTime;
    public SortedDictionary<int, string> SortedDict;

    public struct GenericHolder<T> { public T Value; }
    public GenericHolder<List<int>> List;
    public GenericHolder<SortedList<int, string>> SortedList;
    public Dictionary<EnumValue, List<float>> EnumDict;
    public KeyValuePair<int, float> KeyValuePair;
    public GenericHolder<int?> HoldInt;

    //public void Update() {
    //    GetComponent<TextMesh>().text = MyInterface.ToString();
    //    Debug.LogError(MyInterface);
    //}
}