using System;
using System.Collections.Generic;

public class GuidProvider : BaseProvider<Guid> {
    public override IEnumerable<Guid> GetValues() {
        yield return new Guid();
        yield return Guid.NewGuid();
    }
}
