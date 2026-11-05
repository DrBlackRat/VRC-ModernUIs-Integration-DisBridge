
using System;
using UdonSharp;
using UdonVR.DisBridge.Plugins.PrivateRooms;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.Udon;

namespace UdonVR.DisBridge.Editors
{
    public class PrivateRoom_BuildTools : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -9999;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }

        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
            PrivateRoom_Display_Inside[] __DisplayInsides = GameObject.FindObjectsOfType<PrivateRoom_Display_Inside>();
            foreach (PrivateRoom_Display_Inside __DisplayInside in __DisplayInsides)
            {
                try
                {
                    __DisplayInside.messageInput.characterLimit = __DisplayInside.door.pvtRoom.messageLimit;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DisBridge][BuildTools] Error setting up PrivateRooms:\n{e.Message}");
                }
            }
        }
    }
}


