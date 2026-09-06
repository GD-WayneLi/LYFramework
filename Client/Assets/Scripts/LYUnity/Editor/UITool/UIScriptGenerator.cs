using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LYUnity.UITool;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LYUnity.UITool.Editor
{
    internal static class UIScriptGenerator
    {
        private const string GeneratedComponentRoot = "Assets/Scripts/Demo/Model/Client/Generage/UI";
        private const string ComponentRoot = "Assets/Scripts/Demo/Model/Client/UI";
        private const string GeneratedSystemRoot = "Assets/Scripts/Demo/HotUpdate/Clinet/Generage/UI";
        private const string SystemRoot = "Assets/Scripts/Demo/HotUpdate/Clinet/UI";
        private const string TemplateRoot = "Assets/Scripts/LYUnity/Editor/UITool/Template";
        private const string AssetsMenuPath = "Assets/生成UI脚本";
        private const string GameObjectMenuPath = "GameObject/生成UI脚本";
        
        [MenuItem(AssetsMenuPath, true)]
        private static bool ValidateGenerateUIScriptsFromAssets()
        {
            return TryGetSelectedPrefab(out _);
        }

        [MenuItem(AssetsMenuPath, false, 2000)]
        private static void GenerateUIScriptsFromAssets()
        {
            if (!TryGetSelectedPrefab(out GameObject prefab))
            {
                return;
            }

            GenerateUIScripts(prefab);
        }

        [MenuItem(GameObjectMenuPath, true)]
        private static bool ValidateGenerateUIScriptsFromHierarchy(MenuCommand menuCommand)
        {
            return menuCommand?.context is GameObject || Selection.activeGameObject != null;
        }

        [MenuItem(GameObjectMenuPath, false, 2000)]
        private static void GenerateUIScriptsFromHierarchy(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand?.context as GameObject ?? Selection.activeGameObject;
            if (selectedObject != null)
            {
                GenerateUIScripts(selectedObject);
            }
        }

        private static void GenerateUIScripts(GameObject prefab)
        {
            try
            {
                Generate(prefab);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"UI scripts generated for prefab '{prefab.name}'.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to generate UI scripts for prefab '{prefab.name}': {exception}");
            }
        }

        private static void Generate(GameObject prefab)
        {
            string prefabName = prefab.name;
            string className = prefab.name;
            string generatedFolder = CombineAssetPath(GeneratedComponentRoot, prefabName);
            string componentFolder = CombineAssetPath(ComponentRoot, prefabName);
            string generatedSystemFolder = CombineAssetPath(GeneratedSystemRoot, prefabName);
            string systemFolder = CombineAssetPath(SystemRoot, prefabName);
            EnsureAssetFolder(generatedFolder);
            EnsureAssetFolder(componentFolder);
            EnsureAssetFolder(generatedSystemFolder);
            EnsureAssetFolder(systemFolder);

            List<FieldDefinition> fields = CollectFields(prefab);
            string generatedComponentPath = CombineAssetPath(generatedFolder, className + "Component.cs");
            string componentPath = CombineAssetPath(componentFolder, className + "Component.cs");
            string generatedSystemPath = CombineAssetPath(generatedSystemFolder, className + "System.cs");
            string systemPath = CombineAssetPath(systemFolder, className + "System.cs");

            WriteGeneratedAssetFile(generatedComponentPath, BuildFromTemplate("FieldComponent.cs.txt", className, BuildFieldDeclarations(fields)));
            WriteCustomAssetFile(componentPath, BuildFromTemplate("CustomComponent.cs.txt", className, string.Empty));
            WriteGeneratedAssetFile(generatedSystemPath, BuildFromTemplate("FieldSystem.cs.txt", className, BuildFieldAssignments(fields, prefab.transform)));
            WriteCustomAssetFile(systemPath, BuildFromTemplate("CustomSystem.cs.txt", className, string.Empty));
        }

        private static List<FieldDefinition> CollectFields(GameObject prefab)
        {
            var fields = new List<FieldDefinition>();
            var usedFieldNames = new HashSet<string>(StringComparer.Ordinal)
            {
                "GameObject",
                "m_GameObject"
            };
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform.name.IndexOf("m_", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                string fieldName = MakeUniqueIdentifier(transform.name, usedFieldNames);
                Type componentType = FindUGUIComponentType(transform.gameObject);
                fields.Add(new FieldDefinition(fieldName, componentType, transform));
            }

            return fields;
        }

        private static Type FindUGUIComponentType(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            Type graphicType = null;
            Type otherUIType = null;
            foreach (Component component in components)
            {
                if (component == null)
                {
                    continue;
                }

                Type componentType = component.GetType();
                if (!IsUGUIComponent(componentType))
                {
                    continue;
                }

                if (typeof(Selectable).IsAssignableFrom(componentType))
                {
                    return componentType;
                }

                if (graphicType == null && typeof(Graphic).IsAssignableFrom(componentType))
                {
                    graphicType = componentType;
                }
                else if (otherUIType == null)
                {
                    otherUIType = componentType;
                }
            }

            return graphicType ?? otherUIType;
        }

        private static bool IsUGUIComponent(Type componentType)
        {
            return componentType.Namespace == "UnityEngine.UI" || typeof(Selectable).IsAssignableFrom(componentType) || typeof(Graphic).IsAssignableFrom(componentType);
        }

        private static string BuildFieldDeclarations(IReadOnlyList<FieldDefinition> fields)
        {
            var builder = new StringBuilder();
            foreach (FieldDefinition field in fields)
            {
                builder.AppendLine($"        public {GetFieldTypeName(field.ComponentType)} {field.FieldName};");
            }

            return builder.ToString();
        }

        private static string BuildFieldAssignments(IReadOnlyList<FieldDefinition> fields, Transform prefabRoot)
        {
            var builder = new StringBuilder();
            builder.AppendLine("            self.m_GameObject = self.Owner?.GetComponent<UIComponent>()?.GameObject;");
            if (fields.Count == 0)
            {
                return builder.ToString();
            }

            builder.AppendLine("            if (self.m_GameObject == null)");
            builder.AppendLine("            {");
            builder.AppendLine("                return;");
            builder.AppendLine("            }");
            foreach (FieldDefinition field in fields)
            {
                string path = GetRelativePath(prefabRoot, field.Transform);
                string targetExpression = string.IsNullOrEmpty(path) ? "self.m_GameObject.transform" : $"self.m_GameObject.transform.Find(\"{EscapeString(path)}\")";
                if (field.ComponentType == null)
                {
                    builder.AppendLine($"            self.{field.FieldName} = {targetExpression}.gameObject;");
                }
                else
                {
                    builder.AppendLine($"            self.{field.FieldName} = {targetExpression}.GetComponent<{GetFieldTypeName(field.ComponentType)}>();");
                }
            }

            return builder.ToString();
        }

        private static string BuildFromTemplate(string templateName, string prefabName, string fields)
        {
            string templatePath = ToAbsolutePath(CombineAssetPath(TemplateRoot, templateName));
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"UI script template was not found: {templatePath}");
            }

            return File.ReadAllText(templatePath).Replace("#NAME#", prefabName).Replace("#FIELD#", fields);
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            var path = new List<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                path.Add(current.name);
                current = current.parent;
            }

            path.Reverse();
            return string.Join("/", path);
        }

        private static string EscapeString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string GetFieldTypeName(Type componentType)
        {
            if (componentType == null)
            {
                return "GameObject";
            }

            if (componentType.Namespace == "UnityEngine.UI" || componentType.Namespace == "TMPro")
            {
                return componentType.Name;
            }

            return componentType.FullName.Replace('+', '.');
        }

        private static string MakeUniqueIdentifier(string value, ISet<string> usedNames)
        {
            string baseName = value;
            string uniqueName = baseName;
            var suffix = 2;
            while (!usedNames.Add(uniqueName))
            {
                uniqueName = $"{baseName}_{suffix++}";
            }

            return uniqueName;
        }
        
        private static string CombineAssetPath(string parent, string child)
        {
            return (parent.TrimEnd('/') + "/" + child.Trim('/')).Replace('\\', '/');
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string[] segments = assetPath.Replace('\\', '/').Split('/');
            string currentPath = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                string nextPath = currentPath + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }

            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                throw new InvalidOperationException($"Unable to create asset folder: {assetPath}");
            }
        }

        private static void WriteGeneratedAssetFile(string assetPath, string content)
        {
            File.WriteAllText(ToAbsolutePath(assetPath), content, new UTF8Encoding(false));
        }

        private static void WriteCustomAssetFile(string assetPath, string content)
        {
            if (File.Exists(ToAbsolutePath(assetPath)))
            {
                return;
            }

            File.WriteAllText(ToAbsolutePath(assetPath), content, new UTF8Encoding(false));
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static bool TryGetSelectedPrefab(out GameObject prefab)
        {
            prefab = null;
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return prefab != null;
        }

        private sealed class FieldDefinition
        {
            public FieldDefinition(string fieldName, Type componentType, Transform transform)
            {
                FieldName = fieldName;
                ComponentType = componentType;
                Transform = transform;
            }

            public string FieldName { get; }

            public Type ComponentType { get; }

            public Transform Transform { get; }
        }
    }
}
