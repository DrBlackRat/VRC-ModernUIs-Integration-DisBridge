//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UdonSharp.Localization;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AccessList : UdonSharpBehaviour
    {
        [UdonSynced()] private string syncedAccessList;
        private DataList accessList;
        [SerializeField] private bool forceDisBridge;

        public bool _AddUser(VRCPlayerApi _requester)
        {
            if (forceDisBridge) return false;
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
            }
            if (!Utilities.IsValid(_requester)) return false;
            if (accessList.Contains(_requester.playerId)) return false;
            DataToken tmp;
            accessList.Add(_requester.playerId);
            VRCJson.TrySerializeToJson(accessList, JsonExportType.Minify, out tmp);
            syncedAccessList = tmp.String;
            RequestSerialization();
            return true;
        }
        
        public bool _AddUser(VRCPlayerApi _requester, VRCPlayerApi _accessMember, RoleContainer _role)
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
            }
            if (!Utilities.IsValid(_requester)) return false;
            if (accessList.Contains(_requester.playerId)) return false;
            DataToken tmp;
            if (!Utilities.IsValid(_accessMember)) return false;
            if (_role == null) return false;
            if (_role.IsMember(_accessMember))
            {
                accessList.Add(_requester.playerId);
                VRCJson.TrySerializeToJson(accessList, JsonExportType.Minify, out tmp);
                syncedAccessList = tmp.String;
                RequestSerialization();
                return true;
            }

            return false;
        }

        public void _RemoveUser(VRCPlayerApi _requester)
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
            if (accessList.Contains(_requester.playerId))
            {
                accessList.Remove(_requester.playerId);
            }
        }

        public bool IsUserOnList(VRCPlayerApi _requester)
        {
            return accessList.Contains(_requester.playerId);
        }

        //public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        //{
        //    if (requestedOwner.isMaster) return false;
        //    if (requestingPlayer.isMaster) return true;
        //    return false;
        //}

        public override void OnDeserialization()
        {
            DataToken tmp;
            if (VRCJson.TryDeserializeFromJson(syncedAccessList, out tmp))
            {
                accessList = tmp.DataList;
            }
        }
    }
}