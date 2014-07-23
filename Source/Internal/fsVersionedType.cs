using System;

namespace FullSerializer.Internal {
    public struct fsVersionedType {
        /// <summary>
        /// The direct ancestors that this type can import.
        /// </summary>
        public fsVersionedType[] Ancestors;

        /// <summary>
        /// The identifying string that is unique among all ancestors.
        /// </summary>
        public string VersionString;

        /// <summary>
        /// The modeling type that this versioned type maps back to.
        /// </summary>
        public Type ModelType;

        /// <summary>
        /// Migrate from an instance of an ancestor.
        /// </summary>
        public object Migrate(object ancestorInstance) {
            return Activator.CreateInstance(ModelType, ancestorInstance);
        }

        public override string ToString() {
            return "fsVersionedType [ModelType=" + ModelType + ", VersionString=" + VersionString + ", Ancestors.Length=" + Ancestors.Length + "]";
        }

        public static bool operator ==(fsVersionedType a, fsVersionedType b) {
            return a.ModelType == b.ModelType;
        }

        public static bool operator !=(fsVersionedType a, fsVersionedType b) {
            return a.ModelType != b.ModelType;
        }

        public override bool Equals(object obj) {
            return
                obj is fsVersionedType &&
                ModelType == ((fsVersionedType)obj).ModelType;
        }

        public override int GetHashCode() {
            return ModelType.GetHashCode();
        }
    }
}