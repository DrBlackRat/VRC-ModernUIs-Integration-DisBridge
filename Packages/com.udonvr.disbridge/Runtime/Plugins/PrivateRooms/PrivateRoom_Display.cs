//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    public abstract class PrivateRoom_Display : UdonSharpBehaviour
    {
        [SerializeField] internal PrivateRoom_Display otherDisplay;
        [SerializeField] internal PrivateRoom_Door door;
        [SerializeField] internal AudioSource src;
        [SerializeField] internal AudioSource srcDoorbell;
        [SerializeField] internal TextMeshProUGUI roomNameDisplay;
        [SerializeField] internal GameObject lockIcon;
        [SerializeField] internal GameObject unlockIcon;
        [SerializeField] private GameObject dndIcon_On;
        [SerializeField] private GameObject dndIcon_Off;
        
        [SerializeField] internal GameObject defaultDisplay;

        [SerializeField] internal Camera intercomCamera;
        [SerializeField] internal GameObject intercomDisplay_Outside;
        [SerializeField] internal GameObject intercomDisplay_Inside;
        [SerializeField] internal GameObject intercomCallDisplay;
        [SerializeField] internal DataList boostUsers;

        [SerializeField] internal bool playerIsInIntercom = false;

        [SerializeField] internal bool isOnCall = false;
        [SerializeField] internal bool isRinging = false;
        [SerializeField] internal TextMeshProUGUI nameList;
        [SerializeField] internal TextMeshProUGUI roomMessage;
        
        //[SerializeField] internal RenderTexture insideImages;
        //[SerializeField] internal RenderTexture outsideImages;
        //[SerializeField] internal RenderTexture cameraImages;
        [SerializeField] internal RawImage RawImage_intercomDisplay_Outside;
        [SerializeField] internal RawImage RawImage_intercomDisplay_Inside;
        [SerializeField] internal RawImage RawImage_intercomCall_BG;


        private void Start()
        {
            boostUsers = new DataList();
        }

        internal void _AddUser(VRCPlayerApi _player)
        {
            Debug.LogWarning($"_AddUser {gameObject.name}");
            if (_player.isLocal)
            {
                Debug.LogWarning($"_AddUser {gameObject.name} -- islocal");
                playerIsInIntercom = true;
                VRCPlayerApi _tmpPlayer;
                for (int i = 0; i < otherDisplay.boostUsers.Count; i++)
                {
                    _tmpPlayer = VRCPlayerApi.GetPlayerById((int)otherDisplay.boostUsers[i]);
                    if (!Utilities.IsValid(_tmpPlayer))
                    {
                        otherDisplay.boostUsers.Remove(otherDisplay.boostUsers[i]);
                        continue;
                    }
                    if (!isOnCall) return;
                    _tmpPlayer.SetVoiceDistanceFar(10000f);
                    _tmpPlayer.SetVoiceDistanceNear(10000f);
                    Debug.LogWarning($"_AddUser {gameObject.name} -- set to max {_tmpPlayer.displayName}");
                }
            }
            else
            {
                Debug.LogWarning($"_AddUser {gameObject.name} -- {_player.displayName} is not local, Adding");
                boostUsers.Add((int)_player.playerId);
                if (!otherDisplay.playerIsInIntercom) return;
                if (!isOnCall) return;
                _player.SetVoiceDistanceFar(10000f);
                _player.SetVoiceDistanceNear(10000f);
                Debug.LogWarning($"_AddUser {gameObject.name} -- set to max {_player.displayName}");
            }
        }
        internal void _RemoveUser(VRCPlayerApi _player)
        {
            Debug.LogWarning($"_RemoveUser {gameObject.name}");
            if (_player.isLocal)
            {
                Debug.LogWarning($"_RemoveUser {gameObject.name} -- islocal");
                playerIsInIntercom = false;
                VRCPlayerApi _tmpPlayer;
                for (int i = 0; i < otherDisplay.boostUsers.Count; i++)
                {
                    _tmpPlayer = VRCPlayerApi.GetPlayerById((int)otherDisplay.boostUsers[i]);
                    if (!Utilities.IsValid(_tmpPlayer))
                    {
                        otherDisplay.boostUsers.Remove(otherDisplay.boostUsers[i]);
                        continue;
                    }
                    if (!isOnCall) return;
                    _tmpPlayer.SetVoiceDistanceFar(door.pvtRoom.defaultPlayerFar);
                    _tmpPlayer.SetVoiceDistanceNear(door.pvtRoom.defaultPlayerNear);
                    Debug.LogWarning($"_RemoveUser {gameObject.name} -- set to default {_tmpPlayer.displayName}");
                }
            }
            else
            {
                Debug.LogWarning($"_RemoveUser {gameObject.name} -- {_player.displayName} is not local, Removing");
                boostUsers.Remove((int)_player.playerId);
                if (!otherDisplay.playerIsInIntercom) return;
                if (!isOnCall) return;
                _player.SetVoiceDistanceFar(door.pvtRoom.defaultPlayerFar);
                _player.SetVoiceDistanceNear(door.pvtRoom.defaultPlayerNear);
                Debug.LogWarning($"_RemoveUser {gameObject.name} -- set to default {_player.displayName}");
            }
        }

        internal void _RefreshCallComs()
        {
            Debug.LogWarning($"_RefreshCallComs {gameObject.name}");
            VRCPlayerApi _tmpPlayer;
            if (!playerIsInIntercom)
            {
                Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- !playerIsInIntercom -- resetting {otherDisplay.boostUsers.Count} users");
                for (int i = 0; i < otherDisplay.boostUsers.Count; i++)
                {
                    _tmpPlayer = VRCPlayerApi.GetPlayerById((int)otherDisplay.boostUsers[i]);
                    if (!Utilities.IsValid(_tmpPlayer))
                    {
                        otherDisplay.boostUsers.Remove(otherDisplay.boostUsers[i]);
                        continue;
                    }
                    _tmpPlayer.SetVoiceDistanceFar(door.pvtRoom.defaultPlayerFar);
                    _tmpPlayer.SetVoiceDistanceNear(door.pvtRoom.defaultPlayerNear);
                    Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- set to default {_tmpPlayer.displayName}");
                }
                return;
            }
            else
            {
                if (!isOnCall)
                {
                    Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- !isCalling -- resetting {otherDisplay.boostUsers.Count} users");
                    for (int i = 0; i < otherDisplay.boostUsers.Count; i++)
                    {
                        _tmpPlayer = VRCPlayerApi.GetPlayerById((int)otherDisplay.boostUsers[i]);
                        if (!Utilities.IsValid(_tmpPlayer))
                        {
                            otherDisplay.boostUsers.Remove(otherDisplay.boostUsers[i]);
                            continue;
                        }
                        _tmpPlayer.SetVoiceDistanceFar(door.pvtRoom.defaultPlayerFar);
                        _tmpPlayer.SetVoiceDistanceNear(door.pvtRoom.defaultPlayerNear);
                        Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- set to default {_tmpPlayer.displayName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- isCalling -- boosting {otherDisplay.boostUsers.Count} users");
                    for (int i = 0; i < otherDisplay.boostUsers.Count; i++)
                    {
                        _tmpPlayer = VRCPlayerApi.GetPlayerById((int)otherDisplay.boostUsers[i]);
                        if (!Utilities.IsValid(_tmpPlayer))
                        {
                            otherDisplay.boostUsers.Remove(otherDisplay.boostUsers[i]);
                            continue;
                        }
                        _tmpPlayer.SetVoiceDistanceFar(10000f);
                        _tmpPlayer.SetVoiceDistanceNear(10000f);
                        Debug.LogWarning($"_RefreshCallComs {gameObject.name} -- set to max {_tmpPlayer.displayName}");
                    }
                    return;
                }
            }
        }

        internal void _SetDnd(bool _dnd)
        {
            if (dndIcon_On != null)
                dndIcon_On.SetActive(_dnd);
            if (dndIcon_Off != null)
                dndIcon_Off.SetActive(!_dnd);
        }
        internal void _SetLock(bool _lock)
        {
            lockIcon.SetActive(_lock);
            unlockIcon.SetActive(!_lock);
        }
        //////////////////////////////////////////////////////////////////////////////
        public void N_Intercom_StartCall()
        {
            door.pvtRoom._RingCall();
        }
        public void _Intercom_StartCall_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_Intercom_StartCall));
        }
        //////////////////////////////////////////////////////////////////////////////
        public void N_Intercom_Hangup()
        {
            Debug.LogWarning($"N_Intercom_Hangup -- {gameObject.name}");
            door.pvtRoom._EndCall();
        }
        public void _Intercom_Hangup_Ui()
        {
            Debug.LogWarning($"_Intercom_Hangup_Ui -- {gameObject.name}");
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_Intercom_Hangup));
        }
        //////////////////////////////////////////////////////////////////////////////
        public void N_Doorbell()
        {
            door.pvtRoom._Doorbell();
        }
        public void _Doorbell_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_Doorbell));
        }
        //////////////////////////////////////////////////////////////////////////////
        
        internal virtual void _RingCall()
        {
            intercomCamera.enabled = true;
            intercomDisplay_Outside.SetActive(true);
            intercomDisplay_Inside.SetActive(false);
            intercomCallDisplay.SetActive(true);
            defaultDisplay.SetActive(false);
            isRinging = true;
            _PlaySound(door.pvtRoom.intercomRingLong);
        }
        
        internal void _AnswerCall()
        {
            intercomCamera.enabled = true;
            intercomDisplay_Outside.SetActive(true);
            intercomDisplay_Inside.SetActive(true);
            intercomCallDisplay.SetActive(false);
            defaultDisplay.SetActive(false);
            _PlaySound(door.pvtRoom.intercomAnswer);
            isRinging = false;
            isOnCall = true;
            _RefreshCallComs();
        }

        internal void _EndCall()
        {
            Debug.LogWarning($"_EndCall -- {gameObject.name}");
            if (isRinging || isOnCall)
            {
                _PlaySound(door.pvtRoom.intercomHangup);
            }
            isRinging = false;
            isOnCall = false;
            intercomCamera.enabled = false;
            intercomDisplay_Outside.SetActive(false);
            intercomDisplay_Inside.SetActive(false);
            intercomCallDisplay.SetActive(false);
            defaultDisplay.SetActive(true);
            _RefreshCallComs();
        }

        internal void _PlaySound(AudioClip _clip, bool isDoorbell = false)
        {
            if (isDoorbell)
            {
                if (Vector3.Distance(transform.position, Networking.LocalPlayer.GetPosition()) > door.pvtRoom.soundDistance)
                {
                    srcDoorbell.mute = true;
                    return;
                }
                srcDoorbell.mute = false;
                srcDoorbell.Stop();
                _clip = door.pvtRoom.intercomRingOnce;
                if (_clip == null) return;
                srcDoorbell.clip = _clip;
                srcDoorbell.Play();
                return;
            }
            src.mute = false;
            src.Stop();
            if (_clip == null) return;
            src.clip = _clip;
            src.Play();
        }
    }
}