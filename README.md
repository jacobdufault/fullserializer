# Full Serializer

Full Serializer is an extremely easy to use serializer for Unity that *just works*.  It is as simple as possible but no simpler. It currently ships with a robust JSON parser and printer.

Full Serializer has been designed to support all Unity export platforms, including tricky ones such as the WebPlayer and iOS. Additionally, it has been designed to support full stripping mode on iOS (Full Serializer does not use exceptions).

Best of all, Full Serializer is completely free to use and available under the MIT license!

## Why?

There were no serializers that just work in Unity that are free and target all export platforms. [Full Inspector](http://forum.unity3d.com/threads/224270-Full-Inspector-Inspector-and-serialization-for-structs-dicts-generics-interfaces) needed one, so here it is.

## Installation

Import the `Source` folder into your Unity project! You're good to go!

# Usage

## Annotations and Default Behavior

Usage is identical to Unity's default serialization, *except* that you don't have to mark types as `[Serializable]`. Here's an example:

```c#
struct SerializedStruct {
    public int Field;

    public Dictionary<string, string> DictionaryAutoProperty { get; set; }

    [SerializeField]
    private int PrivateField;
}
```

Here are the precise rules:

- Public fields are serialized by default
- Auto-properties that are at least partially public are serialized by default
- All fields or properties annotated with `[SerializeField]` are serialized
- The default name in serialization data for a field/property is that field/property's name. However, you can override this with `[fsProperty("name")]`. `[fsProperty]` will also cause private fields to be serialized.
- Public fields/public auto-properties are not serialized if annotated with `[NonSerialized]` or `[fsIgnore]`. `[fsIgnore]` can be used on properties (unlike `[NonSerialized]`).


Inheritance and cycles are both correctly handled and fully supported by Full Serializer. You don't need to do anything -- they are automatically detected.

## API

### Serialization and Deserialization

Here's a simple example of how to use the Full Serializer API to serialize objects to and from strings.

```c#
using FullSerializer;

public static class StringSerializationAPI {
    private static fsSerializer _serializer = new fsSerializer();

    public static string Serialize(Type type, object value) {
        // serialize the data
        fsData data;
        var fail = _serializer.TrySerialize(type, value, out data);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        // emit the data via JSON
        return fsJsonPrinter.CompressedJson(data);
    }

    public static object Deserialize(Type type, string serializedState) {
        fsFailure fail;

        // step 1: parse the JSON data
        fsData data;
        fail = fsJsonParser.Parse(serializedState, out data);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        // step 2: deserialize the data
        object deserialized = null;
        fail = _serializer.TryDeserialize(data, type, ref deserialized);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        return deserialized;
    }
}
```

While the API may look noisy, rest assured that it cleanly separates different library concerns, such as parsing and serialization. If you wanted to add BSON support, you would only have to write a parser and printer -- no need to touch any serialization or deserialization code.

### Custom Serialization

You can completely override serialization by registering your own `fsConverter` type, which can override serialization for any type. Inheritance and cycles are properly handled even when you use a custom converter.

Please see the next few sections for an example of how to define a custom converter. Custom converters should only be necessary in really exceptional cases -- Full Serializer has been designed to *just work* with any object.

#### Example Definition

```c#
using FullSerializer;

public class MyType {
    public string Value;
}

public class MyTypeConverter : fsConverter {
    public override bool CanProcess(Type type) {
        // CanProcess will be called over every type that Full Serializer
        // attempts to serialize. If this converter should be used, return true
        // in this function.
        return type == typeof(MyType);
    }

    public override fsFailure TrySerialize(object instance,
        out fsData serialized, Type storageType) {
        
        // Serialize the data into the serialized parameter. fsData is a
        // strongly typed object store that maps directly to a JSON object model.
        // It's really easy to use.

        var myType = (MyType)instance;
        serialized = new fsData(myType.Value);
        return fsFailure.Success;
    }

    public override fsFailure TryDeserialize(fsData storage,
        ref object instance, Type storageType) {
        
        // Always make to sure to verify that the deserialized data is the of
        // the expected type. Otherwise, on platforms where exceptions are
        // disabled bad things can happen (if the data was actually an object
        // and you try to access a string, an exception will be thrown).
        if (storage.Type != fsDataType.String) {
            return fsFailure.Fail("Expected string fsData type but got " +
                storage.Type);
        }

        // We just want to deserialize into the existing object instance. If
        // instance is a value type, then we can assign directly into instance
        // to update the value.

        var myType = (MyType)instance;
        myType.Value = storage.AsString;
        return fsFailure.Success;
    }

    // Object instance construction is separated from deserialization so that
    // cycles can be correctly handled. If it's not possible to construct an
    // instance of the expected type here, then just return any non-null value
    // and construct the proper instance in TryDeserialize (though cycles will
    // *not* be handled properly).
    //
    // You do not need to override this method if your converted type is a
    // struct.
    public override object CreateInstance(fsData data, Type storageType) {
        return new MyType();
    }
}
```

#### Example Registration

```c#
var serializer = new fsSerializer();
serializer.AddConverter(new MyTypeConverter());
```

After registration, use your serializer like normal and when it comes time to serialize or deserialize, your custom `MyTypeConverter` will automatically be invoked to serialize/deserialize `MyType` objects.

# Limitations

Full Serializer has minimal limitations, however, there are as follows:

- The WebPlayer build target requires all deserialized types to have a default constructor
- No multidimensional array support (this can be added with a custom converter, however)
- Delegates are not serialized (how? If you have any ideas, please let me know!)

# Adding Full Serializer to my project

Import the `Source` folder into your Unity project! You're good to go!

## How do I run the tests?

To run automated tests, please also import [Unity Test Tools](https://www.assetstore.unity3d.com/en/#!/content/13802) into your project. Then you can run the NUnit tests via the standard unit test menu `Unity Test Tools\Unit Tests\Run all unit tests`.

There are also a set of manual tests; simply add `Testing\RuntimeTests\RuntimeTestObject.prefab` to an empty scene and enter play-mode.

> note: You need [Full Inspector](http://forum.unity3d.com/threads/224270-Full-Inspector-Inspector-and-serialization-for-structs-dicts-generics-interfaces) to run the manual tests. You can use the trial for in-editor testing, but to actually test on export platforms you need the full version.


# License

Full Serializer is freely available under the MIT license. If you make any improvements, it would be greatly appreciated if you would submit a pull request with them.
