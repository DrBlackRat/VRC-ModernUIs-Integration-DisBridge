using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;

namespace DrBlackRat.VRC.ModernUIs.Integrations.Editor
{
    public static class ModernUIsIntegrationsDisBridgeMenu
    {
        [MenuItem("Tools/DrBlackRat/ModernUIs Integrations/DisBridge/Add DisBridge to Whitelist Bridge Prefab to Scene", false, 2)]
        public static void AddPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/xyz.drblackrat.vrc.modernuis.integrations.disbridge/Runtime/Prefabs/DisBridge To Whitelist Bridge.prefab");
            PrefabUtility.InstantiatePrefab(prefab);
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs Integrations/DisBridge/Show ReadMe", false, 3)]
        public static void ShowReadMe()
        {
            PingAsset("Packages/xyz.drblackrat.vrc.modernuis.integrations.disbridge/README.md");
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs Integrations/DisBridge/Open GitHub", false, 4)]
        public static void OpenGitHub()
        {
            Application.OpenURL("https://github.com/DrBlackRat/VRC-ModernUIs-Integration-DisBridge");
        }
        
        private static void PingAsset(string path)
        {
            var folder = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }
    }
}
