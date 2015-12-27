using FullSerializer;
using System.Collections.Generic;

[fsObject(MemberSerialization = fsMemberSerialization.OptOut)]
public class OptOut {
    public OptOut() { }
    public OptOut(int publicField, int publicAutoProperty, int publicManualProperty, int privateField, int privateAutoProperty, int ignoredField, int ignoredAutoProperty) {
        this.publicField = publicField;
        this.publicAutoProperty = publicAutoProperty;
        this.publicManualProperty = publicManualProperty;
        this.privateField = privateField;
        this.privateAutoProperty = privateAutoProperty;
        this.ignoredField = ignoredField;
        this.ignoredAutoProperty = ignoredAutoProperty;
    }

    public int publicField;
    public int publicAutoProperty { get; set; }
    public int publicManualProperty { get { return publicField; } set { publicField = value; } }
    private int privateField;
    private int privateAutoProperty { get; set; }
    public int GetPrivateField() { return privateField; }
    public int GetPrivateAutoProperty() { return privateAutoProperty; }

    [fsIgnore]
    private int ignoredField;

    [fsIgnore]
    private int ignoredAutoProperty { get; set; }

    public int GetIgnoredField() { return ignoredField; }
    public int GetIgnoredAutoProperty() { return ignoredAutoProperty; }
}

public class OptOutProvider : TestProvider<OptOut> {
    public override bool Compare(OptOut before, OptOut after) {
        return
            before.publicField == after.publicField &&
            before.publicAutoProperty == after.publicAutoProperty &&
            before.publicManualProperty == after.publicManualProperty &&
            before.GetPrivateField() == after.GetPrivateField() &&
            before.GetPrivateAutoProperty() == after.GetPrivateAutoProperty() &&

            before.GetIgnoredField() != after.GetIgnoredField() &&
            before.GetIgnoredAutoProperty() != after.GetIgnoredAutoProperty();
    }

    public override IEnumerable<OptOut> GetValues() {
        yield return new OptOut(1, 1, 1, 1, 1, 1, 1);
    }
}
