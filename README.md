# Full Json

Full Json is an extremely easy to use serializer for Unity that *just works*.  It is as simple as possible but no simpler.

Full Json has been designed to support all Unity export platforms, including tricky ones such as the WebPlayer and iOS. Additionally, it has been designed to support full stripping mode on iOS (Full Json does not use exceptions if properly used).

Best of all, Full Json is completely free to use and available under the MIT license!

## Why?

There were no serializers that just work in Unity that are free and target all export platforms. [Full Inspector](http://forum.unity3d.com/threads/224270-Full-Inspector-Inspector-and-serialization-for-structs-dicts-generics-interfaces) needed one, so here it is.

# Usage

## Serialization

Usage is identical to Unity's default serialization, *except* that you don't have to mark types as `[Serializable]`. Here's an example:

```c#
struct SerializedStruct {
    public int Field;

    public Dictionary<string, string> DictionaryAutoProperty { get; set; }

    [SerializeField]
    private int PrivateField;
}
```

Public fields and auto-properties (that are at least partially publicly visible) are serialized by default. If you wish to serialize a private field/property or a non-auto property, then simply annotate it with `[SerializeField]`.

Inheritance is fully suppoted within Full Json; types are included in serialization data automatically.

Full Json will correctly serialize and deserialize object graphs that contain cycles.

## API

### Serialization and Deserialization

Here's a simple example of how to use the Full Json API to serialize objects to and from strings.

```c#
public static class StringSerializationAPI {
    public static string Serialize(Type type, object value) {
        JsonConverter converter = new JsonConverter();

        // serialize the data
        JsonData data;
        var fail = converter.TrySerialize(type, value, out data);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        return data.CompressedJson;
    }

    public static object Deserialize(Type type, string serializedState) {
        JsonFailure fail;

        // step 1: parse the data
        JsonData data;
        fail = JsonParser.Parse(serializedState, out data);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        // step 2: deserialize the data
        JsonConverter converter = new JsonConverter();
        object deserialized = null;
        fail = converter.TryDeserialize(data, type, ref deserialized);
        if (fail.Failed) throw new Exception(fail.FailureReason);

        return deserialized;
    }
}
```

While the API may look noisy, rest assured that it cleanly separates different library concerns, such as parsing and serialization. If you wanted to add BSON support, you would only have to write a parser and printer -- no need to touch any serialization or deserialization code.

### Custom Serialization

You can completely override serialization by registering your own `ISerializationConverter` type, which can override serialization for any type. Inheritance and cycles are properly handled even when you use a custom converter.

Please see the next few sections for an example of how to define a custom converter. Custom converters should only be necessary in really exceptional cases -- Full Json has been designed to *just work* with any object.

#### Example Definition

```c#
public class MyType {
    public string Value;
}

public class MyTypeConverter : ISerializationConverter {
    // You can use this variable to access the converter that was used to invoke this converter.
    public JsonConverter Converter {
        get;
        set;
    }

    public bool CanProcess(Type type) {
        // CanProcess will be called over every type that Full Json attempts to serialize. If this
        // converter should be used, return true in this function.
        return type == typeof(MyType);
    }

    public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
        // Serialize the data into the serialized parameter. JsonData is a strongly typed object
        // store that maps directly to JSON. It's really easy to use.

        var myType = (MyType)instance;
        serialized = new JsonData(myType.Value);
        return JsonFailure.Success;
    }

    public JsonFailure TryDeserialize(JsonData storage, ref object instance, Type storageType) {
        // Always make to sure to verify that the deserialized data is the of the expected type.
        // Otherwise, on platforms where exceptions are disabled bad things can happen (if the data
        // was actually an object and you try to access a string, an exception will be thrown).
        if (storage.Type != JsonType.String) {
            return JsonFailure.Fail("Bad JsonData type " + storage.Type + "; expected string");
        }

        // We just want to deserialize into the existing object instance. If instance is a value
        // type, then we can assign directly into instance to update the value.

        var myType = (MyType)instance;
        myType.Value = storage.AsString;
        return JsonFailure.Success;
    }

    // Object instance construction is separated from deserialization so that cycles can be
    // correctly handled. If it's not possible to construct an instance of the expected type here,
    // then just return any non-null value and construct the proper instance in TryDeserialize
    // (though cycles will *not* be handled properly).
    public object CreateInstance(JsonData data, Type storageType) {
        return new MyType();
    }
}
```

#### Example Registration

```c#
JsonConverter converter = new JsonConverter();
converter.AddConverter(new MyTypeConverter());
```

After registration, use your converter like normal and when it comes time to serialize or deserialize, your custom `MyTypeConverter` will automatically be invoked to serialize/deserialize `MyType` objects.

# License

Full Json is freely available under the MIT license. If you make any improvements, it would be greatly appreciated if you would submit a pull request with them.