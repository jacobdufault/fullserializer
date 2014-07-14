using System;
namespace FullSerializer {
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

        public fsObjectAttribute() { }
        public fsObjectAttribute(string versionString, params Type[] previousModels) {
            VersionString = versionString;
            PreviousModels = previousModels;
        }
    }
}