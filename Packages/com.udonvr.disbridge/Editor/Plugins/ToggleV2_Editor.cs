//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UdonVR.DisBridge.Editors;
using UdonVR.DisBridge.Plugins;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Editors.Plugins
{

    [CustomEditor(typeof(ToggleV2))]
    public class ToggleV2_Editor : UnityEditor.Editor
    {
        private ToggleV2 myScript;
        private Rect typeRect;

        private SerializedProperty _offIfNotUsable;

        private SerializedProperty _disallowUnlock;
        private SerializedProperty _canOwnerUnlock;
        private SerializedProperty _canMasterUnlock;
        
        private SerializedProperty _button;
        private SerializedProperty _buttonIndicator;
        private SerializedProperty _buttonCollider;
        private SerializedProperty _isPhysical;
        private SerializedProperty _globalLocalIndicator;

        private bool advanceSettings = false;

        void OnEnable()
        {
            // Setup the SerializedProperties.
            //_defaultOn = serializedObject.FindProperty("defaultOn");
            //_defaultOff = serializedObject.FindProperty("defaultOff");
            //_defaultOn_EnforeStart = serializedObject.FindProperty("defaultOn_EnforeStart");
            //_defaultOff_EnforeStart = serializedObject.FindProperty("defaultOff_EnforeStart");
            //_targetAnimators = serializedObject.FindProperty("targetAnimators");
            //_targetAnimatorStrings = serializedObject.FindProperty("targetAnimatorStrings");
            _offIfNotUsable = serializedObject.FindProperty(nameof(ToggleV2.offIfNotUsable));
            _disallowUnlock = serializedObject.FindProperty(nameof(ToggleV2.disallowUnlock));
            _canOwnerUnlock = serializedObject.FindProperty(nameof(ToggleV2.canOwnerUnlock));
            _canMasterUnlock = serializedObject.FindProperty(nameof(ToggleV2.canMasterUnlock));

            //_whitelists = serializedObject.FindProperty("whitelists");

            _button = serializedObject.FindProperty(nameof(ToggleV2.button));
            _buttonIndicator = serializedObject.FindProperty(nameof(ToggleV2.buttonIndicator));
            _buttonCollider = serializedObject.FindProperty(nameof(ToggleV2.buttonCollider));
            _isPhysical = serializedObject.FindProperty(nameof(ToggleV2.isPhysical));
            _globalLocalIndicator = serializedObject.FindProperty(nameof(ToggleV2.globalIndicator));
        }

        public override void OnInspectorGUI()
        {
            myScript = (ToggleV2) target;
            Undo.RecordObject(myScript.gameObject,$"Undo on {myScript.name}");
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.PlaymodeWarning(target);
            DrawInteractButton();
            EditorFunctions.GuiLine();
            EditorFunctions.DrawPluginManagerCheck(myScript, false, true);
            EditorFunctions.DrawPermissions(myScript);
            
            DrawSettings();
            DrawWhitelists();
            DrawStartEnforcement();
            DrawAnimators();
            DrawObjectSelection();
            DrawAdvanceSettings();

            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }

        private void DrawStartEnforcement()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Enforce Lock state on Start?", "Changes global settings for how the start state of the toggle behaves."));
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUI.BeginDisabledGroup(myScript.neverEnforceLockState);
                    EditorGUILayout.LabelField(new GUIContent("Always", "Lock states will always be enforced on Start.\n\nThis is for compatibility with other prefabs/whitelists controlling objects and generally should be off."));
                    myScript.alwaysEnforceLockState = EditorGUILayout.Toggle(myScript.alwaysEnforceLockState,GUILayout.Width(25f));
                    EditorGUI.EndDisabledGroup();
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState);
                    EditorGUILayout.LabelField(new GUIContent("Never", "Lock states will never be enforced on Start.\n\nThis is for compatibility with other prefabs/whitelists controlling objects and generally should be off."));
                    myScript.neverEnforceLockState = EditorGUILayout.Toggle(myScript.neverEnforceLockState,GUILayout.Width(25f));
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        private void DrawSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Settings:", "General behaviour settings."));

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    UdonBehaviour backingScript = UdonSharpEditorUtility.GetBackingUdonBehaviour(myScript);
                    Undo.RecordObject(backingScript, $"Undo on {backingScript.name}");
                    string OWOMYGUY = backingScript.SyncMethod == Networking.SyncType.Manual ? "Global" : "Local";
                    if (GUILayout.Button($"Sync Mode [{OWOMYGUY}]"))
                    {
                        backingScript.SyncMethod = backingScript.SyncMethod == Networking.SyncType.Manual ? Networking.SyncType.None : Networking.SyncType.Manual;
                    }
                    PrefabUtility.RecordPrefabInstancePropertyModifications(backingScript);
                    Undo.RecordObject(myScript, $"Undo on {myScript.name}");
                    myScript.isGlobal = backingScript.SyncMethod != Networking.SyncType.None;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
                }
                
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Hide if not usable?","Will hide the button if the user does not have access to use the button."));
                    EditorGUILayout.PropertyField(_offIfNotUsable, new GUIContent("","Will hide the button if the user does not have access to use the button."),GUILayout.Width(25f));
                }
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Disallow Unlock?", "Makes it so the button can only be toggled on. You will no longer be able to disable the toggle with this enabled."));
                    EditorGUILayout.PropertyField(_disallowUnlock,new GUIContent("", "Makes it so the button can only be toggled on. You will no longer be able to disable the toggle with this enabled."),GUILayout.Width(25f));
                }
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Can Instance Owner Unlock?", "Allows the Owner of the instance to lock and unlock the Toggle."));
                    EditorGUILayout.PropertyField(_canOwnerUnlock,new GUIContent("", "Allows the Owner of the instance to lock and unlock the Toggle."),GUILayout.Width(25f));
                }
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Can Master Unlock?", "Allows the Network Master to lock and unlock the Toggle."));
                    EditorGUILayout.PropertyField(_canMasterUnlock,new GUIContent("", "Allows the Network Master to lock and unlock the Toggle."),GUILayout.Width(25f));
                }

            }
        }

        private void DrawAdvanceSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorFunctions.DrawAdvSettingsButton(ref advanceSettings);
                if (advanceSettings)
                {
                    
                    EditorGUILayout.PropertyField(_button,new GUIContent("Button Object:","The physical button object. Used for hiding or showing the button. This should be a child of the toggle script."));
                    EditorGUILayout.PropertyField(_buttonIndicator,new GUIContent("Button Indicator:","The indicator object used to show if the toggle is On or Off"));
                    
                    EditorGUILayout.PropertyField(_isPhysical,new GUIContent("Use physical button?","Marks the button as a physical button, allowing use user to press it (collider interact)."));
                    EditorGUILayout.PropertyField(_buttonCollider,new GUIContent("Button Collider:","Interaction Collider for the button. This is used with the \"Use physical button?\" toggle for turning interaction on/off independent of the button's hide state."));
                    EditorGUILayout.PropertyField(_globalLocalIndicator, new GUIContent("Global Local Indicator", "TextMeshPro field for indicating what mode the toggle is in."));
                }
            }
        }

        private void DrawInteractButton()
        {
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlayingOrWillChangePlaymode);
            if (GUILayout.Button(new GUIContent("Interact", "Interact is only usable in Play Mode.\n\nUsing Interact will flip the toggle's state, ignoring Permissions."), GUILayout.Height(50)))
            {
                myScript._Editor_Use();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        //##############################################################################################################
        #region ObjectSelection
        private void DrawObjectSelection()
        {
            if (myScript.defaultOn == null)
            {
                myScript.defaultOn = new GameObject[0];
            }

            if (myScript.defaultOff == null)
            {
                myScript.defaultOff = new GameObject[0];
            }

            if (myScript.defaultOn_EnforceStart == null)
            {
                myScript.defaultOn_EnforceStart = new bool[myScript.defaultOn.Length];
            }

            if (myScript.defaultOff_EnforceStart == null)
            {
                myScript.defaultOff_EnforceStart = new bool[myScript.defaultOff.Length];
            }

            if (myScript.defaultOn_EnforceStart.Length != myScript.defaultOn.Length)
            {
                Array.Resize(ref myScript.defaultOn_EnforceStart, myScript.defaultOn.Length);
            }

            if (myScript.defaultOff_EnforceStart.Length != myScript.defaultOff.Length)
            {
                Array.Resize(ref myScript.defaultOff_EnforceStart, myScript.defaultOff.Length);
            }


            EditorGUILayout.BeginHorizontal();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Toggle On:",
                        "These objects turn ON when the toggle is ON\n\nThese objects turn OFF when the toggle is OFF"),
                    GUILayout.MinWidth(0));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = myScript.defaultOff.Length + 1;
                    Array.Resize(ref myScript.defaultOff, newVal);
                    Array.Resize(ref myScript.defaultOff_EnforceStart, newVal);
                    myScript.defaultOff_EnforceStart[^1] = true;
                }
                
                if (GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.defaultOff.Length - 1, 0, int.MaxValue);
                    if (myScript.defaultOff.Length == newVal) return;
                    Array.Resize(ref myScript.defaultOff, newVal);
                    Array.Resize(ref myScript.defaultOff_EnforceStart, newVal);
                }

                if (DropAreaGUI_ObjectSelect(false)) return;
                EditorGUILayout.EndHorizontal();
                
                for (int i = 0; i < myScript.defaultOff.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.defaultOff[i] = (GameObject) EditorGUILayout.ObjectField(myScript.defaultOff[i], typeof(GameObject), true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState || myScript.neverEnforceLockState);
                        myScript.defaultOff_EnforceStart[i] =
                            EditorGUILayout.Toggle(myScript.defaultOff_EnforceStart[i], GUILayout.Width(15f));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "Enforce state on Start.\n" +
                                                               "\n" +
                                                               "Sets if the object should be set \"OFF\" during Start.\n" +
                                                               "If toggled off, the object's state will be un touched and left as the current state on Upload.\n" +
                                                               "\n" +
                                                               "This should be ON in most cases, and is used for fixing overlapping whitelists."));
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button("swp", GUILayout.Width(35f)))
                        {
                            int newVal = myScript.defaultOn.Length + 1;
                            Array.Resize(ref myScript.defaultOn, newVal);
                            Array.Resize(ref myScript.defaultOn_EnforceStart, newVal);
                            myScript.defaultOn[^1] = myScript.defaultOff[i];
                            myScript.defaultOn_EnforceStart[^1] = myScript.defaultOff_EnforceStart[i];
                            RemoveFromDefaultOff(i);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default On\" list"));
                        }
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("-", "Removes object from the list."), GUILayout.Width(20f)))
                        {
                            RemoveFromDefaultOff(i);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    new GUIContent("Toggle Off:",
                        "These objects turn OFF when the toggle is ON\n\nThese objects turn ON when the toggle is OFF"),
                    GUILayout.MinWidth(0));
                if (GUILayout.Button("Swap Lists", GUILayout.Width(75f)))
                {
                    int TDOff = myScript.defaultOff.Length;
                    int TDOn = myScript.defaultOn.Length;

                    GameObject[] tmpList = new GameObject[TDOn];
                    Array.Copy(myScript.defaultOn, tmpList, TDOn);

                    myScript.defaultOn = new GameObject[TDOff];
                    Array.Copy(myScript.defaultOff, myScript.defaultOn, TDOff);

                    myScript.defaultOff = new GameObject[tmpList.Length];
                    Array.Copy(tmpList, myScript.defaultOff, tmpList.Length);

                    bool[] tmpList2 = new bool[TDOn];

                    Array.Copy(myScript.defaultOn_EnforceStart, tmpList2, TDOn);

                    myScript.defaultOn_EnforceStart = new bool[TDOff];
                    Array.Copy(myScript.defaultOff_EnforceStart, myScript.defaultOn_EnforceStart, TDOff);

                    myScript.defaultOff_EnforceStart = new bool[tmpList2.Length];
                    Array.Copy(tmpList2, myScript.defaultOff_EnforceStart, tmpList2.Length);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = myScript.defaultOn.Length + 1;
                    Array.Resize(ref myScript.defaultOn, newVal);
                    Array.Resize(ref myScript.defaultOn_EnforceStart, newVal);
                    myScript.defaultOn_EnforceStart[^1] = true;
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.defaultOn.Length - 1, 0, int.MaxValue);
                    if (myScript.defaultOn.Length == newVal) return;
                    Array.Resize(ref myScript.defaultOn, newVal);
                    Array.Resize(ref myScript.defaultOn_EnforceStart, newVal);
                }

                if (DropAreaGUI_ObjectSelect(true)) return;
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < myScript.defaultOn.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.defaultOn[i] = (GameObject) EditorGUILayout.ObjectField(myScript.defaultOn[i], typeof(GameObject), true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState || myScript.neverEnforceLockState);
                        myScript.defaultOn_EnforceStart[i] =
                            EditorGUILayout.Toggle(myScript.defaultOn_EnforceStart[i], GUILayout.Width(15f));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "Enforce state on Start.\n" +
                                                               "\n" +
                                                               "Sets if the object should be set \"ON\" during Start.\n" +
                                                               "If toggled off, the object's state will be un touched and left as the current state on Upload.\n" +
                                                               "\n" +
                                                               "This should be ON in most cases, and is used for fixing overlapping whitelists."));
                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("swp", GUILayout.Width(35f)))
                        {
                            int newVal = myScript.defaultOff.Length + 1;
                            Array.Resize(ref myScript.defaultOff, newVal);
                            Array.Resize(ref myScript.defaultOff_EnforceStart, newVal);
                            myScript.defaultOff[^1] = myScript.defaultOn[i];
                            myScript.defaultOff_EnforceStart[^1] = myScript.defaultOn_EnforceStart[i];
                            RemoveFromDefaultOn(i);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default Off\" list"));
                        }
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("-", "Removes object from the list."), GUILayout.Width(20f)))
                        {
                            RemoveFromDefaultOn(i);
                        }
                        GUI.backgroundColor = Color.white;
                        //else
                        //{
                        //    typeRect = GUILayoutUtility.GetLastRect();
                        //    GUI.Label(typeRect, new GUIContent("", "Removes object from the list."));
                        //}
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        public bool DropAreaGUI_ObjectSelect(bool _DefaultOn)
        {
            Event evt = Event.current;
            string _s = "Drag Object Here";
            if (Selection.objects.Length >= 2) _s = "Drag Objects Here";
            Rect drop_area = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, _s);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        if (_DefaultOn)
                        {
                            int k = myScript.defaultOn_EnforceStart.Length;
                            Array.Resize(ref myScript.defaultOn,myScript.defaultOn.Length + DragAndDrop.objectReferences.Length);
                            Array.Resize(ref myScript.defaultOn_EnforceStart,myScript.defaultOn.Length);
                            //Array.Copy(DragAndDrop.objectReferences,0,myScript.defaultOn,myScript.defaultOn.Length - 1, DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                myScript.defaultOn[k] = (GameObject)DragAndDrop.objectReferences[i];
                                myScript.defaultOn_EnforceStart[k] = true;
                                Debug.Log($"[i:{i}][k:{k}]{myScript.defaultOn[k].name}");
                                k++;
                            }
                            return true;
                        }
                        else
                        {
                            int k = myScript.defaultOff_EnforceStart.Length;
                            Array.Resize(ref myScript.defaultOff,myScript.defaultOff.Length + DragAndDrop.objectReferences.Length);
                            Array.Resize(ref myScript.defaultOff_EnforceStart,myScript.defaultOff.Length);
                            //Array.Copy(DragAndDrop.objectReferences,0,myScript.defaultOn,myScript.defaultOn.Length - 1, DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                myScript.defaultOff[k] = (GameObject)DragAndDrop.objectReferences[i];
                                myScript.defaultOff_EnforceStart[k] = true;
                                Debug.Log($"[i:{i}][k:{k}]{myScript.defaultOff[k].name}");
                                k++;
                            }
                            return true;
                        }
                        
                    }

                    break;
            }
            return false;
        }
        private void RemoveFromDefaultOff(int _i)
        {
            int newVal = Mathf.Clamp(myScript.defaultOff.Length - 1, 0, int.MaxValue);
            if (myScript.defaultOff.Length == newVal) return;
            GameObject[] newArray = new GameObject[newVal];
            bool[] newArrayBool = new bool[newVal];
            int k = 0;
            for (int i = 0; i < myScript.defaultOff.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.defaultOff[i];
                newArrayBool[k] = myScript.defaultOff_EnforceStart[i];
                k++;
            }

            myScript.defaultOff = newArray;
            myScript.defaultOff_EnforceStart = newArrayBool;
            //myScript.defaultOff = new GameObject[newVal];
            //Array.Copy(newArray,myScript.defaultOff,0);
        }
        private void RemoveFromDefaultOn(int _i)
        {
            int newVal = Mathf.Clamp(myScript.defaultOn.Length - 1, 0, int.MaxValue);
            if (myScript.defaultOn.Length == newVal) return;
            GameObject[] newArray = new GameObject[newVal];
            bool[] newArrayBool = new bool[newVal];
            int k = 0;
            for (int i = 0; i < myScript.defaultOn.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.defaultOn[i];
                newArrayBool[k] = myScript.defaultOn_EnforceStart[i];
                k++;
            }

            myScript.defaultOn = newArray;
            myScript.defaultOn_EnforceStart = newArrayBool;
            //myScript.defaultOn = new GameObject[newVal];
            //Array.Copy(newArray,myScript.defaultOn,0);
        }
        
        #endregion
        //##############################################################################################################
        #region Whitelists

        public void DrawWhitelists()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Whitelists:",
                        "These are Whitelists that the toggle will bind to.\nThis will tie the state of the Toggle and Whitelist together, meaning if the Whitelist toggles on, so will the Toggle, and if the Toggle toggles on, so will the Whitelist."),
                    GUILayout.MinWidth(0));
                EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("+", GUILayout.MaxWidth(50f)))
                    {
                        int newVal = myScript.whitelists.Length + 1;
                        Array.Resize(ref myScript.whitelists, newVal);
                    }

                if (GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.whitelists.Length - 1, 0, int.MaxValue);
                    if (myScript.whitelists.Length == newVal) return;
                    Array.Resize(ref myScript.whitelists, newVal);
                }

                if (DropAreaGUI_Whitelists()) return;
                EditorGUILayout.EndHorizontal();

                myScript.whitelists ??= Array.Empty<DisBridgeWhitelist_Class>();
                for (int i = 0; i < myScript.whitelists.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.whitelists[i] = (DisBridgeWhitelist_Class) EditorGUILayout.ObjectField(myScript.whitelists[i], typeof(DisBridgeWhitelist_Class), true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The Whitelists to bind to."));

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("-", "Removes Whitelist from the list."), GUILayout.Width(20f)))
                        {
                            RemoveFromWhitelists(i);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
        }
        public bool DropAreaGUI_Whitelists()
        {
            Event evt = Event.current;
            string _s = "Drag Whitelists Here";
            if (Selection.objects.Length >= 2) _s = "Drag Whitelists Here";
            Rect drop_area = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, _s);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        int k = myScript.whitelists.Length;
                        List<DisBridgeWhitelist_Class> newWhitelists = new List<DisBridgeWhitelist_Class>();
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            DisBridgeWhitelist_Class[] _testClasss = ((GameObject) DragAndDrop.objectReferences[i])
                                .transform.GetComponents<DisBridgeWhitelist_Class>();
                            if (_testClasss == null) continue;
                            if (_testClasss.Length == 0) continue;
                            for (int j = 0; j < _testClasss.Length; j++)
                            {
                                newWhitelists.Add(_testClasss[i]);
                            }
                        }
                        Array.Resize(ref myScript.whitelists,myScript.whitelists.Length + newWhitelists.Count);
                        
                        for (int i = 0; i < newWhitelists.Count; i++)
                        {
                            myScript.whitelists[k] = newWhitelists[i];
                            Debug.Log($"[i:{i}][k:{k}]{myScript.defaultOn[k].name}");
                            k++;
                        }
                        return true;

                    }

                    break;
            }
            return false;
        }
        private void RemoveFromWhitelists(int _i)
        {
            int newVal = Mathf.Clamp(myScript.whitelists.Length - 1, 0, int.MaxValue);
            if (myScript.whitelists.Length == newVal) return;
            DisBridgeWhitelist_Class[] newArray = new DisBridgeWhitelist_Class[newVal];
            int k = 0;
            for (int i = 0; i < myScript.whitelists.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.whitelists[i];
                k++;
            }

            myScript.whitelists = newArray;
            //myScript.defaultOn = new GameObject[newVal];
            //Array.Copy(newArray,myScript.defaultOn,0);
        }

        #endregion
        //##############################################################################################################
        #region Animators

        public void DrawAnimators()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Animators:", ""), GUILayout.MinWidth(0));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = myScript.targetAnimators.Length + 1;
                    Array.Resize(ref myScript.targetAnimators, newVal);
                    Array.Resize(ref myScript.targetAnimatorStrings, newVal);
                    Array.Resize(ref myScript.targetAnimator_EnforceStart, newVal);
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.targetAnimators.Length - 1, 0, int.MaxValue);
                    if (myScript.targetAnimators.Length == newVal) return;
                    Array.Resize(ref myScript.targetAnimators, newVal);
                    Array.Resize(ref myScript.targetAnimatorStrings, newVal);
                    Array.Resize(ref myScript.targetAnimator_EnforceStart, newVal);
                }

                if (DropAreaGUI_Animators()) return;
                EditorGUILayout.EndHorizontal();
                
                
                for (int i = 0; i < myScript.targetAnimators.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.targetAnimators[i] = (Animator) EditorGUILayout.ObjectField(myScript.targetAnimators[i], typeof(Animator), true, GUILayout.MinWidth(0),GUILayout.MaxWidth(200f));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The Animator to bind to."));
                        
                        myScript.targetAnimatorStrings[i] = EditorGUILayout.TextField(myScript.targetAnimatorStrings[i], GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The name of the Animator's Bool value to toggle."));
                        
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState || myScript.neverEnforceLockState);
                        myScript.targetAnimator_EnforceStart[i] = EditorGUILayout.Toggle(myScript.targetAnimator_EnforceStart[i], GUILayout.Width(15f));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "Enforce state on Start.\n" +
                                                               "\n" +
                                                               "Sets if the animator's bool should be set during Start.\n" +
                                                               "If toggled off, the object's state will be un touched and left as the current state on Upload.\n" +
                                                               "\n" +
                                                               "This should be ON in most cases, and is used for fixing overlapping whitelists."));
                        EditorGUI.EndDisabledGroup();
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent("-", "Removes Animator from the list."), GUILayout.Width(20f)))
                        {
                            RemoveFromAnimators(i);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
        }
        public bool DropAreaGUI_Animators()
        {
            Event evt = Event.current;
            string _s = "Drag Animators Here";
            if (Selection.objects.Length >= 2) _s = "Drag Animators Here";
            Rect drop_area = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, _s);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        int k = myScript.targetAnimators.Length;
                        List<Animator> newAnimators = new List<Animator>();
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            Animator[] _testClasss = ((GameObject) DragAndDrop.objectReferences[i])
                                .transform.GetComponents<Animator>();
                            if (_testClasss == null) continue;
                            if (_testClasss.Length == 0) continue;
                            for (int j = 0; j < _testClasss.Length; j++)
                            {
                                newAnimators.Add(_testClasss[i]);
                            }
                        }
                        Array.Resize(ref myScript.targetAnimators,myScript.targetAnimators.Length + newAnimators.Count);
                        
                        for (int i = 0; i < newAnimators.Count; i++)
                        {
                            myScript.targetAnimators[k] = newAnimators[i];
                            Debug.Log($"[i:{i}][k:{k}]{myScript.targetAnimators[k].name}");
                            k++;
                        }
                        return true;

                    }

                    break;
            }
            return false;
        }
        private void RemoveFromAnimators(int _i)
        {
            int newVal = Mathf.Clamp(myScript.targetAnimators.Length - 1, 0, int.MaxValue);
            if (myScript.targetAnimators.Length == newVal) return;
            
            //#######################################################
            //animator Array
            Animator[] newArray = new Animator[newVal];
            int k = 0;
            for (int i = 0; i < myScript.targetAnimators.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.targetAnimators[i];
                k++;
            }
            myScript.targetAnimators = newArray;
            //#######################################################
            //animator string Array
            String[] new_targetAnimatorStrings = new String[newVal];
            k = 0;
            for (int i = 0; i < myScript.targetAnimatorStrings.Length; i++)
            {
                if (_i == i) { continue; }
                new_targetAnimatorStrings[k] = myScript.targetAnimatorStrings[i];
                k++;
            }
            myScript.targetAnimatorStrings = new_targetAnimatorStrings;
            //#######################################################
            //animator enforce start bool Array
            bool[] new_targetAnimator_EnforceStart = new bool[newVal];
            k = 0;
            for (int i = 0; i < myScript.targetAnimator_EnforceStart.Length; i++)
            {
                if (_i == i) { continue; }
                new_targetAnimator_EnforceStart[k] = myScript.targetAnimator_EnforceStart[i];
                k++;
            }
            myScript.targetAnimator_EnforceStart = new_targetAnimator_EnforceStart;
            //#######################################################
        }

        #endregion
    }
}