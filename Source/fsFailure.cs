using System;

namespace FullSerializer {
    public class fsFailure {
        private bool _success;
        private string _reason;

        private fsFailure() {
        }

        public static fsFailure Success = new fsFailure() {
            _success = true,
            _reason = string.Empty
        };

        public static fsFailure Fail(string reason) {
            return new fsFailure() {
                _success = false,
                _reason = reason
            };
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
                    throw new InvalidOperationException("Successful operations have no failure reason");
                }

                return _reason;
            }
        }
    }
}