using System;
using System.Collections.Generic;
using System.Text;
using FullSerializer.Internal;

namespace FullSerializer {
    /// <summary>
    /// The AOT compilation manager
    /// </summary>
    public class fsAotCompilationManager {
        private static bool HasMember(fsAotVersionInfo versionInfo, fsAotVersionInfo.Member member) {
            foreach (fsAotVersionInfo.Member foundMember in versionInfo.Members) {
                if (foundMember == member)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given aotModel can be used. Returns false if it needs to
        /// be recompiled.
        /// </summary>
        public static bool IsAotModelUpToDate(fsMetaType currentModel, fsIAotConverter aotModel) {
            if (currentModel.IsDefaultConstructorPublic != aotModel.VersionInfo.IsConstructorPublic)
                return false;

            if (currentModel.Properties.Length != aotModel.VersionInfo.Members.Length)
                return false;

            foreach (fsMetaProperty property in currentModel.Properties) {
                if (HasMember(aotModel.VersionInfo, new fsAotVersionInfo.Member(property)) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ahead of time compilations that are available. The type maps to the
        /// object type the generated converter will serialize/deserialize, and
        /// the string is the text content for a converter that will do the
        /// serialization.
        /// <para/>
        /// The generated serializer is completely independent and you don't need
        /// to do anything. Simply add the file to your project and it'll get
        /// used instead of the reflection based one.
        /// </summary>
        public static string RunAotCompilationForType(fsConfig config, Type type) {
            fsMetaType metatype = fsMetaType.Get(config, type);
            metatype.EmitAotData(/*throwException:*/ true);
            return GenerateDirectConverterForTypeInCSharp(type, metatype.Properties, metatype.IsDefaultConstructorPublic);
        }

        /// <summary>
        /// A set of types which should be considered for AOT compilation. This
        /// set is populated by the reflected converter.
        /// </summary>
        public static HashSet<Type> AotCandidateTypes = new HashSet<Type>();

        private static string EmitVersionInfo(string prefix, Type type, fsMetaProperty[] members, bool isConstructorPublic) {
            var sb = new StringBuilder();

            sb.AppendLine("new fsAotVersionInfo {");
            sb.AppendLine(prefix + "    IsConstructorPublic = " + (isConstructorPublic ? "true" : "false") + ",");
            sb.AppendLine(prefix + "    Members = new fsAotVersionInfo.Member[] {");
            foreach (fsMetaProperty member in members) {
                sb.AppendLine(prefix + "        new fsAotVersionInfo.Member {");
                sb.AppendLine(prefix + "            MemberName = \"" + member.MemberName + "\",");
                sb.AppendLine(prefix + "            JsonName = \"" + member.JsonName + "\",");
                sb.AppendLine(prefix + "            StorageType = \"" + member.StorageType.CSharpName(true) + "\",");
                if (member.OverrideConverterType != null)
                    sb.AppendLine(prefix + "            OverrideConverterType = \"" + member.OverrideConverterType.CSharpName(true) + "\",");
                sb.AppendLine(prefix + "        },");
            }
            sb.AppendLine(prefix + "    }");
            sb.Append(prefix + "}");

            return sb.ToString();
        }

        private static string GetConverterString(fsMetaProperty member) {
            if (member.OverrideConverterType == null)
                return "null";

            return string.Format("typeof({0})",
                                 member.OverrideConverterType.CSharpName(/*includeNamespace:*/ true));
        }

        public static string GetQualifiedConverterNameForType(Type type) {
            return "FullSerializer.Speedup." + type.CSharpName(true, true) + "_DirectConverter";
        }

        /// <summary>
        /// AOT compiles the object (in C#).
        /// </summary>
        private static string GenerateDirectConverterForTypeInCSharp(Type type, fsMetaProperty[] members, bool isConstructorPublic) {
            var sb = new StringBuilder();
            string typeName = type.CSharpName(/*includeNamespace:*/ true);
            string typeNameSafeDecl = type.CSharpName(true, true);

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace FullSerializer {");
            sb.AppendLine("    partial class fsConverterRegistrar {");
            sb.AppendLine("        public static Speedup." + typeNameSafeDecl + "_DirectConverter " + "Register_" + typeNameSafeDecl + ";");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("namespace FullSerializer.Speedup {");
            sb.AppendLine("    public class " + typeNameSafeDecl + "_DirectConverter : fsDirectConverter<" + typeName + ">, fsIAotConverter {");
            sb.AppendLine("        private fsAotVersionInfo _versionInfo = " + EmitVersionInfo("        ", type, members, isConstructorPublic) + ";");
            sb.AppendLine("        fsAotVersionInfo fsIAotConverter.VersionInfo { get { return _versionInfo; } }");
            sb.AppendLine();
            sb.AppendLine("        protected override fsResult DoSerialize(" + typeName + " model, Dictionary<string, fsData> serialized) {");
            sb.AppendLine("            var result = fsResult.Success;");
            sb.AppendLine();
            foreach (var member in members) {
                sb.AppendLine("            result += SerializeMember(serialized, " + GetConverterString(member) + ", \"" + member.JsonName + "\", model." + member.MemberName + ");");
            }
            sb.AppendLine();
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref " + typeName + " model) {");
            sb.AppendLine("            var result = fsResult.Success;");
            sb.AppendLine();
            for (int i = 0; i < members.Length; ++i) {
                var member = members[i];
                sb.AppendLine("            var t" + i + " = model." + member.MemberName + ";");
                sb.AppendLine("            result += DeserializeMember(data, " + GetConverterString(member) + ", \"" + member.JsonName + "\", out t" + i + ");");
                sb.AppendLine("            model." + member.MemberName + " = t" + i + ";");
                sb.AppendLine();
            }
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override object CreateInstance(fsData data, Type storageType) {");
            if (isConstructorPublic) {
                sb.AppendLine("            return new " + typeName + "();");
            }
            else {
                sb.AppendLine("            return Activator.CreateInstance(typeof(" + typeName + "), /*nonPublic:*/true);");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}