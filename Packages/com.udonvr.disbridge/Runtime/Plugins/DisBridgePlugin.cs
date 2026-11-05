//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonVR.DisBridge
{
    public abstract class DisBridgePlugin : UdonSharpBehaviour
    {
        // variables
        [Tooltip("The DisBridge Plugin Manager.")]
        public PluginManager disBridge;
        [HideInInspector] public bool isDebug = false;
        [HideInInspector] public string[] roleContainerIDs; //only used to build the array for editor script in case role changes were made
        public RoleContainer[] _roleContainers;
        public PermissionGroup[] _permissionGroups;
        public bool useStaff;
        public bool useSupporters;
        public bool useBoosters;

        /// <summary>
        /// Called when DisBridge starts
        /// </summary>
        public virtual void _UVR_Init() { }

        /// <summary>
        /// Called when DisBridge receives a permission update 
        /// </summary>
        public virtual void _UVR_Update() { }

        /// <summary>
        /// Called when a player joins the world and is a member of any role.
        /// </summary>
        /// <param name="_player"></param>
        public virtual void _UVR_UserJoined(VRCPlayerApi _player) { }

        /// <summary>
        /// Called when a player leaves the world and is a member of any role.
        /// </summary>
        /// <param name="_player"></param>
        public virtual void _UVR_UserLeft(VRCPlayerApi _player) { }

        /// <summary>
        /// Gets called when the version of the DisBridge Bot doesn't match the version of DisBridge inside of Unity. 
        /// </summary>
        public virtual void _UVR_VersionMismatch() { }
        
        /// <summary>
        /// Gets called when a Group Container gets enabled or disabled.
        /// </summary>
        /// <param name="_group">The Group Container that changed.</param>
        public virtual void _UVR_GroupUpdate(GroupContainer _group) { }

        /// <summary>
        /// Gets the highest RoleContainer of the player from this plugin's RoleContainer list.
        /// </summary>
        /// <param name="_player">The target player.</param>
        /// <param name="_roleContainer">The parameter used to return the player's RoleContainer.</param>
        /// <out>
        /// <b>RoleContainer</b> – The highest RoleContainer the player has.<br></br>
        /// This will be <b>null</b> if the player does not have a RoleContainer.
        /// </out>
        /// <returns>
        /// <b>true</b> – The player has a RoleContainer.<br></br>
        /// <b>false</b> – The player does not have any RoleContainers.
        /// </returns>
        public bool _TryGetPlayerRoleContainer(VRCPlayerApi _player, out RoleContainer _roleContainer)
        {
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (_roleContainers[i].IsMember(_player))
                {
                    _roleContainer = _roleContainers[i];
                    return true;
                }
            }
            _roleContainer = null;
            return false;
        }
        /// <summary>
        /// Gets all the RoleContainers of the player from this plugin's RoleContainer list.
        /// </summary>
        /// <param name="_player">The target player.</param>
        /// <ref>
        /// <b>_playersRoles[]</b> – The array used to return the player's RoleContainers.<br></br>
        /// This will be <b>null</b> if the player does not have any RoleContainers.
        /// </ref>
        /// <returns>
        /// <b>true</b> – The player has a RoleContainer.<br></br>
        /// <b>false</b> – The player does not have any RoleContainers.
        /// </returns>
        public bool _TryGetPlayerRoleContainer(VRCPlayerApi _player, ref RoleContainer[] _playersRoles)
        {
            int _i2 = 0;
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (_roleContainers[i].IsMember(_player))
                {
                    _i2++;
                }
            }

            if (_i2 == 0)
            {
                _playersRoles = null;
                return false;
            }
            
            _playersRoles = new RoleContainer[_i2];
            int k = 0;
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (_roleContainers[i].IsMember(_player))
                {
                    _playersRoles[k] = _roleContainers[i];
                    k++;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// returns whether or not the player is in a RoleContainer from this plugin's RoleContainer list,
        /// Or if the player is inside a generic staff or supporter role if allowed.
        /// </summary>
        /// <param name="_player">The target player.</param>
        /// <returns>
        /// <b>true</b> – The player has a RoleContainer.<br></br>
        /// <b>false</b> – The player does not have any RoleContainers.
        /// </returns>
        public bool _IsMemberInRoles(VRCPlayerApi _player, bool unlockIfNull = true)
        {
            disBridge.log($"_IsMemberInRoles on {gameObject.name}");

            if (unlockIfNull && !useStaff && !useSupporters && (_roleContainers == null || _roleContainers.Length == 0))
            {
                return true;
            }
            
            if (useStaff && disBridge.IsStaff(_player)) { return true; }
            if (useSupporters && disBridge.IsSupporter(_player)) { return true; }
            if (useBoosters && disBridge.IsBooster(_player)) { return true; }
            foreach (var _role in _roleContainers)
            {
                if (_role != null)
                    if (_role.IsMember(_player)) return true;
            }
            
            if (Utilities.IsValid(_permissionGroups))
            {
                foreach (var _group in _permissionGroups)
                {
                    if (Utilities.IsValid(_group)) if (_group._IsMember(_player)) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// returns whether or not the player is in a RoleContainer from this plugin's RoleContainer list,
        /// Or if the player is inside a generic staff or supporter role if allowed.
        /// <br></br><br></br> Use <b>_IsMemberInRoles(VRCPlayerApi, bool)</b> unless you are required to do use a DisplayName.
        /// <br></br> Using a <b>VRCPlayerApi</b> is more efficient due to not having to search for a VRCPlayerApi.
        /// </summary>
        /// <param name="_player">The target player.</param>
        /// <returns>
        /// <b>true</b> – The player has a RoleContainer.<br></br>
        /// <b>false</b> – The player does not have any RoleContainers.
        /// </returns>
        public bool _IsMemberInRoles(string _player, bool unlockIfNull = true)
        {
            if (unlockIfNull && !useStaff && !useSupporters && (_roleContainers == null || _roleContainers.Length == 0))
            {
                return true;
            }
            if (useStaff && disBridge.IsStaff(_player)) { return true; }
            if (useSupporters && disBridge.IsSupporter(_player)) { return true; }
            if (useBoosters && disBridge.IsBooster(_player)) { return true; }
            foreach (var _role in _roleContainers)
            {
                if (_role != null)
                    if (_role.IsMember(_player)) return true;
            }
            
            if (Utilities.IsValid(_permissionGroups))
            {
                foreach (var _group in _permissionGroups)
                {
                    if (Utilities.IsValid(_group)) if (_group._IsMember(_player)) return true;
                }
            }
            return false;
        }
    }
}