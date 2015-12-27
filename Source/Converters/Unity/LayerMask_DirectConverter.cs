#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.LayerMask_DirectConverter Register_LayerMask_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class LayerMask_DirectConverter : fsDirectConverter<LayerMask> {
        protected override fsResult DoSerialize(LayerMask model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "value", model.value);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref LayerMask model) {
            var result = fsResult.Success;

            var t0 = model.value;
            result += DeserializeMember(data, null, "value", out t0);
            model.value = t0;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new LayerMask();
        }
    }
}
#endif