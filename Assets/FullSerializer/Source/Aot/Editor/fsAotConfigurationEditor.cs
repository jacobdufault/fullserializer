#if !NO_UNITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FullSerializer.Internal;
using UnityEditor;
using UnityEngine;

namespace FullSerializer {
	[InitializeOnLoad]
	public static class PlayStateNotifier {
		static PlayStateNotifier() {
			EditorApplication.playmodeStateChanged += ModeChanged;
		}

		private static void ModeChanged () {
			if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying) {
				Debug.Log("There are " + fsAotCompilationManager.AotCandidateTypes.Count + " candidate types");
				foreach (fsAotConfiguration target in Resources.FindObjectsOfTypeAll<fsAotConfiguration>()) {
					var seen = new HashSet<string>(target.aotTypes.Select(t => t.FullTypeName));
					foreach (Type type in fsAotCompilationManager.AotCandidateTypes) {
						if (seen.Contains(type.FullName) == false) {
							target.aotTypes.Add(new fsAotConfiguration.Entry(type));
							EditorUtility.SetDirty(target);
						}
					}
				}
			}
		}
	}

	[CustomEditor(typeof(fsAotConfiguration))]
	public class fsAotConfigurationEditor : Editor {
		[NonSerialized]
		private List<Type> _allAotTypes;
		private List<Type> allAotTypes {
			get {
				if (_allAotTypes == null)
					_allAotTypes = FindAllAotTypes().ToList();
				return _allAotTypes;
			}
		}

		private string[] options = new string[] { "On", "Off", "[?]" };
		private int GetIndexForState(fsAotConfiguration.AotState state) {
			switch (state) {
				case fsAotConfiguration.AotState.Enabled:
					return 0;
				case fsAotConfiguration.AotState.Disabled:
					return 1;
				case fsAotConfiguration.AotState.Default:
					return 2;
			}

			throw new ArgumentException("state is invalid " + state);
		}
		private fsAotConfiguration.AotState GetStateForIndex(int index) {
			switch (index) {
				case 0: return fsAotConfiguration.AotState.Enabled;
				case 1: return fsAotConfiguration.AotState.Disabled;
				case 2: return fsAotConfiguration.AotState.Default;
			}

			throw new ArgumentException("invalid index " + index);
		}

		private IEnumerable<Type> FindAllAotTypes() {
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type t in assembly.GetTypes()) {
					bool performAot = false;

					// check for [fsObject]
					{
						var props = t.GetCustomAttributes(typeof(fsObjectAttribute), true);
						if (props != null && props.Length > 0) performAot = true;
					}

					// check for [fsProperty]
					if (!performAot) {
						foreach (PropertyInfo p in t.GetProperties()) {
							var props = p.GetCustomAttributes(typeof(fsPropertyAttribute), true);
							if (props.Length > 0) {
								performAot = true;
								break;
							}
						}
					}

					if (performAot)
						yield return t;
				}
			}
		}

		private enum OutOfDateResult {
			NoAot,
			Stale,
			Current
		}
		private OutOfDateResult IsOutOfDate(Type type) {
			string converterName = fsAotCompilationManager.GetQualifiedConverterNameForType(type);
			Type converterType = fsTypeCache.GetType(converterName);
			if (converterType == null)
				return OutOfDateResult.NoAot;

			// TODO: We should also verify that the type is contained inside of fsConverterRegistrar as
			//       an additional diagnostic. If it is not, then that means the type will not be used
			//       at runtime.

			object instance_ = null;
			try {
				instance_ = Activator.CreateInstance(converterType);
			} catch (Exception) {}
			if (instance_ is fsIAotConverter == false)
				return OutOfDateResult.NoAot;
			var instance = (fsIAotConverter)instance_;

			var metatype = fsMetaType.Get(new fsConfig(), type);
			if (fsAotCompilationManager.IsAotModelUpToDate(metatype, instance) == false)
				return OutOfDateResult.Stale;

			return OutOfDateResult.Current;
		}

		private void DrawType(fsAotConfiguration.Entry entry, Type resolvedType) {
			var target = (fsAotConfiguration)this.target;

			EditorGUILayout.BeginHorizontal();

			int currentIndex = GetIndexForState(entry.State);
			int newIndex = GUILayout.Toolbar(currentIndex, options, GUILayout.ExpandWidth(false));
			if (currentIndex != newIndex) {
				entry.State = GetStateForIndex(newIndex);
				target.UpdateOrAddEntry(entry);
				EditorUtility.SetDirty(target);
			}

			string displayName = entry.FullTypeName;
			string tooltip = "";
			if (resolvedType != null) {
				displayName = resolvedType.CSharpName();
				tooltip = resolvedType.CSharpName(true);
			}
			GUIContent label = new GUIContent(displayName, tooltip);
			GUILayout.Label(label);

			GUILayout.FlexibleSpace();

			GUIStyle messageStyle = new GUIStyle(EditorStyles.label);
			string message;
			if (resolvedType != null) {
				message = GetAotCompilationMessage(resolvedType);
				if (string.IsNullOrEmpty(message) == false) {
					messageStyle.normal.textColor = Color.red;
				} else {
					switch (IsOutOfDate(resolvedType)) {
						case OutOfDateResult.NoAot:
							message = "No AOT model found";
							break;
						case OutOfDateResult.Stale:
							message = "Stale";
							break;
						case OutOfDateResult.Current:
							message = "\u2713";
							break;
					}
				}
			} else {
				message = "Cannot load type";
			}

			GUILayout.Label(message, messageStyle);

			EditorGUILayout.EndHorizontal();
		}

		private string GetAotCompilationMessage(Type type) {
			try {
				fsMetaType.Get(new fsConfig(), type).EmitAotData(true);
			} catch (Exception e) {
				return e.Message;
			}
			return "";
		}

		private Vector2 _scrollPos;
		public override void OnInspectorGUI() {
			var target = (fsAotConfiguration)this.target;

			if (GUILayout.Button("Compile")) {
				if (Directory.Exists(target.outputDirectory) == false)
					Directory.CreateDirectory(target.outputDirectory);

				foreach (fsAotConfiguration.Entry entry in target.aotTypes) {
					if (entry.State == fsAotConfiguration.AotState.Enabled) {
						Type resolvedType = fsTypeCache.GetType(entry.FullTypeName);
						if (resolvedType == null) {
							Debug.LogError("Cannot find type " + entry.FullTypeName);
							continue;
						}

						try {
							string compilation = fsAotCompilationManager.RunAotCompilationForType(new fsConfig(), resolvedType);
							string path = Path.Combine(target.outputDirectory, "AotConverter_" + resolvedType.CSharpName(true, true) + ".cs");
							File.WriteAllText(path, compilation);
						} catch (Exception e) {
							Debug.LogWarning("AOT compiling " + resolvedType.CSharpName(true) + " failed: " + e.Message);
						}
					}
				}
				AssetDatabase.Refresh();
			}

			target.outputDirectory = EditorGUILayout.TextField("Output Directory", target.outputDirectory);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Set All");
			int newIndex = GUILayout.Toolbar(-1, options, GUILayout.ExpandWidth(false));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			if (newIndex != -1) {
				var newState = fsAotConfiguration.AotState.Default;
				if (newIndex == 0)
					newState = fsAotConfiguration.AotState.Enabled;
				else if (newIndex == 1)
					newState = fsAotConfiguration.AotState.Disabled;

				for (int i = 0; i < target.aotTypes.Count; ++i) {
					var entry = target.aotTypes[i];
					entry.State = newState;
					target.aotTypes[i] = entry;
				}
			}


			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			foreach (fsAotConfiguration.Entry entry in target.aotTypes) {
				Type resolvedType = fsTypeCache.GetType(entry.FullTypeName);
				EditorGUI.BeginDisabledGroup(resolvedType == null || string.IsNullOrEmpty(GetAotCompilationMessage(resolvedType)) == false);
				DrawType(entry, resolvedType);
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();
		}

	}
}
#endif