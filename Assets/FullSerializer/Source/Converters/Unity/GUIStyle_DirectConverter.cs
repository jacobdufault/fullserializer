#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    partial class fsConverterRegistrar {
        public static Internal.DirectConverters.GUIStyle_DirectConverter Register_GUIStyle_DirectConverter;
    }
}

namespace FullSerializer.Internal.DirectConverters {
    public class GUIStyle_DirectConverter : fsDirectConverter<GUIStyle> {
        protected override fsResult DoSerialize(GUIStyle model, Dictionary<string, fsData> serialized) {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "active", model.active);
            result += SerializeMember(serialized, null, "alignment", model.alignment);
            result += SerializeMember(serialized, null, "border", model.border);
            result += SerializeMember(serialized, null, "clipping", model.clipping);
            result += SerializeMember(serialized, null, "contentOffset", model.contentOffset);
            result += SerializeMember(serialized, null, "fixedHeight", model.fixedHeight);
            result += SerializeMember(serialized, null, "fixedWidth", model.fixedWidth);
            result += SerializeMember(serialized, null, "focused", model.focused);
            result += SerializeMember(serialized, null, "font", model.font);
            result += SerializeMember(serialized, null, "fontSize", model.fontSize);
            result += SerializeMember(serialized, null, "fontStyle", model.fontStyle);
            result += SerializeMember(serialized, null, "hover", model.hover);
            result += SerializeMember(serialized, null, "imagePosition", model.imagePosition);
            result += SerializeMember(serialized, null, "margin", model.margin);
            result += SerializeMember(serialized, null, "name", model.name);
            result += SerializeMember(serialized, null, "normal", model.normal);
            result += SerializeMember(serialized, null, "onActive", model.onActive);
            result += SerializeMember(serialized, null, "onFocused", model.onFocused);
            result += SerializeMember(serialized, null, "onHover", model.onHover);
            result += SerializeMember(serialized, null, "onNormal", model.onNormal);
            result += SerializeMember(serialized, null, "overflow", model.overflow);
            result += SerializeMember(serialized, null, "padding", model.padding);
            result += SerializeMember(serialized, null, "richText", model.richText);
            result += SerializeMember(serialized, null, "stretchHeight", model.stretchHeight);
            result += SerializeMember(serialized, null, "stretchWidth", model.stretchWidth);
            result += SerializeMember(serialized, null, "wordWrap", model.wordWrap);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref GUIStyle model) {
            var result = fsResult.Success;

            var t0 = model.active;
            result += DeserializeMember(data, null, "active", out t0);
            model.active = t0;

            var t2 = model.alignment;
            result += DeserializeMember(data, null, "alignment", out t2);
            model.alignment = t2;

            var t3 = model.border;
            result += DeserializeMember(data, null, "border", out t3);
            model.border = t3;

            var t4 = model.clipping;
            result += DeserializeMember(data, null, "clipping", out t4);
            model.clipping = t4;

            var t5 = model.contentOffset;
            result += DeserializeMember(data, null, "contentOffset", out t5);
            model.contentOffset = t5;

            var t6 = model.fixedHeight;
            result += DeserializeMember(data, null, "fixedHeight", out t6);
            model.fixedHeight = t6;

            var t7 = model.fixedWidth;
            result += DeserializeMember(data, null, "fixedWidth", out t7);
            model.fixedWidth = t7;

            var t8 = model.focused;
            result += DeserializeMember(data, null, "focused", out t8);
            model.focused = t8;

            var t9 = model.font;
            result += DeserializeMember(data, null, "font", out t9);
            model.font = t9;

            var t10 = model.fontSize;
            result += DeserializeMember(data, null, "fontSize", out t10);
            model.fontSize = t10;

            var t11 = model.fontStyle;
            result += DeserializeMember(data, null, "fontStyle", out t11);
            model.fontStyle = t11;

            var t12 = model.hover;
            result += DeserializeMember(data, null, "hover", out t12);
            model.hover = t12;

            var t13 = model.imagePosition;
            result += DeserializeMember(data, null, "imagePosition", out t13);
            model.imagePosition = t13;

            var t16 = model.margin;
            result += DeserializeMember(data, null, "margin", out t16);
            model.margin = t16;

            var t17 = model.name;
            result += DeserializeMember(data, null, "name", out t17);
            model.name = t17;

            var t18 = model.normal;
            result += DeserializeMember(data, null, "normal", out t18);
            model.normal = t18;

            var t19 = model.onActive;
            result += DeserializeMember(data, null, "onActive", out t19);
            model.onActive = t19;

            var t20 = model.onFocused;
            result += DeserializeMember(data, null, "onFocused", out t20);
            model.onFocused = t20;

            var t21 = model.onHover;
            result += DeserializeMember(data, null, "onHover", out t21);
            model.onHover = t21;

            var t22 = model.onNormal;
            result += DeserializeMember(data, null, "onNormal", out t22);
            model.onNormal = t22;

            var t23 = model.overflow;
            result += DeserializeMember(data, null, "overflow", out t23);
            model.overflow = t23;

            var t24 = model.padding;
            result += DeserializeMember(data, null, "padding", out t24);
            model.padding = t24;

            var t25 = model.richText;
            result += DeserializeMember(data, null, "richText", out t25);
            model.richText = t25;

            var t26 = model.stretchHeight;
            result += DeserializeMember(data, null, "stretchHeight", out t26);
            model.stretchHeight = t26;

            var t27 = model.stretchWidth;
            result += DeserializeMember(data, null, "stretchWidth", out t27);
            model.stretchWidth = t27;

            var t28 = model.wordWrap;
            result += DeserializeMember(data, null, "wordWrap", out t28);
            model.wordWrap = t28;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new GUIStyle();
        }
    }
}
#endif