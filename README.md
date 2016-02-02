# Full Serializer

Full Serializer is an easy to use and robust JSON serializer that *just works*. It'll serialize pretty much anything you can throw at it and work on every major Unity platform, including consoles. Full Serializer doesn't use exceptions, so you can activate more stripping options on iOS.

Best of all, Full Serializer is completely free to use and available under the MIT license!

## Why?

There were no serializers that just work in Unity that are free and target all export platforms. [Full Inspector](http://forum.unity3d.com/threads/224270-Full-Inspector-Inspector-and-serialization-for-structs-dicts-generics-interfaces) needed one, so here it is.

## Installation

Import the `Source` folder into your Unity project! You're good to go! (Also see the DLL based import at the bottom of this document).

# Usage

Usage is identical to Unity's default serialization, *except* that you don't have to mark types as `[Serializable]`. Here's an example:

```c#
struct SerializedStruct {
    public int Field;

    public Dictionary<string, string> DictionaryAutoProperty { get; set; }

    [SerializeField]
    private int PrivateField;
}
```

Here are the precise (default) rules:

- Public fields are serialized by default
- Auto-properties that are at least partially public are serialized by default
- All fields or properties annotated with `[SerializeField]` or `[fsProperty]` are serialized
- Public fields/public auto-properties are not serialized if annotated with `[NonSerialized]` or `[fsIgnore]`. `[fsIgnore]` can be used on properties (unlike `[NonSerialized]`).

Inheritance and cyclic object graphs are automatically handled by Full Serializer. You don't need to do anything.

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

# Custom Serialization

Full Serializer allows you to easily customize how serialization happens, via `[fsObject]`, `fsConverter`, and `fsObjectProcessor`.

## Simple Customization with [fsObject]

You can easily customize the serialization of a specific object by utilizing `[fsObject]`. There are a number of options:

You can specify what the default member serialization is by changing the `MemberSerialization` parameter. The options are `OptIn`, `OptOut`, and `Default`. `OptIn` requires that every serialize member be annotated with `fsProperty`, `OptOut` will serialize every member *except* those annotated with `fsIgnore`, and `Default` uses the default intelligent behavior where visibility level and property type are examined.

You can also specify a custom converter or processor to use directly on the model. This is more efficient than registering a custom converter / processor on the `fsSerializer` instance and additionally provides portability w.r.t. the actual `fsSerializer` instance; the `fsSerializer` creator does not need to know about this specific converter / processor.

## Advanced Customization with Converters

Converters (to/from JSON) enable complete customization over the serialization of an object. Each converter expresses interest in what types it wants to take serialization over; there are two methods to do this. The more powerful (but slower) method is present in `fsConverter`, which determines if it interested via a function callback, and `fsDirectConverter`, which directly specifies which type of object it will take over conversion for. The primary limitation for `fsDirectConverter` is that it does not handle inheritance.

### Example

Suppose we have this model:

```c#
public struct Id {
    public int Identifier;
}
```

We want to serialize this `Id` instance directly to an integer. Normally this is difficult, but with converters it is doable.

For example, we want to serialize `new Id { Identifier = 3 }` to `3`.

Let's take a look at the converter which will handle this:

```c#
public class IdConverter : fsDirectConverter {
    public override Type ModelType { get { return typeof(Id); } }

    public override object CreateInstance(fsData data, Type storageType) {
        return new Id();
    }

    public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
        serialized = new fsData(((Id)instance).Identifier);
        return fsResult.Success;
    }

    public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
        if (data.IsInt64 == false) return fsResult.Fail("Expected int in " + data);

        instance = new Id { Identifier = (int)data.AsInt64 };
        return fsResult.Success;
    }
}
```

The converter is fairly straight-forward. `ModelType` maps to the type of object this converter applies to. It is used when you register a converter using either the `fsConverterRegistrar` or `AddConverter`.

`CreateInstance` allocates the actual object instance. If you're curious why this method is separate from `TryDeserialize`, then rest assured knowing that it is because cyclic object graphs require deserializing and object instance allocation to be separated (otherwise deserialization would enter into an infinite loop).

`TrySerialize` serializes the object instance. `instance` is guaranteed to be an instance of `Id` (or rather whatever `CreateInstance` returned), and `storageType` is guaranteed to be equal to `typeof(Id)`.

`TryDeserialize` deserializes the json data into the object instance.

What's up with all of this `fsResult` stuff? Quite simply, `fsResult` is used so that Full Serializer doesn't have to use exceptions. Errors and problems happen when (typically when deserializing) - the `fsResult` instance can contain warning information or error information. If there is an error, then that almost certainly means that you want to stop deserialization, but for a warning you should keep going. You can ignore errors or treat them as warnings if your converter supports partial deserialization.

You may be curious what happens if we try to serialize, say, this object graph:

```c#
class Hello {
    public object field;
}

Serialize(new Hello { field = new Id() });
```

Rest assured knowning that it serializes correctly and as expected, even though we have a custom converter. Full Serializer takes care of these type of details so you don't have to worry about it.

### Converter Registration

There are three ways to register a converter.

- If you have access to the model type itself, then you can simply add an `[fsObject]` annotation. This registration method is static and cannot be modified at runtime. Here's an example:

```c#
[fsObject(Converter = typeof(IdConverter))]
public struct Id {
    public int Identifier;
}
```

- If you don't have access to the model type or the serializer, then you can register the converter using `fsConverterRegistrar`. This registration method is static and cannot be modified at runtime. Here's an example:

```c#
namespace FullSerializer {
    partial class fsConverterRegistrar {
        // Method 1: Via a field
        // Important: The name *must* begin with Register
        public static IdConverter Register_IdConverter;

        // Method 2: Via a method
        // Important: The name *must* begin with Register
        public static void Register_IdConverter() {
            // do something here, ie:
            Converters.Add(typeof(IdConverter));
        }
    }
}
```

- If you have access to the `fsSerializer` instance, then you can dynamically determine which converters to register. For example:

```c#
void CreateSerializer() {
    var serializer = new fsSerializer();
    serializer.AddConverter(new IdConverter());
}
```

### Full Converter Example

Here's the full example:

```c#
using System;
using FullSerializer;

[fsObject(Converter = typeof(IdConverter))]
public struct Id {
    public int Identifier;
}

public class IdConverter : fsDirectConverter {
    public override Type ModelType { get { return typeof(Id); } }

    public override object CreateInstance(fsData data, Type storageType) {
        return new Id();
    }

    public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
        serialized = new fsData(((Id)instance).Identifier);
        return fsResult.Success;
    }

    public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
        if (data.IsInt64 == false) return fsResult.Fail("Expected int in " + data);

        instance = new Id { Identifier = (int)data.AsInt64 };
        return fsResult.Success;
    }
}
```

### Another Converter Example

Here's an example converter, with lots of comments to explain things as you read:

```c#
using System;
using FullSerializer;

// We're going to serialize MyType directly to/from a string.
[fsObject(Converter = typeof(MyTypeConverter))]
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
            return fsResult.Fail("Expected string fsData type but got " + storage.Type);
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

### A Third Converter Example

Let's take a look at this model:

```c#
public class Person {
    public string FirstName;
    public string LastName;
    public int Age;
}
```

We want to serialize it, but we want the serialized data to look like this:

```json
{
    "Name": "John Doe",
    "Age": 25
}
```

which translates to this model instance:

```c#
var person = new Person {
    FirstName = "John",
    LastName = "Doe",
    Age = 25
};
```

Essentially, when we process an instance of `Person`, we want to serialize it to a single string field, ie, concat the first and last name.

Here's the converter:

```c#
using System;
using System.Collections.Generic;
using FullSerializer;

public class PersonConverter : fsDirectConverter<Person> {
    public override object CreateInstance(fsData data, Type storageType) {
        return new Person();
    }

    protected override fsResult DoSerialize(Person model, Dictionary<string, fsData> serialized) {
        // Serialize name manually
        serialized["Name"] = new fsData(model.FirstName + " " + model.LastName);

        // Serialize age using helper methods
        SerializeMember(serialized, null, "Age", model.Age);

        return fsResult.Success;
    }

    protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Person model) {
        var result = fsResult.Success;

        // Deserialize name mainly manually (helper methods CheckKey and CheckType)
        fsData nameData;
        if ((result += CheckKey(data, "Name", out nameData)).Failed) return result;
        if ((result += CheckType(nameData, fsDataType.String)).Failed) return result;
        var names = nameData.AsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (names.Length != 2) return result += fsResult.Fail("Too many names");
        model.FirstName = names[0];
        model.LastName = names[1];

        // Deserialize age using basically only helper methods
        if ((result += DeserializeMember(data, null, "Age", out model.Age)).Failed) return result;

        return result;
    }
}
```

We're using `fsDirectConverter` again, but this time we derived from the generic variant. This generic variant makes writing the converter a bit easier, but it assumes that we will be serializing to a json object (and not, say, a string or number).

As you can see, the converter itself is pretty similar in structure to the last one. Our serialization logic is pretty simple - we are utilizing a few helper methods like `SerializeMember` though. Deserialization is a bit more hairy, since we have to validate a lot of information (since we don't want to throw any exceptions).

Here's the full example:

```c#
using System;
using System.Collections.Generic;
using FullSerializer;

[fsObject(Converter = typeof(PersonConverter))]
public class Person {
    public string FirstName;
    public string LastName;
    public int Age;
}

public class PersonConverter : fsDirectConverter<Person> {
    public override object CreateInstance(fsData data, Type storageType) {
        return new Person();
    }

    protected override fsResult DoSerialize(Person model, Dictionary<string, fsData> serialized) {
        // Serialize name manually
        serialized["Name"] = new fsData(model.FirstName + " " + model.LastName);

        // Serialize age using helper methods
        SerializeMember(serialized, null, "Age", model.Age);

        return fsResult.Success;
    }

    protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Person model) {
        var result = fsResult.Success;

        // Deserialize name mainly manually (helper methods CheckKey and CheckType)
        fsData nameData;
        if ((result += CheckKey(data, "Name", out nameData)).Failed) return result;
        if ((result += CheckType(nameData, fsDataType.String)).Failed) return result;
        var names = nameData.AsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (names.Length != 2) return result += fsResult.Fail("Too many names");
        model.FirstName = names[0];
        model.LastName = names[1];

        // Deserialize age using basically only helper methods
        if ((result += DeserializeMember(data, null, "Age", out model.Age)).Failed) return result;

        return result;
    }
}
```

## Advanced Customization with [fsObjectProcessor]

Object processors give you a bit of control of the serialization process before

Object processors are significantly more straightforward than converters. Here's the API:

### Processor Example

```c#
using System;
using FullSerializer;

public class MyProcessor : fsObjectProcessor {
    public override bool CanProcess(Type type) {
        return true; // process everything that goes through the serializer
        return type == typeof(int); // process only ints
        return typeof(IInterface).IsAssignableFrom(type); // process only types that derive from IInterface
    }

    public override void OnBeforeDeserialize(Type storageType, ref fsData data) {
        // Invoked before deserialization begins. Feel free to modify the data that will be used to deserialize.
    }

    public override void OnAfterDeserialize(Type storageType, object instance) {
        // Invoked after deserialization has finished. Update any state in instance / etc.
    }

    public override void OnBeforeSerialize(Type storageType, object instance) {
        // Invoked before serialization begins. Update any state inside of instance / etc.
    }

    public override void OnAfterSerialize(Type storageType, object instance, ref fsData data) {
        // Invoked after serialization has finished. Update any state inside of instance, modify the output data, etc.
    }
}
```

### Processor Registration

There are two ways to register a processor:

- You can specify it directly on the model. In this case, `CanProcess` will never get invoked (you should probably throw a `NotSupportedException` if it does).

```c#
[fsObject(Processor = typeof(MyProcessor))]
public class MyModel {}
```

- Add the processor to the serializer. `CanProcess` will be invoked on every type that the serializer tries to serialize/deserialize to determine if it is interested in the given type. For this reason, `fsObject` is the preferred method of registration since it is more performant.

```c#
void CreateSerializer() {
    var serializer = new fsSerializer();
    serializer.AddProcessor(new MyProcessor());
}
```

# Versioning

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

    // note: We still should have a default constructor, but since we're a
    // struct one is automatically created for us      
}
```

Notice in particular that we have a constructor on `Model` that accepts an instance of `Model_v1`. If Full Serializer detects that we are deserializing old data, it will first deserialize it into an instance of `Model_v1` and then return a newly constructed instance of `Model` via the `Model_v1` constructor.

All version strings have to be unique (if not, an error will be issued) and there can be no cycles in the versioning import graph (there can be more than one previous model).

We can easily introduce a new `Model` type and then we just rename `Model` to `Model_v2` and Full Serializer will automatically send a `Model_v1` instance through to `Model_v1(deserialized)` -> `Model_v2(Model_v1)` -> `Model(Model_v2)`. Running deserializing this way prevents an explosion of required constructor types.

# AOT Compilation

Full Serializer has introduced some support for automatically creating converters when appropriate. These converters will provide a speedup because they can completely eliminate the usage of reflection. Further, these AOT compiled converters enable usage of Full Serializer on Unity platforms where reflection is broken (ie, consoles), or where reflection requires the full .NET framework target (ie, il2cpp).

The AOT compiled serializers are a bit interesting. As Full Serializer runs serialization and it notices a type can be AOT compiled, it will emit metadata to perform the AOT compilation. After having run some serialization code, you can check `fsAotCompilationManager` to see if there are any available compilations (also see below for a function which will automatically generate AOT compilations for types which contain `[fsProperty]` or `[fsObject]`). If there are AOT compilations available, the output will be in the form of a C# file (stored as a `string`). You should save this file to your project / `Assets` folder.

The following class makes using the AOT system easier. There are two methods, `AddSeenAotCompilations`, and `AddDiscoverableAotCompilations`. `AddSenAotCompilations` will emit AOT compilations for types that the serializer has actually seen and serialized or deserialized (so you're guaranteed to use them), whereas `AddDiscoverableAotCompilations` tries to compile all types which have either a `[fsObject]` or `[fsProperty]` annotation.

```c#
using System;
using System.IO;
using System.Reflection;
using FullSerializer;
using UnityEngine;

public static class AotHelpers {
    public const string OutputDirectory = "Assets/fsAotCompilations/";

    [UnityEditor.MenuItem("FullSerializer/Add Seen Aot Compilations (minimal output)")]
    public static void AddSeenAotCompilations() {
        if (Directory.Exists(OutputDirectory) == false) {
            Directory.CreateDirectory(OutputDirectory);
        }

        foreach (var aot in fsAotCompilationManager.AvailableAotCompilations) {
            Debug.Log("Performing AOT compilation for " + aot.Key.CSharpName(true));
            var path = Path.Combine(OutputDirectory, "AotConverter_" + aot.Key.CSharpName(true, true) + ".cs");
            var compilation = aot.Value;
            File.WriteAllText(path, compilation);
        }
    }

    [UnityEditor.MenuItem("FullSerializer/Add Discoverable Aot Compilations (more output)")]
    public static void AddAllDiscoverableAotCompilations() {
        if (Directory.Exists(OutputDirectory) == false) {
            Directory.CreateDirectory(OutputDirectory);
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (Type t in assembly.GetTypes()) {
                bool performAot = false;

                // check for [fsObject]
                {
                    var props = t.GetCustomAttributes(typeof(fsObjectAttribute), true);
                    if (props != null && props.Length > 0) performAot = true;
                }

                // check for [fsProperty]
                if (!performAot) {
                    foreach (PropertyInfo p in t.GetProperties()) {
                        var props = p.GetCustomAttributes(typeof(fsPropertyAttribute), true);
                        if (props.Length > 0) {
                            performAot = true;
                            break;
                        }
                    }
                }

                if (performAot) {
                    string compilation = null;
                    if (fsAotCompilationManager.TryToPerformAotCompilation(t, out compilation)) {
                        Debug.Log("Performing AOT compilation for " + t);
                        string path = Path.Combine(OutputDirectory, "AotConverter_" + t.CSharpName(true, true) + ".cs");
                        File.WriteAllText(path, compilation);
                    } else {
                        Debug.Log("Failed AOT compilation for " + t.CSharpName(true));
                    }
                }
            }
        }
    }
}
```

**Please note**: If you change a model that has been AOT compiled, the model changes will not be reflected in serialization pipeline until you update the AOT compiled converter. Stale models are not currently detected.

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

# Linker Options

If you're getting strange errors when exporting or running (like missing constructors), Unity might be stripping out Full Serializer methods. You can prevent this with a `link.xml` that looks something like

```xml
<linker>
    <assembly fullname="FullSerializer">
        <namespace fullname="FullSerializer" preserve="all"/>
        <namespace fullname="FullSerializer.Internal" preserve="all"/>
        <namespace fullname="FullSerializer.Internal.DirectConverters" preserve="all"/>
    </assembly>
</linker>
```

# Using Full Serializer in an Asset Store Package

Feel free to use Full Serializer in your own asset store package. If you do so, please rename the Full Serializer namespace to something like MyPackage.FullSerializer so that there will be no conflict if there are multiple versions of Full Serializer installed.

# NPM Support

This project can be installed directly into the `Assets/packages/` folder from github using [npm](https://docs.npmjs.com/getting-started/what-is-npm) via:

    npm init
    npm install jacobdufault/fullserializer --save

# License

Full Serializer is freely available under the MIT license. If you make any improvements, it would be greatly appreciated if you would submit a pull request with them (please match the existing code style).
