//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class PlayerTrackerExample_DualParent : PlayerTracker_ColliderClass
    {
        internal bool isInParent;
        internal bool isInChild;
        
        public GameObject[] objectsToToggleOn;
        public GameObject[] objectsToToggleOff;

        void Start()
        {
            __SetState(false);
        }

        public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            isInParent = true;
            _UpdateStates();
        }
        public override void _OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            isInParent = false;
            _UpdateStates();
        }

        public void _UpdateStates()
        {
            if (isInParent && isInChild)
            {
                __SetState(true);
            }
            if (!isInParent && !isInChild)
            {
                __SetState(false);
            }
        }

        private void __SetState(bool _state)
        {
            for (int i = 0; i < objectsToToggleOn.Length; i++)
            {
                objectsToToggleOn[i].SetActive(_state);
            }

            for (int i = 0; i < objectsToToggleOff.Length; i++)
            {
                objectsToToggleOff[i].SetActive(!_state);
            }
        }
    }
}