//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Data;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(PermissionGroup))]
    public class PermissionGroup_Editor : Editor
    {
        PermissionGroup myScript;
        SerializedProperty RoleContainers;
        SerializedProperty Products;
        SerializedProperty Names;
        private string owo;

        private void OnEnable()
        {
            RoleContainers = serializedObject.FindProperty(nameof(myScript.RoleContainers));
            Products = serializedObject.FindProperty(nameof(myScript.Products));
            Names = serializedObject.FindProperty(nameof(myScript.Names));
        }

        public override void OnInspectorGUI()
        {
            myScript = (PermissionGroup)target;
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            EditorFunctions.DrawDisBridgeLogo();
            
            _DrawRoleContainers();
            EditorFunctions.GuiLine();
            _DrawProducts();
            EditorFunctions.GuiLine();
            _DrawNames();
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }

        private void _DrawRoleContainers()
        {
            EditorGUILayout.PropertyField(RoleContainers, new GUIContent("Role Containers", ""));
        }

        private void _DrawProducts()
        {
            EditorGUILayout.PropertyField(Products, new GUIContent("Creator Economy Products", ""));
        }

        private void _DrawNames()
        {
            EditorGUILayout.PropertyField(Names, new GUIContent("Player Names", ""));
            /*using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {

                for (int i = 0; i < myScript.Names.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        myScript.Names[i] = GUILayout.TextField(myScript.Names[i].ToString());
                        RemoveName(i);
                    }
                }
                _NameField();
            }
            */
        }

        private void RemoveName(int _index)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("", GUILayout.Width(100f)))
            {
                //myScript.Names.Remove(myScript.Names[_index]);
            }
            GUI.backgroundColor = Color.white;
        }

        private void _NameField()
        {
            /*
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                owo = GUILayout.TextField(owo);
                if (string.IsNullOrEmpty(owo))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(100f)))
                    {
                        //myScript.Names.Add("");
                    }
                    EditorGUI.BeginDisabledGroup(true);
                    GUILayout.Button("Remove", GUILayout.Width(100f));
                    EditorGUI.EndDisabledGroup();
                }
                else if (!myScript.Names.Contains(owo))
                {
                    if (GUILayout.Button("Add", GUILayout.Width(100f)))
                    {
                        //myScript.Names.Add(owo);
                        owo = "";
                    }
                    EditorGUI.BeginDisabledGroup(true);
                    GUILayout.Button("Remove", GUILayout.Width(100f));
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    GUILayout.Button("Add", GUILayout.Width(100f));
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("Remove", GUILayout.Width(100f)))
                    {
                        //myScript.Names.Remove(owo);
                    }
                }
            }
            */
        }
    }
}