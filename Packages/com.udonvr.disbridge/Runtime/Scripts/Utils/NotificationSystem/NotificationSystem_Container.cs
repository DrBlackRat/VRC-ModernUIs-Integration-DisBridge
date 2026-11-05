//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.Drawing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVr.DisBridge.Utils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NotificationSystem_Container : UdonSharpBehaviour
    {
        public NotificationSystem notificationSystem;

        public Sprite iconImage;
        public string headerText;
        public string notificationText;
        public string notificationTextAlt;
        public AudioClip sound;

        public bool isLoaded = false;

        public bool _LoadNotification(string _n, string _na = "", string _h = "", AudioClip _sound = null, Sprite _icon = null)
        {
            if (isLoaded) return false;
            isLoaded = true;
            iconImage = _icon;
            notificationText = _n;
            notificationTextAlt = _na;
            sound = _sound;
            headerText = _h;
            return true;
        }

        public bool _SendNotification()
        {
            if (!isLoaded) return false;
            notificationSystem._SendInstantNotification(notificationText, notificationTextAlt,headerText, sound, iconImage);
            _ResetNotification();
            return true;
        }

        public void _ResetNotification()
        {
            isLoaded = false;
            notificationText = string.Empty;
            notificationTextAlt = string.Empty;
            iconImage = null;
        }
    }
}