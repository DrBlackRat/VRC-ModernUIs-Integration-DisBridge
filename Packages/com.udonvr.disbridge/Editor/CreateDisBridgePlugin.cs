//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.IO;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    public class CreateDisBridgePlugin : Editor
    {
        [MenuItem("Assets/Create/UdonVR/DisBridge Plugin", false, 4)]
        private static void CreateDisBridgeScript()
        {
            string folderPath = "Assets/";
            if (Selection.activeObject != null)
            {
                folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (Selection.activeObject.GetType() != typeof(UnityEditor.DefaultAsset))
                {
                    folderPath = Path.GetDirectoryName(folderPath);
                }
            }
            else if (Selection.assetGUIDs.Length > 0)
            {
                folderPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            }

            folderPath = folderPath.Replace('\\', '/');

            string chosenFilePath = UnityEditor.EditorUtility.SaveFilePanelInProject("Save DisBridgePlugin File", "", "cs", "Save DisBridgePlugin file", folderPath);

            if (chosenFilePath.Length > 0)
            {
                chosenFilePath = SanitizeScriptFilePath(chosenFilePath);
                string chosenFileName = Path.GetFileNameWithoutExtension(chosenFilePath).Replace(" ", "").Replace("#", "Sharp");
                string assetFilePath = Path.Combine(Path.GetDirectoryName(chosenFilePath), $"{chosenFileName}.asset");

                if (AssetDatabase.LoadAssetAtPath<UdonSharpProgramAsset>(assetFilePath) != null)
                {
                    if (!UnityEditor.EditorUtility.DisplayDialog("File already exists", $"Corresponding asset file '{assetFilePath}' already found for new DisBridgePlugin script. Overwrite?", "Ok", "Cancel"))
                        return;
                }

                string fileContents = GetProgramTemplateString(chosenFileName);

                File.WriteAllText(chosenFilePath, fileContents, System.Text.Encoding.UTF8);

                AssetDatabase.ImportAsset(chosenFilePath, ImportAssetOptions.ForceSynchronousImport);
                MonoScript newScript = AssetDatabase.LoadAssetAtPath<MonoScript>(chosenFilePath);

                UdonSharpProgramAsset newProgramAsset = ScriptableObject.CreateInstance<UdonSharpProgramAsset>();
                newProgramAsset.sourceCsScript = newScript;

                AssetDatabase.CreateAsset(newProgramAsset, assetFilePath);

                AssetDatabase.Refresh();
            }
        }

        private static string SanitizeName(string name)
        {
            return name.Replace(" ", "")
                        .Replace("#", "Sharp")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("*", "")
                        .Replace("<", "")
                        .Replace(">", "")
                        .Replace("-", "_")
                        .Replace("!", "")
                        .Replace("$", "")
                        .Replace("@", "")
                        .Replace("+", "");
        }

        // Unity does not like having scripts with different names from their classes and will start breaking things weirdly, so enforce it by default.
        // If people really want to rename the asset afterwards they can, but there will be a compile warning that they can't get rid of without fixing the names.
        internal static string SanitizeScriptFilePath(string file)
        {
            string fileName = SanitizeName(Path.GetFileNameWithoutExtension(file));

            string filePath = Path.GetDirectoryName(file);

            return $"{filePath}/{fileName}{Path.GetExtension(file)}";
        }

        private static string GetProgramTemplateString(string scriptName)
        {
            var template = Resources.Load<TextAsset>("DefaultDisBridgePluginTemplate");
            if (template == null)
                throw new System.Exception("Unable to load DefaultDisBridgePluginTemplate");

            var templateStr = template.ToString();
            templateStr = templateStr.Replace("<TemplateClassName>", scriptName);

            return templateStr;
        }
    }
}