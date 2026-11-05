//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    public static class DisBridgePrefabUtilities
    { 
        private const string logPrefix = "<color=#888888>[UdonVR]</color><color=#436b8c>[DisBridge]</color>";
        internal const string folderLocation = "Assets/DisBridge_Generated/";
        internal const string customPrefabLocation = "Assets/DisBridge_Generated/Prefabs/";
        
        /// <summary>
        /// Checks if a scene component's GameObject is an instance of a specific prefab asset, identified by its project path.
        /// </summary>
        /// <param name="sceneComponent">The component in the scene hierarchy to check. Can be on the root or any child of the prefab instance.</param>
        /// <param name="expectedPrefabPath">The project path to the prefab asset (e.g., "Assets/Prefabs/MyButton.prefab").</param>
        /// <returns>True if the component's GameObject is an instance of the prefab at the specified path, false otherwise.</returns>
        public static bool IsInstanceOfPrefab(Component sceneComponent, string expectedPrefabPath, out GameObject sourcePrefabAsset)
        {
            sourcePrefabAsset = null;
            if (sceneComponent == null)
            {
                Debug.LogError($"{logPrefix} IsInstanceOfPrefab check failed: The input sceneComponent was null.");
                return false;
            }
            
            if (string.IsNullOrEmpty(expectedPrefabPath))
            {
                Debug.LogError($"{logPrefix} IsInstanceOfPrefab check failed: The input expectedPrefabPath was null or empty.", sceneComponent);
                return false;
            }
            
            GameObject expectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(expectedPrefabPath);
            
            if (expectedPrefab == null)
            {
                Debug.LogWarning($"{logPrefix} IsInstanceOfPrefab check failed: Could not load a prefab GameObject from path '{expectedPrefabPath}'. Ensure the path is correct and points to a GameObject asset.", sceneComponent);
                return false;
            }
            
            GameObject rootInstanceOfPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(sceneComponent.gameObject);

            if (rootInstanceOfPrefab == null)
            {
                return false;
            }
            
            sourcePrefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(rootInstanceOfPrefab);

            if (sourcePrefabAsset == null)
            {
                return false;
            }
            
            return sourcePrefabAsset == expectedPrefab;
        }
        
        /// <summary>
        /// Checks if a scene component's GameObject is an instance of a specific DisBridge prefab asset.
        /// <br></br>The path provided is relative to the root folder defined in <see cref="CreatePrefabs.filePath"/>.
        /// </summary>
        /// <param name="sceneComponent">The component in the scene hierarchy to check.</param>
        /// <param name="expectedPrefabPath">The relative path of the prefab, starting from the root defined in CreatePrefabs.</param>
        /// <returns>True if the component's GameObject is an instance of the specified DisBridge prefab.</returns>
        public static bool IsInstanceOfDisBridgePrefab(Component sceneComponent, string expectedPrefabPath, out GameObject sourcePrefabAsset)
        {
            return IsInstanceOfPrefab(sceneComponent, $"{CreatePrefabs.filePath}{expectedPrefabPath}", out sourcePrefabAsset);
        }
        
        /// <summary>
        /// Creates a new prefab variant from a component's GameObject instance and saves it to the custom prefab location.
        /// </summary>
        /// <param name="sceneComponent">A component on the GameObject instance in the scene that you want to create a variant from.</param>
        /// <param name="newVariantName">The desired file name for the new prefab variant (without the .prefab extension).</param>
        /// <returns>The newly created prefab variant asset as a GameObject, or null if the operation fails.</returns>
        public static GameObject CreatePrefabVariantFromComponent(Component sceneComponent)
        {
            if (sceneComponent == null)
            {
                Debug.LogError($"{logPrefix} CreatePrefabVariant failed: The input sceneComponent was null.");
                return null;
            }
            
            GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(sceneComponent.gameObject);
            if (instanceRoot == null)
            {
                Debug.LogError($"{logPrefix} CreatePrefabVariant failed: The provided component is not part of a prefab instance.", sceneComponent);
                return null;
            }
            
            GameObject sourcePrefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            if (sourcePrefabAsset == null)
            {
                Debug.LogError($"{logPrefix} CreatePrefabVariant failed: Could not find the source prefab asset for the instance.", instanceRoot);
                return null;
            }
            string prefabName = sourcePrefabAsset.name;
            
            if (!Directory.Exists(customPrefabLocation))
            {
                Directory.CreateDirectory(customPrefabLocation);
            }
            
            string variantPath = $"{customPrefabLocation}{prefabName}.prefab";
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(variantPath);
            
            GameObject newVariant = PrefabUtility.SaveAsPrefabAsset(instanceRoot, uniquePath);

            if (newVariant == null)
            {
                Debug.LogError($"{logPrefix} Failed to create prefab variant at path '{uniquePath}'.", instanceRoot);
                return null;
            }

            Debug.Log($"{logPrefix} Successfully created prefab variant at '{uniquePath}'.", newVariant);
            return newVariant;
        }
        
        /// <summary>
        /// Checks if an object is an instance of a protected "Master Prefab".
        /// <br></br>- If it is, it prompts the user to create their own variant to prevent overwriting the master.
        /// <br></br>- If it's already a user variant, it asks whether to update that variant or create a new one.
        /// </summary>
        /// <param name="sceneComponent">The component on the object to test.</param>
        /// <param name="expectedPrefabPath">The relative path to the protected "Master Prefab".</param>
        /// <returns>The resulting prefab asset (either updated or newly created), or null if the user cancels.</returns>
        public static GameObject TestDisBridgePrefab(Component sceneComponent, string expectedPrefabPath)
        {
            if (!IsInstanceOfDisBridgePrefab(sceneComponent, expectedPrefabPath, out GameObject sourcePrefabAsset))
            {
                //The object is NOT the master prefab. It's a user-made variant or other prefab.
                string dialogTitle = "Update Existing Prefab?";
                string dialogMessage = $"This object is an instance of your own prefab variant '{sourcePrefabAsset.name}'.\n\nDo you want to apply your changes to this existing prefab, or save them as a new variant?";

                int option = EditorUtility.DisplayDialogComplex(dialogTitle, dialogMessage, "Update Existing", "Cancel", "Create New");

                switch (option)
                {
                    case 0: // Update Existing
                        PrefabUtility.ApplyPrefabInstance(sourcePrefabAsset, InteractionMode.AutomatedAction);
                        Debug.Log($"{logPrefix} Applied changes to existing prefab variant '{sourcePrefabAsset.name}'.", sourcePrefabAsset);
                        return sourcePrefabAsset;
                    
                    case 1: // Cancel
                        return null;

                    case 2: // Create New
                        return CreatePrefabVariantFromComponent(sceneComponent);
                    
                    default:
                        return null;
                }
            }
            else
            {
                // The object IS the master prefab. Create a new Prefab or cancel.
                string dialogTitle = "Create Prefab Variant";
                string dialogMessage = "This object is an instance of a protected master prefab. To save your changes, you must create your own prefab variant.\n\nDo you want to create a new prefab variant now?";
                
                if (EditorUtility.DisplayDialog(dialogTitle, dialogMessage, "Create Variant", "Cancel"))
                {
                    return CreatePrefabVariantFromComponent(sceneComponent);
                }
                return null;
            }
        }
    }
}