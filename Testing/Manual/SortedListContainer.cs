using FullInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortedListContainer : BaseBehavior<FullSerializerSerializer> {
    public IDictionary<int, string> SortedList;

    public void Reset() {
        SortedList = new SortedList<int, string>();
    }

    [InspectorButton]
    public void PrintDictType() {
        Debug.Log(SortedList.GetType());
    }

    [InspectorButton]
    public void AddNullString() {
        SortedList.Add(1, null);
    }

    [InspectorButton]
    public void AddEmptyString() {
        SortedList.Add(2, string.Empty);
    }
}
