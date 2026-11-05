//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonVR.DisBridge.Plugins;
using UdonVR.DisBridge.PosterSystem;
using UdonVr.DisBridge.Utils;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditorInternal;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    public class EditorFunctions : Editor
    {
        public static void GuiLine( int i_height = 1 , bool _seperators = true)
        {
            if (_seperators) EditorGUILayout.Separator();
            Rect rect = EditorGUILayout.GetControlRect(false, i_height );
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
            if (_seperators) EditorGUILayout.Separator();
        }

        public static GameObject CreatePrefab(string _filepath, GameObject _parent, bool undo = true, bool moveSelection = true, bool unpack = true)
        {
            var loadedObject = AssetDatabase.LoadAssetAtPath(_filepath, typeof(UnityEngine.Object));
            if (loadedObject == null)
            {
                Debug.LogError("[UdonVR] Failed to find File, did you move the _UdonVR folder? File[" + _filepath + "]");
                return null;
            }
            GameObject _obj = (GameObject)PrefabUtility.InstantiatePrefab(loadedObject);
            if (undo) Undo.RegisterCreatedObjectUndo(_obj, "[UdonVR] Created Prefab");
            if (_parent != null)
            {
                _obj.transform.SetParent(_parent.transform);
                _obj.transform.SetPositionAndRotation(_parent.transform.position, _parent.transform.rotation);
                _obj.layer = _parent.layer;
            }

            
            _obj.transform.SetAsLastSibling();
            if (moveSelection) Selection.activeGameObject = _obj;
            if (unpack) PrefabUtility.UnpackPrefabInstance(_obj.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            return _obj;
        }
        public static IEnumerable[] arrayremoveLinq(IEnumerable[] array, int index)
        {
            IEnumerable[] newarray = array.Where(arr => arr != array[index]).ToArray(); 
            return newarray;
        }

        public static void PlaymodeWarning(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You are in Playmode. Changes made will not be saved.", MessageType.Warning);
            }
            UdonSharpEditor.UdonSharpGUI.DrawUtilities(target);
            GuiLine();
        }

        public static void DrawImageloaderCheck(DisBridgeImageLoaderPlugin _plugin,bool guiLine = true, bool forceFill = true)
        {
            if (guiLine) GuiLine();
            
            
            GUILayout.BeginHorizontal();
            DisBridgeImageLoader _imageLoader = (DisBridgeImageLoader)EditorGUILayout.ObjectField("Image Loader", _plugin.imageLoader, typeof(DisBridgeImageLoader), true);
            if (_plugin.imageLoader != _imageLoader)
            {
                Undo.RecordObject(_plugin,$"Changed manager: {_plugin.imageLoader} => {_imageLoader}");
                _plugin.imageLoader = _imageLoader;
            }

            if (_plugin.imageLoader != null){
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = _plugin.imageLoader.gameObject;
                }
            }
            GUILayout.EndHorizontal();
            
            
            if (_plugin.imageLoader == null)
            {
                DisBridgeImageLoader[] imageloaders = FindObjectsOfType<DisBridgeImageLoader>();
                if (imageloaders == null || (imageloaders.Length > 1 || forceFill == false))
                {
                    
                    EditorGUILayout.HelpBox("!!! DisBridgeImageLoader is not bound !!!\n\n multiple DisBridgeImageLoaders were found, please bind one manually or below.", MessageType.Error);

                    for (int i = 0; i < imageloaders.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(imageloaders[i], typeof(DisBridgeImageLoader), true);
                        if(GUILayout.Button($"Select",GUILayout.Width(100)))
                        {
                            Undo.RecordObject(_plugin,$"Changed manager: {_plugin.imageLoader} => {imageloaders[i]}");
                            _plugin.imageLoader = imageloaders[i];
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if (imageloaders.Length == 0)
                {
                    EditorGUILayout.HelpBox("!!! DisBridgeImageLoader is not bound !!!\n\n No DisBridgeImageLoaders were found, please create one.", MessageType.Error);
                }
                else
                {
                    Undo.RecordObject(_plugin,$"Changed manager: {_plugin.imageLoader} => {imageloaders[0]}");
                    _plugin.imageLoader = imageloaders[0];
                }
            }
            if (guiLine) GuiLine();
        }

        public static void DrawPluginManagerCheck(DisBridgePlugin _plugin, bool guiLine = true, bool forceFill = true)
        {
            if (guiLine) GuiLine();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {

                GUILayout.BeginHorizontal();
                PluginManager _imageLoader = (PluginManager)EditorGUILayout.ObjectField("DisBridge Plugin Manager:",
                    _plugin.disBridge, typeof(PluginManager), true);
                if (_plugin.disBridge != _imageLoader)
                {
                    Undo.RecordObject(_plugin, $"Changed manager: {_plugin.disBridge} => {_imageLoader}");
                    _plugin.disBridge = _imageLoader;
                }

                if (_plugin.disBridge != null)
                {
                    if (GUILayout.Button("Select", GUILayout.Width(50)))
                    {
                        Selection.activeObject = _plugin.disBridge.gameObject;
                    }
                }

                GUILayout.EndHorizontal();


                if (_plugin.disBridge == null)
                {
                    PluginManager[] pluginManagers = FindObjectsOfType<PluginManager>();
                    if (pluginManagers == null || (pluginManagers.Length > 1 || forceFill == false))
                    {

                        EditorGUILayout.HelpBox(
                            "!!! DisBridge PluginManager is not bound !!!\n\n multiple DisBridge PluginManagers were found, please bind one manually or below.",
                            MessageType.Error);

                        for (int i = 0; i < pluginManagers.Length; i++)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(pluginManagers[i], typeof(PluginManager), true);
                            if (GUILayout.Button($"Select", GUILayout.Width(100)))
                            {
                                Undo.RecordObject(_plugin,
                                    $"Changed manager: {_plugin.disBridge} => {pluginManagers[i]}");
                                _plugin.disBridge = pluginManagers[i];
                            }

                            GUILayout.EndHorizontal();
                        }
                    }
                    else if (pluginManagers.Length == 0)
                    {
                        EditorGUILayout.HelpBox(
                            "!!! DisBridge PluginManager is not bound !!!\n\n No DisBridge PluginManagers were found, please create one.",
                            MessageType.Error);
                    }
                    else
                    {
                        Undo.RecordObject(_plugin, $"Changed manager: {_plugin.disBridge} => {pluginManagers[0]}");
                        _plugin.disBridge = pluginManagers[0];
                    }
                }
            }

            if (guiLine) GuiLine();
        }

        public static void DrawPermissions(DisBridgePlugin _script, bool _usableByAnyone = true)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawRoleContainerSelector(_script);
                DrawStaffSupporterSelector(_script);
                _script._roleContainers ??= Array.Empty<RoleContainer>();
                _script._permissionGroups ??= Array.Empty<PermissionGroup>();
                DrawPermissionGroupsList(_script);
                DrawPermissionWarning(_script, _usableByAnyone);
            }
        }

        public static void DrawPermissionWarning(DisBridgePlugin _script, bool _usableByAnyone = true)
        {
            // The condition for _permissionGroups is now updated.
            // It now triggers the warning if the array is null, empty, OR if all of its elements are null.
            if (_script._roleContainers.Length == 0 && 
                !_script.useStaff && 
                !_script.useSupporters && 
                !_script.useBoosters && 
                (_script._permissionGroups == null || _script._permissionGroups.All(item => item == null)))
            {
                if (_usableByAnyone)
                {
                    EditorGUILayout.HelpBox("There are no roles checked. This will be usable by Anyone!", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("There are no roles checked. This will not be usable!", MessageType.Warning);
                }
            }
        }
        public static void DrawRoleContainerSelector(DisBridgePlugin _script)
        {
            if (_script.disBridge == null) return;
            
            Undo.RecordObject(_script,$"RoleContainer Mask Change");
            int _flags = 0;
            String[] _RoleContainerLabels = new string[_script.disBridge.roles.childCount];
            RoleContainer[] _allRoleContainers = new RoleContainer[_RoleContainerLabels.Length];
            String[] _RoleContainerIds = new String[_RoleContainerLabels.Length];

            //Build current RoleContainer arrays
            for (int i = 0; i < _script.disBridge.roles.childCount; i++)
            {
                _allRoleContainers[i] = _script.disBridge.roles.GetChild(i).GetComponent<RoleContainer>();
                _RoleContainerLabels[i] = _allRoleContainers[i].roleName;
                _RoleContainerIds[i] = _allRoleContainers[i].roleId;
            }

            //calculate flags based on RoleID
            //Debug.Log("_script.roleContainerIDs.Length: " + _script.roleContainerIDs.Length);
            if (_script.roleContainerIDs != null)
            {
                for (int i = 0; i < _script.roleContainerIDs.Length; i++)
                {
                    //Debug.Log("Array.IndexOf(_RoleContainerLabels,_script.roleContainerIDs[i])): " + Array.IndexOf(_RoleContainerLabels,_script.roleContainerIDs[i]));
                    //Debug.Log(_script.roleContainerIDs[i]);
                    int j = Array.IndexOf(_RoleContainerIds, _script.roleContainerIDs[i]);
                    if (j > -1)
                    {
                        //Debug.LogWarning("Found Match: " + _RoleCOntainerIds[j]);
                        _flags ^= (1 << j);
                    }
                }
            }

            //create mask

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (_RoleContainerLabels == null || _RoleContainerLabels.Length == 0)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    GUILayout.Button("You have no Roles, Please make some!");
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    _flags = EditorGUILayout.MaskField ("Role Containers", _flags, _RoleContainerLabels);
                }
            }

            List<RoleContainer> selectedOptions = new List<RoleContainer>();
            for (int i = 0; i < _RoleContainerLabels.Length; i++)
            {
                if ((_flags & (1 << i )) == (1 << i) ) selectedOptions.Add(_allRoleContainers[i]);
            }

            //Save data to _Script
            _script._roleContainers = new RoleContainer[selectedOptions.Count];
            //Debug.Log("_roleContainers: " + _script._roleContainers.Length);
            for (int i = 0; i < selectedOptions.Count; i++)
            {
                //Debug.Log("Adding " + selectedOptions[i].roleName);
                _script._roleContainers[i] = selectedOptions[i];
            }
            //_script._roleContainers = selectedOptions.ToArray();
            _script.roleContainerIDs = new string[_script._roleContainers.Length];
            for (int i = 0; i < _script._roleContainers.Length; i++)
            {
                //Debug.Log("Adding " + _script._roleContainers[i].roleId);
                _script.roleContainerIDs[i] = _script._roleContainers[i].roleId;
            }
        }

        public static void DrawStaffSupporterSelector(DisBridgePlugin _script)
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Staff: ",GUILayout.MinWidth(0f));
                _script.useStaff = EditorGUILayout.Toggle(_script.useStaff,GUILayout.Width(25f));
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Supporters: ",GUILayout.MinWidth(0f));
                _script.useSupporters = EditorGUILayout.Toggle(_script.useSupporters,GUILayout.Width(25f));
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Boosters: ",GUILayout.MinWidth(0f));
                _script.useBoosters = EditorGUILayout.Toggle(_script.useBoosters,GUILayout.Width(25f));
            }
            EditorGUILayout.EndHorizontal();
        }
        
        public static bool DrawDebug(DisBridgePlugin _script, ref bool _debug)
        {
            _debug = EditorGUILayout.Toggle("Advanced Settings",_debug);
            if (_debug)
            {
                _script.isDebug = EditorGUILayout.Toggle("Debug Mode",_script.isDebug);
                EditorGUILayout.HelpBox($"You probably shouldn't touch these!", MessageType.Warning);
                
                return true;
            }
            return false;
        }

        public static void DrawAdvSettingsButton(ref bool _state)
        {
            if (_state)
            {
                if (GUILayout.Button("Hide Advance Settings"))
                {
                    _state = false;
                }
                EditorGUILayout.HelpBox("These settings are for advanced users doing custom setups. Please know what you're doing before touching these.", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Show Advance Settings"))
                {
                    _state = true;
                }
            }
        }

        public static void DrawDisBridgeLogo(bool drawLinks = true, string customUrl = "")
        {
            GUIStyle owo2 = new GUIStyle(EditorStyles.helpBox);
            owo2.normal.background = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.udonvr.disbridge/Assets/BG_Blue.png", typeof(Texture2D));

            using (new EditorGUILayout.HorizontalScope(owo2))
            {
                GUIStyle owo = new GUIStyle();
                owo.alignment = TextAnchor.MiddleCenter;
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.udonvr.disbridge/Assets/4K-Transparent-Disbridge-Logo-White.png", typeof(Texture2D));
                EditorGUILayout.LabelField(new GUIContent("", ""), GUILayout.MinWidth(0f));
                GUILayout.Box(texture,owo, GUILayout.Width(300f),GUILayout.Height(100f));
                EditorGUILayout.LabelField(new GUIContent("", ""), GUILayout.MinWidth(0f));
            }

            if (drawLinks)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    if (GUILayout.Button("Documentation"))
                    {
                        if (string.IsNullOrWhiteSpace(customUrl))
                        {
                            Application.OpenURL("http://wiki.disbridge.com/");
                        }
                        else
                        {
                            int _i = EditorUtility.DisplayDialogComplex("Open Documentation",
                                "Do you want to go to the Wiki's Home Page or the page for this Prefab?","Home Page",
                                "This Prefab", "Cancel");
                            switch (_i)
                            {
                                case 0: // ok
                                    Application.OpenURL("http://wiki.disbridge.com/");
                                    break;
                                case 1: // cancel
                                    Application.OpenURL(customUrl);
                                    break;
                                case 2: // alt
                                    break;
                            }
                        }
                    }
                    if (GUILayout.Button("Disbridge.com"))
                    {
                        Application.OpenURL("http://disbridge.com/");
                    }
                    if (GUILayout.Button("Discord"))
                    {
                        Application.OpenURL("https://discord.gg/U33qg5r6Be");
                    }
                    if (GUILayout.Button("Link Accounts"))
                    {
                        Application.OpenURL("https://vrc.disbridge.com/");
                    }
                }
            }
        }

        public static void DrawPlayerMoverField(ref PlayerMover _playerMover)
        {
            GUILayout.BeginHorizontal();
            _playerMover = (PlayerMover)EditorGUILayout.ObjectField(_playerMover, typeof(PlayerMover), true);
            if (_playerMover == null)
            {
                PlayerMover owo = FindObjectOfType<PlayerMover>(true);
                if (owo != null)
                {
                    if (GUILayout.Button("Auto Bind", GUILayout.Width(100f)))
                    {
                        _playerMover = owo;
                    }
                }
                else
                {
                    if (GUILayout.Button("Create New", GUILayout.Width(100f)))
                    {
                        GameObject obj = CreatePrefabs.CreateDisBridgePrefab("Plugins/PlayerMover.prefab",false, false);
                        _playerMover = obj.GetComponent<PlayerMover>();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        public static void DrawNotificationField(ref NotificationSystem _notificationSystem)
        {
            GUILayout.BeginHorizontal();
            _notificationSystem = (NotificationSystem)EditorGUILayout.ObjectField(_notificationSystem, typeof(NotificationSystem), true);
            if (_notificationSystem == null)
            {
                NotificationSystem owo = FindObjectOfType<NotificationSystem>(true);
                if (owo != null)
                {
                    if (GUILayout.Button("Auto Bind", GUILayout.Width(100f)))
                    {
                        _notificationSystem = owo;
                    }
                }
                else
                {
                    if (GUILayout.Button("Create New", GUILayout.Width(100f)))
                    {
                        _notificationSystem = CreateNotificationSystem();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        public static NotificationSystem CreateNotificationSystem()
        {
            GameObject obj = CreatePrefabs.CreateDisBridgePrefab("Utils/Notification System.prefab",false, false);
            return obj.GetComponent<NotificationSystem>();
        }

        /// <summary>
        /// Draws a layer selection dropdown in a custom inspector, behaving like a normal inspector field.
        /// </summary>
        /// <param name="label">The label to display next to the dropdown.</param>
        /// <param name="currentLayer">The currently selected layer index.</param>
        /// <returns>The newly selected layer index.</returns>
        public static int DrawLayerDropdown(string label, int currentLayer)
        {
            var layerNames = new List<string>();
            var layerNumbers = new List<int>();

            // Add a "None" option to the top of the list.
            layerNames.Add("- NONE -");
            layerNumbers.Add(-1); // Use -1 to represent no layer selection.

            // Get all layers defined in the project.
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;

            // Populate our lists, skipping any empty/undefined layers.
            for (int i = 0; i < layers.Length; i++)
            {
                string layer = layers[i];
                if (!string.IsNullOrEmpty(layer))
                {
                    layerNames.Add($"[{i}] {layer}");
                    layerNumbers.Add(i);
                }
            }

            // Find the index of the currently selected layer in our list.
            int selectedIndex = layerNumbers.IndexOf(currentLayer);

            // If the current value isn't found (e.g., it was not in the list), default to "None".
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                // Draw the popup dropdown menu using EditorGUILayout.
                selectedIndex = EditorGUILayout.Popup(selectedIndex, layerNames.ToArray());
            }
            else
            {
                // Draw the popup dropdown menu using EditorGUILayout.
                selectedIndex = EditorGUILayout.Popup(label, selectedIndex, layerNames.ToArray());
            }
            

            // Return the actual layer number that corresponds to the selected index.
            return layerNumbers[selectedIndex];
        }

        public static int DrawLayerDropdown(int currentLayer)
        {
            return DrawLayerDropdown(null, currentLayer);
        }
        
        #region PermissionGroup
        
        // A private class to hold all the cached data for a single inspector instance.
        private class ListDataCache
        {
            public ReorderableList list;
            public PermissionGroup[] permissionGroups;
            public string[] dropdownOptions;
            public bool showPermissionGroups;
        }
        
        private static readonly Dictionary<int, ListDataCache> listCaches = new Dictionary<int, ListDataCache>();

        public static void DrawPermissionGroupsList(DisBridgePlugin _script)
        {
            // 1. Create the SerializedObject from the passed-in script. This is the key change.
            var serializedObject = new SerializedObject(_script);
            serializedObject.Update();

            // Use the script's instance ID as the unique key.
            int instanceId = _script.GetInstanceID();
            ListDataCache cache;

            if (!listCaches.TryGetValue(instanceId, out cache))
            {
                cache = new ListDataCache();
                
                cache.permissionGroups = FindObjectsByType<PermissionGroup>(FindObjectsSortMode.None);
                cache.dropdownOptions = new string[] { "None (Null)" }
                    .Concat(cache.permissionGroups.Select(pg => pg.gameObject.name))
                    .ToArray();

                var property = serializedObject.FindProperty("_permissionGroups");
                var list = new ReorderableList(serializedObject, property,
                    draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);
                
                // (Callbacks remain the same as before)
                list.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, property.displayName);
                };

                list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    var currentSelection = element.objectReferenceValue as PermissionGroup;

                    int selectedIndex = 0;
                    if (currentSelection != null)
                    {
                        int foundIndex = System.Array.IndexOf(cache.permissionGroups, currentSelection);
                        if (foundIndex != -1) selectedIndex = foundIndex + 1;
                    }
                    
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    
                    int newIndex = EditorGUI.Popup(rect, selectedIndex, cache.dropdownOptions);

                    if (newIndex != selectedIndex)
                    {
                        element.objectReferenceValue = (newIndex == 0) ? null : cache.permissionGroups[newIndex - 1];
                    }
                };
                
                cache.list = list;
                listCaches[instanceId] = cache;
            }
            
            cache.list.serializedProperty = serializedObject.FindProperty(nameof(DisBridgePlugin._permissionGroups));
            
            if (GUILayout.Button(cache.showPermissionGroups ? "Hide Permission Groups" : "Show Permission Groups"))
            {
                cache.showPermissionGroups = !cache.showPermissionGroups;
            }

            if (cache.showPermissionGroups)
            {
                cache.list.DoLayoutList();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
        
        #region Gizmos


        public static void DrawSpawnPoint(Transform target)
        {
            Color originalHandlesColor = Handles.color;
            
            DrawCircleAndArrow(target, new Color(0.58f,0.62f,1f));
            Handles.color = Color.black;
            Handles.DrawWireDisc(target.position + Vector3.up * 0.001f,Vector3.up, .25f, 10f);
            Handles.color = originalHandlesColor;
            DrawTransformHandles(target,false,true,false);

        }
        
        /// <summary>
        /// Draws a circle on top of a transform and an arrow pointing towards its local Z-axis (forward).
        /// This method uses UnityEditor.Handles and is visible in the Unity Editor's Scene view.
        /// It is intended for Editor scripts (e.g., custom inspectors, scene view tools).
        /// </summary>
        /// <param name="target">The Transform to draw on. Its position and rotation will be used.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color of the circle and arrow.</param>
        /// <param name="segments">The number of segments to use for drawing the circle. More segments mean a smoother circle.</param>
        /// <param name="arrowLength">The length of the arrow shaft.</param>
        /// <param name="arrowheadSize">The size of the arrowhead's wings.</param>
        /// <param name="verticalOffset">Vertical offset from the transform's position for the drawing.</param>
        public static void DrawCircleAndArrow(
            Transform target,
            Color color,
            float radius = .25f,
            float arrowLength = .5f,
            float arrowheadSize = .1f,
            float verticalOffset = 0f)
        {
            if (target == null)
            {
                Debug.LogWarning("EditorDebugDraw.DrawCircleAndArrow: Target Transform is null.");
                return;
            }

            // Store the current Handles color to restore it later
            Color originalHandlesColor = Handles.color;
            Handles.color = color;

            Vector3 center = target.position + Vector3.up * verticalOffset; // Center of the circle, with vertical offset

            // --- Draw the Circle ---
            // Handles.DrawSolidDisc draws a filled circle in the Scene view.
            // The normal is Vector3.up to draw it on the XZ plane.
            Handles.DrawSolidDisc(center, Vector3.up, radius);

            // --- Draw the Arrow ---
            Vector3 arrowStart = center; // Arrow starts at the circle's center (with offset)
            Vector3 arrowEnd = center + target.forward * arrowLength; // Arrow points along local Z (forward)

            // Draw the main shaft of the arrow
            Handles.DrawLine(arrowStart, arrowEnd);

            // Draw the arrowhead (two lines forming a V shape)
            // Calculate directions for the arrowhead wings relative to the arrow's direction
            // We use target.forward and target.right to ensure the arrow is aligned with the target's local axes.
            Vector3 rightWingDirection = (target.forward * arrowheadSize) + (target.right * arrowheadSize * 0.5f);
            Vector3 leftWingDirection = (target.forward * arrowheadSize) - (target.right * arrowheadSize * 0.5f);

            // Calculate arrowhead points
            Vector3 arrowHeadRight = arrowEnd - rightWingDirection;
            Vector3 arrowHeadLeft = arrowEnd - leftWingDirection;

            // Draw the arrowhead wings
            Handles.DrawLine(arrowEnd, arrowHeadRight);
            Handles.DrawLine(arrowEnd, arrowHeadLeft);

            // Restore the original Handles color
            Handles.color = originalHandlesColor;
        }
        
        /// <summary>
        /// Draws standard Unity transform handles (position and rotation) for a given Transform.
        /// This method allows moving the transform and conditionally enabling rotation handles for specific axes.
        /// It is intended for Editor scripts (e.g., custom inspectors, scene view tools).
        /// </summary>
        /// <param name="target">The Transform to draw handles for and modify.</param>
        /// <param name="enableRotationX">If true, enables rotation handle for the X-axis (red).</param>
        /// <param name="enableRotationY">If true, enables rotation handle for the Y-axis (green).</param>
        /// <param name="enableRotationZ">If true, enables rotation handle for the Z-axis (blue).</param>
        /// <returns>The modified Transform (or the original if no changes).</returns>
        public static Transform DrawTransformHandles(
            Transform target,
            bool enableRotationX = true,
            bool enableRotationY = true,
            bool enableRotationZ = true)
        {
            if (target == null)
            {
                Debug.LogWarning("EditorDebugDraw.DrawTransformHandles: Target Transform is null.");
                return target;
            }

            // Get a dynamic size for the handles so they scale appropriately with Scene view zoom
            float handleSize = HandleUtility.GetHandleSize(target.position) * 0.8f; // 0.8f is a common multiplier

            // Store the current Handles color to restore it later
            Color originalHandlesColor = Handles.color;

            // --- Position Handle ---
            // Start tracking changes for the position handle
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.PositionHandle(target.position, target.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Transform");
                target.position = newPosition;
                EditorUtility.SetDirty(target);
            }

            // --- Individual Rotation Handles ---
            // We'll use Handles.Disc for individual axis rotation handles.
            // Begin tracking changes for all rotation handles combined.
            EditorGUI.BeginChangeCheck();
            Quaternion currentRotation = target.rotation; // Store current rotation to detect changes

            // Rotation X (Red)
            if (enableRotationX)
            {
                Handles.color = Handles.xAxisColor; // Set color to Unity's red for X-axis
                // Handles.Disc returns the new rotation of the disc after manipulation.
                // target.right is the world-space X-axis of the target.
                currentRotation = Handles.Disc(currentRotation, target.position, target.right, handleSize, false, 0f);
            }

            // Rotation Y (Green)
            if (enableRotationY)
            {
                Handles.color = Handles.yAxisColor; // Set color to Unity's green for Y-axis
                // target.up is the world-space Y-axis of the target.
                currentRotation = Handles.Disc(currentRotation, target.position, target.up, handleSize, false, 0f);
            }

            // Rotation Z (Blue)
            if (enableRotationZ)
            {
                Handles.color = Handles.zAxisColor; // Set color to Unity's blue for Z-axis
                // target.forward is the world-space Z-axis of the target.
                currentRotation = Handles.Disc(currentRotation, target.position, target.forward, handleSize, false, 0f);
            }

            // Check if any of the rotation handles caused a change during this frame
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rotate Transform");
                target.rotation = currentRotation; // Apply the accumulated rotation
                EditorUtility.SetDirty(target);
            }

            // Restore the original Handles color
            Handles.color = originalHandlesColor;

            return target; // Return the potentially modified transform
        }
        #endregion
    }
}


