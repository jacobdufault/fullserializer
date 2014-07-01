using System;
using System.Collections;
using System.Collections.Generic;

public class IDictionaryProvider : BaseProvider<IDictionary> {
    public override IEnumerable<IDictionary> GetValues() {
        yield return new Dictionary<int, int>();

        yield return new Dictionary<int, int> {
            { 1, 1 },
            { 2, 0 },
            { 3, 32 }
        };

        yield return new Dictionary<string, int> {
            { "ok", 3 },
            { string.Empty, 2 }
        };

        yield return new Dictionary<string, string> {
            { string.Empty, null }
        };
    }
}

public class SortedDictionaryProvider : BaseProvider<IDictionary> {
    public enum Enum {
        A, B, C, D, E, F
    }
    [Flags]
    public enum FlagsEnum {
        A, B, C, D, E, F
    }

    public override IEnumerable<IDictionary> GetValues() {
#if !(UNITY_WP8 || UNITY_WINRT)
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
            { FlagsEnum.A, 3 }
        };

        yield return new SortedDictionary<FlagsEnum, int> {
            { FlagsEnum.A | FlagsEnum.B, 1 },
            { FlagsEnum.C, 2 },
            { FlagsEnum.D | FlagsEnum.E | FlagsEnum.F, 3 },
        };
#endif
        yield break;
    }
}