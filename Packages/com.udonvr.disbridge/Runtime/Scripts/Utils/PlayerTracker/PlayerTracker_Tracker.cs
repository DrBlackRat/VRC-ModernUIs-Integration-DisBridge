//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class PlayerTracker_Tracker : UdonSharpBehaviour
    {
        public PlayerTracker controller;
        public bool isUsed = false;
        public VRCPlayerApi thisPlayer;
        public Collider thisCollider;
        private bool unBinding = false;

        public bool _BindPlayer(VRCPlayerApi player)
        {
            if (Utilities.IsValid(thisPlayer))
            {
                return false;
            }

            if (isUsed)
            {
                return false;
            }
            player.SetPlayerTag(controller.playerTag,"true");
            thisCollider.enabled = true;
            isUsed = true;
            thisPlayer = player;
            controller._UsePlayerTracker(this);
            return true;
        }

        public VRCPlayerApi _GetPlayer()
        {
            if (!Utilities.IsValid(thisPlayer))
            {
                __UnBind();
                return null;
            }

            return thisPlayer;
        }

        private void __UnBind()
        {
            if (unBinding) return;
            unBinding = true;
            transform.localPosition = Vector3.zero;
            SendCustomEventDelayedFrames(nameof(_UnBindDelay), 1);
        }

        public void _UnBindDelay()
        {
            thisCollider.enabled = false;
            isUsed = false;
            thisPlayer = null;
            controller._UnUsePlayerTracker(this);
            unBinding = false;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == thisPlayer) __UnBind();
        }

        public void _MoveCollider()
        {
            if (!Utilities.IsValid(thisPlayer))
            {
                __UnBind();
                return;
            }
            transform.position = thisPlayer.GetPosition();
        }
    }
}