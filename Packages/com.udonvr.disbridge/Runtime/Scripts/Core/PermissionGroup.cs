//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class PermissionGroup : UdonSharpBehaviour
    {
        [HideInInspector, SerializeField] internal RoleContainer[] RoleContainers;
        [HideInInspector, SerializeField] internal UdonProduct[] Products;
        [HideInInspector, SerializeField] internal string[] Names;
        

        public bool _IsMember(VRCPlayerApi _player)
        {
            if (Utilities.IsValid(_player)) return false;
            for (int i = 0; i < RoleContainers.Length; i++)
            {
                if (RoleContainers[i].IsMember(_player)) return true;
            }
            for (int i = 0; i < Products.Length; i++)
            {
                if (Store.DoesPlayerOwnProduct(_player,Products[i])) return true;
            }
            if (Array.IndexOf(Names,_player) != -1) return true;
            return false;
        }
        
        public bool _IsMember(String _player)
        {
            Debug.LogWarning("[DisBridge][PermissionGroup] A Display Name Lookup was ran. This is in-efficient and a VRCPlayerAPI should have been used. If possible update your code to use a VRCPlayerAPI instead of a DisplayName.");
            return DisBridgeFunctions._TryGetPlayerByDisplayName(_player, out VRCPlayerApi _playerApi) && _IsMember(_playerApi);
        }
    }
}

