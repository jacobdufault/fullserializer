using System;
using System.Collections.Generic;

namespace FullSerializer.Internal {
    public class fsCyclicReferenceManager {
        private Dictionary<object, int> _objectIds = new Dictionary<object, int>();
        private int _nextId;

        private Dictionary<int, object> _marked = new Dictionary<int, object>();
        private int _depth;

        public void Enter() {
            _depth++;
        }

        public bool Exit() {
            _depth--;

            if (_depth == 0) {
                _objectIds = new Dictionary<object, int>();
                _nextId = 0;
                _marked = new Dictionary<int, object>();
            }

            if (_depth < 0) {
                _depth = 0;
                throw new InvalidOperationException("Internal Error - Mismatched Enter/Exit");
            }

            return _depth == 0;
        }

        public object GetReferenceObject(int id) {
            if (_marked.ContainsKey(id) == false) {
                throw new InvalidOperationException("Internal Deserialization Error - Object " +
                    "definition has not been encountered for object with id=" + id +
                    "; have you reordered or modified the serialized data? If this is an issue " +
                    "with an unmodified Full Json implementation and unmodified serialization " +
                    "data, please report an issue with an included test case.");
            }

            return _marked[id];
        }

        public void AddReferenceWithId(int id, object reference) {
            _marked[id] = reference;
        }

        public int GetReferenceId(object item) {
            int id;
            if (_objectIds.TryGetValue(item, out id) == false) {
                id = _nextId++;
                _objectIds[item] = id;
            }
            return id;
        }

        public bool IsReference(object item) {
            return _marked.ContainsKey(GetReferenceId(item));
        }

        public void MarkSerialized(object item) {
            int referenceId = GetReferenceId(item);

            if (_marked.ContainsKey(referenceId)) {
                throw new InvalidOperationException("Internal Error - " + item +
                    " has already been marked as serialized");
            }

            _marked[referenceId] = item;
        }
    }
}