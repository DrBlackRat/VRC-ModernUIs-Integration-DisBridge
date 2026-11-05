//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace UdonVR.DisBridge.PosterSystem.Editors
{
    public class DisBridgeImageLoader_BuildTools : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -9999;
        
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }

        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
        //    bool buildPC = false;
        //#if UNITY_64
        //    buildPC = true;
        //#endif
                
            Debug.Log($"[DisBridge][ImageLoader][Build] Setting up URLs");
            
            DisBridgeImageLoader[] imageLoaders = GameObject.FindObjectsOfType<DisBridgeImageLoader>();
            string guildID;
            for (int i = 0; i < imageLoaders.Length; i++)
            {
                if (imageLoaders[i].pluginManager != null && imageLoaders[i].pluginManager._keyUrl != null && imageLoaders[i].pluginManager._keyUrl.Length > 0)
                {
                    guildID = imageLoaders[i].pluginManager._keyUrl[0].ToString().Replace("https://udonvr-db.github.io/DB/","");
                    guildID = guildID.Replace("https://udonvr-db.github.io/","");
                    guildID = guildID.Replace("/String","");
                    guildID = guildID.Replace("/Json","");
                    for (int j = 0; j < imageLoaders[i].urls.Length; j++)
                    {
                        if (imageLoaders[i].urlTypes[i] == 0) imageLoaders[i].urls[j] = new VRCUrl($"https://api.disbridge.com/api/serve/{guildID}/images/{imageLoaders[i].urlSlots[j]}.png");
                    }
                }
                else
                {
                    continue;
                }
                
            }
        }
    }
}