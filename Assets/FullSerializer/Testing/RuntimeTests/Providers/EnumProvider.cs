using System;
using System.Collections.Generic;

public enum SimpleEnum {
    A, B, C, D, E
}

[Flags]
public enum SimpleFlagsEnum {
    A = 1, B = 2, C = 4, D = 8, E = 16
}

public class SimpleEnumProvider : TestProvider<SimpleEnum> {
    public override bool Compare(SimpleEnum before, SimpleEnum after) {
        return before == after;
    }

    public override IEnumerable<SimpleEnum> GetValues() {
        yield return SimpleEnum.A;
        yield return SimpleEnum.C;
        yield return SimpleEnum.D;
    }
}

public class FlagsEnumProvider : TestProvider<SimpleFlagsEnum> {
    public override bool Compare(SimpleFlagsEnum before, SimpleFlagsEnum after) {
        return before == after;
    }

    public override IEnumerable<SimpleFlagsEnum> GetValues() {
        yield return SimpleFlagsEnum.A;
        yield return SimpleFlagsEnum.A | SimpleFlagsEnum.B;
        yield return SimpleFlagsEnum.B | SimpleFlagsEnum.C | SimpleFlagsEnum.D | SimpleFlagsEnum.E;
    }
}