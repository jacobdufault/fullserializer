using System.Collections.Generic;
using System.Linq;

public class KeyValuePairProvider : TestProvider<object> {
    private static KeyValuePair<TKey, TValue> Make<TKey, TValue>(TKey key, TValue value) {
        return new KeyValuePair<TKey, TValue>(key, value);
    }

    public override bool Compare(object before, object after) {
        if (before is List<KeyValuePair<int, int>>) {
            var beforeA = (List<KeyValuePair<int, int>>)before;
            var afterA = (List<KeyValuePair<int, int>>)after;

            return
                beforeA.Except(afterA).Count() == 0 &&
                afterA.Except(beforeA).Count() == 0;
        }

        return EqualityComparer<object>.Default.Equals(before, after);
    }

    public override IEnumerable<object> GetValues() {
        yield return Make(1, 2);
        yield return Make("yes", 2);
        yield return Make("1", "2");
        yield return Make(Make(1, 2), Make("1", "2"));
        yield return new List<KeyValuePair<int, int>>() {
            Make(1, 2),
            Make(2, 3),
            Make(4, 5)
        };
        yield return Make<string, string>(null, string.Empty);
    }
}
