//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UdonVR.DisBridge.Editors;
using UdonVR.DisBridge.Plugins;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UdonVR.DisBridge.Editors.Plugins
{
    [CustomEditor(typeof(DisBridgeWhitelistV2))]
    public class DisBridgeWhitelistV2_Editor : UnityEditor.Editor
    {
        DisBridgeWhitelistV2 myScript;
        private Rect typeRect;

        private DisBridgeWhitelist whitelistV1;
        public override void OnInspectorGUI()
        {
            myScript = (DisBridgeWhitelistV2)target;
            Undo.RecordObject(myScript.gameObject,$"Undo on {myScript.name}");
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.DrawPluginManagerCheck(myScript, false, true);
            EditorFunctions.DrawPermissions(myScript, false);
            DrawSettings();
            DrawObjectSelection();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                whitelistV1 = (DisBridgeWhitelist)EditorGUILayout.ObjectField(whitelistV1, typeof(DisBridgeWhitelist), true, GUILayout.MinWidth(0));
            
                if (GUILayout.Button( "Migrate from V1", GUILayout.MaxWidth(150f)))
                {
                    if (whitelistV1 == null) return;
                    myScript.TargetsDefaultOn = whitelistV1.TargetsDefaultOn;
                    myScript.TargetsDefaultOff = whitelistV1.TargetsDefaultOff;

                    myScript.useStaff = whitelistV1.useStaff || whitelistV1.checkStaff;
                    myScript.useSupporters = whitelistV1.useSupporters || whitelistV1.checkSupporters;
                }
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }
        
        private void DrawSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Is Lock state Global? ", GUILayout.MinWidth(0));
                        myScript.isGlobal = EditorGUILayout.Toggle(myScript.isGlobal, GUILayout.Width(25f));
                    }

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Can Master Unlock? ", GUILayout.MinWidth(0));
                        myScript.canMasterUnlock = EditorGUILayout.Toggle(myScript.canMasterUnlock, GUILayout.Width(25f));
                    }
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Can Instance Owner Unlock? ", GUILayout.MinWidth(0));
                        myScript.canOwnerUnlock = EditorGUILayout.Toggle(myScript.canOwnerUnlock, GUILayout.Width(25f));
                    }
                }
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Enforce Lock state on Start?", GUILayout.MinWidth(0));
                    //EditorGUILayout.BeginHorizontal();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUI.BeginDisabledGroup(myScript.neverEnforceLockState);
                        EditorGUILayout.LabelField("Always", GUILayout.MinWidth(0));
                        myScript.alwaysEnforceLockState =
                            EditorGUILayout.Toggle(myScript.alwaysEnforceLockState, GUILayout.Width(25f));
                        EditorGUI.EndDisabledGroup();
                    }

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState);
                        EditorGUILayout.LabelField("Never", GUILayout.MinWidth(0));
                        myScript.neverEnforceLockState =
                            EditorGUILayout.Toggle(myScript.neverEnforceLockState, GUILayout.Width(25f));
                        EditorGUI.EndDisabledGroup();
                    }
                    //EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        private void DrawObjectSelection()
        {
            if (myScript.TargetsDefaultOn == null)
            {
                myScript.TargetsDefaultOn = new GameObject[0];
            }
            if (myScript.TargetsDefaultOff == null)
            {
                myScript.TargetsDefaultOff = new GameObject[0];
            }
            
            if (myScript.TargetsDefaultOn_EnforceStart == null)
            {
                myScript.TargetsDefaultOn_EnforceStart = new bool[myScript.TargetsDefaultOn.Length];
            }
            if (myScript.TargetsDefaultOff_EnforceStart == null)
            {
                myScript.TargetsDefaultOff_EnforceStart = new bool[myScript.TargetsDefaultOff.Length];
            }

            if (myScript.TargetsDefaultOn_EnforceStart.Length != myScript.TargetsDefaultOn.Length)
            {
                Array.Resize(ref myScript.TargetsDefaultOn_EnforceStart, myScript.TargetsDefaultOn.Length);
            }
            if (myScript.TargetsDefaultOff_EnforceStart.Length != myScript.TargetsDefaultOff.Length)
            {
                Array.Resize(ref myScript.TargetsDefaultOff_EnforceStart, myScript.TargetsDefaultOff.Length);
            }
            
            
            EditorGUILayout.BeginHorizontal(); 
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Toggle On:", "These objects turn ON when the whitelist gets toggled ON\n\nThese objects turn OFF when the whitelist gets toggled OFF"),GUILayout.MinWidth(0));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button( "+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = myScript.TargetsDefaultOff.Length + 1;
                    Array.Resize(ref myScript.TargetsDefaultOff, newVal);
                    Array.Resize(ref myScript.TargetsDefaultOff_EnforceStart, newVal);
                    myScript.TargetsDefaultOff_EnforceStart[^1] = true;
                }
                if (GUILayout.Button( "-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.TargetsDefaultOff.Length - 1, 0, int.MaxValue);
                    if (myScript.TargetsDefaultOff.Length == newVal) return;
                    Array.Resize(ref myScript.TargetsDefaultOff, newVal);
                    Array.Resize(ref myScript.TargetsDefaultOff_EnforceStart, newVal);
                }
                if (DropAreaGUI(false)) return;
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < myScript.TargetsDefaultOff.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.TargetsDefaultOff[i] = (GameObject)EditorGUILayout.ObjectField(myScript.TargetsDefaultOff[i], typeof(GameObject),true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState || myScript.neverEnforceLockState);
                        myScript.TargetsDefaultOff_EnforceStart[i] = EditorGUILayout.Toggle(myScript.TargetsDefaultOff_EnforceStart[i], GUILayout.Width(15f));
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
                            int newVal = myScript.TargetsDefaultOn.Length + 1;
                            Array.Resize(ref myScript.TargetsDefaultOn, newVal);
                            Array.Resize(ref myScript.TargetsDefaultOn_EnforceStart, newVal);
                            myScript.TargetsDefaultOn[^1] = myScript.TargetsDefaultOff[i];
                            myScript.TargetsDefaultOn_EnforceStart[^1] = myScript.TargetsDefaultOff_EnforceStart[i];
                            RemoveFromDefaultOff(i);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default On\" list"));
                        }
                        if (GUILayout.Button( "-", GUILayout.Width(20f)))
                        {
                            RemoveFromDefaultOff(i);
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
                    int TDOff = myScript.TargetsDefaultOff.Length;
                    int TDOn = myScript.TargetsDefaultOn.Length;
                    
                    GameObject[] tmpList = new GameObject[TDOn];
                    Array.Copy(myScript.TargetsDefaultOn,tmpList, TDOn);
                    
                    myScript.TargetsDefaultOn = new GameObject[TDOff];
                    Array.Copy(myScript.TargetsDefaultOff,myScript.TargetsDefaultOn, TDOff);
                    
                    myScript.TargetsDefaultOff = new GameObject[tmpList.Length];
                    Array.Copy(tmpList,myScript.TargetsDefaultOff, tmpList.Length);
                    
                    bool[] tmpList2 = new bool[TDOn];
                    
                    Array.Copy(myScript.TargetsDefaultOn_EnforceStart,tmpList2, TDOn);
                    
                    myScript.TargetsDefaultOn_EnforceStart = new bool[TDOff];
                    Array.Copy(myScript.TargetsDefaultOff_EnforceStart,myScript.TargetsDefaultOn_EnforceStart, TDOff);
                    
                    myScript.TargetsDefaultOff_EnforceStart = new bool[tmpList2.Length];
                    Array.Copy(tmpList2,myScript.TargetsDefaultOff_EnforceStart, tmpList2.Length);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button( "+", GUILayout.MaxWidth(50f)))
                {
                    int newVal = myScript.TargetsDefaultOn.Length + 1;
                    Array.Resize(ref myScript.TargetsDefaultOn, newVal);
                    Array.Resize(ref myScript.TargetsDefaultOn_EnforceStart, newVal);
                    myScript.TargetsDefaultOn_EnforceStart[^1] = true;
                }
                if (GUILayout.Button( "-", GUILayout.MaxWidth(50f)))
                {
                    int newVal = Mathf.Clamp(myScript.TargetsDefaultOn.Length - 1, 0, int.MaxValue);
                    if (myScript.TargetsDefaultOn.Length == newVal) return;
                    Array.Resize(ref myScript.TargetsDefaultOn, newVal);
                    Array.Resize(ref myScript.TargetsDefaultOn_EnforceStart, newVal);
                }
                if (DropAreaGUI(true)) return;
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < myScript.TargetsDefaultOn.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        myScript.TargetsDefaultOn[i] = (GameObject)EditorGUILayout.ObjectField(myScript.TargetsDefaultOn[i], typeof(GameObject), true, GUILayout.MinWidth(0));
                        typeRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(typeRect, new GUIContent("", "The object to toggle."));
                        EditorGUI.BeginDisabledGroup(myScript.alwaysEnforceLockState || myScript.neverEnforceLockState);
                        myScript.TargetsDefaultOn_EnforceStart[i] = EditorGUILayout.Toggle(myScript.TargetsDefaultOn_EnforceStart[i], GUILayout.Width(15f));
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
                            int newVal = myScript.TargetsDefaultOff.Length + 1;
                            Array.Resize(ref myScript.TargetsDefaultOff, newVal);
                            Array.Resize(ref myScript.TargetsDefaultOff_EnforceStart, newVal);
                            myScript.TargetsDefaultOff[^1] = myScript.TargetsDefaultOn[i];
                            myScript.TargetsDefaultOff_EnforceStart[^1] = myScript.TargetsDefaultOn_EnforceStart[i];
                            RemoveFromDefaultOn(i);
                        }
                        else
                        {
                            typeRect = GUILayoutUtility.GetLastRect();
                            GUI.Label(typeRect, new GUIContent("", "Swaps the object to the \"Default Off\" list"));
                        }
                        if (GUILayout.Button( "-", GUILayout.Width(20f)))
                        {
                            RemoveFromDefaultOn(i);
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

        private void RemoveFromDefaultOff(int _i)
        {
            int newVal = Mathf.Clamp(myScript.TargetsDefaultOff.Length - 1, 0, int.MaxValue);
            if (myScript.TargetsDefaultOff.Length == newVal) return;
            GameObject[] newArray = new GameObject[newVal];
            bool[] newArrayBool = new bool[newVal];
            int k = 0;
            for (int i = 0; i < myScript.TargetsDefaultOff.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.TargetsDefaultOff[i];
                newArrayBool[k] = myScript.TargetsDefaultOff_EnforceStart[i];
                k++;
            }

            myScript.TargetsDefaultOff = newArray;
            myScript.TargetsDefaultOff_EnforceStart = newArrayBool;
            //myScript.TargetsDefaultOff = new GameObject[newVal];
            //Array.Copy(newArray,myScript.TargetsDefaultOff,0);
        }
        private void RemoveFromDefaultOn(int _i)
        {
            int newVal = Mathf.Clamp(myScript.TargetsDefaultOn.Length - 1, 0, int.MaxValue);
            if (myScript.TargetsDefaultOn.Length == newVal) return;
            GameObject[] newArray = new GameObject[newVal];
            bool[] newArrayBool = new bool[newVal];
            int k = 0;
            for (int i = 0; i < myScript.TargetsDefaultOn.Length; i++)
            {
                if (_i == i) { continue; }
                newArray[k] = myScript.TargetsDefaultOn[i];
                newArrayBool[k] = myScript.TargetsDefaultOn_EnforceStart[i];
                k++;
            }

            myScript.TargetsDefaultOn = newArray;
            myScript.TargetsDefaultOn_EnforceStart = newArrayBool;
            //myScript.TargetsDefaultOn = new GameObject[newVal];
            //Array.Copy(newArray,myScript.TargetsDefaultOn,0);
        }

        public bool DropAreaGUI(bool _array)
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
                            int k = myScript.TargetsDefaultOn_EnforceStart.Length;
                            Array.Resize(ref myScript.TargetsDefaultOn,myScript.TargetsDefaultOn.Length + DragAndDrop.objectReferences.Length);
                            Array.Resize(ref myScript.TargetsDefaultOn_EnforceStart,myScript.TargetsDefaultOn.Length);
                            //Array.Copy(DragAndDrop.objectReferences,0,myScript.TargetsDefaultOn,myScript.TargetsDefaultOn.Length - 1, DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                myScript.TargetsDefaultOn[k] = (GameObject)DragAndDrop.objectReferences[i];
                                myScript.TargetsDefaultOn_EnforceStart[k] = true;
                                Debug.Log($"[i:{i}][k:{k}]{myScript.TargetsDefaultOn[k].name}");
                                k++;
                            }
                            return true;
                        }
                        else
                        {
                            int k = myScript.TargetsDefaultOff_EnforceStart.Length;
                            Array.Resize(ref myScript.TargetsDefaultOff,myScript.TargetsDefaultOff.Length + DragAndDrop.objectReferences.Length);
                            Array.Resize(ref myScript.TargetsDefaultOff_EnforceStart,myScript.TargetsDefaultOff.Length);
                            //Array.Copy(DragAndDrop.objectReferences,0,myScript.TargetsDefaultOn,myScript.TargetsDefaultOn.Length - 1, DragAndDrop.objectReferences.Length);
                            
                            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                            {
                                myScript.TargetsDefaultOff[k] = (GameObject)DragAndDrop.objectReferences[i];
                                myScript.TargetsDefaultOff_EnforceStart[k] = true;
                                Debug.Log($"[i:{i}][k:{k}]{myScript.TargetsDefaultOff[k].name}");
                                k++;
                            }
                            return true;
                        }
                        
                    }

                    break;
            }
            return false;
        }
    }
}