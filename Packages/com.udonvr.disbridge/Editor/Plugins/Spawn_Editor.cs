//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UdonVR.DisBridge.Editors;
using UdonVR.DisBridge.Plugins;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors.Plugins
{

    [CustomEditor(typeof(DisBridge_Spawn))]
    public class Spawn_Editor : Editor
    {
        private DisBridge_Spawn _script;

        private SerializedProperty _roleContainers;
        private SerializedProperty _spawns;
        private SerializedProperty _fadeCube;
        private SerializedProperty _matProperty;
        private SerializedProperty _matProperty2;
        private SerializedProperty _fadeLength;

        private SerializedProperty _fadeCurve;
        private SerializedProperty _iconCurve;


        public override void OnInspectorGUI()
        {
            _script = (DisBridge_Spawn)target;
            Undo.RecordObject(_script, $"Generic Undo event on {nameof(Spawn_Editor)} -- {target.name}");
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.PlaymodeWarning(_script);

            EditorFunctions.DrawPluginManagerCheck(_script, false);

            if (_script.spawns.Length != _script._roleContainers.Length)
            {
                Array.Resize(ref _script.spawns, _script._roleContainers.Length);
            }

            _roleContainers = serializedObject.FindProperty(nameof(DisBridge_Spawn._roleContainers));
            _spawns = serializedObject.FindProperty(nameof(DisBridge_Spawn.spawns));

            _fadeCube = serializedObject.FindProperty(nameof(DisBridge_Spawn.fadeCube));
            _matProperty = serializedObject.FindProperty(nameof(DisBridge_Spawn.matProperty));
            _matProperty2 = serializedObject.FindProperty(nameof(DisBridge_Spawn.matProperty2));
            _fadeLength = serializedObject.FindProperty(nameof(DisBridge_Spawn.fadeLength));

            _fadeCurve = serializedObject.FindProperty(nameof(DisBridge_Spawn.fadeCurve));
            _iconCurve = serializedObject.FindProperty(nameof(DisBridge_Spawn.iconCurve));

            DrawProperty(_fadeCurve);
            DrawProperty(_iconCurve);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawProperty(_fadeCube);
                DrawProperty(_matProperty);
                DrawProperty(_matProperty2);
                DrawProperty(_fadeLength);
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                DrawProperty(_roleContainers);
                DrawProperty(_spawns);
            }

            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }

        private void DrawProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
        }
    }
}