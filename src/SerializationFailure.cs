using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullJson {
    public class JsonFailure {
        private bool _success;
        private string _reason;

        private JsonFailure() {
        }

        public static JsonFailure Success = new JsonFailure() {
            _success = true,
            _reason = string.Empty
        };

        public static JsonFailure Fail(string reason) {
            throw new InvalidOperationException("JsonFailure: " + reason);
            /*
            return new JsonFailure() {
                _success = false,
                _reason = reason
            };
            */
        }

        public bool Failed {
            get {
                return _success == false;
            }
        }

        public bool Succeeded {
            get {
                return _success;
            }
        }

        public string FailureReason {
            get {
                if (Succeeded) {
                    throw new InvalidOperationException("Successful operations have no failure " +
                        "reason");
                }

                return _reason;
            }
        }
    }
}