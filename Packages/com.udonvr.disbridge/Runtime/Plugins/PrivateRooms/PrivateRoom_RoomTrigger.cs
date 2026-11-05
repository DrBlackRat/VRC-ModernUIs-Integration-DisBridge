//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UdonVR.DisBridge.Plugins.PrivateRooms;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PrivateRoom_RoomTrigger : PlayerTracker_ColliderClass
    {
        [SerializeField] private PrivateRoom_RoomTriggerController controller;

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            controller._AddPlayer(player);
        }

        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            controller._RemovePlayer(player);
        }
    }
}