using System;
using System.Collections.Generic;
using FullSerializer.Internal;

namespace FullSerializer {
    /// <summary>
    /// This class allows arbitrary code to easily register global converters. To add a converter,
    /// simply declare a new field called "Register_*" that stores the type of converter you would
    /// like to add. Alternatively, you can do the same with a method called "Register_*"; just add
    /// the converter type to the `Converters` list.
    /// </summary>
    public partial class fsConverterRegistrar {
        static fsConverterRegistrar() {
            Converters = new List<Type>();

            foreach (var field in typeof(fsConverterRegistrar).GetDeclaredFields()) {
                if (field.Name.StartsWith("Register_")) Converters.Add(field.FieldType);
            }

            foreach (var method in typeof(fsConverterRegistrar).GetDeclaredMethods()) {
                if (method.Name.StartsWith("Register_")) method.Invoke(null, null);
            }

        }

        public static List<Type> Converters;

        // Example field registration:
        //public static AnimationCurve_DirectConverter Register_AnimationCurve_DirectConverter;

        // Example method registration:
        //public static void Register_AnimationCurve_DirectConverter() {
        //    Converters.Add(typeof(AnimationCurve_DirectConverter));
        //}
    }
}