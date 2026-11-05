//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors.Plugins
{
    [CustomEditor(typeof(SimpleDoor))]
    public class SimpleDoorEditor : Editor
    {
        private SimpleDoor myScript;
        SerializedProperty doorSpawn;
        SerializedProperty doorInteractMode;
        private void OnEnable()
        {
            doorSpawn = serializedObject.FindProperty(nameof(SimpleDoor.doorSpawn));
            doorInteractMode = serializedObject.FindProperty(nameof(SimpleDoor.doorInteractMode));
        }
        public override void OnInspectorGUI()
        {
            myScript = (SimpleDoor)target;
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");
            EditorFunctions.DrawDisBridgeLogo();
            EditorFunctions.DrawPluginManagerCheck(myScript,false,true);
            EditorFunctions.DrawPermissions(myScript);
            EditorFunctions.DrawPlayerMoverField(ref myScript.playerMover);
            EditorGUILayout.PropertyField(doorSpawn, new GUIContent("TP Point:", "Target Transform to move the player to."));
            EditorGUILayout.PropertyField(doorInteractMode, new GUIContent("Door Mode:", "Changes the Door Interaction Mode.\nInteract: Player clicks on the door.\nPlayerEnter: When the player enters the trigger, they move.\nPlayerLeave: When the player leaves the trigger, they move."));

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

