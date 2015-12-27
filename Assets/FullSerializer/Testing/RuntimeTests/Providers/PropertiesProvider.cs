using System;
using System.Collections.Generic;
using FullSerializer;

public class PropertiesProvider : TestProvider<object> {
    public struct PublicGetPublicSet {
        public PublicGetPublicSet(int value) : this() { Value = value; }
        public int Value { get; set; }
    }

    public struct PrivateGetPublicSet {
        public PrivateGetPublicSet(int value) : this() { Value = value; }
        [fsProperty]
        public int Value { private get; set; }

        public static bool Compare(PrivateGetPublicSet a, PrivateGetPublicSet b) {
            return a.Value == b.Value;
        }
    }

    public struct PublicGetPrivateSet {
        public PublicGetPrivateSet(int value) : this() { Value = value; }
        public int Value { get; private set; }
    }

    public struct PrivateGetPrivateSet {
        public PrivateGetPrivateSet(int value) : this() { Value = value; }
        private int Value { get; set; }

        public bool Verify() {
            if (Value != 0) {
                throw new Exception("Private autoproperty was deserialized");
            }

            return true;
        }
    }

    public override bool Compare(object before, object after) {
        if (before is PublicGetPublicSet) {
            var beforeA = (PublicGetPublicSet)before;
            var afterA = (PublicGetPublicSet)after;

            return beforeA.Value == afterA.Value;
        }

        if (before is PrivateGetPublicSet) {
            var beforeA = (PrivateGetPublicSet)before;
            var afterA = (PrivateGetPublicSet)after;

            return PrivateGetPublicSet.Compare(beforeA, afterA);
        }

        if (before is PublicGetPrivateSet) {
            var beforeA = (PublicGetPrivateSet)before;
            var afterA = (PublicGetPrivateSet)after;

            return beforeA.Value == afterA.Value;
        }

        if (after is PrivateGetPrivateSet) {
            return ((PrivateGetPrivateSet)after).Verify();
        }

        throw new Exception("Unknown type");
    }

    public override IEnumerable<object> GetValues() {
        for (int i = -1; i <= 1; ++i) {
            yield return new PublicGetPublicSet(i);
            yield return new PrivateGetPublicSet(i);
            yield return new PublicGetPrivateSet(i);
            yield return new PrivateGetPrivateSet(i);
        }
    }
}
