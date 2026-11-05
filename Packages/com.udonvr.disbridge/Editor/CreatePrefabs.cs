//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    public class CreatePrefabs
    {
        private const int Menu = 49;
        internal const string filePath = "Packages/com.udonvr.disbridge/Runtime/Prefabs/";
        private const string logPrefix = "<color=#888888>[UdonVR]</color><color=#436b8c>[DisBridge]</color>";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Plugins

        //Toggles ///////////////////////////////////////////////////////////////////////////////////////////
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Toggle V2/Global", false, Menu)]
        static void CreateLocalToggleV2Global(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/ToggleV2 (Global Prefab).prefab", _cmd);
        }
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Toggle V2/Local", false, Menu)]
        static void CreateLocalToggleV2Local(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/ToggleV2 (Local Prefab).prefab", _cmd);
        }
        //static void CreateLocalToggle(MenuCommand _cmd)
        //{
        //    Debug.Log("[UdonVR][DisBridge] Creating Local_DisbridgeToggle");
        //    CreateDisBridgePrefab("Plugins/Local_DisbridgeToggle.prefab", _cmd);
        //}
        //[MenuItem("GameObject/UdonVR/DisBridge/Plugins/Toggles/Global", false, Menu)]
        //static void CreateGlobalToggle(MenuCommand _cmd)
        //{
        //    Debug.Log("[UdonVR][DisBridge] Creating Global_DisbridgeToggle");
        //    CreateDisBridgePrefab("Plugins/Global_DisbridgeToggle.prefab", _cmd);
        //}
        
        //RoleBoard ///////////////////////////////////////////////////////////////////////////////////////////
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/RoleBoard-V2", false, Menu)]
        static void CreateRoleBoard(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/RoleBoardV2.prefab", _cmd);
        }
        
        //Role Tags///////////////////////////////////////////////////////////////////////////////////////////
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/RoleTags-V2", false, Menu)]
        static void CreateRoleTags(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/RoleTagsV2.prefab", _cmd);
        }
        
        //Keypad ///////////////////////////////////////////////////////////////////////////////////////////
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Keypad", false, Menu)]
        static void CreateKeypad(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/Keypad.prefab", _cmd);
        }
        
        //Whitelist ///////////////////////////////////////////////////////////////////////////////////////////
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Whitelist V2", false, Menu)]
        static void CreateWhitelist(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/Whitelist.prefab", _cmd);
        }
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Spawn Selector", false, Menu)]
        static void CreateSpawnSelector(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/SpawnSelector.prefab", _cmd);
        }
        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Simple Door", false, Menu)]
        static void CreateSimpleDoor(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/SimpleDoor.prefab", _cmd);
        }
        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Core

        /*
        [MenuItem("GameObject/UdonVR/DisBridge/Parts/PluginManager", false, Menu)]
        static void CreatePluginManager(MenuCommand _cmd)
        {
            Debug.Log("[UdonVR][DisBridge] Creating RoleContainer");
            CreateDisBridgePrefab("Parts/PluginManager.prefab", _cmd);
        }

        [MenuItem("GameObject/UdonVR/DisBridge/Parts/RoleContainer", false, Menu)]
        static void CreateRoleContainer(MenuCommand _cmd)
        {
            Debug.Log("[UdonVR][DisBridge] Creating RoleContainer");
            CreateDisBridgePrefab("Parts/RoleContainer.prefab", _cmd);
        }
        */

        [MenuItem("GameObject/UdonVR/DisBridge/DisBridge (PluginManager)", false, Menu)]
        static void CreateDisBridge(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("DisBridge.prefab", _cmd);
        }

        [MenuItem("GameObject/UdonVR/DisBridge/Plugins/Debugger", false, Menu)]
        static void CreateDisBridgeDebugger(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Plugins/DisBridgeDebugger.prefab", _cmd);
        }
        
        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Backend

        public static GameObject CreateDisBridgePrefab(string filename, MenuCommand _cmd, bool _unPack = false, bool focusObject = true)
        {
            //Debug.Log("[UdonVR] Trying to load Prefab from file [Assets/_UdonVR/Tools/Utility/Prefabs/" + filename + "]");
            var loadedObject = AssetDatabase.LoadAssetAtPath(filePath + filename, typeof(UnityEngine.Object));
            if (loadedObject == null)
            {
                Debug.LogError($"{logPrefix} Failed to find File, did you move the _UdonVR folder? File[{filename}]");
                return null;
            }

            return CreateObj(loadedObject, _cmd, _unPack, focusObject);
        }
        public static GameObject CreateDisBridgePrefab(string filename, bool _unPack = false, bool focusObject = true)
        {
            //Debug.Log("[UdonVR] Trying to load Prefab from file [Assets/_UdonVR/Tools/Utility/Prefabs/" + filename + "]");
            var loadedObject = AssetDatabase.LoadAssetAtPath(filePath + filename, typeof(UnityEngine.Object));
            if (loadedObject == null)
            {
                Debug.LogError($"{logPrefix} Failed to find File, did you move the _UdonVR folder? File[{filename}]");
                return null;
            }

            return CreateObj(loadedObject, _unPack, focusObject);
        }

        private static GameObject CreatePrefabFromFile(string filename, MenuCommand _cmd, bool _unPack = false)
        {
            //Debug.Log("[UdonVR] Trying to load Prefab from file [" + filename + "]");
            var loadedObject = AssetDatabase.LoadAssetAtPath(filename, typeof(UnityEngine.Object));
            if (loadedObject == null)
            {
                Debug.LogError($"{logPrefix} Failed to find File at [{filename}]?");
                return null;
            }

            return CreateObj(loadedObject, _cmd, _unPack);
        }

        private static GameObject CreateObj(UnityEngine.Object _loadedObject, MenuCommand _cmd, bool _unPack, bool focusObject = true)
        {
            GameObject _obj = (GameObject)PrefabUtility.InstantiatePrefab(_loadedObject);
            GameObject _target = (_cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(_obj, $"{logPrefix} Created Prefab");
            if (_target != null)
            {
                _obj.transform.SetParent(_target.transform);
                _obj.transform.SetPositionAndRotation(_target.transform.position, _target.transform.rotation);
                _obj.layer = _target.layer;
            }

            _obj.transform.SetAsLastSibling();
            if (focusObject) Selection.activeGameObject = _obj;
            if (_unPack)
            {
                PrefabUtility.UnpackPrefabInstance(_obj.gameObject, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }
            Debug.Log($"{logPrefix} Created {_obj.name}");
            return _obj;
        }
        private static GameObject CreateObj(UnityEngine.Object _loadedObject, bool _unPack, bool focusObject = true)
        {
            GameObject _obj = (GameObject)PrefabUtility.InstantiatePrefab(_loadedObject);
            Undo.RegisterCreatedObjectUndo(_obj, $"{logPrefix} Created Prefab");

            _obj.transform.SetAsLastSibling();
            if (focusObject) Selection.activeGameObject = _obj;
            if (_unPack)
            {
                PrefabUtility.UnpackPrefabInstance(_obj.gameObject, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }
            Debug.Log($"{logPrefix} Created {_obj.name}");
            return _obj;
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Poster System

        [MenuItem("GameObject/UdonVR/DisBridge/Poster System/ImageLoader", false, Menu)]
        static void CreateImageloader(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Posters/ImageLoader.prefab", _cmd, true);
        }
        
        [MenuItem("GameObject/UdonVR/DisBridge/Poster System/Plugins/Material Proxy", false, Menu)]
        static void CreateMaterialProxy(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Posters/Plugins/Material Proxy.prefab", _cmd, true);
        }
        
        [MenuItem("GameObject/UdonVR/DisBridge/Poster System/Plugins/Display Controller", false, Menu)]
        static void CreateMaterialDisplayController(MenuCommand _cmd)
        {
            CreateDisBridgePrefab("Posters/Plugins/Display Controller.prefab", _cmd, true);
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Utils

        [MenuItem("GameObject/UdonVR/DisBridge/Utils/PlayerTracker", false, Menu)]
        public static GameObject CreatePlayerTracker(MenuCommand _cmd)
        {
            PlayerTracker playerTracker = GameObject.FindObjectOfType<PlayerTracker>();
            if (playerTracker != null)
            {
                Debug.LogError($"{logPrefix} Error Creating PlayerTracker, a player tracker already exists!");
                return null;
            }
            return CreateDisBridgePrefab("Utils/PlayerTracker.prefab", _cmd);
        }
        public static GameObject CreatePlayerTracker()
        {
            PlayerTracker playerTracker = GameObject.FindObjectOfType<PlayerTracker>();
            if (playerTracker != null)
            {
                Debug.LogError($"{logPrefix} Error Creating PlayerTracker, a player tracker already exists!");
                return null;
            }
            return CreateDisBridgePrefab("Utils/PlayerTracker.prefab",false,false);
        }
        
        [MenuItem("GameObject/UdonVR/DisBridge/Utils/Permission Group", false, Menu)]
        public static GameObject CreatePermissionGroup(MenuCommand _cmd)
        {
            return CreateDisBridgePrefab("Utils/PermissionGroup.prefab", _cmd);
        }
        public static GameObject CreatePermissionGroup()
        {
            return CreateDisBridgePrefab("Utils/PermissionGroup.prefab",false,false);
        }

        #endregion
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
    }
}
