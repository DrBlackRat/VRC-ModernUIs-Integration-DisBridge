//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge.Plugins.PrivateRooms
{
    public class PrivateRoom_Display_Inside : PrivateRoom_Display
    {
        [SerializeField] internal PrivateRoom_RoomTriggerController roomTrigger;
        [SerializeField] internal GameObject callWidget;

        [SerializeField] internal Camera photoCamera;

        [SerializeField] internal GameObject flash;
        [SerializeField] internal bool isTakingPhoto = false;

        
        [SerializeField] internal GameObject anonOn;
        [SerializeField] internal GameObject anonOff;

        [SerializeField, UdonSynced] internal string doorMessage;
        [SerializeField,UdonSynced] internal bool hideUsers = false;
        [SerializeField, UdonSynced] internal bool dndMode = false;

        [SerializeField] internal InputField messageInput;
        [SerializeField] internal Button startCallButton;
        [SerializeField] internal GameObject LockButton_Object;
        [SerializeField] internal string format = "<nobr>[%USER%]</nobr><br>%MESSAGE%";
        
        [SerializeField] internal RawImage RawImage_intercomCall_PopupOutside;

        public void _ToggleLock()
        {
            door.pvtRoom._ToggleLock();
        }

        public void UpdateAnon()
        {
            anonOn.SetActive(hideUsers);
            anonOff.SetActive(!hideUsers);
            roomTrigger.__OnDeserialization();
        }

        internal override void _RingCall()
        {
            isRinging = true;
            _ShowRingDisplay();
        }

        private void _ShowRingDisplay()
        {
            if (!isRinging) return;
            if (dndMode || isTakingPhoto)
            {
                if (callWidget != null)
                    callWidget.SetActive(true);
                return;
            }
            intercomCamera.enabled = false;
            intercomDisplay_Outside.SetActive(false);
            intercomDisplay_Inside.SetActive(false);
            intercomCallDisplay.SetActive(true);
            defaultDisplay.SetActive(false);
            _PlaySound(door.pvtRoom.intercomRingLong);
            if (callWidget != null)
                callWidget.SetActive(false);
        }

        public override void OnDeserialization()
        {
            _SetDnd(dndMode);
            otherDisplay._SetDnd(dndMode);
            UpdateAnon();
            string __message = DisBridgeFunctions._TrimString(doorMessage, door.pvtRoom.messageLimit);
            otherDisplay.roomMessage.text = __message;
            roomMessage.text = __message;
            if (Networking.IsOwner(Networking.LocalPlayer, gameObject)) return;
            messageInput.text = __message;
        }
        
        private void _ToggleDnd()
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            dndMode = !dndMode;
            RequestSerialization();
            OnDeserialization();
            //if (Networking.LocalPlayer.IsOwner(gameObject)){}
        }
        private void _ToggleAnon()
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            hideUsers = !hideUsers;
            RequestSerialization();
            OnDeserialization();
            //if (Networking.LocalPlayer.IsOwner(gameObject)){}
        }

        private void _SetMessage()
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            string _prefix = format.Replace($"%USER%",Networking.LocalPlayer.displayName);
            _prefix = _prefix.Replace("%MESSAGE%",messageInput.text);
            doorMessage = _prefix;
            RequestSerialization();
            OnDeserialization();
        }

        //////////////////////////////////////////////////////////////////////////////
        public void N_Intercom_Answer()
        {
            if (callWidget != null)
                callWidget.SetActive(false);
            door.pvtRoom._AnswerCall();
        }
        public void _Intercom_Answer_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_Intercom_Answer));
        }
        //////////////////////////////////////////////////////////////////////////////
        public void N_Intercom_SilentDeny()
        {
            intercomCamera.enabled = false;
            src.Stop();
            intercomDisplay_Inside.SetActive(false);
            intercomCallDisplay.SetActive(false);
            defaultDisplay.SetActive(true);
            if (callWidget != null)
                callWidget.SetActive(true);
        }
        public void _Intercom_SilentDeny_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_Intercom_SilentDeny));
        }
        //////////////////////////////////////////////////////////////////////////////
        //public void N_ToggleDnd()
        //{
        //    _ToggleDnd();
        //}
        public void _ToggleDnd_Ui()
        {
            _ToggleDnd();
            //SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_ToggleDnd));
        }
        //////////////////////////////////////////////////////////////////////////////
        //public void N_ToggleAnon()
        //{
        //    _ToggleAnon();
        //}
        public void _ToggleAnon_Ui()
        {
            _ToggleAnon();
            //SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_ToggleAnon));
        }
        //////////////////////////////////////////////////////////////////////////////
        //public void N_SetMessage()
        //{
        //    _ToggleAnon();
        //}
        public void _SetMessage_Ui() { _SetMessage(); }
        //////////////////////////////////////////////////////////////////////////////
        public void N_CallOut()
        {
            if (photoCamera.enabled) return;
            door.pvtRoom._AnswerCall();
            door.pvtRoom.SendCustomEventDelayedSeconds(nameof(PrivateRoom._EndCall),60f);
        }
        public void _CallOut_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_CallOut));
        }
        //////////////////////////////////////////////////////////////////////////////
        public void N_UpdatePhoto()
        {
            if (photoCamera.enabled) return;
            photoCamera.enabled = true;
            defaultDisplay.SetActive(false);
            isTakingPhoto = true;
            _PlaySound(door.pvtRoom.intercomRingOnce);
            flash.SetActive(true);
            if (startCallButton != null) startCallButton.interactable = false;
            SendCustomEventDelayedSeconds(nameof(_FlashOff), .02f);
            SendCustomEventDelayedSeconds(nameof(_Photo_2), 1f);
            SendCustomEventDelayedSeconds(nameof(_Photo_1), 2f);
            SendCustomEventDelayedSeconds(nameof(_Photo_Update), 3f);
            
        }
        public void _UpdatePhoto_Ui()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(N_UpdatePhoto));
        }

        public void _FlashOff()
        {
            flash.SetActive(false);
        }

        public void _Photo_2()
        {
            _PlaySound(door.pvtRoom.intercomRingOnce);
            flash.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(_FlashOff), .02f);
        }
        public void _Photo_1()
        {
            _PlaySound(door.pvtRoom.intercomRingOnce);
            flash.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(_FlashOff), .02f);
        }
        public void _Photo_Update()
        {
            _PlaySound(door.pvtRoom.cameraClick);
            flash.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(_FlashOff), .02f);
            photoCamera.enabled = false;
            isTakingPhoto = false;
            defaultDisplay.SetActive(true);
            if (startCallButton != null) startCallButton.interactable = true;
            _ShowRingDisplay();
        }
        //////////////////////////////////////////////////////////////////////////////

        [SerializeField] internal Transform buttonParent;
        internal PrivateRoom_AccessButton[] accessButtons;

        private PrivateRoom_AccessButton owo;

        #region AccessList

        private void CheckButtonCount(int _target)
        {
            while (buttonParent.childCount < _target)
            {
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
                Instantiate(accessButtons[0].gameObject, buttonParent);
            }
            accessButtons = buttonParent.GetComponentsInChildren<PrivateRoom_AccessButton>();

        }
        

        #endregion
    }
}