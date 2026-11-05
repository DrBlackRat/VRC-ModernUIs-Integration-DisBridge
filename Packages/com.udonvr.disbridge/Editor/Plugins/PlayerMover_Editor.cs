//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UdonVR.DisBridge.Plugins;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(PlayerMover))]
    public class PlayerMover_Editor : Editor
    {
        SerializedProperty fadeCube;
        SerializedProperty matProperty;
        SerializedProperty matProperty2;
        SerializedProperty fadeLength;
        SerializedProperty fadeCurve_In;
        SerializedProperty iconCurve_In;
        SerializedProperty fadeCurve_Out;
        SerializedProperty iconCurve_Out;
        
        bool advancedFoldout = false;
        private void OnEnable()
        {
            fadeCube = serializedObject.FindProperty(nameof(PlayerMover.fadeCube));
            matProperty = serializedObject.FindProperty(nameof(PlayerMover.matProperty));
            matProperty2 = serializedObject.FindProperty(nameof(PlayerMover.matProperty2));
            fadeLength = serializedObject.FindProperty(nameof(PlayerMover.fadeLength));
            fadeCurve_In = serializedObject.FindProperty(nameof(PlayerMover.fadeCurve_In));
            iconCurve_In = serializedObject.FindProperty(nameof(PlayerMover.iconCurve_In));
            fadeCurve_Out = serializedObject.FindProperty(nameof(PlayerMover.fadeCurve_Out));
            iconCurve_Out = serializedObject.FindProperty(nameof(PlayerMover.iconCurve_Out));
        }

        public override void OnInspectorGUI()
        {
            PlayerMover myScript = (PlayerMover)target;
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.PlaymodeWarning(myScript);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Settings:", ""));
                EditorGUILayout.PropertyField(fadeLength, new GUIContent("Fade Length", ""));
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Fade In Curves:", ""));
                    EditorGUILayout.PropertyField(fadeCurve_In, new GUIContent("Fade:", ""));
                    EditorGUILayout.PropertyField(iconCurve_In, new GUIContent("Icon:", ""));
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Fade Out Curves:", ""));
                    EditorGUILayout.PropertyField(fadeCurve_Out, new GUIContent("Fade:", ""));
                    EditorGUILayout.PropertyField(iconCurve_Out, new GUIContent("Icon:", ""));
                }
            }

            DrawAdvanceMode();
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAdvanceMode()
        {
            if (GUILayout.Button("Advance Mode"))
            {
                advancedFoldout = !advancedFoldout;
            }
            if (!advancedFoldout) return;
            EditorGUILayout.PropertyField(fadeCube, new GUIContent("Target Mesh", ""));
            EditorGUILayout.PropertyField(matProperty, new GUIContent("Material Property Fade", ""));
            EditorGUILayout.PropertyField(matProperty2, new GUIContent("Material Property Icon", ""));
        }
    }
}