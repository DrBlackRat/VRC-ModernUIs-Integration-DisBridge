//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace UdonVR.DisBridge.Editors
{

    [InitializeOnLoad]
    public class DefineSymbol : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;
        private static string sybl = "UVR_DISBRIDGE";

        static void AddDefine()
        {
            var platform = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            if (!defines.Contains(sybl))
            {
                if (defines.Length > 0)
                {
                    defines += ";";
                }

                defines += sybl;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines);
            }
        }

        static DefineSymbol()
        {
            AddDefine();
        }

        public void OnActiveBuildTargetChanged(BuildTarget prev, BuildTarget cur)
        {
            AddDefine();
        }
    }
}
#endif