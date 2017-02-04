#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer {
    [CreateAssetMenu(menuName = "Full Serializer AOT Configuration")]
    public class fsAotConfiguration : ScriptableObject {
        public enum AotState {
            Default, Enabled, Disabled
        }

        [Serializable]
        public struct Entry {
            public AotState State;
            public string FullTypeName;

            public Entry(Type type) {
                FullTypeName = type.FullName;
                State = AotState.Default;
            }

            public Entry(Type type, AotState state) {
                FullTypeName = type.FullName;
                State = state;
            }
        }
		public List<Entry> aotTypes = new List<Entry>();
        public string outputDirectory = "Assets/AotModels";

        public bool TryFindEntry(Type type, out Entry result) {
            string searchFor = type.FullName;
            foreach (Entry entry in aotTypes) {
                if (entry.FullTypeName == searchFor) {
                    result = entry;
                    return true;
                }
            }

            result = default(Entry);
            return false;
        }

        public void UpdateOrAddEntry(Entry entry) {
            for (int i = 0; i < aotTypes.Count; ++i) {
                if (aotTypes[i].FullTypeName == entry.FullTypeName) {
                    aotTypes[i] = entry;
                    return;
                }
            }

            aotTypes.Add(entry);
        }
	}
}
#endif