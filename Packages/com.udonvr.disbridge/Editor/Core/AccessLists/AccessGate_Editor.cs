using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(AccessGate))]
    public class AccessGate_Editor : Editor
    {
        private AccessGate accessGate;
        private Rect typeRect;
        private void OnEnable()
        {
            accessGate = (AccessGate)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(accessGate, $"Undo on {accessGate.name}");

            EditorFunctions.DrawDisBridgeLogo();
            DrawSettings();
            DrawObjectSelection(ref accessGate.object_TurnOff, ref accessGate.object_TurnOn);
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(accessGate);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (accessGate.playerMoverTransform != null)
            {
                EditorFunctions.DrawSpawnPoint(accessGate.playerMoverTransform);
            }
        }

        private void DrawSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    accessGate.userAccessList = (UserAccessList)EditorGUILayout.ObjectField(accessGate.userAccessList, typeof(UserAccessList), true);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("This is the default position the player will get Teleported to if they dont have access to the area.\nThis can be Overrided per-area.", MessageType.Info);
                    accessGate.playerMoverTransform = (Transform)EditorGUILayout.ObjectField(accessGate.playerMoverTransform, typeof(Transform), true);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorFunctions.DrawPlayerMoverField(ref accessGate.playerMover);
                    EditorFunctions.DrawNotificationField(ref accessGate.notificationSystem);
                }
            }
        }
        
        //##############################################################################################################
        #region Object selection
        
        private void DrawObjectSelection(ref GameObject[] _turnOff, ref GameObject[] _turnOn)
        {
            if (_turnOn == null)
            {
                _turnOn = new GameObject[0];
            }
            if (_turnOff == null)
            {
                _turnOff = new GameObject[0];
            }
            
            EditorGUILayout.BeginHorizontal(); 
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Toggle On:", "These objects turn ON when the whitelist gets toggled ON\n\nThese objects turn OFF when the whitelist gets toggled OFF"),GUILayout.MinWidth(0));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button( "+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = _turnOff.Length + 1;
                    Array.Resize(ref _turnOff, newVal);
                }
                if (GUILayout.Button( "-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(_turnOff.Length - 1, 0, int.MaxValue);
                    if (_turnOff.Length == newVal) return;
                    Array.Resize(ref _turnOff, newVal);
                }
                if (DropAreaGUI(true, ref _turnOff, ref _turnOn)) return;
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < _turnOff.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        _turnOff[i] = (GameObject)EditorGUILayout.ObjectField(_turnOff[i], typeof(GameObject),true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "Enforce state on Start.\n" +
                                                               "\n" +
                                                               "Sets if the object should be set \"OFF\" during Start.\n" +
                                                               "If toggled off, the object's state will be un touched and left as the current state on Upload.\n" +
                                                               "\n" +
                                                               "This should be ON in most cases, and is used for fixing overlapping whitelists."));
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button( "swp", GUILayout.Width(35f)))
                        {
                            int newVal = _turnOn.Length + 1;
                            Array.Resize(ref _turnOn, newVal);
                            _turnOn[^1] = _turnOff[i];
                            RemoveFromArray(i,ref  _turnOff);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default On\" list"));
                        }
                        if (GUILayout.Button( "-", GUILayout.Width(20f)))
                        {
                            RemoveFromArray(i,ref  _turnOff);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Removes object from the list."));
                        }
                    }
                }
            }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Toggle Off:", "These objects turn OFF when the whitelist gets toggled ON\n\nThese objects turn ON when the whitelist gets toggled OFF"),GUILayout.MinWidth(0));
                if (GUILayout.Button( "Swap Lists", GUILayout.Width(75f)))
                {
                    int TDOff = _turnOff.Length;
                    int TDOn = _turnOn.Length;
                    
                    GameObject[] tmpList = new GameObject[TDOn];
                    Array.Copy(_turnOn,tmpList, TDOn);
                    
                    _turnOn = new GameObject[TDOff];
                    Array.Copy(_turnOff,_turnOn, TDOff);
                    
                    _turnOff = new GameObject[tmpList.Length];
                    Array.Copy(tmpList,_turnOff, tmpList.Length);
                    
                    bool[] tmpList2 = new bool[TDOn];
                    
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button( "+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = _turnOn.Length + 1;
                    Array.Resize(ref _turnOn, newVal);
                }
                if (GUILayout.Button( "-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(_turnOn.Length - 1, 0, int.MaxValue);
                    if (_turnOn.Length == newVal) return;
                    Array.Resize(ref _turnOn, newVal);
                }
                if (DropAreaGUI(true, ref _turnOff, ref _turnOn)) return;
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < _turnOn.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        _turnOn[i] = (GameObject)EditorGUILayout.ObjectField(_turnOn[i], typeof(GameObject), true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "Enforce state on Start.\n" +
                                                               "\n" +
                                                               "Sets if the object should be set \"ON\" during Start.\n" +
                                                               "If toggled off, the object's state will be un touched and left as the current state on Upload.\n" +
                                                               "\n" +
                                                               "This should be ON in most cases, and is used for fixing overlapping whitelists."));
                        EditorGUI.EndDisabledGroup();
                        
                        if (GUILayout.Button( "swp", GUILayout.Width(35f)))
                        {
                            int newVal = _turnOff.Length + 1;
                            Array.Resize(ref _turnOff, newVal);
                            _turnOff[^1] = _turnOn[i];
                            RemoveFromArray(i,ref  _turnOn);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default Off\" list"));
                        }
                        if (GUILayout.Button( "-", GUILayout.Width(20f)))
                        {
                            RemoveFromArray(i,ref  _turnOn);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Removes object from the list."));
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        public bool DropAreaGUI(bool _array, ref GameObject[] _turnOff, ref GameObject[] _turnOn)
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
                        if (_array)
                        {
                            int k = _turnOn.Length;
                            Array.Resize(ref _turnOn,_turnOn.Length + DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                _turnOn[k] = (GameObject)DragAndDrop.objectReferences[i];
                                k++;
                            }
                            return true;
                        }
                        else
                        {
                            int k = _turnOff.Length;
                            Array.Resize(ref _turnOff,_turnOff.Length + DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                _turnOff[k] = (GameObject)DragAndDrop.objectReferences[i];
                                k++;
                            }
                            return true;
                        }
                        
                    }

                    break;
            }
            return false;
        }
        
        private void RemoveFromArray(int _i, ref GameObject[] _array)
        {
            int newVal = Mathf.Clamp(_array.Length - 1, 0, int.MaxValue);
            if (_array.Length == newVal) return;
            GameObject[] newArray = new GameObject[newVal];
            int k = 0;
            for (int i = 0; i < _array.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = _array[i];
                k++;
            }
            _array =  newArray;
        }
        #endregion
        //##############################################################################################################
    }

    [CustomEditor(typeof(AccessGate_Trigger))]
    public class AccessGate_Trigger_Editor : Editor
    {
        private AccessGate_Trigger accessGateTrigger;
        
        private void OnEnable()
        {
            accessGateTrigger = (AccessGate_Trigger)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(accessGateTrigger, $"Undo on {accessGateTrigger.name}");

            EditorFunctions.DrawDisBridgeLogo();
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(accessGateTrigger);
            DrawDefaultInspector();

            PrefabUtility.RecordPrefabInstancePropertyModifications(accessGateTrigger);
            serializedObject.ApplyModifiedProperties();
        }
        private void OnSceneGUI()
        {
            if (accessGateTrigger.playerMoverTransform != null)
            {
                EditorFunctions.DrawSpawnPoint(accessGateTrigger.playerMoverTransform);
            }
            else
            {
                if (accessGateTrigger.accessGate != null)
                {
                    if (accessGateTrigger.accessGate.playerMoverTransform != null)
                    {
                        EditorFunctions.DrawSpawnPoint(accessGateTrigger.accessGate.playerMoverTransform);
                    }
                }
            }
        }
    }
}