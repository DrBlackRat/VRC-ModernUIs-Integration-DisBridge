//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{

    public class PlayerTrackerExample_ObjectTogggle : PlayerTracker_ColliderClass
    {
        public GameObject[] objectsToToggleOn;
        public GameObject[] objectsToToggleOff;

        private void Start()
        {
            for (int i = 0; i < objectsToToggleOn.Length; i++)
            {
                objectsToToggleOn[i].SetActive(false);
            }

            for (int i = 0; i < objectsToToggleOff.Length; i++)
            {
                objectsToToggleOff[i].SetActive(true);
            }
        }

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            for (int i = 0; i < objectsToToggleOn.Length; i++)
            {
                objectsToToggleOn[i].SetActive(true);
            }

            for (int i = 0; i < objectsToToggleOff.Length; i++)
            {
                objectsToToggleOff[i].SetActive(false);
            }
        }

        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            for (int i = 0; i < objectsToToggleOn.Length; i++)
            {
                objectsToToggleOn[i].SetActive(false);
            }

            for (int i = 0; i < objectsToToggleOff.Length; i++)
            {
                objectsToToggleOff[i].SetActive(true);
            }
        }
        
    }
}