using FullInspector;
using System;
using System.Collections.Generic;

public class PropertiesProvider : BaseProvider<object> {
    public struct PublicGetPublicSet {
        public PublicGetPublicSet(int value) { Value = value; }
        public int Value { get; set; }
    }

    public struct PrivateGetPublicSet {
        public PrivateGetPublicSet(int value) { Value = value; }
        public int Value { private get; set; }
    }

    public struct PublicGetPrivateSet {
        public PublicGetPrivateSet(int value) { Value = value; }
        public int Value { get; private set; }
    }

    public struct PrivateGetPrivateSet : ICustomCompareRequested {
        public PrivateGetPrivateSet(int value) { Value = value; }
        [ShowInInspector]
        private int Value { get; set; }

        public bool AreEqual(object original) {
            if (Value != 0) {
                throw new Exception("Private autoproperty was deserialized");
            }

            return true;
        }
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