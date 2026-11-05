
using System;
using JetBrains.Annotations;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GroupContainer : DisBridgePlugin
{
    [UdonSynced] internal string preferredOwner = "";
    private bool syncedPreferredOwner = false;

    [UdonSynced]internal bool inUse = false;
    private bool _inUse = false;

    public RoleContainer[] admins;
    public RoleContainer[] staff;
    private void Start()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            preferredOwner = Networking.LocalPlayer.displayName;
            RequestSerialization();
        }
    }

    [PublicAPI]
    public bool _IsInGroup(VRCPlayerApi __player)
    {
        if (!inUse) return false;
        if (_IsInGroup_Admin(__player)) return true;
        if (_IsInGroup_Staff(__player)) return true;
        if (_IsInGroup_Member(__player)) return true;
        return false;
    }
    [PublicAPI]
    public bool _IsInGroup_Admin(VRCPlayerApi __player)
    {
        if (!inUse) return false;
        foreach (var role in admins)
        {
            if (role.IsMember(__player)) return true;
        }
        return false;
    }
    [PublicAPI]
    public bool _IsInGroup_Staff(VRCPlayerApi __player)
    {
        if (!inUse) return false;
        foreach (var role in staff)
        {
            if (role.IsMember(__player)) return true;
        }
        return false;
    }
    [PublicAPI]
    public bool _IsInGroup_Member(VRCPlayerApi __player)
    {
        if (!inUse) return false;
        return _IsMemberInRoles(__player);
    }
    
    /// <summary>
    /// Checks if the user is the Preferred Owner.
    /// <br></br>This can return false if asked too soon. Network data needs to sync before this is updated.
    /// </summary>
    /// <param name="player">VRCPlayerAPI of the target player.</param>
    /// <returns>True/False depending on the user's access level.</returns>
    [PublicAPI]
    public bool _IsPreferredOwner(VRCPlayerApi __player)
    {
        if (preferredOwner == __player.displayName) return true;
        return false;
    }
    
    public override bool OnOwnershipRequest(VRCPlayerApi __requestingPlayer, VRCPlayerApi __requestedOwner)
    {
        return _IsPreferredOwner(__requestingPlayer);
    }
    public override void OnDeserialization()
    {
        if (!syncedPreferredOwner)
        {
            if (preferredOwner != String.Empty)
            {
                syncedPreferredOwner = true;
            }
        }

        if (inUse != _inUse)
        {
            _inUse = inUse;
            disBridge._SendGroupContainerChange(this);
        }
    }
}
