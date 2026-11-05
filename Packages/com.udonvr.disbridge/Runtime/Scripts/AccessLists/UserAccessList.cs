
using System;
using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UdonVR.DisBridge;
using UdonVR.DisBridge.Plugins;
using UdonVr.DisBridge.Utils;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Editors")]
namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UserAccessList : DisBridgePlugin
    {
        [SerializeField] public UserAccessList_Class[] plugins;
        [SerializeField] public string accessListName;
        [SerializeField] internal NotificationSystem notificationSystem;
        [SerializeField] internal bool allowPreferredOwnerToChangePermissions = true;
        [SerializeField] internal bool usePreferredOwner = true;
        [UdonSynced] internal string preferredOwner = "";
        private bool syncedPreferredOwner = false;

        public bool OnlyPreferredOwnerCanMakeChangers = false;
        public bool PreferredNeedsToBePrivilegedUser = false;

        [SerializeField] internal UserAccessList privilegedUserAccessList;

        [SerializeField, HideInInspector] internal DataList playerList = new DataList();
        [SerializeField, UdonSynced] private string _playerList = "";

        private DataList childrenAccessLists;
        
        private bool hasInit = false;

        private bool isListOpen = false;

        public UserAccessList_Controller controller;

        public TextMeshProUGUI debugOut;
        private bool hasLocalAccess;

        // ######################################
        // User Cache
        private bool isWaitingForRequest = false;
        private bool isWaitingForRequest_processing = false;
        private int __cachedRequestingUser = -1;
        private int __cachedTargetUser = -1;
        public bool requestType = false;
        // ######################################

        void Start()
        {
            disBridge.AddPlugin(gameObject);
            if (Utilities.IsValid(privilegedUserAccessList)) privilegedUserAccessList._RegisterChildAccessList(this);
            playerList = new DataList();
            if (usePreferredOwner && Networking.LocalPlayer.IsOwner(gameObject))
            {
                preferredOwner = Networking.LocalPlayer.displayName;
                RequestSerialization();
            }
        }

        public override void _UVR_Init()
        {
            hasInit = true;
            controller._InitMenu();
        }

        public override void _UVR_Update()
        {
            if (isWaitingForRequest)
            {
                isWaitingForRequest_processing = false;
                if (requestType)
                {
                    N_AddUserAccess(__cachedRequestingUser, __cachedTargetUser);
                }
                else
                {
                    N_RemoveUserAccess(__cachedRequestingUser, __cachedTargetUser);
                }
                isWaitingForRequest = false;
                __cachedRequestingUser = -1;
                __cachedTargetUser = -1;
            }
        }

        //##################################################################################################################
        #region API

        /// <summary>
        /// Checks if the user is the Preferred Owner.
        /// <br></br>This can return false if asked too soon. Network data needs to sync before this is updated.
        /// </summary>
        /// <param name="player">VRCPlayerAPI of the target player.</param>
        /// <returns>True/False depending on the user's access level.</returns>
        [PublicAPI]
        public bool _IsPreferredOwner(VRCPlayerApi __player)
        {
            if (!Utilities.IsValid(__player)) return false;
            if (!usePreferredOwner) return false;
            
            if (PreferredNeedsToBePrivilegedUser)
            {
                if (__player.displayName == preferredOwner)
                {
                    if (_IsMemberInRoles(__player, false)) return true;
                    if (Utilities.IsValid(privilegedUserAccessList))
                    {
                        if (privilegedUserAccessList != this)
                        {
                            if (privilegedUserAccessList._HasAccess(__player)) return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return __player.displayName == preferredOwner;
            }
            return false;
        }
        /// <summary>
        /// Check if the player is a member a Privileged Member.
        /// <br></br>A Privileged Member is either a member of the DisBridge Roles, Permission Group, or if configured, the Preferred Owner.
        /// </summary>
        /// <param name="player">the player to check</param>
        /// <returns>True/False depending on the user's access level.</returns>
        [PublicAPI]
        public bool _IsAdmin(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return false;
            if (_IsMemberInRoles(player, false)) return true;
            if (allowPreferredOwnerToChangePermissions) if (_IsPreferredOwner(player)) return true;

            if (Utilities.IsValid(privilegedUserAccessList))
            {
                if (privilegedUserAccessList != this)
                {
                    if (privilegedUserAccessList._HasAccess(player)) return true;
                }
            }

            return false;
        }
        /// <summary>
        /// This will return True or False if the player is either an Admin or a Member in the list.
        /// </summary>
        /// <param name="player">the player to check</param>
        /// <returns>True/False depending on the user's access level.</returns>
        [PublicAPI]
        public bool _HasAccess(VRCPlayerApi player)
        {
            if (_IsAdmin(player)) return true;
            if (_IsMember(player)) return true;
            return false;
        }

        /// <summary>
        /// Check if the player is a member of ONLY the user list
        /// </summary>
        /// <param name="player">the player to check</param>
        /// <returns>True/False depending on the user's access level.</returns>
        [PublicAPI]
        public bool _IsMember(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return false;
            return playerList.Contains(player.displayName);
        }

        /// <summary>
        /// Attempts to add the user as a member of the User List.
        /// <br></br>Can only be done by an Admin.
        /// <br></br>The Owner needs to accept the new Member.
        /// </summary>
        /// <param name="_requestingPlayerID">The Player trying to add the user.</param>
        /// <param name="_targetPlayerID">The Player trying to be added.</param>
        [PublicAPI]
        public void _AddUserAccess(int _requestingPlayerID, int _targetPlayerID)
        {
            disBridge.log($"[AL]_AddUserAccess {_requestingPlayerID} - {_targetPlayerID}");
            if (_requestingPlayerID != Networking.LocalPlayer.playerId)
            {
                __DenyNotification(false);
                return;
            }
            if (!_IsAdmin(Networking.LocalPlayer))
            {
                __DenyNotification(false);
                return;
            }

            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(N_AddUserAccess), _requestingPlayerID, _targetPlayerID);
        }

        /// <summary>
        /// Attempts to remove the user as a member of the User List.
        /// <br></br>Can only be done by a Privileged Member.
        /// <br></br>The Owner needs to accept the new Member.
        /// </summary>
        /// <param name="_requestingPlayerID">The Player trying to add the user.</param>
        /// <param name="_targetPlayerID">The Player trying to be added.</param>
        [PublicAPI]
        public void _RemoveUserAccess(int _requestingPlayerID, int _targetPlayerID)
        {
            disBridge.log($"[AL]_RemoveUserAccess {_requestingPlayerID} - {_targetPlayerID}");
            if (_requestingPlayerID != Networking.LocalPlayer.playerId)
            {
                __DenyNotification(false);
                return;
            }
            if (!_IsAdmin(Networking.LocalPlayer))
            {
                __DenyNotification(false);
                return;
            }

            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(N_RemoveUserAccess), _requestingPlayerID,
                _targetPlayerID);
        }

        /// <summary>
        /// Registers a Child Access List for this list to update as this list does.
        /// <br></br>If the Child Access List is already registered, the request is ignored.
        /// </summary>
        /// <param name="_accessList">The target list to update</param>
        [PublicAPI]
        public void _RegisterChildAccessList(UserAccessList _accessList)
        {
            if (!Utilities.IsValid(childrenAccessLists)) childrenAccessLists = new DataList();
            if (!childrenAccessLists.Contains(_accessList)) childrenAccessLists.Add(_accessList);
        }


        /// <summary>
        /// Opens the UserList and refreshes the menu to use the latest information.
        /// This also opens and refreshes any children lists.
        /// </summary>
        [PublicAPI]
        public void _OpenList()
        {
            isListOpen = true;
            __UnpackageDataList();
            if (!Utilities.IsValid(childrenAccessLists)) return;
            for (int i = 0; i < childrenAccessLists.Count; i++)
            {
                ((UserAccessList)childrenAccessLists[i].Reference)._OpenList();
            }
            controller._QueueMenuRefresh();
        }
        
        /// <summary>
        /// Closes the UserList and stops refreshing the menu to use the latest information.
        /// This also closes and stops refreshing any children lists.
        /// </summary>
        [PublicAPI]
        public void _CloseList()
        {
            isListOpen = false;
            if (!Utilities.IsValid(childrenAccessLists)) return;
            for (int i = 0; i < childrenAccessLists.Count; i++)
            {
                ((UserAccessList)childrenAccessLists[i].Reference)._CloseList();
            }
        }

        public void _UpdateChildrenAccessLists()
        {
            if (!Utilities.IsValid(childrenAccessLists)) return;
            for (int i = 0; i < childrenAccessLists.Count; i++)
            {
                ((UserAccessList)childrenAccessLists[i].Reference)._UpdateThisMenu();
            }
        }

        public void _UpdateThisMenu()
        {
            controller._QueueMenuRefresh();
            _UpdateChildrenAccessLists();
        }


        #endregion
        //##################################################################################################################
        #region Network Events

        [NetworkCallable]
        public void N_AddUserAccess(int _requestingPlayerID, int _targetPlayerID)
        {
            if (!Networking.LocalPlayer.IsOwner(gameObject)) return;

            if (OnlyPreferredOwnerCanMakeChangers)
            {
                if (_IsPreferredOwner(Networking.LocalPlayer))
                {
                    // continue
                }
                else
                {
                    //cant make changes
                    return;
                }
            }
            
            VRCPlayerApi _requestingPlayer = VRCPlayerApi.GetPlayerById(_requestingPlayerID);
            VRCPlayerApi _targetPlayer = VRCPlayerApi.GetPlayerById(_targetPlayerID);
            
            if (!Utilities.IsValid(_requestingPlayer)) return;
            if (!Utilities.IsValid(_targetPlayer)) return;

            if (NetworkCalling.InNetworkCall)
            {
                if (NetworkCalling.CallingPlayer != _requestingPlayer)
                {
                    if (!Utilities.IsValid(notificationSystem)) return;
                    notificationSystem._RegisterNotification(
                        $"Potential Client User: {NetworkCalling.CallingPlayer.displayName}!!",
                        $"{NetworkCalling.CallingPlayer.displayName} just tried assigning a permission as a different user.",
                        $"{accessListName} access List",
                        notificationSystem.failSound);
                    return;
                }
            }
            else
            {
                
            }
            
            if (playerList.Contains(_targetPlayer.displayName)) return;
            if (_IsAdmin(_requestingPlayer))
            {
                __AddUserAccess(_requestingPlayerID, _targetPlayer);
                return;
            }

            if (disBridge._RequestFetch())
            {
                isWaitingForRequest = true;
                isWaitingForRequest_processing = true;
                __cachedRequestingUser = _requestingPlayerID;
                __cachedTargetUser = _targetPlayerID;
                requestType = true;
                return;
            }
            if (isWaitingForRequest_processing)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_OwnerPulling_Notification), _requestingPlayerID);
                return;
            }
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_UserAccept_Notification), _requestingPlayerID, false);
        }

        [NetworkCallable]
        public void N_RemoveUserAccess(int _requestingPlayerID, int _targetPlayerID)
        {
            if (!Networking.LocalPlayer.IsOwner(gameObject)) return;
            VRCPlayerApi _requestingPlayer = VRCPlayerApi.GetPlayerById(_requestingPlayerID);
            VRCPlayerApi _targetPlayer = VRCPlayerApi.GetPlayerById(_targetPlayerID);
            
            if (!Utilities.IsValid(_requestingPlayer)) return;
            if (!Utilities.IsValid(_targetPlayer)) return;

            if (NetworkCalling.InNetworkCall)
            {
                if (NetworkCalling.CallingPlayer != _requestingPlayer)
                {
                    if (!Utilities.IsValid(notificationSystem)) return;
                    notificationSystem._RegisterNotification(
                        $"Potential Client User: {NetworkCalling.CallingPlayer.displayName}!!",
                        $"{NetworkCalling.CallingPlayer.displayName} just tried assigning a permission as a different user.",
                        $"{accessListName} access List",
                        notificationSystem.failSound);
                    return;
                }
            }
            
            if (!playerList.Contains(_targetPlayer.displayName)) return;
            if (_IsAdmin(_requestingPlayer))
            {
                __RemoveUserAccess(_requestingPlayerID, _targetPlayer);
                return;
            }

            if (disBridge._RequestFetch())
            {
                isWaitingForRequest = true;
                isWaitingForRequest_processing = true;
                __cachedRequestingUser = _requestingPlayerID;
                __cachedTargetUser = _targetPlayerID;
                requestType = false;
                return;
            }

            if (isWaitingForRequest_processing)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_OwnerPulling_Notification), _requestingPlayerID);
                return;
            }
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_UserAccept_Notification), _requestingPlayerID, false);
        }

        [NetworkCallable]
        public void N_UserAccept_Notification(int _requestingPlayerID, bool accepted)
        {
            if (Networking.LocalPlayer.playerId != _requestingPlayerID) return;
            if (accepted)
            {
                //__AcceptNotification();
            }
            else
            {
                __DenyNotification(true);
            }
        }

        public void N_OwnerPulling_Notification(int _requestingPlayerID)
        {
            if (Networking.LocalPlayer.playerId != _requestingPlayerID) return;
            if (!Utilities.IsValid(notificationSystem)) return;
            notificationSystem._RegisterNotification("The Owner is Updating",
                "Please wait to make a new Request.",
                $"{accessListName} access List", notificationSystem.failSound);
        }

        #endregion
        //##################################################################################################################
        private void __AcceptNotification()
        {
            if (!Utilities.IsValid(notificationSystem)) return;
            notificationSystem._RegisterNotification("Your action has been Approved",
                $"The user has been updated in {accessListName}", $"{accessListName} access List",
                notificationSystem.successSound);
        }

        private void __DenyNotification(bool fromOwner)
        {
            if (!Utilities.IsValid(notificationSystem)) return;
            notificationSystem._RegisterNotification("Your action has been Denied",
                fromOwner ? "The Owner has denied your action" : "You don't have permission",
                $"{accessListName} access List", notificationSystem.failSound);
        }

        private void __AddUserAccess(int _requestingPlayerID, VRCPlayerApi _targetPlayer)
        {
            playerList.Add(_targetPlayer.displayName);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_UserAccept_Notification), _requestingPlayerID, true);
            __PackageDataList();
            _UpdateThisMenu();
            
        }

        private void __RemoveUserAccess(int _requestingPlayerID, VRCPlayerApi _targetPlayer)
        {
            playerList.Remove(_targetPlayer.displayName);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(N_UserAccept_Notification), _requestingPlayerID, true);
            __PackageDataList();
            _UpdateThisMenu();
        }

        private void __PackageDataList()
        {
            if (VRCJson.TrySerializeToJson(playerList, JsonExportType.Minify, out DataToken jsonString))
            {
                _playerList = jsonString.ToString();
                RequestSerialization();
            }
            else
            {
                Debug.LogError("[AL]Error -- Did not Serialize");
            }
        }

        /// <summary>
        /// Unpacks network data into the menu, and refreshes this menu and it's children.
        /// </summary>
        /// <param name="_refreshMenu"> Defaults to true, if false, skips updating the menu.</param>
        private void __UnpackageDataList(bool _refreshMenu = true)
        {
            if (VRCJson.TryDeserializeFromJson(_playerList, out DataToken uwu))
            {
                if (uwu.TokenType == TokenType.DataList)
                {
                    playerList = uwu.DataList;
                    if (_refreshMenu)
                    {
                        // menu is closed, don't update it. it will update next time it's opened.
                        _UpdateThisMenu();
                    }
                    foreach (var plugin in plugins)
                    {
                        plugin._ReceivePermissionChange();
                    }
                }
                else
                {
                    Debug.LogError("[AL]Error -- Did Deserialize but not into a DataList");
                }
            }
            else
            {
                Debug.LogError("[AL]Error -- Did not Deserialize");
            }
            __LocalAccessNotification();
        }
        
        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return _IsPreferredOwner(requestingPlayer);
        }
        
        public override void OnDeserialization()
        {
            __UnpackageDataList(isListOpen);
            if (!syncedPreferredOwner)
            {
                if (preferredOwner != String.Empty)
                {
                    syncedPreferredOwner = true;
                    _UpdateThisMenu();
                }
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal && _IsPreferredOwner(player))
            {
                Networking.SetOwner(player, gameObject);
            }
        }

        private void Update()
        //TODO delete this on release
        {
            if (Utilities.IsValid(debugOut))
            {
                debugOut.text = $"=={accessListName}==" +
                                $"\nisOpen:{isListOpen}" +
                                $"\nOwner:{Networking.GetOwner(gameObject).displayName}" +
                                $"\n{_playerList}";
            }
        }

        private void __LocalAccessNotification()
        {
            bool __hasAccess = _IsMember(Networking.LocalPlayer);
            if (__hasAccess != hasLocalAccess)
            {
                hasLocalAccess =  __hasAccess;
                if (hasLocalAccess)
                {
                    notificationSystem._RegisterNotification("Permissions Update",$"You have be added to {accessListName}",$"Access List - {accessListName}",notificationSystem.successSound);
                }
                else
                {
                    notificationSystem._RegisterNotification("Permissions Update",$"You have be removed from {accessListName}",$"Access List - {accessListName}",notificationSystem.failSound);
                }
            }
        }
    }
}