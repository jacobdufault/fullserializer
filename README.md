# Full Serializer

Full Serializer is an extremely easy to use JSON serializer that *just works*.  It is as simple as possible but no simpler.

Full Serializer has been designed to support all Unity export platforms, including tricky ones such as the WebPlayer and iOS (it's also being used on consoles like the PS4). Additionally, it has been designed to support full stripping mode on iOS (Full Serializer does not use exceptions). Full Serializer can also be used outside of Unity; just make sure to define `NO_UNITY` in your project-level defines.

Best of all, Full Serializer is completely free to use and available under the MIT license!

## Why?

There were no serializers that just work in Unity that are free and target all export platforms. [Full Inspector](http://forum.unity3d.com/threads/224270-Full-Inspector-Inspector-and-serialization-for-structs-dicts-generics-interfaces) needed one, so here it is.

## Installation

Import the `Source` folder into your Unity project! You're good to go! (Also see the DLL based import at the bottom of this document).

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

## [fsObject] Customization

You can easily customize the serialization of a specific object by utilizing `[fsObject]`. There are a number of options:

### Member Serialization

You can specify what the default member serialization is by changing the `MemberSerialization` parameter. The options are `OptIn`, `OptOut`, and `Default`. `OptIn` requires that every serialize member be annotated with `fsProperty`, `OptOut` will serialize every member *except* those annotated with `fsIgnore`, and `Default` uses the default intelligent behavior where visibility level and property type are examined.

### Converter

You can specify a custom converter to use directly on the model. This is more efficient than registering a custom converter on the `fsSerializer` and additionally provides portability w.r.t. the actual `fsSerializer` instance; the `fsSerializer` creator does not need to know about this specific converter.

Here is an example usage. See docs below for a more detailed explanation of `fsConverter`:

```c#
using System;
using FullSerializer;

[fsObject(Converter = typeof(MyConverter))]
public class MyModel {
}

public class MyConverter : fsConverter {
    public override bool CanProcess(Type type) {
        throw new NotSupportedException();
    }

    public override object CreateInstance(fsData data, Type storageType) {
        return new MyModel();
    }

    public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
        serialized = new fsData();
        return fsResult.Success;
    }

    public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
        return fsResult.Success;
    }
}

```

### Versioning

Full Serializer supports versioning for serialization. You can specify the previous version of an object by utilizing these parameters for `[fsObject]`.
```c#
PreviousModels
VersionString
```

`PreviousModels` is an array of types that specify the prior models that this object was migrated from. `VersionString` specifies a unique identifier for this model that separates it from all other prior model instances. Note that if a model should be versioned, it needs to initially have a `VersionString` parameter, otherwise no migration will be performed.

Here is a simple object migration:

```c#
[fsObject("1")]
public struct Model_v1 {
    public int A;
}

[fsObject("2", typeof(Model_v1))]
public struct Model {
    public int B;

    public Model(Model_v1 model) {
        B = model.A;
    }
}
```

Notice in particular that we have a constructor on `Model` that accepts an instance of `Model_v1`. If Full Serializer detects that we are deserializing old data, it will first deserialize it into an instance of `Model_v1` and then return a newly constructed instance of `Model` via the `Model_v1` constructor.

All version strings have to be unique (if not, an error will be issued) and there can be no cycles in the versioning import graph (there can be more than one previous model).

We can easily introduce a new `Model` type and then we just rename `Model` to `Model_v2` and Full Serializer will automatically send a `Model_v1` instance through to `Model_v1(deserialized)` -> `Model_v2(Model_v1)` -> `Model(Model_v2)`. Running deserializing this way prevents an explosion of required constructor types.

## API

### Serialization and Deserialization

Here's a simple example of how to use the Full Serializer API to serialize objects to and from strings.

```c#
using System;
using FullSerializer;

public static class StringSerializationAPI {
    private static readonly fsSerializer _serializer = new fsSerializer();

    public static string Serialize(Type type, object value) {
        // serialize the data
        fsData data;
        _serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        return fsJsonPrinter.CompressedJson(data);
    }

    public static object Deserialize(Type type, string serializedState) {
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(serializedState);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }
}
```

Note that this API example will throw exceptions if any errors occur. Additionally, error recovery is disabled in this example - if you wish to enable it, simply replace the `AssertSuccessWithoutWarnings` calls with `AssertSuccess`.

### Custom Serialization

You can completely override serialization by registering your own `fsConverter` type, which can override serialization for any type. Inheritance and cycles are properly handled even when you use a custom converter.

Please see the next few sections for an example of how to define a custom converter. Custom converters should only be necessary in really exceptional cases -- Full Serializer has been designed to *just work* with any object.

#### Example Definition

```c#
using System;
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

    public override fsResult TrySerialize(object instance,
        out fsData serialized, Type storageType) {

        // Serialize the data into the serialized parameter. fsData is a
        // strongly typed object store that maps directly to a JSON object model.
        // It's really easy to use.

        var myType = (MyType)instance;
        serialized = new fsData(myType.Value);
        return fsResult.Success;
    }

    public override fsResult TryDeserialize(fsData storage,
        ref object instance, Type storageType) {

        // Always make to sure to verify that the deserialized data is the of
        // the expected type. Otherwise, on platforms where exceptions are
        // disabled bad things can happen (if the data was actually an object
        // and you try to access a string, an exception will be thrown).
        if (storage.Type != fsDataType.String) {
            return fsResult.Fail("Expected string fsData type but got " +
                storage.Type);
        }

        // We just want to deserialize into the existing object instance. If
        // instance is a value type, then we can assign directly into instance
        // to update the value.

        var myType = (MyType)instance;
        myType.Value = storage.AsString;
        return fsResult.Success;
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

or:

```c#
[fsObject(Converter = typeof(MyTypeConverter))]
public class MyType {}
```

After registration, use your serializer like normal and when it comes time to serialize or deserialize, your custom `MyTypeConverter` will automatically be invoked to serialize/deserialize `MyType` objects.

# Limitations

Full Serializer has minimal limitations, however, there are as follows:

- The WebPlayer build target requires all deserialized types to have a default constructor
- No multidimensional array support (this can be added with a custom converter, however)
- Delegates are not serialized (how? If you have any ideas, please let me know!)

# Adding Full Serializer to my project

## Source Based Import

Import the `Source` folder into your Unity project! You're good to go!

## DLL Based Import

These instructions are easy to follow and will set you up with the DLLs for any version of Full Serializer.

- Download Full Serializer and unzip it to some directory.
- Navigate to the `Build Files (DLL)` folder.
- Open `CommonData.csproj` in your favorite text editor.
- Change the `UnityInstallFolder` value on line 6 to point to your Unity installation directory
    - On OSX this is likely `/Applications/Unity/Editor`
    - On Windows this is likely `C:\Program Files (x86)\Unity\Editor`
- Double click `FullSerializer.sln` to open up the solution
- Run a build-all (F6 in visual studio). Alternatively, you can right-click any of the three projects to build only one of them.
    - `FullSerializer - NoUnity` builds Full Serializer so that you can use it outside of Unity.
    - `FullSerializer - Unity` builds Full Serializer to a DLL
    - `FullSerializer - Unity - WinRT` builds Full Serializer with WinRT APIs (if you're targeting the Windows Store or the Windows Phone export platforms)
- You will find the DLLs inside of the `Build` folder. Please add them to your Unity project's Asset folder.


## How do I run the tests?

To run automated tests, please also import [Unity Test Tools](https://www.assetstore.unity3d.com/en/#!/content/13802) into your project. Then you can run the NUnit tests via the standard unit test menu `Unity Test Tools\Unit Tests\Run all unit tests`.

Full Serializer also has a suite of runtime tests to ensure that various platform support actually works when deployed. You can run these tests by opening up the `Testing/test_scene` scene and hitting play.

# License

Full Serializer is freely available under the MIT license. If you make any improvements, it would be greatly appreciated if you would submit a pull request with them (please match the existing code style).
