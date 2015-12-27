using System;
using System.Collections.Generic;

public class GuidProvider : TestProvider<Guid> {
    public override bool Compare(Guid before, Guid after) {
        return before == after;
    }

    public override IEnumerable<Guid> GetValues() {
        yield return new Guid();
        yield return Guid.NewGuid();
    }
}
