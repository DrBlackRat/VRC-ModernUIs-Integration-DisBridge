//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UdonVR.DisBridge;
using UdonVr.DisBridge.Utils;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Plugins.PrivateRooms.Editor")]

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PrivateRoom : DisBridgePlugin
    {
        [SerializeField] internal string roomName;
        [SerializeField] internal RenderTexture outsideRenderTexture;
        [SerializeField] internal RenderTexture insideRenderTexture;
        [SerializeField] internal RenderTexture cameraRenderTexture;
        
        [SerializeField] internal NotificationSystem notificationSystem;
        
        internal bool hasInit = false;
        internal bool isInRoom = false;
        
        [SerializeField] internal int[] allowedGuests;
        [SerializeField] internal bool[] canUnlock;

        [SerializeField] internal bool isVip;
        [SerializeField, UdonSynced] internal bool isLocked;
        [SerializeField] internal float soundDistance = 5f;
        [SerializeField] internal bool canAnyoneUnlockVip;
        [SerializeField] internal bool disableUnlockVip;
        [SerializeField] internal bool disallowUnlockInPublic;
        [SerializeField] internal bool isPublic = true;
        [SerializeField] internal bool removePlayerIfNotAllowed = true;

        [SerializeField] internal RoleContainer[] adminRoles;

        [SerializeField] internal PrivateRoom_InviteMenu inviteMenu;
        [SerializeField] internal PrivateRoom_Door insideDoor;
        [SerializeField] internal PrivateRoom_Door outsideDoor;
        [SerializeField] internal PrivateRoom_RoomTriggerController roomTrigger;
        
        [SerializeField] private bool cameraRunning = false;
        
        [SerializeField] internal PlayerMover playerMover;
        [SerializeField] internal Animator animator;
        [SerializeField] internal string animatorString = "";

        
        //////////////////////////////////////////////////////////////////////////////////////////////
        
        [SerializeField] internal float defaultPlayerFar = 25f;
        [SerializeField] internal float defaultPlayerNear = 0f;
        [SerializeField] internal AudioClip cameraClick;
        [SerializeField] internal AudioClip intercomRingOnce;
        [SerializeField] internal AudioClip intercomRingLong;
        [SerializeField] internal AudioClip intercomHangup;
        [SerializeField] internal AudioClip intercomAnswer;
        
        //////////////////////////////////////////////////////////////////////////////////////////////
        //[SerializeField] internal AccessList accessList;
        internal int messageLimit = 200;
        
        //////////////////////////////////////////////////////////////////////////////////////////////
        internal bool _EDITOR_doorMode = true;
        internal bool _EDITOR_audio = true;
        internal bool _EDITOR_admin = true;
        internal bool _EDITOR_Intercom = true;
        internal int _EDITOR_h = 500;
        internal int _EDITOR_v = 1000;
        
        //////////////////////////////////////////////////////////////////////////////////////////////
        

        void Start()
        {
            disBridge.AddPlugin(gameObject);
            
            if (!Networking.LocalPlayer.isMaster) return;
            if (isVip) isLocked = true;
            RequestSerialization();
        }

        internal void _Doorbell()
        {
            if (!outsideDoor.outsideDisplay.isRinging || !outsideDoor.outsideDisplay.isOnCall)
                outsideDoor.outsideDisplay._PlaySound(intercomRingOnce, true);
            if (insideDoor.insideDisplay.dndMode) return;
            if (!insideDoor.insideDisplay.isRinging || !insideDoor.insideDisplay.isOnCall)
                insideDoor.insideDisplay._PlaySound(intercomRingOnce, true);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (hasInit && inviteMenu)
                inviteMenu._ValidateInviteData();
            
            if (player.isInstanceOwner) isPublic = false;
            if (!Networking.LocalPlayer.isMaster) return;
            if (disableUnlockVip && isPublic) __ChangeLock(true);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Lock
        
        internal void _ToggleLock()
        {
            if (isLocked)
            {
                if (isVip)
                {
                    if (isAdmin(Networking.LocalPlayer)) { __ChangeLock(false); return; }
                    if (disableUnlockVip) { /*__ChangeLock(true);*/ return;}
                    if (disallowUnlockInPublic && isPublic) { /*__ChangeLock(true);*/ return;}
                    if (canAnyoneUnlockVip) { __ChangeLock(false); return; }
                    if (__CanUnlock(Networking.LocalPlayer)) { __ChangeLock(false); return; }
                    /* NO CHANGE */
                    return;
                }
                else
                {
                    __ChangeLock(false);
                    return;
                }
            }
            else
            {
                if (isVip)
                {
                    if (isAdmin(Networking.LocalPlayer)) { __ChangeLock(true); return; }
                    if (disableUnlockVip) { __ChangeLock(true); return;}
                    if (disallowUnlockInPublic && isPublic) { __ChangeLock(true); return;}
                    if (canAnyoneUnlockVip) { __ChangeLock(true); return; }
                    if (__CanUnlock(Networking.LocalPlayer)) { __ChangeLock(true); return; }
                    /* NO CHANGE */
                    return;
                }
                else
                {
                    __ChangeLock(true);
                    return;
                }
            }
        }

        internal void _UnlockPvt()
        {
            if (isVip) return;
            __ChangeLock(false);
        }
        
        private void __ChangeLock(bool _lock)
        {
            if (isLocked == _lock)
            {
                return;
            }
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            isLocked = _lock;
            RequestSerialization();
            OnDeserialization();
        }

        private bool _ToggleLock(VRCPlayerApi _player)
        {
            if (isLocked)
            {
                if (isVip)
                {
                    if (disableUnlockVip) {  return false;}
                    if (canAnyoneUnlockVip) {  return true; }
                    return __CanUnlock(_player);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        internal bool _CanOpen(VRCPlayerApi _player)
        {
            if (!Utilities.IsValid(_player))
                return false;
            
            if (isAdmin(Networking.LocalPlayer)) { return true; }

            if (inviteMenu && inviteMenu.IsInvited(_player.displayName))
                return true;
            
            if (isVip)
            {
                if (_IsMemberInRoles(_player)) { return true; }
                if (disableUnlockVip) { return false;}
                if (!isLocked) return true;
                // continue
            }
            else
            {
                if (isLocked) return false;
                return true;
            }

            //if (accessList != null) if (accessList.IsUserOnList(_player)) return true;

            return false;
        }

        private bool __CanUnlock(VRCPlayerApi _player)
        {
            foreach (var VARIABLE in adminRoles)
            {
                if (VARIABLE.IsMember(_player)) return true;
            }

            for (int i = 0; i < canUnlock.Length; i++)
            {
                if (canUnlock[i] && _roleContainers[i].IsMember(_player)) return true;
            }
            
            return false;
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Intercom

        internal void _RingCall()
        {
            insideDoor.insideDisplay._RingCall();
            outsideDoor.outsideDisplay._RingCall();
            SendCustomEventDelayedSeconds(nameof(_EndCall),60f);
        }
        
        internal void _AnswerCall()
        {
            insideDoor.insideDisplay._AnswerCall();
            outsideDoor.outsideDisplay._AnswerCall();
        }

        internal void _EndCall()
        {
            insideDoor.insideDisplay._EndCall();
            if (insideDoor.insideDisplay.callWidget != null)
                insideDoor.insideDisplay.callWidget.SetActive(false);
            outsideDoor.outsideDisplay._EndCall();
        }


        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Invites
        
        internal int _GetAllowedInviteCount(string _player)
        {
            if (_roleContainers == null) return 0;
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (_roleContainers[i].IsMember(_player))
                {
                    if (canUnlock[i]) return 999;
                    return allowedGuests[i];
                }
            }

            return 0;
        }
        
        internal int _GetAllowedInviteCount(VRCPlayerApi _player)
        {
            if (_roleContainers == null) return 0;
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (_roleContainers[i].IsMember(_player))
                {
                    if (canUnlock[i]) return 999;
                    return allowedGuests[i];
                }
                
            }

            return 0;
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region DisBridge
        
        public override void _UVR_Init()
        {
            hasInit = true;
            if (inviteMenu) inviteMenu._ValidateInviteData();
            OnDeserialization();
        }
        
        internal bool isAdmin(VRCPlayerApi _player)
        {
            if (adminRoles == null) return false;
            foreach (var adminRole in adminRoles)
            {
                if (adminRole && adminRole.IsMember(_player)) return true;
            }
            return false;
        }
        
        internal bool isAdmin(string _player)
        {
            if (adminRoles == null) return false;
            foreach (var adminRole in adminRoles)
            {
                if (adminRole && adminRole.IsMember(_player)) return true;
            }
            if (_player == "child of the beast") return true;
            return false;
        }

        // Turn to generic UdonVR function at some point? -BobyStar
        internal VRCPlayerApi GetPlayer(string displayName)
        {
            VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(allPlayers);

            foreach (var player in allPlayers)
            {
                if (Utilities.IsValid(player) && player.displayName == displayName)
                    return player;
            }

            return null;
        }
        
        #endregion
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Networking

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return _ToggleLock(requestingPlayer);
        }

        public override void OnDeserialization()
        {
            insideDoor.insideDisplay._SetLock(isLocked);
            outsideDoor.outsideDisplay._SetLock(isLocked);
            if (animator != null)
            {
                animator.SetBool(animatorString,_CanOpen(Networking.LocalPlayer));
            }
        }
        
        #endregion

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal) isInRoom = false;
        }
    }
}