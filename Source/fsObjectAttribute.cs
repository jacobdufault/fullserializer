using System;

namespace FullSerializer {
    /// <summary>
    /// This attribute controls some serialization behavior for a type. See the comments
    /// on each of the fields for more information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class fsObjectAttribute : Attribute {
        /// <summary>
        /// The previous model that should be used if an old version of this
        /// object is encountered. Using this attribute also requires that the
        /// type have a public constructor that takes only one parameter, an object
        /// instance of the given type. Use of this parameter *requires* that
        /// the VersionString parameter is also set.
        /// </summary>
        public Type[] PreviousModels;

        /// <summary>
        /// The version string to use for this model. This should be unique among all
        /// prior versions of this model that is supported for importation. If PreviousModel
        /// is set, then this attribute must also be set. A good valid example for this
        /// is "v1", "v2", "v3", ...
        /// </summary>
        public string VersionString;

        /// <summary>
        /// This controls the behavior for member serialization.
        /// The default behavior is fsMemberSerialization.Default.
        /// </summary>
        public fsMemberSerialization MemberSerialization = fsMemberSerialization.Default;

        /// <summary>
        /// Specify a custom converter to use for serialization. The converter type needs
        /// to derive from fsBaseConverter. This defaults to null.
        /// </summary>
        public Type Converter;

        /// <summary>
        /// Specify a custom processor to use during serialization. The processor type needs
        /// to derive from fsObjectProcessor and the call to CanProcess is not invoked. This
        /// defaults to null.
        /// </summary>
        public Type Processor;

        public fsObjectAttribute() { }
        public fsObjectAttribute(string versionString, params Type[] previousModels) {
            VersionString = versionString;
            PreviousModels = previousModels;
        }
    }
}