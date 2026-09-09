using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LYUnity.UITool.Editor
{
    internal static class UIStructureGenerator
    {
        private const string UIStructureMenuPath = "Tools/生成UI目录结构";
        private const string ScriptsRoot = "Assets/Scripts";
        private const string ModelGeneratedUIPath = "Model/Client/Generage/UI";
        private const string ModelUIPath = "Model/Client/UI";
        private const string HotUpdateGeneratedUIPath = "HotUpdate/Clinet/Generage/UI";
        private const string HotUpdateUIPath = "HotUpdate/Clinet/UI";
        private const string DefaultUIStructurePath = "Demo";
        private static readonly string[] UIStructureRelativePaths =
        {
            ModelGeneratedUIPath,
            ModelUIPath,
            HotUpdateGeneratedUIPath,
            HotUpdateUIPath
        };

        [MenuItem(UIStructureMenuPath, false, 2001)]
        private static void OpenUIStructureWindow()
        {
            UIStructureWindow window = EditorWindow.GetWindow<UIStructureWindow>(true, "生成UI目录结构", true);
            window.minSize = new Vector2(520f, 380f);
            window.maxSize = new Vector2(900f, 600f);
            window.Initialize(DefaultUIStructurePath);
            window.ShowUtility();
        }

        private static bool TryCreateUIStructure(string inputPath, out string errorMessage)
        {
            errorMessage = null;
            if (!TryGetUIStructurePaths(inputPath, out string[] outputPaths, out errorMessage))
            {
                return false;
            }

            string outputRoot = GetOutputRoot(inputPath);
            try
            {
                foreach (string outputPath in outputPaths)
                {
                    EnsureAssetFolder(outputPath);
                }

                if (UIScriptGenerator.TryGetSelectedPrefab(out GameObject prefab) && !UIScriptGenerator.GenerateUIScripts(prefab, outputRoot))
                {
                    errorMessage = $"Prefab '{prefab.name}' 的 UI 脚本生成失败，请查看 Console。";
                    return false;
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"UI directory structure generated under '{outputRoot}'.");
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"生成 UI 目录结构失败：{exception.Message}";
                Debug.LogError($"Failed to generate UI directory structure under '{inputPath}': {exception}");
                return false;
            }
        }

        private static bool TryGetUIStructurePaths(string inputPath, out string[] outputPaths, out string errorMessage)
        {
            outputPaths = null;
            if (!TryNormalizeScriptsRelativePath(inputPath, out string normalizedPath, out errorMessage))
            {
                return false;
            }

            string outputRoot = string.IsNullOrEmpty(normalizedPath) ? ScriptsRoot : CombineAssetPath(ScriptsRoot, normalizedPath);
            outputPaths = new string[UIStructureRelativePaths.Length];
            for (var index = 0; index < UIStructureRelativePaths.Length; index++)
            {
                outputPaths[index] = CombineAssetPath(outputRoot, UIStructureRelativePaths[index]);
            }

            return true;
        }

        private static string GetOutputRoot(string inputPath)
        {
            return TryNormalizeScriptsRelativePath(inputPath, out string normalizedPath, out _) && !string.IsNullOrEmpty(normalizedPath)
                ? CombineAssetPath(ScriptsRoot, normalizedPath)
                : ScriptsRoot;
        }

        private static bool TryNormalizeScriptsRelativePath(string inputPath, out string normalizedPath, out string errorMessage)
        {
            normalizedPath = (inputPath ?? string.Empty).Trim().Replace('\\', '/');
            errorMessage = null;
            if (string.IsNullOrEmpty(normalizedPath))
            {
                errorMessage = "请输入 Scripts 目录下的路径。";
                return false;
            }

            if (normalizedPath.StartsWith("/", StringComparison.Ordinal) || Path.IsPathRooted(normalizedPath))
            {
                errorMessage = "路径必须是 Scripts 目录下的相对路径，不能使用绝对路径。";
                return false;
            }

            while (normalizedPath.StartsWith("./", StringComparison.Ordinal))
            {
                normalizedPath = normalizedPath.Substring(2);
            }

            normalizedPath = normalizedPath.TrimEnd('/');
            if (string.IsNullOrEmpty(normalizedPath))
            {
                errorMessage = "请输入 Scripts 目录下的路径。";
                return false;
            }

            if (normalizedPath.Equals("Assets/Scripts", StringComparison.OrdinalIgnoreCase) || normalizedPath.Equals("Scripts", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = string.Empty;
                return true;
            }

            if (normalizedPath.StartsWith("Assets/Scripts/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring("Assets/Scripts/".Length);
            }
            else if (normalizedPath.StartsWith("Scripts/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring("Scripts/".Length);
            }
            else if (normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "路径必须位于 Assets/Scripts 目录下。";
                return false;
            }

            string[] segments = normalizedPath.Split('/');
            foreach (string segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == "..")
                {
                    errorMessage = "路径不能包含空目录名、'.' 或 '..'。";
                    return false;
                }

                if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    errorMessage = $"目录名 '{segment}' 包含无效字符。";
                    return false;
                }
            }

            return true;
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

        private sealed class UIStructureWindow : EditorWindow
        {
            private string scriptsRelativePath;
            private string errorMessage;

            public void Initialize(string defaultPath)
            {
                scriptsRelativePath = defaultPath;
                errorMessage = null;
            }

            private void OnSelectionChange()
            {
                Repaint();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField("在 Assets/Scripts 下生成 UI 目录结构。", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();
                string editedPath = EditorGUILayout.TextField("路径", scriptsRelativePath);
                if (!string.Equals(editedPath, scriptsRelativePath, StringComparison.Ordinal))
                {
                    scriptsRelativePath = editedPath;
                    errorMessage = null;
                }
                EditorGUILayout.LabelField("例如：Demo，或 Assets/Scripts/Demo", EditorStyles.miniLabel);
                EditorGUILayout.Space();

                bool hasSelectedPrefab = UIScriptGenerator.TryGetSelectedPrefab(out GameObject selectedPrefab);
                if (hasSelectedPrefab)
                {
                    EditorGUILayout.LabelField("当前 Prefab", $"{selectedPrefab.name} ({AssetDatabase.GetAssetPath(selectedPrefab)})");
                }
                else
                {
                    EditorGUILayout.HelpBox("当前未选中 Prefab，确定后只创建目录结构。", MessageType.Info);
                }

                EditorGUILayout.LabelField("生成路径预览", EditorStyles.boldLabel);
                if (TryGetUIStructurePaths(scriptsRelativePath, out string[] outputPaths, out string previewErrorMessage))
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        foreach (string outputPath in outputPaths)
                        {
                            EditorGUILayout.LabelField(outputPath, EditorStyles.wordWrappedMiniLabel);
                        }
                    }

                    if (hasSelectedPrefab)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("脚本生成路径预览", EditorStyles.boldLabel);
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            string outputRoot = GetOutputRoot(scriptsRelativePath);
                            foreach (string scriptPath in UIScriptGenerator.GetGeneratedScriptPaths(selectedPrefab, outputRoot))
                            {
                                EditorGUILayout.LabelField(scriptPath, EditorStyles.wordWrappedMiniLabel);
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(previewErrorMessage, MessageType.Warning);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                }

                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("确定", GUILayout.Height(24)))
                    {
                        if (TryCreateUIStructure(scriptsRelativePath, out errorMessage))
                        {
                            Close();
                        }
                    }

                    if (GUILayout.Button("取消", GUILayout.Height(24)))
                    {
                        Close();
                    }
                }
            }
        }
    }
}
