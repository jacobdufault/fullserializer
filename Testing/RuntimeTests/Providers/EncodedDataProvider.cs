using System.Collections.Generic;

public class MyEncodedData {
    public string value;
}

public class EncodedDataProvider : TestProvider<MyEncodedData> {

    public override bool Compare(MyEncodedData before, MyEncodedData after) {
        return before.value == after.value;
    }

    public override IEnumerable<MyEncodedData> GetValues() {
        yield return new MyEncodedData { value = @"P:\UnityProjects" };
        yield return new MyEncodedData { value = @"P:\\UnityProjects" };
    }
}