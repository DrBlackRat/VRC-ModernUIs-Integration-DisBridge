//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PrivateRoom_InviteMenu : UdonSharpBehaviour
    {
        [SerializeField] internal PrivateRoom privateRoom;
        [UdonSynced, SerializeField, FieldChangeCallback(nameof(NetInviteData))] private string netInviteData;
        internal string NetInviteData
        {
            get => netInviteData;
            set
            {
                netInviteData = value;
                if (netInviteData == "[]" || netInviteData == "{}")
                {
                    _Log($"Resetting netInviteData", this);
                } else if (string.IsNullOrEmpty(netInviteData))
                {
                    _Log($"netInviteData is fully blank", this);
                }
                else
                {
                    _Log($"Updating netInviteData", this);
                }
                if (privateRoom.hasInit)
                    _ValidateInviteData();
                if (debug_netData != null) debug_netData.text = netInviteData;
            }
        }
        internal DataList localInvitesGiven;
        
        //////////////////////////////////////////////////////////////////////////////////////////////
        DataList playersAtDoor;
        private string[] menuPlayerNames;
        [Space]
        [SerializeField] internal TextMeshProUGUI inviteStatus;
        [Space]
        [SerializeField] internal GameObject groupInvited;
        [SerializeField] internal GameObject groupAtDoor;
        [SerializeField] internal GameObject groupOthers;
        [SerializeField] internal GameObject groupNoInviteAvailable;
        [Space]
        [SerializeField] internal Transform layoutContainer_Invited; 
        [SerializeField] internal Transform layoutContainer_AtDoor; 
        [SerializeField] internal Transform layoutContainer_Others; 
        [SerializeField] internal Transform layoutContainer_Unused;
        [Space]
        [SerializeField] internal Toggle[] invitePlayerToggles;
        [Header("Automated:")]
        [SerializeField] internal TextMeshProUGUI[] invitePlayerNameTexts;
        [SerializeField] internal GameObject[] invitePlayerUnavailableIcons;

        private bool isLocalInvited = false;
        private bool isLocalInvited_local = false;
        private bool isLocalInviteInvalid = false;
        private bool isLocalInviteInvalid_local = false;

        public TextMeshProUGUI debug_netData;
        public TextMeshProUGUI debug_Log;


        internal void _AddToAtDoor(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(playersAtDoor))
                playersAtDoor = new DataList();
            
            playersAtDoor.Add(new DataToken(_player));
            _RefreshMenu();
        }
        
        internal void _RemoveFromAtDoor(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(playersAtDoor))
                return;
            
            playersAtDoor.Remove(new DataToken(_player));
            _RefreshMenu();
        }

        void OnEnable()
        {
            _RefreshMenu();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Invite
        
        internal DataDictionary _GetInviteData()
        {
            if (string.IsNullOrWhiteSpace(netInviteData))
                return null;
            
            if (!VRCJson.TryDeserializeFromJson(netInviteData, out DataToken netDataToken))
            {
                _LogError($"Failed to deserialize Networked Invite Data! String attempted:\n{netInviteData}", this);
                return null;
            }

            if (netDataToken.TokenType != TokenType.DataDictionary)
            {
                _LogError($"Networked Invite Data surface type is not DataDictionary! DataToken received:\n{netDataToken.ToString()}", this);
                return null;
            }

            return netDataToken.DataDictionary;
        }

        public void _ValidateInviteData()
        {
            if (!privateRoom.hasInit)
            {
                _LogError("Cannot validate invite data! Plugin has not Initialized.", this);
                return;
            }
            
            VRCPlayerApi localP = Networking.LocalPlayer;
            if (!Utilities.IsValid(localP))
            {
                _Log("Cannot validate invite data with invalid Local Player.", this);
                _RefreshMenu();
                return;
            }
            
            DataDictionary inviteData = _GetInviteData();
            if (!Utilities.IsValid(inviteData))
            {
                _Log("No Invite Data Found.", this);
                _RefreshMenu();
                
                return;
            }
            
            DataList inviterNames = inviteData.GetKeys();

            bool requestForNetwork = false;
            
            DataList invalidInviters = new DataList();
            isLocalInvited = false;
            isLocalInviteInvalid = false;
            for (int i = 0; i < inviterNames.Count; i++)
            {
                DataToken inviterNameToken = inviterNames[i];
                DataToken inviterToken = inviteData[inviterNameToken];
                if (inviterToken.TokenType != TokenType.DataList)
                {
                    _LogWarning($"Inviter \"{inviterNameToken.ToString()}\" does not hold a DataList. Data found:\n{inviterToken.ToString()}", this);
                    continue;
                }

                _Log($"Found inviter {inviterNameToken}.", this);

                bool isLocalValidation = localP.displayName == inviterNameToken.ToString();
                
                int allowedInviteCount = privateRoom._GetAllowedInviteCount(inviterNameToken.ToString());
                DataList invitedList = inviterToken.DataList;

                bool isInviterValid = Utilities.IsValid(privateRoom.GetPlayer(inviterNameToken.ToString()));
                _LogWarning($"isLocalValidation={isLocalValidation}, aIC={allowedInviteCount}, valid={isInviterValid}", this);
                if (!isInviterValid && isLocalValidation)
                {
                    _LogWarning($"Inviter \"{inviterNameToken}\" is not valid!", this);
                    invalidInviters.Add(inviterNameToken);
                    continue; // Only skips when validation, need to advance to loop to see if local has invalid invite.
                }

                if (isInviterValid && privateRoom.isVip && allowedInviteCount <= 0 && !privateRoom.isAdmin(inviterNameToken.ToString()))
                {
                    if (isLocalValidation)
                    {
                        _LogWarning($"Inviter \"{inviterNameToken}\" is not allowed to invite!", this);
                        invalidInviters.Add(inviterNameToken);
                    }
                    continue;
                }

                _Log("Check each invited player that the inviter invited", this);
                // Check each invited player that the inviter invited
                for (int j = 0; j < invitedList.Count; j++)
                {
                    if (invitedList[j].TokenType != TokenType.String)
                    {
                        if (isLocalValidation)
                        {
                            _LogWarning($"Inviter \"{inviterNameToken}\" has an invalid invite name at index {j}. Invite found:\n{invitedList[j]}", this);
                            invalidInviters.Add(inviterNameToken);
                        }
                        continue;
                    }

                    string invitedName = invitedList[j].String;
                    if (privateRoom.isVip && (privateRoom._GetAllowedInviteCount(invitedName) > 0 || privateRoom.isAdmin(invitedName)))
                    {
                        if (isLocalValidation)
                        {
                            _LogWarning($"Inviter \"{inviterNameToken}\" cannot invite another inviter!", this);
                            invalidInviters.Add(inviterNameToken);
                        }
                        continue;
                    }
                    
                    if (invitedName == inviterNameToken.ToString())
                    {
                        if (isLocalValidation)
                        {
                            _LogWarning($"Inviter \"{inviterNameToken}\" cannot have themselves invited!", this);
                            invalidInviters.Add(inviterNameToken);
                        }
                        continue;
                    }

                    bool localInviteExists = invitedName == localP.displayName;
                    isLocalInvited |= localInviteExists && isInviterValid;
                    isLocalInviteInvalid |= localInviteExists && !isInviterValid;
                    
                    _Log($"{invitedList[j]} was Invited by inviter {inviterNameToken}.", this);
                }
                
                // Network and Local do not match. (Only Local Validates)
                if (localP.displayName == inviterNameToken && Utilities.IsValid(invitedList) && Utilities.IsValid(localInvitesGiven) && 
                    invitedList.Count != localInvitesGiven.Count)
                {
                    // Local is invalid, match Network.
                    // TODO: Currently always trusts Network. Invites can be lost from network execution order. 
                    //if ((allowedInviteCount <= 0 || localInvitesGiven.Count > allowedInviteCount) && !isAdmin(localP))
                    {
                        localInvitesGiven = invitedList.DeepClone();
                        continue;
                    }
                    
                    // Local is valid, match Local.
                    // TODO: Possible network fighting with remote that validates differently.
                    inviteData[inviterNameToken] = localInvitesGiven.DeepClone();
                    requestForNetwork = true;
                }
            }
            

            if (invalidInviters.Count > 0)
            {
                requestForNetwork = true;
                _CleanInviteData(ref inviteData,  invalidInviters);
            }
            
            if (requestForNetwork)
                _NetworkInviteData(inviteData);
            
            __CheckAndSendNotification();
            _RefreshMenu();
        }
        
        internal void _CleanInviteData(ref DataDictionary inviteData,  DataList invalidInviters)
        {
            _Log($"_CleanInviteData",this);
            if (!privateRoom.hasInit)
            {
                _LogError("Cannot clean invite data! Plugin has not Initialized.", this);
                return;
            }
            
            for (int i = invalidInviters.Count - 1; i >= 0; i--)
            {
                if (!inviteData.TryGetValue(invalidInviters[i], out DataToken invalidInviterToken))
                {
                    _LogError($"Cleanup Invalid inviter {invalidInviters[i]} was not found in given InviteData:\n{inviteData}", this);
                    continue;
                }

                if (invalidInviterToken.TokenType != TokenType.DataList)
                {
                    _LogWarning($"Invalid inviter Data {invalidInviterToken} from inviter \"{invalidInviters[i]}\" isn't a DataList! Removing...", this);
                    inviteData.Remove(invalidInviters[i]);
                    continue;
                }

                string inviterName = invalidInviters[i].ToString();
                if (!Utilities.IsValid(privateRoom.GetPlayer(inviterName)))
                {
                    _LogWarning($"Inviter \"{invalidInviters[i]}\" is not in the instance! Closing invites...", this);
                    inviteData.Remove(invalidInviters[i]);
                    continue;
                }
                
                DataList invitedFromInvalidInviter = invalidInviterToken.DataList;

                bool inviterIsAdmin = privateRoom.isAdmin(inviterName);
                int allowedInviteCount = privateRoom._GetAllowedInviteCount(inviterName);

                if (!inviterIsAdmin && allowedInviteCount <= 0)
                {
                    _LogWarning($"Inviter \"{invalidInviters[i]}\" cannot invite others! Revoking invited...", this);

                    int initCount = invitedFromInvalidInviter.Count - 1;
                    for (int j = initCount; j >= 0; j--)
                        _RevokeInvite(invitedFromInvalidInviter[j].ToString(), false);

                    invitedFromInvalidInviter.Clear();
                    inviteData.Remove(invalidInviters[i]);
                    continue;
                }

                if (!inviterIsAdmin && invitedFromInvalidInviter.Count > allowedInviteCount)
                {
                    _LogWarning($"Inviter \"{invalidInviters[i]}\" exceeded invite limit {allowedInviteCount}, had invited {invitedFromInvalidInviter.Count}! Revoking extra invited...", this);

                    int initCount = invitedFromInvalidInviter.Count - 1;
                    for (int j = initCount; j >= allowedInviteCount - 1; j--)
                    {
                        _RevokeInvite(invitedFromInvalidInviter[j].ToString(), false);
                        invitedFromInvalidInviter.RemoveAt(j);
                    }

                    continue;
                }

                int invalidCount = invitedFromInvalidInviter.Count;
                for (int j = 0; j < invalidCount; j++)
                {
                    string invitedName = invitedFromInvalidInviter[j].String;
                    if (privateRoom._GetAllowedInviteCount(invitedName) <= 0 && !privateRoom.isAdmin(invitedName))
                        continue;
                    
                    _LogWarning($"Inviter \"{invalidInviters[i]}\" cannot invite other inviter or admin \"{invitedName}\"!", this);
                    inviteData[invalidInviters[i]].DataList.Remove(invitedName);
                }
            }
        }

        public bool IsInvited(string _player)
        {
            if (!privateRoom.hasInit)
                return false;

            DataDictionary inviteData = _GetInviteData();
            if (!Utilities.IsValid(inviteData))
                return false;

            DataList inviterNames = inviteData.GetKeys();
            DataList invitedLists = inviteData.GetValues();

            for (int i = 0; i < invitedLists.Count; i++)
            {
                if (!Utilities.IsValid(privateRoom.GetPlayer(inviterNames[i].ToString())))
                    continue;
                if (invitedLists[i].TokenType != TokenType.DataList)
                    continue;

                DataList invited = invitedLists[i].DataList;
                for (int j = 0; j < invited.Count; j++)
                {
                    if (invited[j].ToString() == _player)
                        return true;
                }
            }

            return false;
        }

        public bool IsPlayerInvitedByInviter(VRCPlayerApi _player, VRCPlayerApi inviter)
        {
            if (!Utilities.IsValid(_player) || !Utilities.IsValid(inviter))
                return false;

            DataDictionary inviteData = _GetInviteData();
            if (!Utilities.IsValid(inviteData))
                return false;

            if (!inviteData.TryGetValue(inviter.displayName, TokenType.DataList, out DataToken inviterInvitedToken))
                return false;

            return inviterInvitedToken.DataList.Contains(_player.displayName);
        }

        #region Add/Remove Invites

        public void _InvitePlayer(VRCPlayerApi _player)
        {
            _Log($"_InvitePlayer_api {_player.displayName}",this);
            if (Utilities.IsValid(_player))
                _InvitePlayer(_player.displayName);
        }
        
        public void _InvitePlayer(string _player)
        {
            _Log($"_InvitePlayer {_player}",this);
            if (!privateRoom.hasInit)
            {
                _LogError($"Cannot invite player {_player}! Plugin has not Initialized.", this);
                return;
            }

            if (!Utilities.IsValid(Networking.LocalPlayer))
                return;

            string localName = Networking.LocalPlayer.displayName;
            int allowedInviteCount = privateRoom._GetAllowedInviteCount(localName);

            if (!Utilities.IsValid(localInvitesGiven))
                localInvitesGiven = new DataList();

            if (privateRoom.isVip && !privateRoom.isAdmin(Networking.LocalPlayer) && localInvitesGiven.Count >= allowedInviteCount)
            {
                _LogError($"Cannot invite player {_player}! You're at or exceeded your allowed invite count and you're not an admin.", this);
                return;
            }

            DataDictionary inviteData = _GetInviteData();
            if (!Utilities.IsValid(inviteData))
                inviteData = new DataDictionary();

            if (!inviteData.TryGetValue(localName, TokenType.DataList, out DataToken localInvited))
            {
                if (inviteData.ContainsKey(localName))
                    inviteData[localName] = new DataList();
                else
                    inviteData.Add(localName, new DataList());

                localInvited = inviteData[localName];
            }
            
            localInvited.DataList.Add(_player);
            localInvitesGiven.Add(_player);
            
            _NetworkInviteData(inviteData);
        }

        public void _RevokeInvite(VRCPlayerApi _player, bool fromLocal)
        {
            _Log($"_RevokeInvite_api {_player.displayName} fromLocal {fromLocal}",this);
            if (Utilities.IsValid(_player))
                _RevokeInvite(_player.displayName, fromLocal);
        }
        
        public void _RevokeInvite(string _player, bool fromLocal)
        {
            _Log($"_RevokeInvite {_player} fromLocal {fromLocal}",this);
            if (!privateRoom.hasInit)
            {
                _LogError("Cannot invite player! Plugin has not Initialized.", this);
                return;
            }
            
            if (!Utilities.IsValid(Networking.LocalPlayer))
                return;

            string localName = Networking.LocalPlayer.displayName;

            if (!Utilities.IsValid(localInvitesGiven))
                localInvitesGiven = new DataList();
            
            DataDictionary inviteData = _GetInviteData();
            if (!Utilities.IsValid(inviteData))
                return;

            if (fromLocal && !privateRoom.isAdmin(Networking.LocalPlayer))
            {
                if (!inviteData.TryGetValue(localName, TokenType.DataList, out DataToken localInvited))
                {
                    if (inviteData.ContainsKey(localName))
                        inviteData[localName] = new DataList();
                    else
                        inviteData.Add(localName, new DataList());
                }
                else
                    localInvited.DataList.Remove(_player);
            }
            else
            {
                DataList invitedLists = inviteData.GetValues();

                for (int i = 0; i < invitedLists.Count; i++)
                {
                    if (invitedLists[i].TokenType == TokenType.DataList)
                        invitedLists[i].DataList.Remove(_player);
                }
            }
            
            localInvitesGiven.Remove(_player);
            _NetworkInviteData(inviteData);
            
            if (!privateRoom.isVip || (privateRoom._GetAllowedInviteCount(_player) <= 0 && !privateRoom.isAdmin(_player)))
            {
                VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
                VRCPlayerApi.GetPlayers(allPlayers);

                foreach (VRCPlayerApi p in allPlayers)
                {
                    if (!Utilities.IsValid(p) || p.displayName != _player)
                        continue;
                    
                    privateRoom.roomTrigger._RemovePlayerFromRoom(p);
                    break;
                }
            }
        }

        #endregion

        internal void _NetworkInviteData(DataDictionary inviteData)
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            if (!VRCJson.TrySerializeToJson(inviteData, JsonExportType.Minify, out DataToken jsonToken))
            {
                _LogError($"Failed to format invite data into Json string! inviteData attempted:\n{inviteData}", this);
                return;
            }

            netInviteData = jsonToken.String;
            
            RequestSerialization();
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Moderation

        /// <summary>
        /// Removes player from room if they are already in the room.
        /// If conditions meet they will be teleported to the outside door.
        /// </summary>
        /// <param name="_playerID">The playerID to teleport to the outside door of the room.</param>
        [NetworkCallable]
        public void RemovePlayerFromRoom(int _playerID)
        {
            VRCPlayerApi localP = Networking.LocalPlayer;
            VRCPlayerApi callingPlayer = NetworkCalling.CallingPlayer;
            if (!Utilities.IsValid(localP) || !Utilities.IsValid(callingPlayer) || _playerID != localP.playerId)
                return;
            
            // Local Checks
            if (privateRoom.isAdmin(localP) || privateRoom._IsMemberInRoles(localP))
                return;
            
            // Remote Checks
            bool isCallerAdmin = privateRoom.isAdmin(callingPlayer);
            if (!isCallerAdmin && !privateRoom._IsMemberInRoles(callingPlayer))
                return;
            
            // Check if current invite was from someone else
            if (isLocalInvited && !IsPlayerInvitedByInviter(localP, callingPlayer) && !isCallerAdmin)
                return;

            // Kick only when in room.
            if (!privateRoom.isInRoom)
                return;
            
            if (Utilities.IsValid(privateRoom.playerMover))
                privateRoom.playerMover._MovePlayer(privateRoom.outsideDoor.spawnPoint, true);
            else
                localP.TeleportTo(privateRoom.outsideDoor.spawnPoint.position, privateRoom.outsideDoor.spawnPoint.rotation);
        }
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Menu
        
        public void _RefreshMenu()
        {
            _Log("_RefreshMenu",this);
            if (!Utilities.IsValid(privateRoom) || !Utilities.IsValid(Networking.LocalPlayer))
                return;

            CheckClickedInvitePlayer();
            
            DataDictionary inviteData = _GetInviteData();
            
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(allPlayers);

            DataList inviterNames = new DataList();
            DataList localInvitedNames = new DataList();
            DataList invitedNames = new DataList();
            DataList invitedLists = new DataList();
            string localName = Networking.LocalPlayer.displayName;
            
            int localAllowedInviteCount = privateRoom._GetAllowedInviteCount(localName);
            // Local is Not Admin, show their invited.
            bool localIsAdmin = privateRoom.isAdmin(Networking.LocalPlayer);
            if (!localIsAdmin)
            {
                if (Utilities.IsValid(inviteData) && inviteData.TryGetValue(localName, TokenType.DataList, out DataToken invitedToken))
                    localInvitedNames = invitedToken.DataList.DeepClone();
                
                inviteStatus.text = $"Invited {localInvitedNames.Count}<size=75%>/{localAllowedInviteCount}</size> Players";
            }
            
            if (Utilities.IsValid(inviteData))
            {
                inviterNames = inviteData.GetKeys();
                invitedLists = inviteData.GetValues();
            }
            
            for (int i = 0; i < invitedLists.Count; i++)
            {
                // Should not happen unless pulled before validation.
                if (invitedLists[i].TokenType != TokenType.DataList)
                    continue;

                DataList invitedList = invitedLists[i].DataList;
                
                for (int j = 0; j < invitedList.Count; j++)
                    invitedNames.Add(invitedList[j]);
            }

            if (localIsAdmin || !privateRoom.isVip)
            {
                if (localIsAdmin)
                    localInvitedNames = invitedNames.DeepClone();
                
                inviteStatus.text = $"Invited {localInvitedNames.Count} Player{(localInvitedNames.Count != 1 ? "s" : "")}";
            }

            DataList atDoorPlayers = new DataList();
            if (Utilities.IsValid(playersAtDoor))
                atDoorPlayers = playersAtDoor;

            groupInvited.SetActive(false);
            groupAtDoor.SetActive(false);
            groupOthers.SetActive(false);
            groupNoInviteAvailable.SetActive(true);
            bool showNoInviteAvailable = true;

            menuPlayerNames = new string[invitePlayerToggles.Length];
            
            // Player Index
            int p = 0;
            
            for (int i = 0; i < invitePlayerToggles.Length; i++)
            {
                string playerName = "";
                VRCPlayerApi player = null;
                
                if ((localAllowedInviteCount == 0 && !localIsAdmin && privateRoom.isVip) || p >= allPlayers.Length)
                {
                    if (localInvitedNames.Count == 0)
                    {
                        invitePlayerToggles[i].transform.SetParent(layoutContainer_Unused);
                        continue;
                    }

                    if (!privateRoom.isVip && CanInviteOthers(localInvitedNames[0].ToString()))
                    {
                        localInvitedNames.RemoveAt(0);
                        i--; // Retry Toggle with Next Player
                        continue;
                    }
                    playerName = localInvitedNames[0].ToString();
                    
                    if (!invitePlayerUnavailableIcons[i].activeSelf)
                        invitePlayerUnavailableIcons[i].SetActive(true);
                }
                else // Player In Instance
                {
                    player = allPlayers[p];
                    p++;
                    // If Player is Local or was invited by someone else or Player is VIP/Admin.
                    if (player.isLocal || (((!localIsAdmin && privateRoom.isVip) || !privateRoom.isVip) && invitedNames.Contains(player.displayName) && !localInvitedNames.Contains(player.displayName)) 
                        || (privateRoom.isVip && CanInviteOthers(player)))
                    {
                        i--; // Retry Toggle with Next Player
                        continue;
                    }
                    
                    playerName = player.displayName;
                    
                    if (invitePlayerUnavailableIcons[i].activeSelf)
                        invitePlayerUnavailableIcons[i].SetActive(false);
                }

                invitePlayerNameTexts[i].text = playerName;
                menuPlayerNames[i] = playerName;

                // Player Invited by Local
                if (localInvitedNames.Contains(playerName))
                {
                    localInvitedNames.Remove(playerName);
                    invitePlayerToggles[i].transform.SetParent(layoutContainer_Invited);

                    if (localIsAdmin)
                    {
                        for (int j = 0; j < invitedLists.Count; j++)
                        {
                            if (!invitedLists[j].DataList.Contains(playerName))
                                continue;
                            
                            invitePlayerNameTexts[i].text = $"{playerName} <size=50%>invited by {inviterNames[j]}</size>";
                            break;
                        }
                    }
                    
                    if (groupInvited.activeSelf)
                        continue;
                    
                    if (showNoInviteAvailable)
                        showNoInviteAvailable = false;
                        
                    groupInvited.SetActive(true);
                }
                // Player At Door
                else if (atDoorPlayers.Contains(new DataToken(player)))
                {
                    atDoorPlayers.Remove(playerName);
                    
                    invitePlayerToggles[i].transform.SetParent(layoutContainer_AtDoor);

                    if (groupAtDoor.activeSelf)
                        continue;
                    
                    if (showNoInviteAvailable)
                        showNoInviteAvailable = false;
                        
                    groupAtDoor.SetActive(true);
                }
                // Player Existing in Instance
                else
                {
                    invitePlayerToggles[i].transform.SetParent(layoutContainer_Others);
                    if (groupOthers.activeSelf)
                        continue;
                    
                    if (showNoInviteAvailable)
                        showNoInviteAvailable = false;
                        
                    groupOthers.SetActive(true);
                }
            }
            
            groupNoInviteAvailable.SetActive(showNoInviteAvailable);

            if (groupInvited.transform.parent.gameObject.activeInHierarchy)
            {
                // This is not great...
                // TODO: Replace with safer solution to rebuild layout groups.
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)groupInvited.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)groupAtDoor.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)groupOthers.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)groupInvited.transform.parent);
            }
        }
        
        bool CanInviteOthers(VRCPlayerApi _player)
        {
            return !privateRoom.isVip || !Utilities.IsValid(_player) || _player.isLocal || privateRoom.isAdmin(_player) || privateRoom._GetAllowedInviteCount(_player) > 0;
        }

        bool CanInviteOthers(string _player)
        {
            return !privateRoom.isVip || privateRoom.isAdmin(_player) || privateRoom._GetAllowedInviteCount(_player) > 0;
        }

        internal void CheckClickedInvitePlayer()
        {
            for (int i = 0; i < invitePlayerToggles.Length; i++)
            {
                if (!Utilities.IsValid(invitePlayerToggles) || !Utilities.IsValid(invitePlayerNameTexts[i]))
                    continue;

                if (!invitePlayerToggles[i].isOn)
                    continue;
                
                invitePlayerToggles[i].SetIsOnWithoutNotify(false);
                
                if (invitePlayerToggles[i].transform.parent != layoutContainer_Invited)
                    _InvitePlayer(menuPlayerNames[i]);
                else
                    _RevokeInvite(menuPlayerNames[i], true);
            }
        }
        
        #endregion

        #region VRChat Player Join/Left
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            _Log($"OnPlayerJoined {player.displayName}", this);
            _RefreshMenu();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            _Log($"OnPlayerLeft {player.displayName}", this);
            _RemoveFromAtDoor(player);
            SendCustomEventDelayedSeconds(nameof(_RefreshMenu), .5f);
        }
        #endregion

        private void __CheckAndSendNotification()
        {
            if (!Utilities.IsValid(privateRoom.notificationSystem)) return;
            if (isLocalInvited == isLocalInvited_local && isLocalInviteInvalid == isLocalInviteInvalid_local) return;
            
            string roomName = privateRoom.roomName;
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = privateRoom.isVip ? "VIP Room" : "Private Room";
            }
                
            isLocalInvited_local = isLocalInvited;
            isLocalInviteInvalid_local = isLocalInviteInvalid;
            if (isLocalInvited)
            {
                privateRoom.notificationSystem._RegisterNotification($"You have been invited to [{roomName}]", "", "",privateRoom.notificationSystem.successSound);
            }
            else if (isLocalInviteInvalid)
            {
                privateRoom.notificationSystem._RegisterNotification($"The player who invited you to [{roomName}] has left. You will not be able to enter the room after leaving.", "", "", privateRoom.notificationSystem.genericSound);
            }
            else
            {
                privateRoom.notificationSystem._RegisterNotification($"Your invite has been removed from [{roomName}]", "", "", privateRoom.notificationSystem.failSound);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void _RefreshDebug()
        {
            if (debug_netData != null) debug_netData.text = netInviteData;
        }

        public void _RequestSerializationDebug()
        {
            RequestSerialization();
        }

        public int length = 1000;

        public void _Log(string _string, UnityEngine.Object _this)
        {
            if (!Utilities.IsValid(debug_Log))
                return;
            
            Debug.Log(_string,_this);
            Print($"{debug_Log.text}\n<#888888>[<#ffffff>L</color>]</color> {_string}");
        }

        public void _LogWarning(string _string, UnityEngine.Object _this)
        {
            if (!Utilities.IsValid(debug_Log))
                return;
            
            Debug.LogWarning(_string,_this);
            Print($"{debug_Log.text}\n<#888888>[<#eeee00>W</color>]</color> {_string}");
        }

        public void _LogError(string _string, UnityEngine.Object _this)
        {
            if (!Utilities.IsValid(debug_Log))
                return;
            
            Debug.LogError(_string,_this);
            Print($"{debug_Log.text}\n<#888888>[<#ee3333>E</color>]</color> {_string}");
        }

        private void Print(string _string)
        {
            if (debug_Log == null) return;
            if (_string.Length >= length)
            {
                _string = _string.Substring(_string.Length - length, length);
            }
            debug_Log.text = _string;
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        void OnValidate()
        {
            // Cannot perform automation with null array.
            if (invitePlayerToggles == null)
                return;
            
            // Text array is null or not equal length to Invite array length, perform automation.
            if (invitePlayerNameTexts == null || invitePlayerToggles.Length != invitePlayerNameTexts.Length)
            {
                invitePlayerNameTexts = new TextMeshProUGUI[invitePlayerToggles.Length];

                // Grab first component in each object.
                for (int i = 0; i < invitePlayerToggles.Length; i++)
                    if (invitePlayerToggles[i])
                        invitePlayerNameTexts[i] = invitePlayerToggles[i].GetComponentInChildren<TextMeshProUGUI>(true);
            }

            // Same length, likely already automated.
            if (invitePlayerUnavailableIcons != null && invitePlayerToggles.Length == invitePlayerUnavailableIcons.Length)
                return;
            
            // Icon array is null or not equal length to Invite array length, perform automation.
            invitePlayerUnavailableIcons = new GameObject[invitePlayerToggles.Length];

            // Grab first component in each object that isn't the toggle target graphic.
            for (int i = 0; i < invitePlayerToggles.Length; i++)
            {
                if (!invitePlayerToggles[i])
                    continue;
                
                Image[] images = invitePlayerToggles[i].GetComponentsInChildren<Image>(true);

                foreach (Image image in images)
                {
                    if (image == invitePlayerToggles[i].targetGraphic)
                        continue;

                    invitePlayerUnavailableIcons[i] = image.gameObject;
                    break;
                }
            }
        }
#endif
    }
}