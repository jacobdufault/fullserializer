using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FullSerializer.Internal {
    public class fsCyclicReferenceManager {
        private ObjectIDGenerator _objectIds = new ObjectIDGenerator();
        private Dictionary<long, object> _marked = new Dictionary<long, object>();
        private int _depth;

        public void Enter() {
            _depth++;
        }

        public void Exit() {
            _depth--;

            if (_depth == 0) {
                _objectIds = new ObjectIDGenerator();
                _marked = new Dictionary<long, object>();
            }

            if (_depth < 0) {
                _depth = 0;
                throw new InvalidOperationException("Internal Error - Mismatched Enter/Exit");
            }
        }

        public object GetReferenceObject(long id) {
            if (_marked.ContainsKey(id) == false) {
                throw new InvalidOperationException("Internal Deserialization Error - Object " +
                    "definition has not been encountered for object with id=" + id +
                    "; have you reordered or modified the serialized data? If this is an issue " +
                    "with an unmodified Full Json implementation and unmodified serialization " +
                    "data, please report an issue with an included test case.");
            }

            return _marked[id];
        }

        public void AddReferenceWithId(long id, object reference) {
            _marked[id] = reference;
        }

        public long GetReferenceId(object item) {
            bool firstTime;
            long id = _objectIds.GetId(item, out firstTime);
            return id;
        }

        public bool IsReference(object item) {
            return _marked.ContainsKey(GetReferenceId(item));
        }

        public void MarkSerialized(object item) {
            long referenceId = GetReferenceId(item);

            if (_marked.ContainsKey(referenceId)) {
                throw new InvalidOperationException("Internal Error - " + item +
                    " has already been marked as serialized");
            }

            _marked[referenceId] = item;
        }
    }
}