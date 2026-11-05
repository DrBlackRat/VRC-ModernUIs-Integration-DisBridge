using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.Editors
{
    public class UserAccessList_Button_Editor : Editor
    {
        UserAccessList_Button myScript;

        private void OnEnable()
        {
            myScript = (UserAccessList_Button)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }
    }
}