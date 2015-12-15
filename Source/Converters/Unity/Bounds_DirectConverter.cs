#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.Bounds_DirectConverter Register_Bounds_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class Bounds_DirectConverter : fsDirectConverter<Bounds> {
        protected override fsResult DoSerialize(Bounds model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, "center", model.center);
            result += SerializeMember(serialized, "size", model.size);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Bounds model) {
            var result = fsResult.Success;

            var t0 = model.center;
            result += DeserializeMember(data, "center", out t0);
            model.center = t0;

            var t1 = model.size;
            result += DeserializeMember(data, "size", out t1);
            model.size = t1;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new Bounds();
        }
    }
}
#endif