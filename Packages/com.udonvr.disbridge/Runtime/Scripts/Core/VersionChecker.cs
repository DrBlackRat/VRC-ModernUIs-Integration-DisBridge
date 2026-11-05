//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge
{
    public class VersionChecker : UdonSharpBehaviour
    {
        
        private VRCUrl _apiUrl = new VRCUrl("https://api.github.com/repos/UdonVR/DisBridge/releases");

        private bool isRunning = false;
        private bool hasRan = false;
        
        public string localVersion;
        public string remoteVersion;
        public GameObject updateAvailable;
        public TextMeshProUGUI versionText;
        
        public void _StartCheck()
        {
            if (isRunning || hasRan) return;
            isRunning = true;
            VRCStringDownloader.LoadUrl(_apiUrl, (IUdonEventReceiver)this);
        }
        private void __VersionCheck()
        {
            switch (DisBridgeFunctions.CompareSemVer(localVersion,remoteVersion))
            {
                case -1:
                    //Debug.Log($"[VersionChecker] Local version is behind remote version. Local: {localVersion} | Remote: {remoteVersion}");
                    versionText.text = $"Version: {localVersion}<br>Update Available! [{remoteVersion}]";
                    updateAvailable.SetActive(true);
                    break;
                case 0:
                    //Debug.Log($"[VersionChecker] Local version is up to date. Local: {localVersion} | Remote: {remoteVersion}");
                    updateAvailable.SetActive(false);
                    //good
                    break;
                case 1:
                    //Debug.Log($"[VersionChecker] Local version is ahead of remote version. Local: {localVersion} | Remote: {remoteVersion}");
                    updateAvailable.SetActive(false);
                    //
                    break;
                default:
                    break;
            }
        }
        
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            Debug.Log("[VersionChecker] Received Github API Data");
            Debug.Log($"[VersionChecker] Result\n{result.Result}");
            if (!(VRCJson.TryDeserializeFromJson(result.Result, out DataToken uwu)))
            {
                //Debug.LogError("[VoteButton] Parse Fail_1");
                isRunning = false;
                return;
            }
            if (!__GetTag(ref uwu))
            {
                //Debug.LogError("[VoteButton] Parse Fail_2");
                isRunning = false;
                return;
            }
            __VersionCheck();
            isRunning = false;
            hasRan = true;
        }
        
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.Log("[VersionChecker] OnStringLoadError");
            isRunning = false;
        }

        private bool __GetTag(ref DataToken _token)
        {
            //Debug.Log($"[VersionChecker] Got [{_token.DataList.Count}] releases");
            for (int i = 0; i < _token.DataList.Count; i++)
            {
                //Debug.Log($"[VersionChecker] Checking release [{i}]");
                if (_token.DataList[i].DataDictionary.TryGetValue("prerelease",out DataToken _prerelease))
                {
                    if (_prerelease.Boolean == false)
                    {
                        //Debug.Log($"[VersionChecker][{i}] prerelease is [true]");
                        if (_token.DataList[i].DataDictionary.TryGetValue("tag_name", out DataToken _tag_name))
                        {
                            //Debug.Log($"[VersionChecker][{i}] Received [tag_name][{_tag_name.String}]");
                            remoteVersion = _tag_name.String;
                            return true;
                        }
                        else
                        {
                            //Debug.Log($"[VersionChecker][{i}] Failed getting [tag_name]");
                            return false;
                        }
                    }
                    else
                    {
                        //Debug.Log($"[VersionChecker][{i}] prerelease is [false]");
                        return false;
                    }
                }
                else
                {
                    //Debug.Log($"[VersionChecker][{i}] Failed getting [prerelease]");
                    return false;
                }
            }
            return false;
        }
        
    }
}