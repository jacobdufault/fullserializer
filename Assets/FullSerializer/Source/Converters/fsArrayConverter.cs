using System;
using System.Collections;

namespace FullSerializer.Internal {
    public class fsArrayConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type.IsArray && type.GetArrayRank() == 1;
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            // note: IList[index] is **significantly** faster than Array.Get, so
            //       make sure we use that instead.

            IList arr = (Array)instance;
            Type elementType = storageType.GetElementType();

            var result = fsResult.Success;

            serialized = fsData.CreateList(arr.Count);
            var serializedList = serialized.AsList;

            for (int i = 0; i < arr.Count; ++i) {
                object item = arr[i];

                fsData serializedItem;

                var itemResult = Serializer.TrySerialize(elementType, item, out serializedItem);
                result.AddMessages(itemResult);
                if (itemResult.Failed) continue;

                serializedList.Add(serializedItem);
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            // Verify that we actually have an List
            if ((result += CheckType(data, fsDataType.Array)).Failed) {
                return result;
            }

            Type elementType = storageType.GetElementType();

            var serializedList = data.AsList;
            var list = new ArrayList(serializedList.Count);
            int existingCount = list.Count;

            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;
                if (i < existingCount) deserialized = list[i];

                var itemResult = Serializer.TryDeserialize(serializedItem, elementType, ref deserialized);
                result.AddMessages(itemResult);
                if (itemResult.Failed) continue;

                if (i < existingCount) list[i] = deserialized;
                else list.Add(deserialized);
            }

            instance = list.ToArray(elementType);
            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(Serializer.Config, storageType).CreateInstance();
        }
    }

    public class fs2DArrayConverter : fsConverter
    {
        public override bool CanProcess(Type type)
        {
            return type.IsArray && type.GetArrayRank() == 2;
        }

        public override bool RequestCycleSupport(Type storageType)
        {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType)
        {
            return false;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            var asArray = (Array)instance;
            IList asIList = asArray;

            Type elementType = storageType.GetElementType();

            var result = fsResult.Success;

            serialized = fsData.CreateDictionary();
            var serializedDict = serialized.AsDictionary;
            var serializedListData = fsData.CreateList(asIList.Count);
            serializedDict.Add("c", new fsData(asArray.GetLength(1)));
            serializedDict.Add("r", new fsData(asArray.GetLength(0)));
            serializedDict.Add("a", serializedListData);
            var serializedList = serializedListData.AsList;

            for(int row = 0; row < asArray.GetLength(0); ++row)
            {
                for(int column = 0; column < asArray.GetLength(1); ++column)
                {
                    object item = asArray.GetValue(row, column);

                    fsData serializedItem;

                    var itemResult = Serializer.TrySerialize(elementType, item, out serializedItem);
                    result.AddMessages(itemResult);
                    if(itemResult.Failed) continue;

                    serializedList.Add(serializedItem);
                }
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            var result = fsResult.Success;

            // Verify that we actually have an Object
            if((result += CheckType(data, fsDataType.Object)).Failed)
            {
                return result;
            }

            Type elementType = storageType.GetElementType();

            var serializedDict = data.AsDictionary;
            int columns;
            if((result += DeserializeMember(serializedDict, null, "c", out columns)).Failed)
            {
                return result;
            }

            int rows;
            if((result += DeserializeMember(serializedDict, null, "r", out rows)).Failed)
            {
                return result;
            }

            fsData flatList;
            if(!serializedDict.TryGetValue("a", out flatList))
            {
                result.AddMessage("Failed to get flattened list");
                return result;
            }

            // Verify that we actually have a List
            if((result += CheckType(flatList, fsDataType.Array)).Failed)
            {
                return result;
            }

            var array = Array.CreateInstance(elementType, rows, columns);
            var serializedList = flatList.AsList;

            if(columns * rows > serializedList.Count)
            {
                result.AddMessage("Serialised list has more items than can fit in multidimensional array");
            }

            for(int row = 0; row < rows; ++row)
            {
                for(int column = 0; column < columns; ++column)
                {
                    var serializedItem = serializedList[column + row * columns];
                    object deserialized = null;

                    var itemResult = Serializer.TryDeserialize(serializedItem, elementType, ref deserialized);
                    result.AddMessages(itemResult);
                    if(itemResult.Failed) continue;

                    // Set the value as if it's a flat array.
                    array.SetValue(deserialized, row, column);
                }
            }

            instance = array;
            return result;
        }

        public override object CreateInstance(fsData data, Type storageType)
        {
            return fsMetaType.Get(Serializer.Config, storageType).CreateInstance();
        }
    }

}