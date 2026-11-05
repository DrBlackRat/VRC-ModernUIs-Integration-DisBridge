//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoleBoard : UdonSharpBehaviour
    {
        [SerializeField] public PluginManager manager;

        [Range(0, 2)]
        [Tooltip("0 = use roleContainer\n1 = use Staff\n2 = use Supporters")]
        public int type = 0;
        [Space(5)]
        [Tooltip("The Role ID of the role you want to use.\nIf this is blank, the below Index will be used instead.\n\nIt is recommend to use the below `Role Container` index if you have a lot of Role Boards.")]
        public string roleID;
        [Tooltip("The index of the role you want to use.\n\nIt is recommend to use this index if you have a lot of Role Boards.")]
        public int roleContainer = 0;
        [Space(5)]
        [Tooltip("Custom title will be used instead of the role name or generic \"staff\" and \"Supporters\" titles.\n\nLeave this blank to not use a Custom Title")]
        public string customTitle = "";
        [Space(5)]
        [Tooltip("The Text Mesh Pro Component ued for the Title")]
        public TextMeshProUGUI roleTitle;
        [Tooltip("The Text Mesh Pro Component ued for the user list")]
        public TextMeshProUGUI roleList;
        [Space(5)]
        [Tooltip("This will be placed in-between names")]
        public string separator = "</color> | <#3498db>";

        [HideInInspector] public bool useID = false;
        [HideInInspector]public bool isDebug = false;

        private string oldTitle;
        private string oldNamelist;
        
        private void Start()
        {
            if (manager == null)
            {
                Debug.LogError($"{name}: No plugin manager found.");
                return;
            }
            manager.log($"Registering {nameof(RoleBoard)}:{gameObject.name}...");
            manager.AddPlugin(gameObject);
            oldNamelist = roleList.text;
            oldTitle = roleTitle.text;
        }
        
        //Runs when DisBridge finishes pulling the roles.
        public void _UVR_Init()
        {
            manager.log($"_Init {nameof(RoleBoard)}:{gameObject.name}...");
            string _roleList = "";
            string _roleTitle = "";
            
            switch (type)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(roleID))
                    {
                        RoleContainer _role = manager.GetRoleById(roleID);
                        if (_role == null)
                        {
                            manager.logError($"No role container found for role ID {roleID}");
                            return;
                        }
                        _roleList = String.Join(separator, _role.GetMembers());
                        _roleTitle = manager.GetRoleById(roleID).roleName;
                    }
                    else
                    {
                        RoleContainer _role = manager.GetRole(roleContainer);
                        if (_role == null)
                        {
                            manager.logError($"No role container found for role {roleContainer}");
                            return;
                        }
                        _roleList = String.Join(separator, _role.GetMembers());
                        _roleTitle = manager.GetRole(roleContainer).roleName;
                    }
                    break;
                case 1:
                    if (manager.GetStaffDisplaynames() != null) { _roleList = String.Join(separator, manager.GetStaffDisplaynames()); }
                    _roleTitle = "Staff";
                    break;
                case 2:
                    if (manager.GetSupporterDisplaynames() != null) { _roleList = String.Join(separator, manager.GetSupporterDisplaynames()); }
                    _roleTitle = "Supporters";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(customTitle)) _roleTitle = customTitle;
            if (roleList != null) roleList.text = oldNamelist.Replace("{roleList}",_roleList);
            if (roleTitle != null) roleTitle.text = oldTitle.Replace("{roleTitle}",_roleTitle);
        }

        public void _UVR_Update()
        {
            _UVR_Init();
        }
    }
}