//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class TriggerZone_Class : UdonSharpBehaviour
    {
        [SerializeField, HideInInspector] internal DataDictionary playerData;

        void Start()
        {
            playerData = new DataDictionary();
            _PostStart();
        }

        public virtual void _PostStart()
        {

        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (playerData.ContainsKey(player.displayName))
            {
                playerData.Remove(player.displayName);
            }

            _OnPlayerLeft(player);
        }

        public virtual void _OnPlayerLeft(VRCPlayerApi player)
        {

        }

        public void _AddPlayer(VRCPlayerApi player)
        {
            string playerName = player.displayName;
            if (playerData.ContainsKey(playerName))
            {
                playerData.SetValue(playerName, (int)playerData[playerName] + 1);
                return;
            }
            playerData.Add(playerName, 1);
            _PostAddPlayer(player);
        }

        public void _RemovePlayer(VRCPlayerApi player)
        {
            string playerName = player.displayName;
            if (playerData.ContainsKey(playerName))
            {
                int i = (int)playerData[playerName] - 1;
                if (i <= 0)
                {
                    playerData.Remove(playerName);
                    _PostRemovePlayer(player);
                }
                else
                {
                    playerData.SetValue(playerName, i);
                }
                return;
            }
        }

        public virtual void _PostAddPlayer(VRCPlayerApi player)
        {

        }

        public virtual void _PostRemovePlayer(VRCPlayerApi player)
        {

        }
    }
}