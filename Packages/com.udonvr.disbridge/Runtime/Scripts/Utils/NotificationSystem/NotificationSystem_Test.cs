//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVr.DisBridge.Utils
{
    public class NotificationSystem_Test : UdonSharpBehaviour
    {
        public NotificationSystem notificationSystem;
        public int howMany = 1;

        public Sprite iconImage;
        public string headerText;
        public string notificationText;
        public string notificationTextAlt;
        public AudioClip sound;

        public override void Interact()
        {
            for (int i = 0; i < howMany; i++)
            {
                notificationSystem._RegisterNotification($"[{i}]{notificationText}", notificationTextAlt,headerText, sound, iconImage);
            }
        }
    }
}