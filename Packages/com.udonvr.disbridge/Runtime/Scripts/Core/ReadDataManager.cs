//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ReadDataManager : UdonSharpBehaviour
    {
        private string outputString;
        
        public PluginManager pluginManager;
        [SerializeField] private Base64Reader base64Reader;
        private GameObject tempDecoder;
        //[SerializeField] private Transform aPed;
        [SerializeField] private TMPro.TextMeshProUGUI testText;
        private int version;
        public bool fallbackMode = false;
        private bool emptyUrl = false;

        private bool requestInitRoleContainers = false;

        private int _initedRoleContainers = 0;
        private int _roleContainerCount = 0;
        
        private void Start()
        {
            emptyUrl = string.IsNullOrWhiteSpace(pluginManager._keyUrl[0].Get());
        #if UNITY_EDITOR || UNITY_EDITOR_64
            if (emptyUrl)
                fallbackMode = true;
        #endif

            //pluginManager.logWarning("ReadDataManager Start");
            if(base64Reader == null) base64Reader = gameObject.GetComponent<Base64Reader>();
            //pluginManager._key[0] = pluginManager._key[0].Trim();
            if (pluginManager._keyFragmentArrays == null || pluginManager._keyFragmentArrays.Length== 0)
            {
                fallbackMode = true;
            }
            else if(emptyUrl)
            {
                pluginManager.log("makeId - Start");
            }
            //pluginManager._Init2();
            DisableInteractive = true;

            if (!fallbackMode)
                SendCustomEventDelayedSeconds(nameof(StartDecoder), 5);
            else ReadIsDone();
        }

        public void StartDecoder()
        {
            if (fallbackMode) return;
            //pluginManager.log("[UdonVR]Starting Decode");
            DisableInteractive = true;
            if (string.IsNullOrWhiteSpace(pluginManager._keyUrl[0].Get()))
            {
            }
            else
            {
                //base64Reader.SetKey(pluginManager._key[0]); <= removed during Backend Transition
                //base64Reader.StartDownload(pluginManager._keyUrl[0]); <= removed during Backend Transition
            }
        }

        public void ReadIsDone()
        {
            //Destroy(tempDecoder);
            DisableInteractive = false;
            //pluginManager.log("[UdonVR] ReadIsDone");
            //pluginManager.log("[UdonVR] Output ["+outputString+"]");
            if (fallbackMode)
            {
                Fallback();
                return;
            }

            if (VRCJson.TryDeserializeFromJson(outputString, out DataToken result))
            {
                // Deserialization succeeded! Let's figure out what we've got.
                if (result.TokenType == TokenType.DataDictionary)
                {
                    pluginManager.log($"Successfully deserialized as a dictionary with {result.DataDictionary.Count} items.");
                    if (result.DataDictionary.TryGetValue("Mods", TokenType.DataDictionary, out DataToken mods))
                    {
                        pluginManager._InitMods(mods.DataDictionary);
                    }
                    else
                    {
                        pluginManager.log($"[Disbridge][mod]Failed to get mods dictionary - {mods}");
                    }
                    if (result.DataDictionary.TryGetValue("Roles",TokenType.DataList, out DataToken roles))
                    {
                        ParseSupporters(roles.DataList);
                    }
                    else
                    {
                        pluginManager.log($"[Disbridge][role]Failed to get roles list - {roles}");
                    }

                }
                else if (result.TokenType == TokenType.DataList)
                {
                    pluginManager.log($"Successfully deserialized as a list with {result.DataList.Count} items.");
                }
                else
                {
                    // This should not be possible. If TryDeserializeFromJson returns true, this it *must* be either a dictionary or a list.
                }
            }
            else
            {
                // Deserialization failed. Let's see what the error was.
                pluginManager.log($"Failed to Deserialize json- {result}");
                Fallback();
                return;
            }

        }

        private void ParseSupporters(DataList roles)
        {
            // Deserialization succeeded!
            pluginManager.log($"Successfully got roles as a list with {roles.Count} items.");
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles.TryGetValue(i, TokenType.DataDictionary, out DataToken role))
                {
                    pluginManager.log($"Successfully got roles[{i}]  as a dictionary with {role.DataDictionary.Count} items.");
                    var supId = role.DataDictionary["SuppRoleId"].ToString();
                    var mems = role.DataDictionary["Members"].DataList;
                    var roleContainer = pluginManager.GetRoleById(supId);
                    pluginManager.log($"SuppRoleId={supId} with {mems.Count} members.");
                    if (roleContainer != null)
                    {
                        //roleContainer._Init(mems, pluginManager, this); <= removed during Backend Transition
                    }
                    else
                        continue;
                }
                else
                {
                    pluginManager.log($"Failed to get roles[{i}] as a dictionary - {role}");
                    // This should not be possible. 
                }
            }

            //pluginManager._FinishInitRoleContainers(this); <= removed during Backend Transition
        }

        public void RoleContainerReady()
        {
            _initedRoleContainers++;
            InitPluginManager();
        }
        private void InitPluginManager()
        {
            if (!requestInitRoleContainers)
            {
                _roleContainerCount = pluginManager.roles.GetComponentsInChildren<RoleContainer>().Length;
                requestInitRoleContainers = true;
            }
            
            if (_initedRoleContainers >= _roleContainerCount)
            {
                //pluginManager._Init();
            }
        }

        private void Fallback()
        {
            pluginManager.logError("Using fallback");
            fallbackMode = true;
            foreach (var _role in pluginManager.roles.GetComponentsInChildren<RoleContainer>())
            {
                // _role._Init(null, pluginManager, this); <= removed during Backend Transition
            }
        }

        public void _ReadDone(string key, string currentOutputString)
        {
            if (version > 3)
            {
                outputString = currentOutputString;
                ReadIsDone();
                return;
            }

            Destroy(tempDecoder);
            outputString += currentOutputString;
            if (testText)
                testText.text = outputString;
            if (key.StartsWith( "fffff"))
            {
                ReadIsDone();
            }
        }

        public void PassVersion(int _version)
        {
            if (_version >= 50000)
            {
                fallbackMode = true;
                Fallback();
            }
            version = _version;
            pluginManager._CheckVersion(_version);
        }
    }
}