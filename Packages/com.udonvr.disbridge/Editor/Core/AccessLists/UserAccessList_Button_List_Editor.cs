using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UdonVr.DisBridge.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(UserAccessList_Controller))]
    public class UserAccessList_Button_List_Editor : Editor
    {
        private UserAccessList_Controller myScript;
        private bool advanceMode = false;
        
        private static readonly List<int> _dirtyInstanceIDs = new List<int>();
        private static bool _isValidationScheduled = false;

        private class ListDataCache
        {
            public ReorderableList list;
            public UserAccessList[] accessLists;
            public string[] dropdownOptions;
        }
        
        private static readonly Dictionary<int, ListDataCache> listCaches = new Dictionary<int, ListDataCache>();

        private void OnEnable()
        {
            myScript = (UserAccessList_Controller)target;
            // Invalidate cache on refocus to pick up scene changes.
            // no need to delay this, it happens on focus only
            listCaches.Remove(myScript.GetInstanceID());
        }

        public override void OnInspectorGUI()
        {
            // Use the editor's built-in serializedObject. This is the single source of truth.
            serializedObject.Update();

            EditorFunctions.DrawDisBridgeLogo();
            DrawColors();
            DrawAccessLists();
            DrawAdvanceMode();

            // Apply all changes made to the single serializedObject at the end.
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawColors()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Permission Colors:");
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("This is the color of the Preferred Owner." +
                                            "\nThe Preferred Owner is the first person who loads into the instance. " +
                                            "The Preferred Owner is also the one who does Permission Checks when they're in the Instance."
                        , MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UserAccessList_Controller.buttonColorPreferredOwner)), new GUIContent("Preferred Owner Color"));
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("This is the color of Admins." +
                                            "\nAdmins have access to assign and remove other users from the Access List. "
                        , MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UserAccessList_Controller.buttonColorPrivilegedMember)), new GUIContent("Privileged Member Color"));
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("This is the color of Members." +
                                            "\nMembers have access to the list, but can not to assign or remove other members to the list. "
                        , MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UserAccessList_Controller.buttonColorMember)), new GUIContent("Member Color"));
                }
            }
        }
        
        private void DrawAdvanceMode()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("Advanced Mode"))
                {
                    advanceMode = !advanceMode;
                }

                if (advanceMode)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.HelpBox(
                            "This is for making Custom Setups, you probably shouldn't touch these if you're using the default Prefab.",
                            MessageType.Error);
                        DrawDefaultInspector();
                    }
                }
            }
        }

        private void DrawAccessLists()
        {
            int instanceId = myScript.GetInstanceID();
            
            if (!listCaches.TryGetValue(instanceId, out ListDataCache cache))
            {
                cache = InitializeCacheAndList(instanceId);
            }
            
            // Re-bind the list to the current serializedObject's property each frame.
            cache.list.serializedProperty = serializedObject.FindProperty(nameof(UserAccessList_Controller.userAccessLists));
            
            EditorGUI.BeginChangeCheck();
            cache.list.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                // A change happened (e.g., renaming). Invalidate the cache to refresh dropdowns.
                ScheduleCacheInvalidation();
                //listCaches.Remove(instanceId);
            }
        }

        private ListDataCache InitializeCacheAndList(int instanceId)
        {
            var cache = new ListDataCache();
            cache.accessLists = FindObjectsByType<UserAccessList>(FindObjectsSortMode.None);
            cache.dropdownOptions = new string[] { "None (Null)" }
                .Concat(cache.accessLists.Select(pg => pg != null ? pg.accessListName : "Empty"))
                .ToArray();
            
            var property = serializedObject.FindProperty(nameof(UserAccessList_Controller.userAccessLists));
            var list = new ReorderableList(serializedObject, property,
                draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);
            
            list.onAddCallback = (ReorderableList l) =>
            // Callback for adding a new array element
            {
                if (EditorUtility.DisplayDialog("Create New Access List", "Do you want to create a new Access List?", "Create New Access List", "Create Blank"))
                {
                    // Create a new AccessList at the end of the array.
                    CreateAccessList(this.serializedObject, -1);
                }
                else
                {
                    // Create a blank entry at the bottom of the array
                    var listProperty = l.serializedProperty;
                    listProperty.arraySize++;
                    var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                    newElement.objectReferenceValue = null;
                }
            };
            
            list.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, property.displayName);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var currentSelection = element.objectReferenceValue as UserAccessList;

                int selectedIndex = 0;
                if (currentSelection != null)
                {
                    int foundIndex = System.Array.IndexOf(cache.accessLists, currentSelection);
                    if (foundIndex != -1) selectedIndex = foundIndex + 1;
                }
                
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                
                float gap = 5f;
                float dropdownWidth = rect.width * 0.3f;
                Rect dropdownRect = new Rect(rect.x, rect.y, dropdownWidth, rect.height);
                float textFieldWidth = rect.width * 0.5f;
                Rect textFieldRect = new Rect(dropdownRect.xMax + gap, rect.y, textFieldWidth, rect.height);
                float buttonWidth = rect.width * 0.2f - (gap * 2);
                Rect openButtonRect = new Rect(textFieldRect.xMax + gap, rect.y, buttonWidth, rect.height);

                int newIndex = EditorGUI.Popup(dropdownRect, selectedIndex, cache.dropdownOptions);
                if (newIndex != selectedIndex)
                {
                    element.objectReferenceValue = (newIndex == 0) ? null : cache.accessLists[newIndex - 1];
                }
                
                if (currentSelection != null)
                {
                    var listNameObject = new SerializedObject(currentSelection);
                    var listNameProperty = listNameObject.FindProperty(nameof(UserAccessList.accessListName));
                    EditorGUI.PropertyField(textFieldRect, listNameProperty, GUIContent.none);
                    listNameObject.ApplyModifiedProperties();
                    
                    if (GUI.Button(openButtonRect, "Open"))
                    {
                        Selection.activeObject = currentSelection.gameObject;
                    }
                }
                else
                {
                    EditorGUI.LabelField(textFieldRect, "(No Access List selected)");
                    if (GUI.Button(openButtonRect, "Create"))
                    {
                        // Create a new AccessList at the given index.
                        CreateAccessList(this.serializedObject, index);
                    }
                }
            };
            
            cache.list = list;
            listCaches[instanceId] = cache;
            return cache;
        }
        
        private void CreateAccessList(SerializedObject mainSerializedObject, int _index)
        {
            AccessListNamePopup.Show((newName) =>
            {
                var newAccessListObject = new GameObject(newName);
                Undo.RegisterCreatedObjectUndo(newAccessListObject, "Create " + newName);
                Undo.SetTransformParent(newAccessListObject.transform, myScript.transform, "Set Parent");
                
                var newAccessList = UdonSharpUndo.AddComponent<UserAccessList>(newAccessListObject);
                var newAccessListSO = new SerializedObject(newAccessList);
                
                newAccessListSO.FindProperty("accessListName").stringValue = newName;
                newAccessListSO.FindProperty("disBridge").objectReferenceValue = FindObjectOfType<PluginManager>();
                newAccessListSO.FindProperty("notificationSystem").objectReferenceValue = FindOrCreateNotificationSystem();
                newAccessListSO.ApplyModifiedProperties();

                var listProperty = mainSerializedObject.FindProperty(nameof(UserAccessList_Controller.userAccessLists));
                
                // If index is -1, we are adding a new element to the end.
                if (_index == -1)
                {
                    _index = listProperty.arraySize;
                    listProperty.arraySize++;
                }

                var element = listProperty.GetArrayElementAtIndex(_index);
                element.objectReferenceValue = newAccessList;
                
                // We must apply the changes immediately within the callback to commit the assignment.
                mainSerializedObject.ApplyModifiedProperties();

                // Invalidate the cache to refresh the UI with the new list.
                ScheduleCacheInvalidation();
                //listCaches.Remove(myScript.GetInstanceID());
                if (EditorUtility.DisplayDialog($"Swap to Access List?", $"Do you want to swap to your newly created Access List for finishing setup?", $"Swap to new Access List","Stay on the Controller"))
                {
                    EditorGUIUtility.PingObject(newAccessListObject);
                    Selection.activeObject = newAccessListObject;
                }
                else
                {
                    EditorGUIUtility.PingObject(newAccessListObject);
                }
            });
        }

        private NotificationSystem FindOrCreateNotificationSystem()
        {
            NotificationSystem _ns = GameObject.FindObjectOfType<NotificationSystem>();
            if (_ns == null)
            {
                if (EditorUtility.DisplayDialog($"No Notification System Found", $"There was no Notification System found in your scene.\nDo you want to create one?" +
                        $"\n\nThis is *HIGHLY RECOMMENDED* but not required." +
                        $"\nThe Notification System allows users to receive feedback when they get assigned a role, or when they they are denied access to update a permission." +
                        $"\nWithout a Notification System, there is no feedback.", $"Create one (recommended)","No"))
                {
                    _ns = EditorFunctions.CreateNotificationSystem();
                }
                else
                {

                }
            }
            return _ns;
        }
        
        private static void InvalidateCaches()
        {
            // Go through each ID in our "to-do" list
            foreach (var instanceId in _dirtyInstanceIDs)
            {
                listCaches.Remove(instanceId);
            }

            // Clear the list and reset the flag
            _dirtyInstanceIDs.Clear();
            _isValidationScheduled = false;
        }
        private void ScheduleCacheInvalidation()
        {
            int instanceId = myScript.GetInstanceID();

            // Add this inspector's ID to the "to-do" list if it's not already there
            if (!_dirtyInstanceIDs.Contains(instanceId))
            {
                _dirtyInstanceIDs.Add(instanceId);
            }

            // If a delayed call isn't already scheduled, schedule one now.
            // This prevents subscribing the same method multiple times.
            if (!_isValidationScheduled)
            {
                EditorApplication.delayCall += InvalidateCaches;
                _isValidationScheduled = true;
            }
        }
    }
    
    public class AccessListNamePopup : EditorWindow
    {
        private string inputText = "New Access List";
        private System.Action<string> onConfirm; // This is the "callback" action
        
        /// <summary>
        /// Creates and shows the modal popup window.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="onConfirmAction">The action to run when the user clicks 'Confirm'. It receives the entered string.</param>
        public static void Show(System.Action<string> onConfirmAction)
        {
            // Get a reference to the window, or create it if it doesn't exist.
            var window = GetWindow<AccessListNamePopup>(true,"Name AccessList", true);
            window.onConfirm = onConfirmAction;
            // Set a minimum size for the window so it's not too small.
            window.minSize = new Vector2(100, 200);
            // Show it as a modal window, which blocks other editor interaction.
            window.ShowModal();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Enter a name for the AccessList:", EditorStyles.wordWrappedLabel);
            

            GUI.SetNextControlName("NameTextField");
            inputText = EditorGUILayout.TextField(inputText);
            
            EditorGUI.FocusTextInControl("NameTextField");

            GUILayout.Space(10);
            
            if (GUILayout.Button("Confirm"))
            {
                ConfirmAndClose();
            }
            
            // Also handle the user pressing the Enter/Return key.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                ConfirmAndClose();
            }
        }
        private void ConfirmAndClose()
        {
            if (!string.IsNullOrWhiteSpace(inputText))
            {
                onConfirm?.Invoke(inputText);
                this.Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Name", "The name cannot be empty.", "OK");
            }
        }
    }
}
