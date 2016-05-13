using System;
using System.Globalization;

namespace FullSerializer.Internal {
    /// <summary>
    /// Supports serialization for DateTime, DateTimeOffset, and TimeSpan.
    /// </summary>
    public class fsDateConverter : fsConverter {
        // The format strings that we use when serializing DateTime and
        // DateTimeOffset types.
        private const string DefaultDateTimeFormatString = @"o";
        private const string DateTimeOffsetFormatString = @"o";

        private string DateTimeFormatString {
            get {
                return Serializer.Config.CustomDateTimeFormatString ?? DefaultDateTimeFormatString;
            }
        }

        public override bool CanProcess(Type type) {
            return
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan);
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            if (instance is DateTime) {
                var dateTime = (DateTime)instance;
                if(Serializer.Config.SerializeDateTimeAsInteger) {
                    serialized = new fsData(dateTime.Ticks);
                } else {
                    serialized = new fsData(dateTime.ToString(DateTimeFormatString));
                    return fsResult.Success;
                }
                return fsResult.Success;
            }

            if (instance is DateTimeOffset) {
                var dateTimeOffset = (DateTimeOffset)instance;
                serialized = new fsData(dateTimeOffset.Ticks);
                return fsResult.Success;
            }

            if (instance is TimeSpan) {
                var timeSpan = (TimeSpan)instance;
                if(Serializer.Config.SerializeDateTimeAsInteger) {
                    serialized = new fsData(timeSpan.Ticks);
                } else {
                    serialized = new fsData(timeSpan.ToString());
                }
                return fsResult.Success;
            }

            throw new InvalidOperationException("FullSerializer Internal Error -- Unexpected serialization type");
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString == false && (data.IsInt64 == false || Serializer.Config.SerializeDateTimeAsInteger == false || instance is DateTimeOffset)) {
                return fsResult.Fail("Date deserialization requires a string or int, not " + data.Type);
            }

            if (storageType == typeof(DateTime)) {
                if (Serializer.Config.SerializeDateTimeAsInteger && data.IsInt64) {
                    instance = new DateTime(data.AsInt64);
                    return fsResult.Success;
                }

                DateTime result;
                if (DateTime.TryParse(data.AsString, null, DateTimeStyles.RoundtripKind, out result)) {
                    instance = result;
                    return fsResult.Success;
                }

                // DateTime.TryParse can fail for some valid DateTime instances.
                // Try to use Convert.ToDateTime.
                if (fsGlobalConfig.AllowInternalExceptions) {
                    try {
                        instance = Convert.ToDateTime(data.AsString);
                        return fsResult.Success;
                    }
                    catch (Exception e) {
                        return fsResult.Fail("Unable to parse " + data.AsString + " into a DateTime; got exception " + e);
                    }
                }

                return fsResult.Fail("Unable to parse " + data.AsString + " into a DateTime");
            }

            if (storageType == typeof(DateTimeOffset)) {
                DateTimeOffset result;
                if (DateTimeOffset.TryParse(data.AsString, null, DateTimeStyles.RoundtripKind, out result)) {
                    instance = result;
                    return fsResult.Success;
                }

                return fsResult.Fail("Unable to parse " + data.AsString + " into a DateTimeOffset");
            }

            if (storageType == typeof(TimeSpan)) {
                if (Serializer.Config.SerializeDateTimeAsInteger && data.IsInt64) {
                    instance = new TimeSpan(data.AsInt64);
                    return fsResult.Success;
                }

                TimeSpan result;
                if (TimeSpan.TryParse(data.AsString, out result)) {
                    instance = result;
                    return fsResult.Success;
                }

                return fsResult.Fail("Unable to parse " + data.AsString + " into a TimeSpan");
            }

            throw new InvalidOperationException("FullSerializer Internal Error -- Unexpected deserialization type");
        }
    }
}