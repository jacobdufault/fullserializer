using System;
using System.Collections.Generic;
using System.Globalization;

public class DateTimeProvider : TestProvider<DateTime> {
    public override bool Compare(DateTime original, DateTime deserialized) {
        return original == deserialized;
    }

    public override IEnumerable<DateTime> GetValues() {
        yield return new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        yield return DateTime.Now;
        yield return DateTime.MaxValue.Subtract(TimeSpan.FromTicks(1));
        yield return DateTime.MinValue;
        yield return new DateTime();
        yield return DateTime.UtcNow;
        yield return DateTime.Now.AddDays(5).AddHours(3).AddTicks(1);
        yield return Convert.ToDateTime("2016-01-22T12:06:57.503005Z");
    }
}

public class DateTimeOffsetProvider : TestProvider<DateTimeOffset> {
    public override bool Compare(DateTimeOffset original, DateTimeOffset deserialized) {
        return original == deserialized;
    }

    public override IEnumerable<DateTimeOffset> GetValues() {
#if !UNITY_WINRT
        yield return new DateTimeOffset(5500, 2, 15, 0, 0, 0, 5, new HebrewCalendar(), new TimeSpan());
#endif
        yield return DateTimeOffset.Now;
        yield return DateTimeOffset.MaxValue.Subtract(TimeSpan.FromTicks(1));
        yield return DateTimeOffset.MinValue;
        yield return new DateTimeOffset();
        yield return DateTimeOffset.UtcNow;

        // TODO: why is this invalid?
        //yield return DateTimeOffset.Now.AddDays(5).AddHours(3).AddTicks(1);
    }
}

public class TimeSpanProvider : TestProvider<TimeSpan> {
    public override bool Compare(TimeSpan original, TimeSpan deserialized) {
        return original == deserialized;
    }

    public override IEnumerable<TimeSpan> GetValues() {
        yield return TimeSpan.MaxValue;
        yield return TimeSpan.MinValue;

        yield return new TimeSpan();

        yield return new TimeSpan()
            .Add(TimeSpan.FromDays(3))
            .Add(TimeSpan.FromHours(2))
            .Add(TimeSpan.FromMinutes(33))
            .Add(TimeSpan.FromSeconds(35))
            .Add(TimeSpan.FromMilliseconds(35))
            .Add(TimeSpan.FromTicks(250));
    }
}

public class NullableDateTimeProvider : TestProvider<ValueHolder<DateTime?>> {
    public override bool Compare(ValueHolder<DateTime?> before, ValueHolder<DateTime?> after) {
        return before.Value == after.Value;
    }

    public override IEnumerable<ValueHolder<DateTime?>> GetValues() {
        yield return new ValueHolder<DateTime?>(null);
        yield return new ValueHolder<DateTime?>(DateTime.UtcNow);
    }
}

public class NullableDateTimeOffsetProvider : TestProvider<ValueHolder<DateTimeOffset?>> {
    public override bool Compare(ValueHolder<DateTimeOffset?> before, ValueHolder<DateTimeOffset?> after) {
        return before.Value == after.Value;
    }

    public override IEnumerable<ValueHolder<DateTimeOffset?>> GetValues() {
        yield return new ValueHolder<DateTimeOffset?>(null);
        yield return new ValueHolder<DateTimeOffset?>(DateTimeOffset.UtcNow);
    }
}

public class NullableTimeSpanProvider : TestProvider<ValueHolder<TimeSpan?>> {
    public override bool Compare(ValueHolder<TimeSpan?> before, ValueHolder<TimeSpan?> after) {
        return before.Value == after.Value;
    }

    public override IEnumerable<ValueHolder<TimeSpan?>> GetValues() {
        yield return new ValueHolder<TimeSpan?>(null);
        yield return new ValueHolder<TimeSpan?>(TimeSpan.FromSeconds(35));
    }
}