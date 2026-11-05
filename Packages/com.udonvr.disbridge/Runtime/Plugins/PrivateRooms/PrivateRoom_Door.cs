//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    public class PrivateRoom_Door : UdonSharpBehaviour
    {
        [SerializeField] internal Transform spawnPoint;

        [SerializeField] private PrivateRoom_Door door;
        [SerializeField] internal PrivateRoom pvtRoom;
        [SerializeField] internal bool isInside;
        [SerializeField] internal PrivateRoom_Display_Inside  insideDisplay;
        [SerializeField] internal PrivateRoom_Display_Outside  outsideDisplay;

        [SerializeField] private bool disableInteract;

        private void Start()
        {
            DisableInteractive = disableInteract;
        }

        public override void Interact()
        {
            if (!isInside)
            {
                _TryDoor();
            }
            else
            {
                door._MoveToDoor();
            }
            
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal) _TryDoor();
        }

        private void _TryDoor()
        {
            if (pvtRoom != null)
            {
                if (pvtRoom._CanOpen(Networking.LocalPlayer)) door._MoveToDoor();
            }
            else
            {
                door._MoveToDoor();
            }
        }

        private void _MoveToDoor()
        {
            if (pvtRoom.playerMover != null)
            {
                pvtRoom.playerMover._MovePlayer(spawnPoint);
            }
            else
            {
                Networking.LocalPlayer.TeleportTo(spawnPoint.position,spawnPoint.rotation);
            }
        }
    }
}