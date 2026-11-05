//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using UdonVR.DisBridge;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase.Editor.BuildPipeline;

namespace UdonVR.DisBridge.Editors
{
    public class PlayerTracker_BuildTools : IVRCSDKBuildRequestedCallback
    {
        private static string logprefix = "[DisBridge Build Tools - PlayerTracker]";

        public int callbackOrder => -9999;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }

        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
            Debug.Log($"{logprefix} Starting");

            bool buildPC = false;
        #if UNITY_64
            buildPC = true;
        #endif
            Debug.Log($"{logprefix} is PC {buildPC}");

            PlayerTracker_ColliderClass[] triggers = GameObject.FindObjectsOfType<PlayerTracker_ColliderClass>();
            if (triggers == null || triggers.Length == 0)
            {
                Debug.Log($"{logprefix} No systems using PlayerTracker, not creating.");
                return;
            }
            
            PlayerTracker playerTracker = GameObject.FindObjectOfType<PlayerTracker>();
            if (playerTracker == null)
            {
                Debug.Log($"{logprefix} No PlayerTracker found, creating one.");
                playerTracker = CreatePrefabs.CreatePlayerTracker().GetComponent<PlayerTracker>();
            }
            else
            {
                Debug.Log($"{logprefix} PlayerTracker found.");
            }

            Debug.Log($"{logprefix} Assigning PlayerTracker to systems.");
            for (int i = 0; i < triggers.Length; i++)
            {
                triggers[i].__playerTracker_internal = playerTracker;
                if (playerTracker.useCustomLayer && playerTracker.customLayer != -1)
                {
                    triggers[i].gameObject.layer = playerTracker.customLayer;
                }
            }
            
            SetupPlayerTracker(playerTracker);
        }

        private static void SetupPlayerTracker(PlayerTracker playerTracker)
        {
            GameObject tmp = playerTracker.playerTrackerParent.GetChild(0).gameObject;
            PlayerTracker_Tracker tmpTracker = tmp.GetComponentInChildren<PlayerTracker_Tracker>();

            SphereCollider sphereCollider = playerTracker.GetComponentInChildren<SphereCollider>();
            CapsuleCollider capsuleCollider = playerTracker.GetComponentInChildren<CapsuleCollider>();
            
            switch (playerTracker.trackerMode)
            {
                case TrackerMode.Sphere:
                    tmpTracker.thisCollider = sphereCollider;
                    GameObject.DestroyImmediate(capsuleCollider.gameObject);
                    break;
                case TrackerMode.Capsule:
                    tmpTracker.thisCollider = capsuleCollider;
                    GameObject.DestroyImmediate(sphereCollider.gameObject);
                    break;
            }
            if (playerTracker.useCustomLayer && playerTracker.customLayer != -1)
            {
                tmpTracker.thisCollider.gameObject.layer = playerTracker.customLayer;
            }
            
            
            
            //playerTracker.Trackers_UnUsed = new DataList();
            //playerTracker.Trackers_Used = new DataList();
            //playerTracker.Trackers_UnUsed.Add(tmpTracker);
            for (int i = 0; i < playerTracker.playerCount; i++)
            {
                GameObject tmp2 = UnityEngine.Object.Instantiate(tmp, playerTracker.playerTrackerParent);
                //playerTracker.Trackers_UnUsed.Add(tmp2.GetComponent<PlayerTracker_Tracker>());
                tmp2.name = $"Tracker ({i+1})";
            }
            
            Debug.Log($"{logprefix} Trackers_UnUsed: [{playerTracker.Trackers_UnUsed.Count}]");
            Debug.Log($"{logprefix} Trackers_Used: [{playerTracker.Trackers_Used.Count}]");
        }
    }
}