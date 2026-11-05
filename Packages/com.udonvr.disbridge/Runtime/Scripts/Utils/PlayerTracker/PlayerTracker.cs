//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace UdonVR.DisBridge
{
    public enum TrackerMode
    {
        Sphere,
        Capsule,
    }
    public class PlayerTracker : UdonSharpBehaviour
    {
        [SerializeField]internal int playerCount = 100;
        [SerializeField,HideInInspector]internal DataList Trackers_UnUsed;
        [SerializeField,HideInInspector]internal DataList Trackers_Used;
        [SerializeField]internal string playerTag = "[UVR]PlayerTracker";
        [SerializeField]internal Transform playerTrackerParent;
        [SerializeField]internal TrackerMode trackerMode = TrackerMode.Sphere;

        [SerializeField] internal bool useCustomLayer = false;
        [SerializeField] internal int customLayer = -1;
        
        void Start()
        {
            Trackers_UnUsed = new DataList();
            Trackers_Used = new DataList();
            PlayerTracker_Tracker[] trackers = playerTrackerParent.GetComponentsInChildren<PlayerTracker_Tracker>();
            for (int i = 0; i < trackers.Length; i++)
            {
                Trackers_UnUsed.Add(trackers[i]);
            }
            __BindToPlayer(Networking.LocalPlayer);
        }

        public override void PostLateUpdate()
        {
            for (int i = 0; i < Trackers_Used.Count; i++)
            {
                ((PlayerTracker_Tracker)Trackers_Used[i].Reference)._MoveCollider();
            }
        }

        private void __BindToPlayer(VRCPlayerApi player)
        {
            if (player.GetPlayerTag(playerTag) == "true")
            {
                Debug.Log($"[PlayerTracker] __BindToPlayer [{player.displayName}] FAILED -- already tracked");
                return;
            }
            for (int i = 0; i < Trackers_UnUsed.Count; i++)
            {
                if (((PlayerTracker_Tracker)Trackers_UnUsed[i].Reference)._BindPlayer(player))
                {
                    Debug.Log($"[PlayerTracker] __BindToPlayer [{player.displayName}] Success");
                    return;
                }
            }
            Debug.Log($"[PlayerTracker] __BindToPlayer [{player.displayName}] FAILED -- no pool");
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            __BindToPlayer(player);
        }

        public void _UnUsePlayerTracker(PlayerTracker_Tracker _tracker)
        {
            Trackers_UnUsed.Add(_tracker);
            Trackers_Used.Remove(_tracker);
        }
        public void _UsePlayerTracker(PlayerTracker_Tracker _tracker)
        {
            Trackers_UnUsed.Remove(_tracker);
            Trackers_Used.Add(_tracker);
        }

        public bool _CheckCollider(Collider collider, out PlayerTracker_Tracker _tracker)
        {
            for (int i = 0; i < Trackers_Used.Count; i++)
            {
                _tracker = ((PlayerTracker_Tracker)Trackers_Used[i].Reference);
                if (_tracker.thisCollider == collider)
                {
                    _Log($"CheckCollider [PASS] player: [{_tracker.thisPlayer.displayName}]");
                    return true;
                }
            }
            _Log("CheckCollider [FAIL]");
            _tracker = null;
            return false;
        }

        public void _Log(string _log)
        {
            Debug.Log($"[PlayerTracker] {_log}]");
        }
    }
}

