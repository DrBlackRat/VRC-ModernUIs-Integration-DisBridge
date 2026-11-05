
using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Editors")]

namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UserAccessList_Button : UdonSharpBehaviour
    {
        public UserAccessList_Controller userAccessList_Button_List;
        public UserAccessList_Button_Press[] buttons;
        public VRCPlayerApi player;
        public TextMeshProUGUI playerName;
        

        [HideInInspector] public Vector2 openSize;
        [HideInInspector] public Vector2 closeSize;
        [HideInInspector] public RectTransform _rectTransform;



        [PublicAPI]
        public bool _AssignPlayer(VRCPlayerApi _player)
        {
            if (Utilities.IsValid(player)) return false;
            player = _player;
            playerName.text = player.displayName;
            _rectTransform.sizeDelta = openSize;
            _RefreshToggle();
            foreach (var _button in buttons)
            {
                _button.gameObject.SetActive(_button.hasAccessList);
            }
            return true;
        }

        public bool _RemovePlayer(VRCPlayerApi _player)
        {
            if (_player == player)
            {
                __ResetButton();
                return true;
            }

            if (Utilities.IsValid(player)) return false;
            return false;
        }

        public void _RefreshToggle()
        {
            if (!Utilities.IsValid(player))
            {
                return;
            }


            for (int i = 0; i < userAccessList_Button_List.userAccessLists.Length; i++)
            {
                var _accessList = userAccessList_Button_List.userAccessLists[i];
                if (_accessList._IsPreferredOwner(player))
                {
                    Debug.Log($"[AL][AccessList Button][{_accessList.accessListName}][{gameObject.name}] _RefreshToggle - player [{player.displayName}] is Preferred Owner");
                    buttons[i].toggleImage.color = userAccessList_Button_List.buttonColorPreferredOwner;
                    buttons[i].toggleImage.enabled = true;
                    continue;
                }
                if (_accessList._IsAdmin(player))
                {
                    Debug.Log($"[AL][AccessList Button][{_accessList.accessListName}][{gameObject.name}] _RefreshToggle - player [{player.displayName}] is a PrivilegedMember");
                    buttons[i].toggleImage.color = userAccessList_Button_List.buttonColorPrivilegedMember;
                    buttons[i].toggleImage.enabled = true;
                    continue;
                }
                buttons[i].toggleImage.color = userAccessList_Button_List.buttonColorMember;
                buttons[i].toggleImage.enabled = _accessList._IsMember(player);
                Debug.Log($"[AL][AccessList Button][{_accessList.accessListName}][{gameObject.name}] _RefreshToggle - player [{player.displayName}] is member? {buttons[i].toggleImage.enabled}");
            }
        }

        //TODO MAKE INTERNAL
        public void __ResetButton()
        {
            playerName.text = string.Empty;
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(false);
                button.toggleImage.enabled = false;
            }
            _rectTransform.sizeDelta = closeSize;
        }
    }
}