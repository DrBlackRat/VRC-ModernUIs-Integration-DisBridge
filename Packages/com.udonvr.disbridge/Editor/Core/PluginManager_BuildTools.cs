//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UdonSharp;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.Udon;

namespace UdonVR.DisBridge.Editors
{
    public class PluginManager_BuildTools : IVRCSDKBuildRequestedCallback
    {
        private static string logprefix = "[DisBridge Build Tools]";
    
        public int callbackOrder => -9999;
    
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }
    
        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
            Debug.Log($"{logprefix} Starting");
            
            bool buildPC = false;
        #if UNITY_64
            buildPC = true;
        #endif
            Debug.Log($"{logprefix} is PC {buildPC}");
            
            PluginManager[] disBridge = GameObject.FindObjectsOfType<PluginManager>();

            if (disBridge == null || disBridge.Length == 0)
            {
                Debug.Log($"{logprefix} No PluginManagers found.");
                return;
            }
            
            Debug.Log($"{logprefix} [{disBridge.Length}] PluginManagers found.");

            for (int i = 0; i < disBridge.Length; i++)
            {
                Debug.Log($"{logprefix} Initializing PluginManager: [{i}].");
                InitPluginManager(disBridge[i]);
            }
        }

        private static void InitPluginManager(PluginManager _manager)
        {
            RoleContainer[] __roles = _manager.roles.GetComponentsInChildren<RoleContainer>();
            _manager.backend = _manager.transform.GetComponentInChildren<DisBridgeBackend>();
            _manager.backend.disBridge = _manager;
            _manager._roles = __roles;
            _manager._roleIds = new string[_manager._roles.Length];
            _manager._rolesDictionary = new DataDictionary();
            Debug.Log($"{logprefix} manager has [{_manager._roles.Length}] roles");
            for(int i = 0; i< _manager._roles.Length; i++)
            {
                Debug.Log($"{logprefix} rIndex {i}");
                if (string.IsNullOrWhiteSpace(_manager._roles[i].roleId))
                {
                    Debug.LogError($"{logprefix} Blank RoleID, replacing with index. role: {_manager._roles[i].name}");
                    _manager._roles[i].roleId = $"{i}_blank";
                }
                _manager._roles[i].disBridge = _manager;
                _manager._rolesDictionary.Add(_manager._roles[i].roleId, _manager._roles[i]);
                _manager._roleIds[i] = _manager._roles[i].roleId;
                Debug.Log($"{logprefix} rID[{i}]({_manager._roles[i].name}) = ({_manager._roles[i].roleId})");


                _manager._roles[i]._membersList_man = new DataList();
                _manager._roles[i]._membersList = new DataList();
                
                _manager._roles[i].members = new string[_manager._roles[i].manualMembers.Length];
                for (int j = 0; j < _manager._roles[i].manualMembers.Length; j++)
                {
                    string _name = _manager._roles[i].manualMembers[j];
                    _manager._roles[i]._membersList_man.Add(_name);
                    _manager._roles[i]._membersList.Add(_name);
                    _manager._roles[i].members[j] = _name;
                }

                if (_manager._roles[i].IsRoleStaff())
                {
                    ArrayUtility.Add(ref _manager.staffRoles,_manager._roles[i]);
                }
                if (_manager._roles[i].IsRoleSupporter())
                {
                    ArrayUtility.Add(ref _manager.supporterRoles,_manager._roles[i]);
                }

                if (_manager._roles[i].IsRoleBooster())
                {
                    _manager.boosterRole = _manager._roles[i];
                }
            }

            _manager.BuildStaffAndSupporterArrays();
            _manager.BuildBoosterRole();
            
            RunSplitKeyString(_manager);
            PackageInfo packageInfo = JsonConvert.DeserializeObject<PackageInfo>(System.IO.File.ReadAllText("Packages/com.udonvr.disbridge/package.json"));
            VersionChecker versionChecker = _manager.backend.versionChecker;
            if (versionChecker != null)
            {
                versionChecker.localVersion = packageInfo.version;
                versionChecker.versionText.text = $"Version: {packageInfo.version}";
            }
        }

        private static void RunSplitKeyString(PluginManager _manager)
        {
            List<string[]> temp = new(); 
            for (int i = 0; i < _manager._key.Length; i++)
            {
                Debug.Log(_manager._key[i]);
                temp.Add(SplitKeyString(_manager._key[i].Trim()));
            }

            _manager._keyFragmentArrays = temp.ToArray();
            _manager._key = Array.Empty<string>();
        }
        private static string[] SplitKeyString(string key, int chunkSize = 8) // unsure how long keys are however 8 seems to be a good number.
        {
            List<string> chunks = new List<string>();
            for (int i = 0; i < key.Length; i += chunkSize)
            {
                string chunk = key.Substring(i, Math.Min(chunkSize, key.Length - i));
                chunks.Add(chunk);
            }
            return chunks.ToArray();
        }
    }
}