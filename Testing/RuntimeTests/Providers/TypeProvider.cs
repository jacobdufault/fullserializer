using System;
using System.Collections.Generic;

public class TypeProvider : BaseProvider<Type> {
    public override IEnumerable<Type> GetValues() {
        yield return typeof(int);
        yield return typeof(BaseProvider<>);
        yield return typeof(BaseProvider<int>);
    }
}

