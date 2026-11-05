
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessGate_Trigger : PlayerTracker_ColliderClass
    {
        public AccessGate accessGate;
        public Transform playerMoverTransform;
        public bool isUserInside = false;

        public override void _OnPlayerTriggerEnter(VRCPlayerApi _player)
        {
            if (!_player.isLocal) return;
            if (!accessGate._IsMember(_player))
            {
                accessGate._MovePlayer(playerMoverTransform);
            }
            isUserInside = true;
        }

        public override void _OnPlayerTriggerExit(VRCPlayerApi _player)
        {
            if (!_player.isLocal) return;
            isUserInside = false;
        }

        public bool _TryRejectPlayer()
        {
            if (isUserInside)
            {
                accessGate._MovePlayer(playerMoverTransform);
                return true;
            }
            return false;
        }
    }
}