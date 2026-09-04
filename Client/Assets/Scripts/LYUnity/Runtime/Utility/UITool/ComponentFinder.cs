using System;
using System.Collections.Generic;
using UnityEngine;

namespace LYUnity.UITool
{
    public class ComponentFinder : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct ComponentEntry
        {
            public string Key;
            public GameObject Value;
        }

        [SerializeField, HideInInspector]
        private List<ComponentEntry> m_SerializedComponents = new();

        private Dictionary<string, GameObject> m_Components = new();

        public IReadOnlyDictionary<string, GameObject> Components => m_Components;

        public void GenerateComponents()
        {
            m_Components.Clear();

            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in transforms)
            {
                if (child == transform || !child.name.StartsWith("m_", StringComparison.Ordinal))
                {
                    continue;
                }

                m_Components[child.name] = child.gameObject;
            }

            OnBeforeSerialize();
        }

        public void OnBeforeSerialize()
        {
            m_SerializedComponents ??= new List<ComponentEntry>();
            m_SerializedComponents.Clear();
            if (m_Components == null)
            {
                return;
            }

            foreach (KeyValuePair<string, GameObject> component in m_Components)
            {
                m_SerializedComponents.Add(new ComponentEntry
                {
                    Key = component.Key,
                    Value = component.Value
                });
            }
        }

        public void OnAfterDeserialize()
        {
            m_Components = new Dictionary<string, GameObject>(m_SerializedComponents?.Count ?? 0);
            if (m_SerializedComponents == null)
            {
                return;
            }

            foreach (ComponentEntry component in m_SerializedComponents)
            {
                if (!string.IsNullOrEmpty(component.Key))
                {
                    m_Components[component.Key] = component.Value;
                }
            }
        }
    }
}
