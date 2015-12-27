using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IDictionaryIntIntProvider : TestProvider<IDictionary<int, int>> {
    public override bool Compare(IDictionary<int, int> before, IDictionary<int, int> after) {
        return
            before.Except(after).Count() == 0 &&
            after.Except(before).Count() == 0;
    }

    public override IEnumerable<IDictionary<int, int>> GetValues() {
        yield return new Dictionary<int, int>();

        yield return new Dictionary<int, int> {
            { 1, 1 },
            { 2, 0 },
            { 3, 32 }
        };
    }
}

public class IDictionaryStringIntProvider : TestProvider<IDictionary<string, int>> {
    public override bool Compare(IDictionary<string, int> before, IDictionary<string, int> after) {
        return
            before.Except(after).Count() == 0 &&
            after.Except(before).Count() == 0;
    }

    public override IEnumerable<IDictionary<string, int>> GetValues() {
        yield return new Dictionary<string, int> {
            { "ok", 3 },
            { string.Empty, 2 }
        };
    }
}

public class IDictionaryStringStringProvider : TestProvider<IDictionary<string, string>> {
    public override bool Compare(IDictionary<string, string> before, IDictionary<string, string> after) {
        return
            before.Except(after).Count() == 0 &&
            after.Except(before).Count() == 0;
    }

    public override IEnumerable<IDictionary<string, string>> GetValues() {
        yield return new Dictionary<string, string> {
            { string.Empty, null }
        };
    }
}

public class SortedDictionaryProvider : TestProvider<IDictionary> {
    public enum Enum {
        A, B, C, D, E, F
    }
    [Flags]
    public enum FlagsEnum {
        FlagA = 1 << 0,
        FlagB = 1 << 1,
        FlagC = 1 << 2,
        FlagD = 1 << 3,
        FlagE = 1 << 4,
        FlagF = 1 << 5
    }

    public override bool Compare(IDictionary before, IDictionary after) {
#if !(UNITY_WP8 || UNITY_METRO)
        if (before is SortedDictionary<double, float>)
            return CompareDicts<SortedDictionary<double, float>, double, float>(before, after);
        if (before is SortedList<int, string>)
            return CompareDicts<SortedList<int, string>, int, string>(before, after);
        if (before is SortedDictionary<int, int>)
            return CompareDicts<SortedDictionary<int, int>, int, int>(before, after);
        if (before is SortedList<string, float>)
            return CompareDicts<SortedList<string, float>, string, float>(before, after);
        if (before is SortedDictionary<Enum, int>)
            return CompareDicts<SortedDictionary<Enum, int>, Enum, int>(before, after);
        if (before is SortedDictionary<FlagsEnum, int>)
            return CompareDicts<SortedDictionary<FlagsEnum, int>, FlagsEnum, int>(before, after);
#endif

        throw new Exception("unknown type");
    }

    private static bool CompareDicts<TDict, TKey, TValue>(object a, object b) where TDict : IDictionary<TKey, TValue> {
        var dictA = (TDict)a;
        var dictB = (TDict)b;

        return
            dictA.Except<KeyValuePair<TKey, TValue>>(dictB).Count() == 0 &&
            dictB.Except<KeyValuePair<TKey, TValue>>(dictA).Count() == 0;
    }


    public override IEnumerable<IDictionary> GetValues() {
#if !(UNITY_WP8 || UNITY_METRO)
        yield return new SortedDictionary<double, float>();

        yield return new SortedList<int, string> {
            { 0, string.Empty },
            { 1, null }
        };

        yield return new SortedDictionary<int, int> {
            { 0, 0 },
            { 1, 1 },
            { -1, -1 }
        };

        yield return new SortedList<string, float> {
            { "ok", 1 },
            { "yes", 2 },
            { string.Empty, 3 }
        };

        yield return new SortedDictionary<Enum, int> {
        };

        yield return new SortedDictionary<Enum, int> {
            { Enum.A, 3 }
        };

        yield return new SortedDictionary<Enum, int> {
            { Enum.A, 1 },
            { Enum.B, 2 },
            { Enum.C, 3 },
        };

        yield return new SortedDictionary<FlagsEnum, int> {
        };

        yield return new SortedDictionary<FlagsEnum, int> {
            { FlagsEnum.FlagA, 3 }
        };

        yield return new SortedDictionary<FlagsEnum, int> {
            { FlagsEnum.FlagA | FlagsEnum.FlagB, 1 },
            { FlagsEnum.FlagC, 2 },
            { FlagsEnum.FlagD | FlagsEnum.FlagE | FlagsEnum.FlagF, 3 },
        };
#endif

        yield break;
    }
}
