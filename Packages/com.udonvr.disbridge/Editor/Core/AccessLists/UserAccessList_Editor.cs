using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(UserAccessList))]
    public class UserAccessList_Editor : Editor
    {
        UserAccessList myScript;
        private bool advanceMode = false;
        private UserAccessList[] AllAccessLists;

        private void OnEnable()
        {
            myScript = (UserAccessList)target;

        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            EditorFunctions.DrawDisBridgeLogo();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorFunctions.DrawPluginManagerCheck(myScript,false,true);
                EditorFunctions.DrawRoleContainerSelector(myScript);
                EditorFunctions.DrawStaffSupporterSelector(myScript);
                EditorFunctions.DrawPermissionGroupsList(myScript);
            }

            DrawSettings();

            DrawPermissionSafeguardCheck();

            DrawAdvanceMode();
            //Do Things

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                myScript.accessListName = EditorGUILayout.TextField("Access List Name: ", myScript.accessListName);
                DrawParentAccessList();
                PreferredOwnerSettings();
                DrawNotificationSystem();
            }
        }

        private void DrawParentAccessList()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Parent Access List:");
                DrawParentAccessListDropdown(myScript, ref AllAccessLists);
                //DrawAccessListDropdown(ref myScript.privilegedUserAccessList, ref AllAccessLists, myScript);
                if (myScript.privilegedUserAccessList == myScript)
                {
                    Debug.LogError("[Access List] You can not assign a list to be a parent of itself!");
                    myScript.privilegedUserAccessList = null;
                }
            }
        }

        private void DrawNotificationSystem()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorFunctions.DrawNotificationField(ref myScript.notificationSystem);
                if (myScript.notificationSystem == null)
                {
                    EditorGUILayout.HelpBox("No Notification System bound. This is not recommend!\n" +
                                            "While this won't break anything. No user feedback will be given when " +
                                            "adding or removing users from the Access List.\n" +
                                            "!!Please add one!!!", MessageType.Warning);
                }
            }
        }

        private void PreferredOwnerSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = myScript.usePreferredOwner ? Color.green : Color.red;
                if (GUILayout.Button(myScript.usePreferredOwner ? "Disable Host" : "Enable Host"))
                {
                    myScript.usePreferredOwner = !myScript.usePreferredOwner;
                }

                GUI.backgroundColor = Color.white;
                if (myScript.usePreferredOwner)
                {
                    EditorGUILayout.HelpBox(
                        "The Host will be the first person who joins the instance. This person will be responsible for Approving or Denying permission changes.",
                        MessageType.Info);
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Host can update Permissions");
                        GUI.backgroundColor = myScript.allowPreferredOwnerToChangePermissions ? Color.green : Color.red;
                        if (GUILayout.Button(myScript.allowPreferredOwnerToChangePermissions ? "Enabled" : "Disabled",
                                GUILayout.Width(100f)))
                        {
                            myScript.allowPreferredOwnerToChangePermissions =
                                !myScript.allowPreferredOwnerToChangePermissions;
                        }

                        GUI.backgroundColor = Color.white;
                        
                    }
                    // TODO add in other host options here
                }

                if (!myScript.usePreferredOwner)
                {
                    EditorGUILayout.HelpBox("Host is OFF. It is recommended to turn Host ON.\n" +
                                            "Host is used as a Security Measure to protect against Malicious Client Usage by having the AccessList prefer a single user for making Access List Changes, and disallowing Owner changes. " +
                                            "Having this Off may allow Malicious Client Users to take ownership of the Access List and make Permission Changes globally.",
                        MessageType.Warning);
                }
            }
        }

        private void DrawPermissionSafeguardCheck()
        {
            if (myScript.allowPreferredOwnerToChangePermissions && myScript.usePreferredOwner) return;
            if (myScript.useSupporters || myScript.useStaff || myScript.useBoosters) return;
            if (myScript._roleContainers != null && myScript._roleContainers.Length != 0) return;
            if (myScript.privilegedUserAccessList != null) return;
            EditorGUILayout.HelpBox("There are no Permissions Assigned!\n" +
                                    "This AccessList does not have any user that will beable to interact with it ever!\n" +
                                    "\n" +
                                    "Please assign either a DisBridge Role, Parent Access List, or allow the Preferred Owner to update permissions.",
                MessageType.Error);
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

        /// <summary>
        /// Draws a dropdown menu for selecting a UserAccessList from the scene.
        /// </summary>
        /// <param name="_value">The currently selected UserAccessList. This will be updated by the selection.</param>
        /// <param name="AllAccessLists">A cached array of all lists. Will be populated if null.</param>
        /// <param name="_ignoredAccessList">An optional UserAccessList to exclude from the dropdown.</param>
        public static void DrawAccessListDropdown(ref UserAccessList _value, ref UserAccessList[] AllAccessLists,
            UserAccessList _ignoredAccessList = null)
        {
            if (AllAccessLists == null)
            {
                AllAccessLists = GameObject.FindObjectsOfType<UserAccessList>();
            }

            var filteredLists = AllAccessLists.ToList();

            if (_ignoredAccessList != null)
            {
                filteredLists.Remove(_ignoredAccessList);
            }

            var displayOptions = new List<string> { "None" };
            displayOptions.AddRange(filteredLists.Select(list => list.accessListName));



            // Determine the current selection's index. Default to 0 ("None").
            int selectedIndex = 0;
            if (_value != null)
            {
                int valueIndex = filteredLists.IndexOf(_value);
                if (valueIndex != -1)
                {
                    selectedIndex = valueIndex + 1;
                }
            }

            // Draw the dropdown popup field in the inspector
            int newIndex = EditorGUILayout.Popup(selectedIndex, displayOptions.ToArray());

            // If the user made a new selection, update the referenced _value
            if (newIndex != selectedIndex)
            {
                _value = newIndex == 0 ? null : filteredLists[newIndex - 1];
            }
        }

        /// <summary>
        /// Draws a dropdown for selecting a parent, preventing circular dependencies.
        /// </summary>
        /// <param name="self">The UserAccessList component that this editor is for.</param>
        /// /// <param name="AllAccessLists">Cached array for all the current Access Lists.</param>
        public static void DrawParentAccessListDropdown(UserAccessList self, ref UserAccessList[] AllAccessLists)
        {
            AllAccessLists ??= GameObject.FindObjectsOfType<UserAccessList>();
            UserAccessList currentParent = self.privilegedUserAccessList;
            
            string buttonText = currentParent != null ? currentParent.accessListName : "None";

            if (GUILayout.Button(new GUIContent(buttonText, "Select a parent access list."), EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();

                // Add the "None" option.
                // The lambda now captures 'self' and modifies its 'parentAccessList' property.
                menu.AddItem(new GUIContent("None"), currentParent == null, () =>
                {
                    self.privilegedUserAccessList = null;
                    // Manually mark the object as dirty so Unity knows to save the change.
                    EditorUtility.SetDirty(self);
                });

                menu.AddSeparator("");

                foreach (var list in AllAccessLists.OrderBy(l => l.name))
                {
                    if (list == self)
                    {
                        continue;
                    }

                    if (IsDescendantOf(list, self))
                    {
                        menu.AddDisabledItem(new GUIContent($"{list.accessListName} (Child of this list)"), true);
                    }
                    else
                    {
                        // The lambda captures 'self' and the 'selectedList' for the assignment.
                        menu.AddItem(new GUIContent(list.accessListName), list == currentParent, (selectedList) =>
                        {
                            self.privilegedUserAccessList = (UserAccessList)selectedList;
                            EditorUtility.SetDirty(self);
                        }, list);
                    }
                }

                menu.ShowAsContext();
            }
        }

        public static bool IsDescendantOf(UserAccessList potentialDescendant, UserAccessList potentialAncestor)
        {
            if (potentialDescendant == null || potentialAncestor == null)
            {
                return false;
            }

            // Keep track of visited lists to detect a loop.
            var visited = new HashSet<UserAccessList>();
            UserAccessList current = potentialDescendant.privilegedUserAccessList;

            while (current != null)
            {
                // ancestor found in the chain.
                if (current == potentialAncestor)
                {
                    return true;
                }

                // If we have already seen this 'current' node, we are in a loop.
                if (visited.Contains(current))
                {
                    // Break out to prevent an editor hang.
                    Debug.LogError(
                        $"Circular dependency detected in UserAccessList hierarchy involving '{current.name}'.",
                        current);
                    if (EditorUtility.DisplayDialog($"Access List Loop Found",
                            $"You have a loop in your Access List parent chain. Please fix it.", $""))
                        return false;
                }

                // Mark the current list as visited.
                visited.Add(current);

                // Move up the chain.
                current = current.privilegedUserAccessList;
            }
            return false;
        }
    }
}