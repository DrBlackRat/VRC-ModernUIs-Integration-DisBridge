//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge")]
namespace UdonVR.DisBridge
{
    public class PlayerTracker_ColliderClass : UdonSharpBehaviour
    {
        [SerializeField,HideInInspector] internal PlayerTracker __playerTracker_internal;

        private void OnTriggerEnter(Collider other)
        {
            __playerTracker_internal._Log($"Object Entered Trigger {gameObject.name}");
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            PlayerTracker_Tracker tmp = other.GetComponentInParent<PlayerTracker_Tracker>();
            if (tmp != null)
            {
                //__playerTracker_internal._Log($"Object Entered Trigger {gameObject.name}");
                _OnPlayerTriggerEnter(tmp.thisPlayer);
            }
            //stopwatch.Stop();
            //Debug.Log($"[DEBUG] [GetComponentInParent] Time: {stopwatch.ElapsedTicks}");
            //stopwatch.Reset();
            //stopwatch.Start();
            //if (__playerTracker_internal._CheckCollider(other, out PlayerTracker_Tracker _tracker))
            //{
            //    __playerTracker_internal._Log($"Player Entered Trigger {gameObject.name}");
            //    //_OnPlayerTriggerEnter(_tracker.thisPlayer);
            //}
            //stopwatch.Stop();
            //Debug.Log($"[DEBUG] [for loop] Time: {stopwatch.ElapsedTicks}");
            _OnTriggerEnter(other);
        }

        public virtual void _OnTriggerEnter(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            __playerTracker_internal._Log($"Object Left Trigger {gameObject.name}");
            if (__playerTracker_internal._CheckCollider(other, out PlayerTracker_Tracker _tracker))
            {
                __playerTracker_internal._Log($"Player Left Trigger {gameObject.name}");
                _OnPlayerTriggerExit(_tracker.thisPlayer);
            }
            _OnTriggerExit(other);
        }
        public virtual void _OnTriggerExit(Collider other)
        {
            
        }

        public virtual void _OnPlayerTriggerEnter(VRCPlayerApi player)
        {

        }

        public virtual void _OnPlayerTriggerExit(VRCPlayerApi player)
        {

        }
    }
}