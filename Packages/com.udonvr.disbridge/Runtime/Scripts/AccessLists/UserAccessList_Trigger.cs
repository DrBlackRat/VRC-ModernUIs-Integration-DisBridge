
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UserAccessList_Trigger : PlayerTracker_ColliderClass
{
    public UserAccessList_Controller[] userAccessLists;
    public override void _OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        foreach (var accessList in userAccessLists)
        {
            accessList._Menu_Open();
        }
    }

    public override void _OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        foreach (var accessList in userAccessLists)
        {
            accessList._Menu_Close();
        }
    }
}
