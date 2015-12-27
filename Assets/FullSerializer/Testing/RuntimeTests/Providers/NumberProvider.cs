using System;
using System.Collections.Generic;

public class NumberProvider : TestProvider<object> {
    public override bool Compare(object before, object after) {
        return EqualityComparer<object>.Default.Equals(before, after);
    }

    public override IEnumerable<object> GetValues() {
        yield return (Single)(-2.5f);
        yield return Single.MaxValue;
        yield return Single.MinValue;

        yield return (Double)(-2.5);
        //yield return Double.MaxValue; // Mono errors on Double.Parse(Double.MaxValue.ToString())
        //yield return Double.MinValue; // Mono errors on Double.Parse(Double.MinValue.ToString())

        yield return (Decimal).2;
        //yield return Decimal.MaxValue;
        //yield return Decimal.MinValue;

        yield return (Byte)2;
        yield return Byte.MinValue;
        yield return Byte.MaxValue;

        yield return (SByte)2;
        yield return SByte.MinValue;
        yield return SByte.MaxValue;

        yield return (UInt16)2;
        yield return UInt16.MinValue;
        yield return UInt16.MaxValue;

        yield return (Int16)4;
        yield return Int16.MinValue;
        yield return Int16.MaxValue;

        yield return (UInt32)5;
        yield return UInt32.MinValue;
        yield return UInt32.MaxValue;

        yield return (Int32)6;
        yield return Int32.MinValue;
        yield return Int32.MaxValue;

        yield return (UInt64)7;
        yield return UInt64.MinValue;
        //yield return UInt64.MaxValue;

        yield return (Int64)8;
        yield return Int64.MinValue;
        yield return Int64.MaxValue;
    }
}