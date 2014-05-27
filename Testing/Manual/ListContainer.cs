using FullInspector;
using System.Collections.Generic;

public struct StructHolder<T> {
    public T Value;
}

public class ListContainer : BaseBehavior<FullSerializerSerializer> {
    public StructHolder<List<int>> IntList;

    [InspectorButton]
    public void Populate() {
        IntList.Value = new List<int>();
        for (int i = 0; i < 5000; ++i) {
            IntList.Value.Add(i);
        }
    }
}