//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    public class PrivateRoom_ComTrigger : UdonSharpBehaviour
    {
        public PrivateRoom_Display display;
        [SerializeField] internal PrivateRoom_InviteMenu inviteMenu; 

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            Debug.LogWarning($"OnPlayerTriggerEnter {player.displayName}");
            display._AddUser(player);
            
            if (Utilities.IsValid(inviteMenu))
                inviteMenu._AddToAtDoor(player);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            Debug.LogWarning($"OnPlayerTriggerExit {player.displayName}");
            display._RemoveUser(player);
            
            if (Utilities.IsValid(inviteMenu))
                inviteMenu._RemoveFromAtDoor(player);
        }
    }
}