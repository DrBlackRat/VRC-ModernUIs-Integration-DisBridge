//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UdonVR.DisBridge.Editors;
using UdonVR.DisBridge.Plugins;
using UdonVR.DisBridge.Plugins.PrivateRooms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace UdonVR.DisBridge.Editors.Plugins
{
    [CustomEditor(typeof(PrivateRoom))]
    public class PrivateRoom_Editor : UnityEditor.Editor
    {
        bool advnaceMode = false;
        private PrivateRoom _script;

        private PrivateRoom[] rooms;
        // Start is called before the first frame update

        private void OnEnable()
        {
            rooms = GameObject.FindObjectsOfType<PrivateRoom>();

        }

        public override void OnInspectorGUI()
        {
            _script = (PrivateRoom)target;
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.PlaymodeWarning(_script);
            EditorFunctions.DrawPluginManagerCheck(_script, false, false);

            SetupCameras();
            
            DrawSettings();
            GUILayout.Space(10f);
            DrawDoorMode();
            GUILayout.Space(10f);
            DrawPlayerMover();
            GUILayout.Space(10f);
            AudioSettings();
            GUILayout.Space(10f);
            DrawRenderTextures();
            GUILayout.Space(10f);
            DrawAdminRoles();

            Undo.RecordObject(_script.insideDoor.insideDisplay.lockIcon, $"Undo on {_script.insideDoor.insideDisplay.lockIcon.name}");
            _script.insideDoor.insideDisplay.lockIcon.SetActive(_script.isLocked);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.lockIcon);
            
            Undo.RecordObject(_script.insideDoor.insideDisplay.unlockIcon, $"Undo on {_script.insideDoor.insideDisplay.unlockIcon.name}");
            _script.insideDoor.insideDisplay.unlockIcon.SetActive(!_script.isLocked);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.unlockIcon);

            Undo.RecordObject(_script.outsideDoor.outsideDisplay.lockIcon, $"Undo on {_script.outsideDoor.outsideDisplay.lockIcon.name}");
            _script.outsideDoor.outsideDisplay.lockIcon.SetActive(_script.isLocked);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.outsideDoor.outsideDisplay.lockIcon);
            
            Undo.RecordObject(_script.outsideDoor.outsideDisplay.unlockIcon, $"Undo on {_script.outsideDoor.outsideDisplay.unlockIcon.name}");
            _script.outsideDoor.outsideDisplay.unlockIcon.SetActive(!_script.isLocked);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.outsideDoor.outsideDisplay.unlockIcon);
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
            EditorFunctions.DrawAdvSettingsButton(ref advnaceMode);
            if (advnaceMode)
            {
                
                EditorGUI.BeginDisabledGroup(_script.disableUnlockVip);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Disable unlocks in public instances: ", GUILayout.MinWidth(0));
                    _script.disallowUnlockInPublic = EditorGUILayout.Toggle(_script.disallowUnlockInPublic, GUILayout.Width(25f));
                }
                EditorGUI.EndDisabledGroup();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Render Texture Generation Settings.", ""));
                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Vertical:", ""), GUILayout.MinWidth(0f), GUILayout.MaxWidth(100f));
                        _script._EDITOR_v = EditorGUILayout.IntField(_script._EDITOR_v);
                    }
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Horizontal:", ""), GUILayout.MinWidth(0f), GUILayout.MaxWidth(100f));
                        _script._EDITOR_h = EditorGUILayout.IntField(_script._EDITOR_h);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Room Message character limit:", ""), GUILayout.MinWidth(0f), GUILayout.MaxWidth(200f));
                    _script.messageLimit = EditorGUILayout.IntField(_script.messageLimit);
                }
                DrawDefaultInspector();
            }
        }

        #region Draw

        private void DrawPlayerMover()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Player Mover (optional, recommended)", "The Player Mover has the player fade in/out when teleporting."));
                Undo.RecordObject(_script, $"[VIP] Updated PlayerMover");
                EditorGUILayout.BeginHorizontal();
                EditorFunctions.DrawPlayerMoverField(ref _script.playerMover);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        
        private void DrawRenderTextures()
        {
            Undo.RecordObject(_script, $"[VIP] Render Textures");
            
            PrivateRoom_Display_Inside inside = _script.insideDoor.insideDisplay;
            PrivateRoom_Display_Outside outside = _script.outsideDoor.outsideDisplay;
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Intercom: ");
                if (GUILayout.Button(_script._EDITOR_Intercom ? "Collapse" : "Expand", GUILayout.Width(100f)))
                {
                    _script._EDITOR_Intercom = !_script._EDITOR_Intercom;
                }
                EditorGUILayout.EndHorizontal();
                if (!_script._EDITOR_Intercom) return;

                if (_script.insideDoor.insideDisplay == null)
                {
                    EditorGUILayout.HelpBox("Improper Setup, Inside room is null", MessageType.Error);
                    Undo.RecordObject(_script.insideDoor.insideDisplay, $"Undo on {inside.name}");
                    _script.insideDoor.insideDisplay = (PrivateRoom_Display_Inside)EditorGUILayout.ObjectField(_script.insideDoor.insideDisplay, typeof(PrivateRoom_Display_Inside), true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay);
                    return;
                }
                if (_script.insideDoor.outsideDisplay == null)
                {
                    EditorGUILayout.HelpBox("Improper Setup, Outside room is null", MessageType.Error);
                    Undo.RecordObject(_script.insideDoor.outsideDisplay, $"Undo on {_script.insideDoor.outsideDisplay.name}");
                    _script.insideDoor.outsideDisplay = (PrivateRoom_Display_Outside)EditorGUILayout.ObjectField(_script.insideDoor.outsideDisplay, typeof(PrivateRoom_Display_Outside), true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.outsideDisplay);
                    return;
                }

                if (_script.cameraRenderTexture == null || _script.outsideRenderTexture == null || _script.insideRenderTexture == null)
                {
                    EditorGUILayout.HelpBox("Improper Setup, Missing render texture.\nPlease Generate or assign your Render Textures.", MessageType.Error);
                }
                /*for (int i = 0; i < rooms.Length; i++)
                {
                    if (rooms[i] == _script) continue;
                    if (rooms[i].insideDoor.insideDisplay.insideImages == inside.insideImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                    else if (rooms[i].insideDoor.insideDisplay.outsideImages == inside.outsideImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                    else if (rooms[i].insideDoor.insideDisplay.cameraImages == inside.cameraImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                    else if (rooms[i].insideDoor.outsideDisplay.insideImages == outside.insideImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                    else if (rooms[i].insideDoor.outsideDisplay.outsideImages == outside.outsideImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                    else if (rooms[i].insideDoor.outsideDisplay.cameraImages == outside.cameraImages)
                    {
                        EditorGUILayout.HelpBox("Improper Setup, make Render Texture unique", MessageType.Error);
                        break;
                    }
                }*/

                if (GUILayout.Button("Generate RenderTextures"))
                {
                    GenerateRenderTextures();
                }
                
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Render Textures: ");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Background Camera: ", GUILayout.MinWidth(0f));
                    _script.cameraRenderTexture = (RenderTexture)EditorGUILayout.ObjectField(_script.cameraRenderTexture,typeof(RenderTexture), false);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Inside VideoCamera: ", GUILayout.MinWidth(0f));
                        _script.insideRenderTexture = (RenderTexture)EditorGUILayout.ObjectField(_script.insideRenderTexture, typeof(RenderTexture), false, GUILayout.Width(100f));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Outside VideoCamera: ", GUILayout.MinWidth(0f));
                        _script.outsideRenderTexture = (RenderTexture)EditorGUILayout.ObjectField(_script.outsideRenderTexture, typeof(RenderTexture), false, GUILayout.Width(100f));
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void DrawSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Room Name: ",GUILayout.Width(100f));
                Undo.RecordObject(_script, $"Undo on {_script.name}");
                _script.roomName = EditorGUILayout.TextField(_script.roomName);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Room Message: ",GUILayout.Width(100f));
                Undo.RecordObject(_script.insideDoor.insideDisplay, $"Undo on {_script.insideDoor.insideDisplay.name}");
                _script.insideDoor.insideDisplay.doorMessage = EditorGUILayout.TextArea(_script.insideDoor.insideDisplay.doorMessage,GUILayout.MinHeight(50f));
                PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay);
                EditorGUILayout.EndHorizontal();
            }
            
            SetupRoomName();
        }
        private void DrawDoorMode()
        {
            Undo.RecordObject(_script,"VIP DoorMode");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Door Mode: ");
                if (GUILayout.Button(_script._EDITOR_doorMode ? "Collapse" : "Expand", GUILayout.Width(100f)))
                {
                    _script._EDITOR_doorMode = !_script._EDITOR_doorMode;
                }
                EditorGUILayout.EndHorizontal();
                if (!_script._EDITOR_doorMode) return;
                
                
                int _tmpInt = 1;

                if (_script.isVip) _tmpInt = 2;
                else if (_script.disableUnlockVip) _tmpInt = 0;
                _tmpInt = EditorGUILayout.Popup(_tmpInt, new string[3] { "No Lock","Private","VIP"});

                if (_tmpInt == 0)
                {
                    EditorGUILayout.HelpBox("No Lock Mode:\nThe lock will be disabled. Anyone can enter or leave the room.", MessageType.None);
                    if (_script.insideDoor.insideDisplay.LockButton_Object != null)
                    {
                        Undo.RecordObject(_script.insideDoor.insideDisplay.LockButton_Object,"VIP DoorMode");
                        _script.insideDoor.insideDisplay.LockButton_Object.SetActive(false);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.LockButton_Object);
                    }
                    Undo.RecordObject(_script,"VIP DoorMode");
                    _script.disableUnlockVip = true;
                    _script.isVip = false;
                    _script.isLocked = false;
                    return;
                }
                else if (_tmpInt == 1)
                {
                    EditorGUILayout.HelpBox("Private Mode:\nOnly Admins will be able to bypass the lock. Anyone can lock or unlock the room.\nThe room will unlock if no one is inside.", MessageType.None);
                    if (_script.insideDoor.insideDisplay.LockButton_Object != null)
                    {
                        Undo.RecordObject(_script.insideDoor.insideDisplay.LockButton_Object,"VIP DoorMode");
                        _script.insideDoor.insideDisplay.LockButton_Object.SetActive(true);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.LockButton_Object);
                    }
                    Undo.RecordObject(_script,"VIP DoorMode");
                    _script.disableUnlockVip = false;
                    _script.isVip = false;
                    _script.isLocked = false;
                    return;
                }
                else if (_tmpInt == 2)
                {
                    if (_script.insideDoor.insideDisplay.LockButton_Object != null)
                    {
                        Undo.RecordObject(_script.insideDoor.insideDisplay.LockButton_Object,"VIP DoorMode");
                        _script.insideDoor.insideDisplay.LockButton_Object.SetActive(true);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.LockButton_Object);
                    }
                    Undo.RecordObject(_script,"VIP DoorMode");
                    _script.disableUnlockVip = false;
                    _script.isVip = true;
                    _script.isLocked = true;
                    
                    EditorGUILayout.HelpBox("VIP Mode:\nMembers with access will bypass the lock. The lock acts as public access and can only be unlocked by admins or roles with the unlock permission.\nThe room will NOT unlock if no one is inside, as it is a permission check.\n\nThe role oder is top to bottom. Higher roles override lower ones.", MessageType.None);

                    //using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUI.BeginDisabledGroup(_script.disableUnlockVip);
                        EditorGUILayout.BeginHorizontal();
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Allow anyone to unlock VIP: ", GUILayout.MinWidth(0));
                            _script.canAnyoneUnlockVip = EditorGUILayout.Toggle(_script.canAnyoneUnlockVip, GUILayout.Width(25f));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Remove not allowed players: ", "If a player gets into the room and isn't allowed, they will be removed."), GUILayout.MinWidth(0));
                            _script.removePlayerIfNotAllowed = EditorGUILayout.Toggle(_script.removePlayerIfNotAllowed, GUILayout.Width(25f));
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                    DrawRoles();
                }
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void AudioSettings()
        {
            Undo.RecordObject(_script,"VIP AudioSettings");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Audio");
                if (GUILayout.Button(_script._EDITOR_audio ? "Collapse" : "Expand", GUILayout.Width(100f)))
                {
                    _script._EDITOR_audio = !_script._EDITOR_audio;
                }
                EditorGUILayout.EndHorizontal();
                if (!_script._EDITOR_audio) return;
                
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("Default Player Audio\nIf you dont know what these are, you probably shouldn't touch them", MessageType.None);
                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Far: ", GUILayout.MinWidth(0));
                        _script.defaultPlayerFar = EditorGUILayout.FloatField(_script.defaultPlayerFar, GUILayout.Width(100f));
                    }

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Near: ", GUILayout.MinWidth(0));
                        _script.defaultPlayerNear = EditorGUILayout.FloatField(_script.defaultPlayerNear, GUILayout.Width(100f));
                    }

                    if (GUILayout.Button("Reset", GUILayout.Width(50f)))
                    {
                        _script.defaultPlayerFar = 25f;
                        _script.defaultPlayerNear = 0f;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Answer: ", GUILayout.Width(100f));
                    _script.intercomAnswer =
                        (AudioClip)EditorGUILayout.ObjectField(_script.intercomAnswer, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Reject: ", GUILayout.Width(100f));
                    _script.intercomHangup =
                        (AudioClip)EditorGUILayout.ObjectField(_script.intercomHangup, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Camera: ", GUILayout.Width(100f));
                    _script.cameraClick = (AudioClip)EditorGUILayout.ObjectField(_script.cameraClick, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Doorbell: ", GUILayout.Width(100f));
                    _script.intercomRingOnce =
                        (AudioClip)EditorGUILayout.ObjectField(_script.intercomRingOnce, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Ring: ", GUILayout.Width(100f));
                    _script.intercomRingLong =
                        (AudioClip)EditorGUILayout.ObjectField(_script.intercomRingLong, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void DrawRoles()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox($"Access Roles", MessageType.None);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Role")) AddRole();
                if (GUILayout.Button("Remove Role")) RemoveRole();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUILayout.TextField("Role Container");
                        EditorGUILayout.TextField("Can Unlock", GUILayout.Width(100f));
                        EditorGUILayout.TextField("Invites", GUILayout.Width(100f));
                        EditorGUILayout.TextField("", GUILayout.Width(50f));
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < _script._roleContainers.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        Undo.RecordObject(_script,"VIP room change");
                        _script._roleContainers[i] = (RoleContainer)EditorGUILayout.ObjectField(_script._roleContainers[i], typeof(RoleContainer), true);
                        
                        EditorGUI.BeginDisabledGroup(_script.disableUnlockVip || _script.canAnyoneUnlockVip);
                        _script.canUnlock[i] = EditorGUILayout.Toggle(_script.canUnlock[i], GUILayout.Width(100f));
                        EditorGUI.EndDisabledGroup();
                        _script.allowedGuests[i] = EditorGUILayout.IntField(_script.allowedGuests[i], GUILayout.Width(100f));
                        _script.allowedGuests[i] = Mathf.Clamp(_script.allowedGuests[i], 0, 9999);

                        PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
                        
                        if (GUILayout.Button("-", GUILayout.Width(50f)))
                        {
                            RemoveRole(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        private void DrawAdminRoles()
        {
            Undo.RecordObject(_script,$"Changed Admin Roles");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox($"Admin Roles\nAdmins bypass room locks, anonymous mode, and invite restrictions.", MessageType.None);
                if (GUILayout.Button(_script._EDITOR_admin ? "Collapse" : "Expand", GUILayout.Width(100f)))
                {
                    _script._EDITOR_admin = !_script._EDITOR_admin;
                }
                EditorGUILayout.EndHorizontal();
                if (!_script._EDITOR_admin) return;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Admin")) AddAdmin();
                if (GUILayout.Button("Remove Admin")) RemoveAdmin();
                EditorGUILayout.EndHorizontal();
                Undo.RecordObject(_script,"Admin array init");
                if (_script.adminRoles == null) _script.adminRoles = new RoleContainer[0];
                PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
                for (int i = 0; i < _script.adminRoles.Length; i++)
                {
                    Undo.RecordObject(_script,"Admin array change");
                    EditorGUILayout.BeginHorizontal(); _script.adminRoles[i] = (RoleContainer)EditorGUILayout.ObjectField(_script.adminRoles[i], typeof(RoleContainer), true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
                    
                    if (GUILayout.Button("-",GUILayout.Width(50f)))
                    {
                        RemoveAdmin(i);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        #endregion

        private void AddAdmin()
        {
            Undo.RecordObject(_script,"Add Admin");
            int _i = _script.adminRoles.Length + 1;
            Array.Resize(ref _script.adminRoles,_i );
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void RemoveAdmin()
        {
            Undo.RecordObject(_script,"Remove Admin");
            int _i = _script.adminRoles.Length - 1;
            if (_i <= 0) _i = 0;
            Array.Resize(ref _script.adminRoles,_i );
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void RemoveAdmin( int _index)
        {
            Undo.RecordObject(_script,"Remove Admin");
            RoleContainer[] newAdmins = new RoleContainer[_script.adminRoles.Length - 1];

            int k = 0;
            for (int i = 0; i < _script.adminRoles.Length; i++)
            {
                if (i == _index) { continue;}
                newAdmins[k] = _script.adminRoles[i];
                k++;
            }
            _script.adminRoles = new RoleContainer[newAdmins.Length];
            for (int i = 0; i < _script.adminRoles.Length; i++)
            {
                _script.adminRoles[i] = newAdmins[i];
            }
            //Array.Copy(newAdmins,_script.adminRoles, 0);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void AddRole()
        {
            Undo.RecordObject(_script,"Add Admin");
            int _i = _script._roleContainers.Length + 1;
            Array.Resize(ref _script._roleContainers,_i );
            Array.Resize(ref _script.canUnlock,_i );
            Array.Resize(ref _script.allowedGuests,_i );
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void RemoveRole()
        {
            Undo.RecordObject(_script,"Remove Admin");
            int _i = _script._roleContainers.Length - 1;
            if (_i <= 0) _i = 0;
            Array.Resize(ref _script._roleContainers,_i );
            Array.Resize(ref _script.canUnlock,_i );
            Array.Resize(ref _script.allowedGuests,_i );
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }
        private void RemoveRole( int _index)
        {
            Undo.RecordObject(_script,"Remove Admin");
            RoleContainer[] newRoleContainers = new RoleContainer[_script._roleContainers.Length - 1];
            bool[] newcanUnlock = new bool[_script.canUnlock.Length - 1];
            int[] newallowedGuests = new int[_script.allowedGuests.Length - 1];

            int k = 0;
            for (int i = 0; i < _script._roleContainers.Length; i++)
            {
                if (i == _index) { continue;}
                newRoleContainers[k] = _script._roleContainers[i];
                newcanUnlock[k] = _script.canUnlock[i];
                newallowedGuests[k] = _script.allowedGuests[i];
                k++;
            }
            _script._roleContainers = new RoleContainer[newRoleContainers.Length];
            _script.canUnlock = new bool[newRoleContainers.Length];
            _script.allowedGuests = new int[newRoleContainers.Length];
            
            for (int i = 0; i < _script._roleContainers.Length; i++)
            {
                _script._roleContainers[i] = newRoleContainers[i];
                _script.canUnlock[i] = newcanUnlock[i];
                _script.allowedGuests[i] = newallowedGuests[i];
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
            //Array.Copy(newAdmins,_script.adminRoles, 0);
        }
        private void SetupRoomName()
        {
            string tmpName;
            Undo.RecordObject(_script, $"Undo on {_script.name}");
            if (string.IsNullOrWhiteSpace(_script.roomName))
            {
                tmpName = _script.isVip ? "VIP Room" : "Private Room";
            }
            else
            {
                tmpName = _script.roomName;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);

            Undo.RecordObject(_script.insideDoor.insideDisplay.roomNameDisplay, $"Undo on {_script.insideDoor.insideDisplay.roomNameDisplay.name}");
            _script.insideDoor.insideDisplay.roomNameDisplay.text = tmpName;
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.roomNameDisplay);

            Undo.RecordObject(_script.outsideDoor.outsideDisplay.roomNameDisplay, $"Undo on {_script.outsideDoor.outsideDisplay.roomNameDisplay.name}");
            _script.outsideDoor.outsideDisplay.roomNameDisplay.text = tmpName;
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.outsideDoor.outsideDisplay.roomNameDisplay);

            Undo.RecordObject(_script.insideDoor.insideDisplay.roomMessage, $"Undo on {_script.insideDoor.insideDisplay.roomMessage.name}");
            _script.insideDoor.insideDisplay.roomMessage.text = _script.insideDoor.insideDisplay.doorMessage;
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.insideDoor.insideDisplay.roomMessage);

            Undo.RecordObject(_script.outsideDoor.outsideDisplay.roomMessage, $"Undo on {_script.outsideDoor.outsideDisplay.roomMessage.name}");
            _script.outsideDoor.outsideDisplay.roomMessage.text = _script.insideDoor.insideDisplay.doorMessage;
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script.outsideDoor.outsideDisplay.roomMessage);
        }
        private void SetupCameras()
        {
            PrivateRoom_Display_Inside inside = _script.insideDoor.insideDisplay;
            PrivateRoom_Display_Outside outside = _script.outsideDoor.outsideDisplay;
            
            Undo.RecordObject(inside.photoCamera, $"Undo on {inside.photoCamera.name}");
            inside.photoCamera.targetTexture = _script.cameraRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.photoCamera);

            Undo.RecordObject(inside.intercomCamera, $"Undo on {inside.intercomCamera.name}");
            inside.intercomCamera.targetTexture = _script.insideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.intercomCamera);

            Undo.RecordObject(outside.intercomCamera, $"Undo on {outside.intercomCamera.name}");
            outside.intercomCamera.targetTexture = _script.outsideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.intercomCamera);
            
            
            Undo.RecordObject(inside.RawImage_intercomDisplay_Outside, $"Undo on {inside.RawImage_intercomDisplay_Outside.name}");
            inside.RawImage_intercomDisplay_Outside.texture = _script.outsideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomDisplay_Outside);
            
            Undo.RecordObject(inside.RawImage_intercomDisplay_Inside, $"Undo on {inside.RawImage_intercomDisplay_Inside.name}");
            inside.RawImage_intercomDisplay_Inside.texture = _script.insideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomDisplay_Inside);
            
            Undo.RecordObject(inside.RawImage_intercomCall_BG, $"Undo on {inside.RawImage_intercomCall_BG.name}");
            inside.RawImage_intercomCall_BG.texture = _script.cameraRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomCall_BG);
            
            Undo.RecordObject(inside.RawImage_intercomCall_PopupOutside, $"Undo on {inside.RawImage_intercomCall_PopupOutside.name}");
            inside.RawImage_intercomCall_PopupOutside.texture = _script.outsideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomCall_PopupOutside);

            Undo.RecordObject(outside.RawImage_intercomDisplay_Outside, $"Undo on {outside.RawImage_intercomDisplay_Outside.name}");
            outside.RawImage_intercomDisplay_Outside.texture = _script.outsideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.RawImage_intercomDisplay_Outside);
            
            Undo.RecordObject(outside.RawImage_intercomDisplay_Inside, $"Undo on {outside.RawImage_intercomDisplay_Inside.name}");
            outside.RawImage_intercomDisplay_Inside.texture = _script.insideRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.RawImage_intercomDisplay_Inside);
            
            Undo.RecordObject(outside.RawImage_intercomCall_BG, $"Undo on {outside.RawImage_intercomCall_BG.name}");
            outside.RawImage_intercomCall_BG.texture = _script.cameraRenderTexture;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.RawImage_intercomCall_BG);
            
            
            
            // Undo.RecordObject(inside, $"Undo on {inside.name}");
            // inside.insideImages = _script.insideRenderTexture;
            // inside.outsideImages = _script.outsideRenderTexture;
            // inside.cameraImages = _script.cameraRenderTexture;
            // PrefabUtility.RecordPrefabInstancePropertyModifications(inside);
            //
            // Undo.RecordObject(outside, $"Undo on {outside.name}");
            // outside.insideImages = _script.insideRenderTexture;
            // outside.outsideImages = _script.outsideRenderTexture;
            // outside.cameraImages = _script.cameraRenderTexture;
            // PrefabUtility.RecordPrefabInstancePropertyModifications(outside);
            
            
            
        }

        private void GenerateRenderTextures()
        {
            if (!EditorUtility.DisplayDialog($"Generate RenderTextures", $"This is about to create 3 render Textures and assign them to this door, is this OK?", $"Yes", "No"))
            {
                return;      
            }
            
            PrivateRoom_Display_Inside inside = _script.insideDoor.insideDisplay;
            PrivateRoom_Display_Outside outside = _script.outsideDoor.outsideDisplay;
            
            if (inside == null)
            {
                EditorUtility.DisplayDialog($"Generate RenderTextures", $"Inside door is Null", $"OK");
                return;
            }
            if (outside == null)
            {
                EditorUtility.DisplayDialog($"Generate RenderTextures", $"Outside door is Null", $"OK");
                return;
            }
            
            RenderTexture InsideDoor = new RenderTexture(_script._EDITOR_h, _script._EDITOR_v, 0, GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture OutsideDoor = new RenderTexture(_script._EDITOR_h, _script._EDITOR_v, 0, GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture PhotoCamera = new RenderTexture(_script._EDITOR_h, _script._EDITOR_v, 0, GraphicsFormat.R8G8B8A8_SRGB);

            int prefix = 0;

            bool uwu = false;
            
            string _location = $"{DisBridgePrefabUtilities.folderLocation}RenderTextures/VipRooms/";
            
            RenderTexture wow =  AssetDatabase.LoadAssetAtPath<RenderTexture>($"{_location}{prefix}_InsideDoor.asset");
            if (wow != null)
            {
                uwu = true;
            }
            
            while (uwu)
            {
                prefix++;
                RenderTexture wow2 =  AssetDatabase.LoadAssetAtPath<RenderTexture>($"{_location}{prefix}_InsideDoor.asset");
                if (wow2 == null)
                {
                    uwu = false;
                }
            }
            
            
            InsideDoor.Create();
            OutsideDoor.Create();
            PhotoCamera.Create();

            
            
            if (!Directory.Exists(_location))
            {
                Directory.CreateDirectory(_location);
            }
            
            AssetDatabase.CreateAsset(InsideDoor, $"{_location}{prefix}_InsideDoor.asset");
            AssetDatabase.CreateAsset(OutsideDoor, $"{_location}{prefix}_OutsideDoor.asset");
            AssetDatabase.CreateAsset(PhotoCamera, $"{_location}{prefix}_PhotoCamera.asset");

            
            
            
            Undo.RecordObject(_script, $"Undo on {_script.name}");
            _script.insideRenderTexture = InsideDoor;
            _script.outsideRenderTexture = OutsideDoor;
            _script.cameraRenderTexture = PhotoCamera;
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
            
            /*Undo.RecordObject(inside.RawImage_intercomDisplay_Outside, $"Undo on {inside.RawImage_intercomDisplay_Outside.name}");
            inside.RawImage_intercomDisplay_Outside.texture = OutsideDoor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomDisplay_Outside);
            
            Undo.RecordObject(inside.RawImage_intercomDisplay_Inside, $"Undo on {inside.RawImage_intercomDisplay_Inside.name}");
            inside.RawImage_intercomDisplay_Inside.texture = InsideDoor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(inside.RawImage_intercomDisplay_Inside);

            Undo.RecordObject(outside.RawImage_intercomDisplay_Outside, $"Undo on {outside.RawImage_intercomDisplay_Outside.name}");
            outside.RawImage_intercomDisplay_Outside.texture = OutsideDoor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.RawImage_intercomDisplay_Outside);
            
            Undo.RecordObject(outside.RawImage_intercomDisplay_Inside, $"Undo on {outside.RawImage_intercomDisplay_Inside.name}");
            outside.RawImage_intercomDisplay_Inside.texture = InsideDoor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(outside.RawImage_intercomDisplay_Inside);*/
            
            // Undo.RecordObject(inside, $"Undo on {inside.name}");
            // inside.insideImages = InsideDoor;
            // inside.outsideImages = OutsideDoor;
            // inside.cameraImages = PhotoCamera;
            // PrefabUtility.RecordPrefabInstancePropertyModifications(inside);
            //
            // Undo.RecordObject(outside, $"Undo on {outside.name}");
            // outside.insideImages = InsideDoor;
            // outside.outsideImages = OutsideDoor;
            // outside.cameraImages = PhotoCamera;
            // PrefabUtility.RecordPrefabInstancePropertyModifications(outside);
            
            AssetDatabase.Refresh();
        }
    }
}