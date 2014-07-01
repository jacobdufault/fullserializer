using System;
using System.Collections;
using System.Collections.Generic;

public class MyEnumerableType : IEnumerable {
    public int A;

    public IEnumerator GetEnumerator() {
        throw new NotImplementedException();
    }
}

public class CustomIEnumerableProvider : BaseProvider<MyEnumerableType> {
    public override IEnumerable<MyEnumerableType> GetValues() {
        yield return new MyEnumerableType { A = -1 };
        yield return new MyEnumerableType { A = 0 };
        yield return new MyEnumerableType { A = 1 };
    }
}
