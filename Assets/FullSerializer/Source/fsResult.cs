using System;
using System.Collections.Generic;
using System.Linq;

namespace FullSerializer {
    /// <summary>
    /// The result of some sort of operation. A result is either successful or not, but if it
    /// is successful then there may be a set of warnings/messages associated with it. These
    /// warnings describe the performed error recovery operations.
    /// </summary>
    public struct fsResult {
        // We cache the empty string array so we can unify some collections processing code.
        private static readonly string[] EmptyStringArray = { };

        /// <summary>
        /// Is this result successful?
        /// </summary>
        /// <remarks>This is intentionally a `success` state so that when the object
        /// is default constructed it defaults to a failure state.</remarks>
        private bool _success;

        /// <summary>
        /// The warning or error messages associated with the result. This may be null if
        /// there are no messages.
        /// </summary>
        private List<string> _messages;

        /// <summary>
        /// Adds a new message to this result.
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(string message) {
            if (_messages == null) {
                _messages = new List<string>();
            }

            _messages.Add(message);
        }

        /// <summary>
        /// Adds only the messages from the other result into this result, ignoring
        /// the success/failure status of the other result.
        /// </summary>
        public void AddMessages(fsResult result) {
            if (result._messages == null) {
                return;
            }

            if (_messages == null) {
                _messages = new List<string>();
            }

            _messages.AddRange(result._messages);
        }

        /// <summary>
        /// Merges the other result into this one. If the other result failed, then
        /// this one too will have failed.
        /// </summary>
        /// <remarks>
        /// Note that you can use += instead of this method so that you don't bury
        /// the actual method call that is generating the other fsResult.
        /// </remarks>
        public fsResult Merge(fsResult other) {
            // Copy success over
            _success = _success && other._success;

            // Copy messages over
            if (other._messages != null) {
                if (_messages == null) _messages = new List<string>(other._messages);
                else _messages.AddRange(other._messages);
            }

            return this;
        }

        /// <summary>
        /// A successful result.
        /// </summary>
        public static fsResult Success = new fsResult {
            _success = true
        };

        /// <summary>
        /// Create a result that is successful but contains the given warning message.
        /// </summary>
        public static fsResult Warn(string warning) {
            return new fsResult {
                _success = true,
                _messages = new List<string> { warning }
            };
        }

        /// <summary>
        /// Create a result that failed.
        /// </summary>
        public static fsResult Fail(string warning) {
            return new fsResult {
                _success = false,
                _messages = new List<string> { warning }
            };
        }

        // TODO: how to make sure this is only used as +=?

        /// <summary>
        /// Only use this as +=!
        /// </summary>
        public static fsResult operator +(fsResult a, fsResult b) {
            return a.Merge(b);
        }

        /// <summary>
        /// Did this result fail? If so, you can see the reasons why in `RawMessages`.
        /// </summary>
        public bool Failed {
            get {
                return _success == false;
            }
        }

        /// <summary>
        /// Was the result a success? Note that even successful operations may have
        /// warning messages (`RawMessages`) associated with them.
        /// </summary>
        public bool Succeeded {
            get {
                return _success;
            }
        }

        /// <summary>
        /// Does this result have any warnings? This says nothing about if it failed
        /// or succeeded, just if it has warning messages associated with it.
        /// </summary>
        public bool HasWarnings {
            get {
                return _messages != null && _messages.Any();
            }
        }

        /// <summary>
        /// A simply utility method that will assert that this result is successful. If it
        /// is not, then an exception is thrown.
        /// </summary>
        public fsResult AssertSuccess() {
            if (Failed) throw AsException;
            return this;
        }

        /// <summary>
        /// A simple utility method that will assert that this result is successful and that
        /// there are no warning messages. This throws an exception if either of those
        /// asserts are false.
        /// </summary>
        public fsResult AssertSuccessWithoutWarnings() {
            if (Failed || RawMessages.Any()) throw AsException;
            return this;
        }

        /// <summary>
        /// Utility method to convert the result to an exception. This method is only defined
        /// is `Failed` returns true.
        /// </summary>
        public Exception AsException {
            get {
                if (!Failed && !RawMessages.Any()) throw new Exception("Only a failed result can be converted to an exception");
                return new Exception(FormattedMessages);
            }
        }

        public IEnumerable<string> RawMessages {
            get {
                if (_messages != null) {
                    return _messages;
                }
                return EmptyStringArray;
            }
        }

        public string FormattedMessages {
            get {
                return string.Join(",\n", RawMessages.ToArray());
            }
        }
    }
}