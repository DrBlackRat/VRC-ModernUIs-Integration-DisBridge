//Copyright (c) 2023 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.PosterSystem
{
    public class DisBridgePosterDisplayController : UdonSharpBehaviour
    {
        public DisBridgeImageLoader imageLoader;
        public Material[] posterDisplays;

        [SerializeField] private int _curPoster;

        [Header("Material Settings")] public string mainTexProperty = "_MainTex";
        public bool useEmission = true;
        public string emissionMapProperty = "_EmissionMap";

        [Header("Update Settings")] public float startupWait = 5f;
        public float batchWait;
        public int updateInterval;

        [Header("Duplicate Loading")] public bool noDuplicates = false;
        private Texture tmpMat;
        private int dupCheck = 0;

        void Start()
        {
            SendCustomEventDelayedSeconds(nameof(_DisplayUpdate), startupWait);
            foreach (var _poster in posterDisplays)
            {
                _poster.SetTexture(mainTexProperty, imageLoader.fallbackPoster.mainTexture);
                if (useEmission) _poster.SetTexture(emissionMapProperty, imageLoader.fallbackPoster.mainTexture);
            }
        }

        public void _DisplayUpdate()
        {
            //Debug.LogError($"DisBridgePosterDisplayController - _DisplayUpdate - {_curPoster}");

            if (!noDuplicates)
            {
                posterDisplays[_curPoster].SetTexture(mainTexProperty, imageLoader.GetActivePoster().mainTexture);
                if (useEmission)
                    posterDisplays[_curPoster]
                        .SetTexture(emissionMapProperty, imageLoader.GetActivePoster().mainTexture);
            }
            else
            {
                dupCheck = 0;
                DuplicateDisplay();
            }

            _curPoster++;

            if (_curPoster >= posterDisplays.Length)
            {
                //Debug.LogError($"DisBridgePosterDisplayController - _DisplayUpdate - resetting to 0");
                _curPoster = 0;
                SendCustomEventDelayedSeconds(nameof(_DisplayUpdate), batchWait);
            }
            else
            {
                //Debug.LogError($"DisBridgePosterDisplayController - _DisplayUpdate - adding 1");
                SendCustomEventDelayedFrames(nameof(_DisplayUpdate), updateInterval);
            }
        }

        private void DuplicateDisplay()
        {
            Debug.LogError($"DuplicateDisplay Start -- dupCheck:{dupCheck}");
            tmpMat = imageLoader.GetActivePoster().mainTexture;

            Debug.LogError($"{dupCheck} > {imageLoader.imageContainers.Length}");
            if (dupCheck > imageLoader.loadedPosters)
            {
                posterDisplays[_curPoster].SetTexture(mainTexProperty, imageLoader.fallbackPoster.mainTexture);
                if (useEmission)
                    posterDisplays[_curPoster].SetTexture(emissionMapProperty, imageLoader.fallbackPoster.mainTexture);
                return;
            }

            if (DuplicateCheck(tmpMat))
            {
                dupCheck++;
                DuplicateDisplay();
            }
            else
            {
                posterDisplays[_curPoster].SetTexture(mainTexProperty, tmpMat);
                if (useEmission) posterDisplays[_curPoster].SetTexture(emissionMapProperty, tmpMat);
            }
        }

        private bool DuplicateCheck(Texture _tex)
        {
            for (int i = 0; i < posterDisplays.Length; i++)
            {
                if (posterDisplays[i].mainTexture == _tex) return true;
            }

            return false;
        }
    }
}