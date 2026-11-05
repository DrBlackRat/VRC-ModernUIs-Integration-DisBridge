//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    
    public class PlayerTrackerExample_PlayerCounter : PlayerTracker_ColliderClass
    {
        public int playerCounter = 0;
        public TextMeshPro textMesh;

        private void Start()
        {
            textMesh.text = "0";
        }

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            playerCounter++;
            textMesh.text = playerCounter.ToString();
        }

        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            playerCounter--;
            textMesh.text = playerCounter.ToString();
        }
    }
}