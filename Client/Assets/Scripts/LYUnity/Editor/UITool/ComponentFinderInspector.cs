using LYUnity.UITool;
using UnityEditor;
using UnityEngine;

namespace LYUnity.UITool.Editor
{
    [CustomEditor(typeof(ComponentFinder))]
    public class ComponentFinderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            ComponentFinder componentFinder = (ComponentFinder)target;
            if (componentFinder.Components.Count == 0)
            {
                EditorGUILayout.HelpBox("No components have been generated.", MessageType.Info);
            }
            else
            {
                foreach (var component in componentFinder.Components)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(component.Key, GUILayout.MinWidth(100));
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(component.Value, typeof(GameObject), true);
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Components"))
            {
                Undo.RecordObject(componentFinder, "Generate Component References");
                componentFinder.GenerateComponents();
                EditorUtility.SetDirty(componentFinder);
                PrefabUtility.RecordPrefabInstancePropertyModifications(componentFinder);
            }
        }
    }
}
