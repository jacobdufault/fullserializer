namespace FullSerializer {
    /// <summary>
    /// Controls how the reflected converter handles member serialization.
    /// </summary>
    public enum fsMemberSerialization {
        /// <summary>
        /// Only members with [SerializeField] or [fsProperty] attributes are
        /// serialized.
        /// </summary>
        OptIn,

        /// <summary>
        /// Only members with [NotSerialized] or [fsIgnore] will not be
        /// serialized.
        /// </summary>
        OptOut,

        /// <summary>
        /// The default member serialization behavior is applied.
        /// </summary>
        Default
    }
}