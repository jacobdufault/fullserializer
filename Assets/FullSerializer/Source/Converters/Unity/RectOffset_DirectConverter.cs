#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.RectOffset_DirectConverter Register_RectOffset_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class RectOffset_DirectConverter : fsDirectConverter<RectOffset> {
        protected override fsResult DoSerialize(RectOffset model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "bottom", model.bottom);
            result += SerializeMember(serialized, null, "left", model.left);
            result += SerializeMember(serialized, null, "right", model.right);
            result += SerializeMember(serialized, null, "top", model.top);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref RectOffset model) {
            var result = fsResult.Success;

            var t0 = model.bottom;
            result += DeserializeMember(data, null, "bottom", out t0);
            model.bottom = t0;

            var t2 = model.left;
            result += DeserializeMember(data, null, "left", out t2);
            model.left = t2;

            var t3 = model.right;
            result += DeserializeMember(data, null, "right", out t3);
            model.right = t3;

            var t4 = model.top;
            result += DeserializeMember(data, null, "top", out t4);
            model.top = t4;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new RectOffset();
        }
    }
}
#endif