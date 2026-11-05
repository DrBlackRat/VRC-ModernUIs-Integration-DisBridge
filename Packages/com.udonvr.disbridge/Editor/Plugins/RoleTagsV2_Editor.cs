//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using UdonVR.DisBridge.Plugins.RoleTagsV2;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.Economy;

namespace UdonVR.DisBridge.Editors.Plugins
{
    [CustomEditor(typeof(RoleTagsV2_Controller))]
    [CanEditMultipleObjects]
    public class RoleTagsV2_Editor : UnityEditor.Editor
    {
        private string[] _roleModePopup = new string[5] { "All Roles", "Staff", "Supporters", "Select Roles","Manual Setup"};
        private string[] _menuModePopup = new string[2] { "Custom Menu", "Auto Setup"};
        private bool __debug = false;

        private bool[] manualSetupBools;

        private RoleTagsV2_Controller _script;

        private void ManualSetupBools()
        {
            if (manualSetupBools == null || manualSetupBools.Length != _script.c_Names.Length)
            {
                manualSetupBools = new bool[_script.c_Names.Length];
            }
        }
        public override void OnInspectorGUI()
        {
            _script = (RoleTagsV2_Controller)target;
            Undo.RecordObject(_script,$"Generic Undo event on {nameof(RoleTagsV2_Editor)} -- {target.name}");
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.PlaymodeWarning(_script);
            
            EditorFunctions.DrawPluginManagerCheck(_script,false);
            EditorFunctions.GuiLine();
            DrawHeadtracker();
            EditorFunctions.GuiLine();
            DrawMenuMode();
            EditorFunctions.GuiLine();
            DrawMenuType();
            EditorFunctions.GuiLine();
            DrawAdminRoles();
            if (EditorFunctions.DrawDebug(_script,ref __debug)) DrawDefaultInspector();
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }

        private void DrawAdminRoles()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Undo.RecordObject(_script,$"Changed Admin Roles");
                EditorGUILayout.HelpBox($"Admin Roles\nMembers of these roles will have access to display any role.", MessageType.None);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Admin")) AddAdmin();
                if (GUILayout.Button("Remove Admin")) RemoveAdmin();
                EditorGUILayout.EndHorizontal();
                if (_script.admins == null) _script.admins = new RoleContainer[0];
                for (int i = 0; i < _script.admins.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _script.admins[i] = (RoleContainer)EditorGUILayout.ObjectField(_script.admins[i], typeof(RoleContainer), true);
                    // TODO remove button
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawMenuMode()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _script.roleMode = (byte)EditorGUILayout.Popup("Mode:",_script.roleMode,_roleModePopup);
                //0 = all roles
                //1 = Staff
                //2 = Supporters
                //3 = Select Roles
                switch (_script.roleMode)
                {
                    case 0:
                        EditorGUILayout.HelpBox($"All of your roles will be selectable.", MessageType.None);
                        break;
                    case 1:
                        EditorGUILayout.HelpBox($"All of your Staff roles will be selectable.", MessageType.None);
                        break;
                    case 2:
                        EditorGUILayout.HelpBox($"All of your Supporter roles will be selectable.", MessageType.None);
                        break;
                    case 3:
                        EditorGUILayout.HelpBox($"Select what roles you want to use below.", MessageType.None);
                        EditorFunctions.DrawRoleContainerSelector(_script);
                        break;
                    case 4:
                        EditorGUILayout.HelpBox($"Manual Setup mode.\nYou will get more control over how tags are given out but will also need to manually setup new Roles added to DisBridge.", MessageType.None);
                        DrawManualMode();
                        break;
                }
                _script.hideButtons = EditorGUILayout.Toggle("Hide Button if you can not use Tag",_script.hideButtons);
            }
        }

        private void DrawMenuType()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _script.menuMode = (byte)EditorGUILayout.Popup("Menu Type:",_script.menuMode,_menuModePopup);
                switch (_script.menuMode)
                {
                    case 0:
                        EditorGUILayout.HelpBox($"You will need to setup your own menu for RoleTags. This is for Advanced Users and will not work out of the box.\nIt is recommended to use Auto Setup.", MessageType.Warning);
                        _script.noTagButton = (RoleTagV2_Button)EditorGUILayout.ObjectField($"\"No Tag\" button: (optional)", _script.noTagButton, typeof(RoleTagV2_Button), true);
                        if (_script.noTagButton != null)
                        {
                            Undo.RecordObject(_script.noTagButton,$"Generic Undo event on {nameof(RoleTagsV2_Editor)} -- {_script.noTagButton.name}");
                            _script.noTagButton.thisTagIndex = -1;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.noTagButton);
                        }
                        if (_script.tagButtons.Length != _script.c_DefaultAssign.Length)
                        {
                            Array.Resize(ref _script.tagButtons,_script.c_DefaultAssign.Length);
                        }
                        for (int i = 0; i < _script.tagButtons.Length; i++)
                        {
                            _script.tagButtons[i] = (RoleTagV2_Button)EditorGUILayout.ObjectField($"[{i}]\" {_script.c_Names[i]}\" button:", _script.tagButtons[i], typeof(RoleTagV2_Button), true);
                            if (_script.tagButtons[i] != null)
                            {
                                Undo.RecordObject(_script.tagButtons[i],$"Generic Undo event on {nameof(RoleTagsV2_Editor)} -- {_script.tagButtons[i].name}");
                                _script.tagButtons[i].thisTagIndex = i;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(_script.tagButtons[i]);
                            }
                        }

                        if (Array.IndexOf(_script.tagButtons, null) != -1)
                        {
                            EditorGUILayout.HelpBox($"!!! YOU HAVE NULL BUTTONS !!!\nplease make sure all of your buttons are filled in, as each roleTag needs a button assigned to it.", MessageType.Error);
                        }
                        break;
                    case 1:
                        EditorGUILayout.HelpBox($"Your menu will auto setup using the included Prefab.", MessageType.None);
                        break;
                }
            }
        }

        private void AddAdmin()
        {
            Undo.RecordObject(_script,"Add Admin");
            int _i = _script.admins.Length + 1;
            Array.Resize(ref _script.admins,_i );
        }

        private void RemoveAdmin()
        {
            Undo.RecordObject(_script,"Remove Admin");
            int _i = _script.admins.Length - 1;
            if (_i <= 0) _i = 0;
            Array.Resize(ref _script.admins,_i );
        }

        private void DrawHeadtracker()
        {
            Undo.RecordObject(_script,"Headtracker Edited");
            EditorGUILayout.BeginHorizontal();
            _script.useHeadtracker = EditorGUILayout.Toggle("use Headtracker", _script.useHeadtracker);
            EditorGUI.BeginDisabledGroup(!_script.useHeadtracker);
            _script.headTracker = (Transform)EditorGUILayout.ObjectField(_script.headTracker,typeof(Transform), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawManualMode()
        {
            Undo.RecordObject(_script,$"Changed ManualMode Option");
            ManualSanityCheck();
            ManualSetupBools();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Tag")) AddManual();
            if (GUILayout.Button("Remove Tag")) RemoveManual();
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _script.c_Names.Length; i++)
            {
                ManualSetupTags(i);
            }
            _script.dontImportFromRoles = EditorGUILayout.Toggle("Dont import From Role Containers",_script.dontImportFromRoles,GUILayout.Width(20f));
        }

        private void ManualSetupTags(int i)
        {
            bool importFromRoleContainer = (!_script.dontImportFromRoles && _script.c_RoleContainers[i] != null);
            GUI.backgroundColor = importFromRoleContainer ? _script.c_RoleContainers[i].roleColor : _script.c_Colors[i];
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUI.backgroundColor = Color.white;
                    _script.c_DefaultAssign[i] = EditorGUILayout.Toggle(_script.c_DefaultAssign[i],GUILayout.Width(20f));
                    GUI.backgroundColor = importFromRoleContainer ? _script.c_RoleContainers[i].roleColor : _script.c_Colors[i];
                    if (importFromRoleContainer)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField(_script.c_RoleContainers[i].name);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        _script.c_Names[i] = EditorGUILayout.TextField(_script.c_Names[i]);
                    }
                    
                    if (GUILayout.Button(manualSetupBools[i] ? "Close" : "Open", GUILayout.Width(100f)))
                    {
                        manualSetupBools[i] = !manualSetupBools[i];
                    }
                }

                if (!manualSetupBools[i])
                {
                    GUI.backgroundColor = Color.white;
                    return;
                }
                    
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if (importFromRoleContainer)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ColorField(_script.c_RoleContainers[i].roleColor);
                        EditorGUILayout.ObjectField(_script.c_RoleContainers[i].roleIcon, typeof(Sprite), false);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        _script.c_Colors[i] = EditorGUILayout.ColorField(_script.c_Colors[i]);
                        _script.c_Icons[i] = (Sprite)EditorGUILayout.ObjectField(_script.c_Icons[i], typeof(Sprite), false);
                    }

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Show Tag", "This is for hiding the tag when using physical tags with GameObjects."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(100f));
                        if (GUILayout.Button(_script.c_useUiTag[i]?"Enabled":"Disabled"))
                        {
                            _script.c_useUiTag[i] =  !_script.c_useUiTag[i];
                        }
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Disable Lookat:", "Disables the LookAt Component for the tag."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(100f));
                    GUI.backgroundColor = importFromRoleContainer ? _script.c_RoleContainers[i].roleColor : _script.c_Colors[i];
                    _script.c_DontUseLookAt[i] = EditorGUILayout.Toggle(_script.c_DontUseLookAt[i], GUILayout.Width(13f));
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(_script.c_DontUseLookAt[i]);
                    _script.c_LookAtConstraints[i] = (LookAtConstraint)EditorGUILayout.ObjectField(_script.c_LookAtConstraints[i], typeof(LookAtConstraint), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    _script.c_RoleContainers[i] = (RoleContainer)EditorGUILayout.ObjectField(_script.c_RoleContainers[i], typeof(RoleContainer), true);
                    _script.c_UdonProducts[i] = (UdonProduct)EditorGUILayout.ObjectField(_script.c_UdonProducts[i], typeof(UdonProduct), false);
                    _script.c_PermissionGroups[i] = (PermissionGroup)EditorGUILayout.ObjectField(_script.c_PermissionGroups[i], typeof(PermissionGroup), true);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void ManualSanityCheck()
        {
            if (_script.c_Names == null) _script.c_Names = new string[0];
            int _i = _script.c_Names.Length;

            if (_script.c_DefaultAssign == null || _script.c_DefaultAssign.Length != _i) { Array.Resize(ref _script.c_DefaultAssign,_i); }
            if (_script.c_Colors == null || _script.c_Colors.Length != _i) { Array.Resize(ref _script.c_Colors,_i); }
            if (_script.c_Icons == null || _script.c_Icons.Length != _i) { Array.Resize(ref _script.c_Icons,_i); }
            if (_script.c_RoleContainers == null || _script.c_RoleContainers.Length != _i) { Array.Resize(ref _script.c_RoleContainers,_i); }
            if (_script.c_UdonProducts == null || _script.c_UdonProducts.Length != _i) { Array.Resize(ref _script.c_UdonProducts,_i); }
            if (_script.c_PermissionGroups == null || _script.c_PermissionGroups.Length != _i) { Array.Resize(ref _script.c_PermissionGroups,_i); }
            if (_script.c_DontUseLookAt == null || _script.c_DontUseLookAt.Length != _i) { Array.Resize(ref _script.c_DontUseLookAt,_i); }
            if (_script.c_LookAtConstraints == null || _script.c_LookAtConstraints.Length != _i) { Array.Resize(ref _script.c_LookAtConstraints,_i); }

            if (_script.c_useUiTag == null || _script.c_useUiTag.Length != _i)
            {
                Array.Resize(ref _script.c_useUiTag,_i);
                for (int i = 0; i < _script.c_useUiTag.Length; i++)
                {
                    _script.c_useUiTag[i] = true;
                }
            }
        }
        
        private void AddManual()
        {
            Undo.RecordObject(_script,"Add Tag");
            int _i = _script.c_Names.Length + 1;
            Array.Resize(ref _script.c_DefaultAssign,_i);
            _script.c_DefaultAssign[_i - 1] = true;
            Array.Resize(ref _script.c_Names,_i);
            Array.Resize(ref _script.c_Colors,_i);
            _script.c_Colors[_i - 1] = Color.white;
            Array.Resize(ref _script.c_Icons,_i);
            Array.Resize(ref _script.c_RoleContainers,_i);
            Array.Resize(ref _script.c_UdonProducts,_i);
            Array.Resize(ref _script.c_PermissionGroups,_i);
            Array.Resize(ref _script.c_DontUseLookAt,_i);
            Array.Resize(ref _script.c_LookAtConstraints,_i);
            Array.Resize(ref _script.c_useUiTag,_i);
            _script.c_useUiTag[_i - 1] = true;
        }

        private void RemoveManual()
        {
            Undo.RecordObject(_script,"Remove Tag");
            int _i = _script.c_Names.Length - 1;
            if (_i <= 0) _i = 0;
            Array.Resize(ref _script.c_Names,_i);
            Array.Resize(ref _script.c_Colors,_i);
            Array.Resize(ref _script.c_Icons,_i);
            Array.Resize(ref _script.c_RoleContainers,_i);
            Array.Resize(ref _script.c_UdonProducts,_i);
            Array.Resize(ref _script.c_PermissionGroups,_i);
            Array.Resize(ref _script.c_DontUseLookAt,_i);
            Array.Resize(ref _script.c_LookAtConstraints,_i);
            Array.Resize(ref _script.c_useUiTag,_i);
        }
    }
}