//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using TMPro;
using UdonSharp;
using UdonVR.DisBridge;
using UdonVR.DisBridge.Plugins;
using UdonVR.DisBridge.Plugins.PrivateRooms;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{

    public class PrivateRoom_AccessButton : UdonSharpBehaviour
    {
        internal VRCPlayerApi player;

        [SerializeField] internal AccessList accessList;
        [SerializeField] internal PrivateRoom room;
        [SerializeField] internal TextMeshProUGUI buttonLabel;

        [SerializeField] internal GameObject hasAccessIndicator;



        internal void setupButton()
        {
            if (!Utilities.IsValid(player))
            {
                buttonLabel.text = "nullPlayer";
                return;
            }

            buttonLabel.text = player.displayName;
            if (accessList.IsUserOnList(player))
            {
                hasAccessIndicator.SetActive(true);
                return;
            }
            else
            {
                hasAccessIndicator.SetActive(false);
                return;
            }
        }

        public override void Interact()
        {
            RoleContainer _tmpRole;
            if (room.isVip)
            {
                if (room._TryGetPlayerRoleContainer(Networking.LocalPlayer, out _tmpRole))
                {
                    if (accessList._AddUser(player, Networking.LocalPlayer, _tmpRole))
                    {
                        hasAccessIndicator.SetActive(true);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (accessList._AddUser(player))
                {
                    hasAccessIndicator.SetActive(true);
                }
                else
                {
                    return;
                }
            }


        }
    }
}