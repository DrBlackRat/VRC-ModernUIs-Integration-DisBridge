
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    public class UserAccessList_Controller : UdonSharpBehaviour
    {
        public UserAccessList[] userAccessLists;
        public Color32 buttonColorMember = Color.white;
        public Color32 buttonColorPrivilegedMember = Color.blue;
        public Color32 buttonColorPreferredOwner = Color.yellow;

        public Transform buttonParent;
        public UserAccessList_Button[] buttons;

        public RectTransform verticalLayoutGroup;
        public GameObject menu;
        
        private bool initMenu = false;
        
        private void __UpdateLayout()
        {
            if (Utilities.IsValid(verticalLayoutGroup)) LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup);
        }
        
        //##################################################################################################################
        #region API

        public void _InitMenu()
        {
            if (initMenu) return;
            initMenu = true;
            VRCPlayerApi[] _player = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(_player);
            int _i = 0;
            for (int i = 0; i < _player.Length; i++)
            {
                if (buttons[_i]._AssignPlayer(_player[i]))
                {
                    // Added player, continue
                }
                else
                {
                    // Button was already assigned, check next button
                    i = Mathf.Clamp(i--, 0, _player.Length - 1);
                }
                // Go to next button
                _i++;
            }
            __UpdateLayout();
        }
        
        /// <summary>
        /// Opens the menu and allows new network data to refresh the menu.
        /// Opening the menu also refreshes the menu.
        /// This also opens all Children Access Lists.
        /// </summary>
        [PublicAPI]
        public void _Menu_Open()
        {

            if (Utilities.IsValid(menu))
            {
                menu.SetActive(true);
                _QueueMenuRefresh();
            }
            foreach (var list in userAccessLists)
            {
                list._OpenList();
            }
            
            // __UnpackageDataList runs _Menu_Refresh(false)
            // -- This is to not double update the menus as that happens automatically when opening them below

        }
        /// <summary>
        /// Closes the menu and disallows new network data to refresh the menu.
        /// This also closes all Children Access Lists.
        /// </summary>
        [PublicAPI]
        public void _Menu_Close()
        {
            
            if (Utilities.IsValid(menu)) menu.SetActive(false);
            foreach (var list in userAccessLists)
            {
                list._CloseList();
            }

        }
        
        /// <summary>
        /// Refreshes this Access list and it's children.
        /// </summary>
        [PublicAPI]
        public void _Menu_Refresh(bool _updateChildren = true)
        {
            foreach (var button in buttons)
            {
                button._RefreshToggle();
            }
            __UpdateLayout();
        }

        private bool _queueMenuRefresh = false;
        public void _QueueMenuRefresh()
        {
            if (_queueMenuRefresh) return;
            _queueMenuRefresh = true;
            SendCustomEventDelayedFrames(nameof(__QueueMenuRefresh),1);
        }

        public void __QueueMenuRefresh()
        {
            _queueMenuRefresh = false;
            _Menu_Refresh(false);
        }
        
        #endregion
        //##################################################################################################################
        #region VRC overrides
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!initMenu) return;
            __AddPlayerToButtons(player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!initMenu) return;
            __RemovePlayerFromButtons(player);
        }
        
        #endregion
        //##################################################################################################################
        private void __AddPlayerToButtons(VRCPlayerApi _player)
        {
            foreach (var button in buttons)
            {
                if (button._AssignPlayer(_player))
                {
                    __UpdateLayout();
                    return;
                }
            }
        }

        private void __RemovePlayerFromButtons(VRCPlayerApi _player)
        {
            foreach (var button in buttons)
            {
                if (button._RemovePlayer(_player))
                {
                    __UpdateLayout();
                    return;
                }
            }
        }
    }
}