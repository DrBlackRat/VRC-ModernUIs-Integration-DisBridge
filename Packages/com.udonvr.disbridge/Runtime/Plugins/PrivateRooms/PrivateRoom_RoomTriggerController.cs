//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    public class PrivateRoom_RoomTriggerController : TriggerZone_Class
    {
        [SerializeField] internal PrivateRoom room;

        [SerializeField] private DataList inRoom;
        [SerializeField] internal string nameList;
        public bool useExclusiveAudio = true;
        private VRCPlayerApi[] players;

        public override void _PostAddPlayer(VRCPlayerApi player)
        {
            if (player.isLocal && room.removePlayerIfNotAllowed)
            {
                if (!room._CanOpen(player))
                {
                    if (room.notificationSystem != null)
                    {
                        string _name = string.IsNullOrWhiteSpace(room.roomName)? "VIP Room" : room.roomName;
                        room.notificationSystem._RegisterNotification("You have been removed from the area.",$"You don't have the required access to enter {_name}.",_name,room.notificationSystem.failSound);
                    }
                    if (room.playerMover != null)
                    {
                        room.playerMover._MovePlayer(room.outsideDoor.spawnPoint, true);
                    }
                    else
                    {
                        player.TeleportTo(room.outsideDoor.spawnPoint.position, room.outsideDoor.spawnPoint.rotation);
                    }
                    //TODO add kicked out notification
                    //TODO maybe move into a function inside of controller for invite system to use
                    room.isInRoom = false;
                    return;
                }
            }
            _Add_Namelist(player);
            if (player.isLocal) room.isInRoom = true;
        }

        public override void _PostRemovePlayer(VRCPlayerApi player)
        {
            _Remove_Namelist(player);
            if (player.isLocal) room.isInRoom = false;
            if (!room.isVip && inRoom.Count == 0) room._UnlockPvt();
        }

        public override void _OnPlayerLeft(VRCPlayerApi player)
        {
            _Remove_Namelist(player);
            if (!room.isVip && inRoom.Count == 0) room._UnlockPvt();
        }

        public void __OnDeserialization()
        {
            if (nameList == null) return;
            if (room.insideDoor.insideDisplay.hideUsers)
            {
                if (room.isAdmin(Networking.LocalPlayer))
                {
                    if (room.insideDoor.insideDisplay.nameList != null) room.insideDoor.insideDisplay.nameList.text = $"<B><nobr>ADMIN ANONYMOUS BYPASS</nobr></B>\n{nameList}";
                    if (room.outsideDoor.outsideDisplay.nameList != null) room.outsideDoor.outsideDisplay.nameList.text = $"<B><nobr>ADMIN ANONYMOUS BYPASS</nobr></B>\n{nameList}";
                    return;
                }
                else
                {
                    if (room.insideDoor.insideDisplay.nameList != null) room.insideDoor.insideDisplay.nameList.text = string.Empty;
                    if (room.outsideDoor.outsideDisplay.nameList != null) room.outsideDoor.outsideDisplay.nameList.text = string.Empty;
                    return;
                }
            }

            if (room.insideDoor.insideDisplay.nameList != null) room.insideDoor.insideDisplay.nameList.text = nameList;
            if (room.outsideDoor.outsideDisplay.nameList != null) room.outsideDoor.outsideDisplay.nameList.text = nameList;
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return requestingPlayer.isMaster;
        }

        public void UpdateExclusiveAudio()
        {
            if (!useExclusiveAudio) return;
            players = VRCPlayerApi.GetPlayers(players);
            for (int i = 0; i < players.Length; i++)
            {
                
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Namelist

        private void _UpdateNamelist()
        {
            if (!Utilities.IsValid(inRoom)) inRoom = new DataList();
            nameList = String.Empty;
            VRCPlayerApi _tmpPlayer;
            for (int i = 0; i < inRoom.Count; i++)
            {
                _tmpPlayer = VRCPlayerApi.GetPlayerById((int)inRoom[i]);
                if (!Utilities.IsValid(_tmpPlayer))
                {
                    inRoom.Remove(inRoom[i]);
                    continue;
                }

                nameList = $"{nameList}<nobr>{_tmpPlayer.displayName}</nobr>\n";
            }
            __OnDeserialization();
        }

        internal void _Add_Namelist(VRCPlayerApi _player)
        {
            if (inRoom.Contains(_player.playerId))
            {
                return;
            }
            if (!Utilities.IsValid(inRoom)) inRoom = new DataList();
            inRoom.Add((int)_player.playerId);
            _UpdateNamelist();
        }

        internal void _Remove_Namelist(VRCPlayerApi _player)
        {
            if (!inRoom.Contains(_player.playerId))
            {
                return;
            }
            if (!Utilities.IsValid(inRoom)) inRoom = new DataList();
            inRoom.Remove((int)_player.playerId);
            _UpdateNamelist();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Moderation

        // Possible future use. Not good to check all players,
        // especially those with a *closed* invite which should be allowed to stay in the room but not re-enter.
        /*public void _CheckPlayersInRoom()
        {
            if (!Utilities.IsValid(inRoom)) return;

            for (int i = 0; i < inRoom.Count; i++)
            {
                if (inRoom[i].TokenType != TokenType.Reference)
                    continue;

                VRCPlayerApi playerInRoom = (VRCPlayerApi)inRoom[i].Reference;
                
                if (!Utilities.IsValid(playerInRoom))
                    continue;
                
                if (room.isAdmin(playerInRoom) || room.IsMemberInRoles(playerInRoom) || room.IsInvited(playerInRoom.displayName))
                    continue;
                
                Debug.LogWarning($"Player {playerInRoom.displayName} is not allowed in this room!", this);
                //_RemovePlayerFromRoom(playerInRoom);
            }
        }*/
        
        /// <summary>
        /// Removes the player from the room if they currently reside inside the room. Conditions must be met in order to succeed.
        /// </summary>
        /// <param name="_player">The player to kick out of the room.</param>
        internal void _RemovePlayerFromRoom(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player))
                return;

            // TODO: Check if in room, preferably Locally.
            /*if (_player.isLocal && !inRoom.Contains(new DataToken(_player)))
            {
                Debug.LogError($"Cannot remove \"{_player.displayName}\" from room, they are not in the room zone.");
                return;
            }*/
            
            if (room.isAdmin(_player) || room._IsMemberInRoles(_player) || (room.inviteMenu && room.inviteMenu.IsInvited(_player.displayName)))
            {
                Debug.LogError($"Cannot remove \"{_player.displayName}\" from room, they are allowed in the room (Admin/Invitee/Invited).");
                return;
            }

            if (!Utilities.IsValid(room.inviteMenu))
                return;
            
            Debug.Log($"Kicking player \"{_player.displayName}\" from room");
            room.inviteMenu.SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(PrivateRoom_InviteMenu.RemovePlayerFromRoom), _player.playerId);
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}