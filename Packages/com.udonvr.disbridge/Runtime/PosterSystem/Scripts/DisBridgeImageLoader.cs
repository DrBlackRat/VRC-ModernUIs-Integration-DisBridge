//Copyright (c) 2023 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.PosterSystem
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DisBridgeImageLoader : UdonSharpBehaviour
    {
        [HideInInspector]public DisBridge.PluginManager pluginManager;
        [HideInInspector]public int[] urlTypes;
        [HideInInspector]public int[] urlSlots;
        [HideInInspector]public string[] urlCustoms;


        [HideInInspector]public Material loading;
        [HideInInspector]public Material unknown;
        [HideInInspector]public Material accessDenied;
        [HideInInspector]public Material downloadError;
        [HideInInspector]public Material invalidImage;
        [HideInInspector]public Material invalidURL;

        public DisBridgeImageContainer[] imageContainers;
        public int loadedPosters = 0;
        private int[] randomPosters;

        public Texture[] textures;

        [HideInInspector]public Material fallbackPoster;
        [HideInInspector]public Texture fallbackPosterTexture;
        private Material _tmpMaterial;

        [HideInInspector]public VRCUrl[] urls;
        
        [HideInInspector]public GameObject[] plugins;
        
        private bool hasPluginInit = false;
        private UdonBehaviour _tmp_UdonBehaviour;
        private string _tmp_String;
        private int _imagecontainer = 0;

        public bool isDebug = false;
        
        
        void Start()
        {
            textures = new Texture[urls.Length];
            fallbackPosterTexture = fallbackPoster.mainTexture;
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = fallbackPosterTexture;
            }
            
            
            InitImageContainers();

            InitPlugins();
        }

        public void InitImageContainers()
        {
            imageContainers = transform.GetComponentsInChildren<DisBridgeImageContainer>();
            imageContainers[_imagecontainer].Initialize();
            _imagecontainer++;
            
            randomPosters = new int[imageContainers.Length];
            for (int i = 0; i < randomPosters.Length; i++)
            {
                randomPosters[i] = i;
            }
            
            if (!(_imagecontainer >= imageContainers.Length))
            {
                SendCustomEventDelayedSeconds(nameof(InitImageContainers),5f);
            }
        }

        public void RetryFailed()
        {
            foreach (var poster in imageContainers)
            {
                if (poster != null)
                    poster.RetryDownload();
            }
        }

        public void ForceUpdate()
        {
            foreach (var poster in imageContainers)
            {
                if (poster != null)
                    poster.StartDownload();
            }
        }

        public void ImagesUpdated() => UpdatePlugins();

        public Material GetActivePoster(bool _returnFallback = true)
        {
            VRC.SDKBase.Utilities.ShuffleArray(randomPosters);
            for (int i = 0; i < imageContainers.Length; i++)
            {
                if (imageContainers[randomPosters[i]].TryGetMaterial(out _tmpMaterial))
                {
                    return _tmpMaterial;
                }
            }

            if (_returnFallback)
            {
                return fallbackPoster;
            }

            return null;
        }

        //////////////////////////////////////////////////////////////////
        #region Plugins

        
        /// <summary>
        /// Registers your plugin into the PluginManager for Initialization. Should be used with the local GameObject `gameObject`.
        /// </summary>
        /// <param name="_plugin">This should be the GameObject that your script is attached to. this is just `gameObject` in most cases.</param>
        public void AddPlugin(GameObject _plugin)
        {
            plugins = DisBridgeFunctions.AddToArray(_plugin, plugins);
            if (hasPluginInit)
            {
                _tmp_UdonBehaviour = plugins[plugins.Length - 1].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_PS_Init");
                _tmp_String = $"{_tmp_String}\n[{plugins.Length - 1}]{_tmp_UdonBehaviour.gameObject.name}";
            }
        }
        
        private void InitPlugins()
        {
            //Debug.LogError("DisBridgeImageLoader - InitPlugins");
            if (plugins == null || plugins.Length == 0)
            {
                //Debug.LogError("NULL NULL NULL");
                hasPluginInit = true;
                return;
            }
            //Debug.LogError(plugins.Length);
            _tmp_String = "";
            for (int i = 0; i < plugins.Length; i++)
            {
                //Debug.LogError(plugins[i].name);
                _tmp_UdonBehaviour = plugins[i].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_PS_Init");
                _tmp_String = $"{_tmp_String}\n[{i}]{_tmp_UdonBehaviour.gameObject.name}";
                
            }
            hasPluginInit = true;
        }
        
        private void UpdatePlugins()
        {
            //Debug.LogError("DisBridgeImageLoader - UpdatePlugins - Started");
            if (!hasPluginInit) return;
            if (plugins == null || plugins.Length == 0)
            {
                return;
            }
            
            _tmp_String = "";
            for (int i = 0; i < plugins.Length; i++)
            {
                //Debug.LogError($"DisBridgeImageLoader - Loop - {i}");
                _tmp_UdonBehaviour = plugins[i].GetComponent<UdonBehaviour>();
                _tmp_UdonBehaviour.SendCustomEvent("_UVR_PS_Update");
                _tmp_String = $"{_tmp_String}\n[{i}]{_tmp_UdonBehaviour.gameObject.name}";
            }
            //Debug.LogError("DisBridgeImageLoader - UpdatePlugins - Ended");
        }
        
        #endregion 
        //////////////////////////////////////////////////////////////////
        #region Editor Fuctions

        public void SyncUrlWithType()
        {
            if (urlTypes.Length != urls.Length)
            {
                string[] tmp = new string[urls.Length];
                

            }
        }

        #endregion
    }
}
