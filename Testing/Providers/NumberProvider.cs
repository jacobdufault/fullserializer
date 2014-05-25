using System;
using System.Collections.Generic;

public class NumberProvider : BaseProvider<object> {
    public override IEnumerable<object> GetValues() {
        yield return (float)-2.5;
        yield return (double)-2.5;
        yield return (decimal).2;

        yield return (UInt16)2;
        yield return (Int16)4;
        yield return (UInt32)5;
        yield return (Int32)6;
        yield return (UInt64)7;
        yield return (Int64)8;
    }
}