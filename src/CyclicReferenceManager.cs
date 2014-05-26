using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FullJson.Internal {
    public class CyclicReferenceManager {
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
                throw new InvalidOperationException("Internal Error - Unable to find reference for id = " + id);
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