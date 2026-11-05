//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Ce
{
    [AddComponentMenu("DisBridge/Creator Economy/CE Popup")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CE_Popup : UdonSharpBehaviour
    {
        public byte mode = 0;
        public string groupId;
        public string listingId;

        public UdonProduct uwu;

        public void Start()
        {
            if (mode == 3)
            {
                DisableInteractive = true;
            }
        }

        public override void Interact()
        {
            switch (mode)
            {
                case 0:
                    _Popup_Listing();
                    break;
                case 1:
                    _Popup_Group();
                    break;
                case 2:
                    _Popup_GroupStore();
                    break;
                case 3:
                    _Popup_WorldStore();
                    break;
            }
        }

        public void _Popup_Listing()
        {
            if (string.IsNullOrWhiteSpace(listingId))
            {
                Debug.LogError("No Listing ID");
                return;
            }

            Store.OpenGroupListing(listingId);
        }

        public void _Popup_Group()
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                Debug.LogError("No Group ID");
                return;
            }

            Store.OpenGroupPage(groupId);
        }

        public void _Popup_GroupStore()
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                Debug.LogError("No Group ID");
                return;
            }

            Store.OpenGroupStorePage(groupId);
        }

        public void _Popup_WorldStore()
        {
            Store.OpenWorldStorePage();
        }
    }
}
