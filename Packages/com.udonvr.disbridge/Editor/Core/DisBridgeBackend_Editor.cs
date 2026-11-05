//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(DisBridgeBackend))]
    public class DisBridgeBackend_Editor : Editor
    {
        DisBridgeBackend myScript;

        SerializedProperty updateRate;
        
        
        private void OnEnable()
        {
            myScript = (DisBridgeBackend)target;

            updateRate = serializedObject.FindProperty(nameof(DisBridgeBackend.updateRate));
        }

        public override void OnInspectorGUI()
        {
            EditorFunctions.DrawDisBridgeLogo();
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            EditorGUILayout.PropertyField(updateRate, new GUIContent("Resync Timer", "The time it takes DisBridge to check if there's any new users to add or remove from the Userlist. (In minutes)"));

            DrawAdvanceMode();
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }

        private bool advanceMode = false;

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
    }
}