using System;
using System.Collections.Generic;
using System.Globalization;

public class DateTimeProvider : BaseProvider<DateTime> {
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
    }
}

public class DateTimeOffsetProvider : BaseProvider<DateTimeOffset> {
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
        yield return DateTimeOffset.Now.AddDays(5).AddHours(3).AddTicks(1);
    }
}

public class TimeSpanProvider : BaseProvider<TimeSpan> {
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

public class NullableDatesProvider : BaseProvider<object> {
    public override IEnumerable<object> GetValues() {
        yield return new ValueHolder<DateTime?>(null);
        yield return new ValueHolder<DateTime?>(DateTime.UtcNow);
        yield return new ValueHolder<DateTimeOffset?>(null);
        yield return new ValueHolder<DateTimeOffset?>(DateTimeOffset.UtcNow);
        yield return new ValueHolder<TimeSpan?>(null);
        yield return new ValueHolder<TimeSpan?>(TimeSpan.FromSeconds(35));
    }
}