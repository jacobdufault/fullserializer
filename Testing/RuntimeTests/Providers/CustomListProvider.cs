using System;
using System.Collections;
using System.Collections.Generic;

public interface IListType : IList<int> {
}

public class MyList : IListType {
    public List<int> _backing;

    public MyList() {
        _backing = new List<int>();
    }

    public MyList(List<int> list) {
        _backing = new List<int>(list);
    }

    public IEnumerator<int> GetEnumerator() {
        return _backing.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(int item) {
        _backing.Add(item);
    }

    public void Clear() {
        throw new NotImplementedException();
    }

    public bool Contains(int item) {
        throw new NotImplementedException();
    }

    public void CopyTo(int[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public bool Remove(int item) {
        throw new NotImplementedException();
    }

    public int Count {
        get { return _backing.Count; }
    }

    public bool IsReadOnly {
        get { throw new NotImplementedException(); }
    }

    public int IndexOf(int item) {
        throw new NotImplementedException();
    }

    public void Insert(int index, int item) {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index) {
        throw new NotImplementedException();
    }

    public int this[int index] {
        get { return _backing[index]; }
        set { _backing[index] = value; }
    }
}

public struct Wrapper {
    public IListType container;
}

public class CustomListProvider : TestProvider<IListType> {
    public override bool Compare(IListType before, IListType after) {
        if (before.Count != after.Count) return false;
        for (int i = 0; i < before.Count; ++i) {
            if (before[i] != after[i]) return false;
        }
        return true;
    }

    public override IEnumerable<IListType> GetValues() {
        yield return new MyList();
        yield return new MyList { 1, 2, 3, 4, 5, 6, 7 };
    }
}