
using System;
using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class UserAccessList_Button_Press : UdonSharpBehaviour
    {
        public UserAccessList_Button userAccessList_Button;
        public Image toggleImage;
        public UserAccessList userAccessList;
        public bool hasAccessList = false;
        public TextMeshProUGUI toggleText;

        private void Start()
        {
            _Setup();
        }

        public void _Setup()
        {
            if (!Utilities.IsValid(userAccessList)) gameObject.SetActive(false);
        }
        
        [PublicAPI]
        public void _Click()
        {
            Debug.Log($"[AL][UserAccessList_Button_Press] {gameObject.name} - _Click");
            VRCPlayerApi player = userAccessList_Button.player;
            if (!Utilities.IsValid(player))
            {
                Debug.LogError($"[AL][UserAccessList_Button_Press] {gameObject.name} - User was not valid");
                userAccessList_Button.__ResetButton();
                return;
            }

            if (userAccessList._IsAdmin(player))
            {
                Debug.Log($"[AL][UserAccessList_Button_Press] {gameObject.name} - Cannot remove Privileged Member {player.displayName} from {userAccessList.accessListName}");
                return;
            }
            if (userAccessList.playerList.Contains(player.displayName))
            {
                Debug.Log($"[AL][UserAccessList_Button_Press] {gameObject.name} - Removing {player.displayName} from {userAccessList.accessListName}");
                userAccessList._RemoveUserAccess(Networking.LocalPlayer.playerId, player.playerId);
                toggleImage.enabled = false;
            }
            else
            {
                Debug.Log($"[AL][UserAccessList_Button_Press] {gameObject.name} - Adding {player.displayName} to {userAccessList.accessListName}");
                userAccessList._AddUserAccess(Networking.LocalPlayer.playerId, player.playerId);
                toggleImage.enabled = true;
            }

            userAccessList_Button._RefreshToggle();
        }
    }
}