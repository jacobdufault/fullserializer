using System;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;

public class MyEnumerableTypeWithoutAdd : IEnumerable {
    public int A;

    public IEnumerator GetEnumerator() {
        throw new NotImplementedException();
    }
}

public class MyEnumerableTypeWithAdd : IEnumerable {
    [fsIgnore]
    public List<int> items = new List<int>();

    public void Add(int item) {
        items.Add(item);
    }

    public IEnumerator GetEnumerator() {
        return items.GetEnumerator();
    }
}

public class CustomIEnumerableProviderWithoutAdd : TestProvider<MyEnumerableTypeWithoutAdd> {
    public override bool Compare(MyEnumerableTypeWithoutAdd before, MyEnumerableTypeWithoutAdd after) {
        return before.A == after.A;
    }

    public override IEnumerable<MyEnumerableTypeWithoutAdd> GetValues() {
        yield return new MyEnumerableTypeWithoutAdd { A = -1 };
        yield return new MyEnumerableTypeWithoutAdd { A = 0 };
        yield return new MyEnumerableTypeWithoutAdd { A = 1 };
    }
}

public class CustomIEnumerableProviderWithAdd : TestProvider<MyEnumerableTypeWithAdd> {
    public override bool Compare(MyEnumerableTypeWithAdd before, MyEnumerableTypeWithAdd after) {
        if (before.items.Count != after.items.Count) return false;
        for (int i = 0; i < before.items.Count; ++i) {
            if (before.items[i] != after.items[i]) return false;
        }
        return true;
    }

    public override IEnumerable<MyEnumerableTypeWithAdd> GetValues() {
        yield return new MyEnumerableTypeWithAdd();
        yield return new MyEnumerableTypeWithAdd { 1 };
        yield return new MyEnumerableTypeWithAdd { 1, 2, 3 };
    }
}