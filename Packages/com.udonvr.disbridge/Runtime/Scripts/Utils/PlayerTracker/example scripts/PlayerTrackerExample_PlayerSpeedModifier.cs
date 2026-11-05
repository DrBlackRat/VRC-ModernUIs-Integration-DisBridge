//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class PlayerTrackerExample_PlayerSpeedModifier : PlayerTracker_ColliderClass
    {
        public float speedModifier;
        private float speedMod_walk;
        private float speedMod_run;
        private void Start()
        {
            speedMod_walk = Networking.LocalPlayer.GetWalkSpeed();
            speedMod_run = Networking.LocalPlayer.GetRunSpeed();
        }

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            speedMod_walk = player.GetWalkSpeed();
            speedMod_run = player.GetRunSpeed();
            player.SetWalkSpeed(speedMod_walk/speedModifier);
            player.SetRunSpeed(speedMod_run/speedModifier);
        }

        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            player.SetWalkSpeed(speedMod_walk);
            player.SetRunSpeed(speedMod_run);
        }
    }
}