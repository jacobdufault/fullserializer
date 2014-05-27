# Limitations
We've tried hard to prevent any serialization restrictions, but some them of are simply unavoidable. Depending on your export platform, certain features in Full Json may not be available.

If you know of a way to remove any of these limitations, please make sure to bring it up.

## WebPlayer

- All serialized types need to have a default constructor (`FormatterServices.GetUninitializedObject` is not available - `Activator.CreateInstance` must be used instead).
