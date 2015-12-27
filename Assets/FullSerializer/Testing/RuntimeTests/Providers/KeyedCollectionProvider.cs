using System.Collections.Generic;
using System.Collections.ObjectModel;

public class TestCollection : KeyedCollection<TestEnum, TestClass> {
    protected override TestEnum GetKeyForItem(TestClass item) {
        return item == null ? TestEnum.Null : item.TestEnum;
    }

    public bool TryGetValue(TestEnum key, out TestClass item) {
        if (Dictionary == null) {
            item = default(TestClass);
            return false;
        }

        return Dictionary.TryGetValue(key, out item);
    }

    public new bool Contains(TestClass item) {
        return base.Contains(GetKeyForItem(item));
    }
}

public enum TestEnum {
    Null,
    Value1,
    Value2,
    Value3
}

public class TestClass {
    public TestEnum TestEnum { get; set; }
}

public class KeyedCollectionProvider : TestProvider<TestCollection> {
    public override bool Compare(TestCollection before, TestCollection after) {
        if (before.Count != after.Count) return false;
        for (int i = 0; i < before.Count; ++i) {
            if (before[i].TestEnum != after[i].TestEnum) return false;
        }
        return true;
    }

    public override IEnumerable<TestCollection> GetValues() {
        yield return new TestCollection();
        yield return new TestCollection {
            new TestClass { TestEnum = TestEnum.Null }
        };
        yield return new TestCollection {
            new TestClass { TestEnum = TestEnum.Null },
            new TestClass { TestEnum = TestEnum.Value1 },
        };
        yield return new TestCollection {
            new TestClass { TestEnum = TestEnum.Null },
            new TestClass { TestEnum = TestEnum.Value1 },
            new TestClass { TestEnum = TestEnum.Value2 },
        };
        yield return new TestCollection {
            new TestClass { TestEnum = TestEnum.Null },
            new TestClass { TestEnum = TestEnum.Value1 },
            new TestClass { TestEnum = TestEnum.Value2},
            new TestClass { TestEnum = TestEnum.Value3 },
        };
    }
}
