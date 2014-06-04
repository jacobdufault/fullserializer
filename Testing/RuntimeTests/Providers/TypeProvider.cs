using System;
using System.Collections.Generic;

public class TypeProvider : BaseProvider<Type> {
    public override IEnumerable<Type> GetValues() {
        yield return typeof(int);
        yield return typeof(BaseProvider<>);
        yield return typeof(BaseProvider<int>);
    }
}

public class TypeListProvider : BaseProvider<List<Type>> {
    public override IEnumerable<List<Type>> GetValues() {
        // This verifies that cycle detection is disabled
        yield return new List<Type> {
            typeof(int), typeof(int), typeof(float), typeof(int)
        };
    }
}

