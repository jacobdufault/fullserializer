#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.AnimationCurve_DirectConverter Register_AnimationCurve_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class AnimationCurve_DirectConverter : fsDirectConverter<AnimationCurve> {
        protected override fsResult DoSerialize(AnimationCurve model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "keys", model.keys);
            result += SerializeMember(serialized, null, "preWrapMode", model.preWrapMode);
            result += SerializeMember(serialized, null, "postWrapMode", model.postWrapMode);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref AnimationCurve model) {
            var result = fsResult.Success;

            var t0 = model.keys;
            result += DeserializeMember(data, null, "keys", out t0);
            model.keys = t0;

            var t1 = model.preWrapMode;
            result += DeserializeMember(data, null, "preWrapMode", out t1);
            model.preWrapMode = t1;

            var t2 = model.postWrapMode;
            result += DeserializeMember(data, null, "postWrapMode", out t2);
            model.postWrapMode = t2;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new AnimationCurve();
        }
    }
}
#endif