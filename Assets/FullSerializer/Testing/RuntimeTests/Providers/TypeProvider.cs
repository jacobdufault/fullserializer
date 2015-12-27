using System;
using System.Collections.Generic;
using System.Linq;

public class TypeProvider : TestProvider<Type> {
    public override bool Compare(Type before, Type after) {
        return before == after;
    }

    public override IEnumerable<Type> GetValues() {
        yield return typeof(int);
        yield return typeof(TestProvider<>);
        yield return typeof(TestProvider<int>);
    }
}

public class TypeListProvider : TestProvider<List<Type>> {
    public override bool Compare(List<Type> before, List<Type> after) {
        return
            before.Except(after).Count() == 0 &&
            after.Except(before).Count() == 0;
    }

    public override IEnumerable<List<Type>> GetValues() {
        // This verifies that cycle detection is disabled
        yield return new List<Type> {
            typeof(int), typeof(int), typeof(float), typeof(int)
        };
    }
}

