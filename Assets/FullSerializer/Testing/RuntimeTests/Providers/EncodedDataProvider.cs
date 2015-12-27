using System.Collections.Generic;

public class MyEncodedData {
    private MyEncodedData() { }

    public static MyEncodedData Make(string value) { return new MyEncodedData { value = value }; }
    public string value;
}

public class EncodedDataProvider : TestProvider<MyEncodedData> {

    public override bool Compare(MyEncodedData before, MyEncodedData after) {
        return before.value == after.value;
    }

    public override IEnumerable<MyEncodedData> GetValues() {
        yield return MyEncodedData.Make(@"P:\UnityProjects");
        yield return MyEncodedData.Make(@"P:\\UnityProjects");
    }
}