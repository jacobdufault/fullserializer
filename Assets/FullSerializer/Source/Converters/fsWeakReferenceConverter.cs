using System;

namespace FullSerializer.Internal {
    /// <summary>
    /// Serializes and deserializes WeakReferences.
    /// </summary>
    public class fsWeakReferenceConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type == typeof(WeakReference);
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            var weakRef = (WeakReference)instance;

            var result = fsResult.Success;
            serialized = fsData.CreateDictionary();

            if (weakRef.IsAlive) {
                fsData data;
                if ((result += Serializer.TrySerialize(weakRef.Target, out data)).Failed) {
                    return result;
                }

                serialized.AsDictionary["Target"] = data;
                serialized.AsDictionary["TrackResurrection"] = new fsData(weakRef.TrackResurrection);
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            if ((result += CheckType(data, fsDataType.Object)).Failed) return result;

            if (data.AsDictionary.ContainsKey("Target")) {
                var targetData = data.AsDictionary["Target"];
                object targetInstance = null;

                if ((result += Serializer.TryDeserialize(targetData, typeof(object), ref targetInstance)).Failed) return result;

                bool trackResurrection = false;
                if (data.AsDictionary.ContainsKey("TrackResurrection") && data.AsDictionary["TrackResurrection"].IsBool) {
                    trackResurrection = data.AsDictionary["TrackResurrection"].AsBool;
                }

                instance = new WeakReference(targetInstance, trackResurrection);
            }

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new WeakReference(null);
        }
    }
}