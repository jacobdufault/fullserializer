using FullSerializer.Internal;

namespace FullSerializer {
    /// <summary>
    /// Version information stored on an AOT model. This is used to determine
    /// if the AOT model is up to date.
    /// </summary>
    public struct fsAotVersionInfo {
        public struct Member {
            public string MemberName;
            public string JsonName;
            public string StorageType;
            public string OverrideConverterType;

            public Member(fsMetaProperty property) {
                MemberName = property.MemberName;
                JsonName = property.JsonName;
                StorageType = property.StorageType.CSharpName(true);
                OverrideConverterType = null;
                if (property.OverrideConverterType != null)
                    OverrideConverterType = property.OverrideConverterType.CSharpName();
            }

            public override bool Equals(object obj) {
                if (obj is Member == false)
                    return false;
                return this == ((Member)obj);
            }
            public override int GetHashCode() {
                return
                    MemberName.GetHashCode() +
                    (17 * JsonName.GetHashCode()) +
                    (17 * StorageType.GetHashCode()) +
                    (string.IsNullOrEmpty(OverrideConverterType) ? 0 : 17 * OverrideConverterType.GetHashCode());
            }
            public static bool operator ==(Member a, Member b) {
                return a.MemberName == b.MemberName &&
                       a.JsonName == b.JsonName &&
                       a.StorageType == b.StorageType &&
                       a.OverrideConverterType == b.OverrideConverterType;
            }
            public static bool operator !=(Member a, Member b) {
                return !(a == b);
            }
        }

        public bool IsConstructorPublic;
        public Member[] Members;
    }


}