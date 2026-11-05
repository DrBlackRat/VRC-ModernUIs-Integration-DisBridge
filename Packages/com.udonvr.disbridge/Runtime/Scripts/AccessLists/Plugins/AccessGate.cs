
using UdonSharp;
using UdonVR.DisBridge.Plugins;
using UdonVr.DisBridge.Utils;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessGate : UserAccessList_Class
    {
        public PlayerMover playerMover;
        public NotificationSystem notificationSystem;

        public Transform playerMoverTransform;

        public GameObject[] object_TurnOff;
        public GameObject[] object_TurnOn;

        public Animator[] animators;
        public string[] animatorBools;

        private bool internalState = false;

        public AccessGate_Trigger[] myTriggers;

        public bool _IsMember(VRCPlayerApi _player)
        {
            if (userAccessList._IsAdmin(_player))
            {
                return true;
            }
            if (userAccessList._IsMember(_player)) return true;
            return false;
        }

        public void _MovePlayer(Transform _movePointOverride = null)
        {
            playerMover._MovePlayer(_movePointOverride == null ? playerMoverTransform : _movePointOverride);
            if (Utilities.IsValid(notificationSystem))
                notificationSystem._RegisterNotification("Entry <color=#CD5C5C>Denied",
                    "You do not have permission to be here.", $"Access Gate [{userAccessList.accessListName}]",
                    notificationSystem.failSound);
        }

        public void _UpdateDoorState(bool _hasAccess)
        {
            if (internalState == _hasAccess) return;
            internalState =  _hasAccess;
            
            foreach (var wall in object_TurnOff)
            {
                if (Utilities.IsValid(wall)) wall.SetActive(_hasAccess);
            }
            foreach (var wall in object_TurnOn)
            {
                if (Utilities.IsValid(wall)) wall.SetActive(!_hasAccess);
            }

            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].SetBool(animatorBools[i], _hasAccess);
            }
        }

        public override void _ReceivePermissionChange()
        {
            bool hasAccess = _IsMember(Networking.LocalPlayer);
            _UpdateDoorState(hasAccess);
            if (!hasAccess)
                // User does not have access, see if they need to be rejected from triggers
            {
                for (int i = 0; i < myTriggers.Length; i++)
                {
                    if (myTriggers[i]._TryRejectPlayer())
                    {
                        // when true, user got rejected from the trigger.
                        // stop looking though triggers to reject the player from. they already got rejected.
                        return;
                    }
                }
            }
        }
    }
}