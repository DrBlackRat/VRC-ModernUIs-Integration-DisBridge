using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace UdonVR.DisBridge.Editors
{

    public class AccessGate_BuildTools : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -9999;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }

        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
            AccessGate[] accessGates = GameObject.FindObjectsOfType<AccessGate>();
            AccessGate_Trigger[] accessGateTriggers = GameObject.FindObjectsOfType<AccessGate_Trigger>();
            foreach (var accessGate in accessGates)
            {
                accessGate._UpdateDoorState(false);
                
                // Add all triggers to their access gates
                List<AccessGate_Trigger> theseTriggers = new List<AccessGate_Trigger>();
                foreach (var trigger in accessGateTriggers)
                {
                    if (trigger.accessGate == accessGate) theseTriggers.Add(trigger);
                }
                accessGate.myTriggers = theseTriggers.ToArray();
            }
        }
    }
}