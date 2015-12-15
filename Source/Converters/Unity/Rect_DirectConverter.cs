#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.Rect_DirectConverter Register_Rect_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class Rect_DirectConverter : fsDirectConverter<Rect> {
        protected override fsResult DoSerialize(Rect model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, "xMin", model.xMin);
            result += SerializeMember(serialized, "yMin", model.yMin);
            result += SerializeMember(serialized, "xMax", model.xMax);
            result += SerializeMember(serialized, "yMax", model.yMax);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Rect model) {
            var result = fsResult.Success;

            var t0 = model.xMin;
            result += DeserializeMember(data, "xMin", out t0);
            model.xMin = t0;

            var t1 = model.yMin;
            result += DeserializeMember(data, "yMin", out t1);
            model.yMin = t1;

            var t2 = model.xMax;
            result += DeserializeMember(data, "xMax", out t2);
            model.xMax = t2;

            var t3 = model.yMax;
            result += DeserializeMember(data, "yMax", out t3);
            model.yMax = t3;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new Rect();
        }
    }
}
#endif