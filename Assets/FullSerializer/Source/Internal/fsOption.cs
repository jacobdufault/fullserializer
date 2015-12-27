using System;

namespace FullSerializer.Internal {
    /// <summary>
    /// Simple option type. This is akin to nullable types.
    /// </summary>
    public struct fsOption<T> {
        private bool _hasValue;
        private T _value;

        public bool HasValue {
            get { return _hasValue; }
        }
        public bool IsEmpty {
            get { return _hasValue == false; }
        }
        public T Value {
            get {
                if (IsEmpty) throw new InvalidOperationException("fsOption is empty");
                return _value;
            }
        }

        public fsOption(T value) {
            _hasValue = true;
            _value = value;
        }

        public static fsOption<T> Empty;
    }

    public static class fsOption {
        public static fsOption<T> Just<T>(T value) {
            return new fsOption<T>(value);
        }
    }
}