//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class PlayerTrackerExample_DualChild : PlayerTracker_ColliderClass
    {
        public PlayerTrackerExample_DualParent parent;

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            parent.isInChild = true;
            parent._UpdateStates();
        }
        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            parent.isInChild = false;
            parent._UpdateStates();
        }
    }
}