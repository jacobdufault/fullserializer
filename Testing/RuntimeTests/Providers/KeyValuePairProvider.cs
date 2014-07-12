using System.Collections.Generic;
public class KeyValuePairProvider : BaseProvider<object> {
    private static KeyValuePair<TKey, TValue> Make<TKey, TValue>(TKey key, TValue value) {
        return new KeyValuePair<TKey, TValue>(key, value);
    }

    public override IEnumerable<object> GetValues() {
        yield return Make(1, 2);
        yield return Make(1, 2.2);
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
