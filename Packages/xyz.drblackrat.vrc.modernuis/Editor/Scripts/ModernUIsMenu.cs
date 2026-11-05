using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class ModernUIsMenu
    {
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open Demo Scene", false, 1)]
        public static void OpenDemoScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Packages/xyz.drblackrat.vrc.modernuis/Runtime/Demo Scene.unity");
            }
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open Prefabs Folder", false, 2)]
        public static void OpenPrefabsFolder()
        {
            PingFolder("Packages/xyz.drblackrat.vrc.modernuis/Runtime/Prefabs");
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open Scripts Folder", false, 3)]
        public static void OpenScriptsFolder()
        {
            PingFolder("Packages/xyz.drblackrat.vrc.modernuis/Runtime/Scripts");
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open Themes Folder", false, 4)]
        public static void OpenThemesFolder()
        {
            PingFolder("Packages/xyz.drblackrat.vrc.modernuis/Runtime/Themes");
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open Resources Folder", false, 5)]
        public static void OpenResourcesFolder()
        {
            PingFolder("Packages/xyz.drblackrat.vrc.modernuis/Runtime/Resources");
        }
        
        [MenuItem("Tools/DrBlackRat/ModernUIs/Open GitHub", false, 6)]
        public static void OpenGitHub()
        {
            Application.OpenURL("https://github.com/DrBlackRat/VRC-ModernUIs");
        }

        private static void PingFolder(string path)
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
