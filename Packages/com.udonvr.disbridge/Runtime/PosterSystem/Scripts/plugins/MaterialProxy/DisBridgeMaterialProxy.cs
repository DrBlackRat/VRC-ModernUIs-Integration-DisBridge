//Copyright (c) 2024 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.PosterSystem
{
    public class DisBridgeMaterialProxy : DisBridgeImageLoaderPlugin
    {
        //[HideInInspector]public bool useFallback;
        public int[] materialIndex;
        public Material[] materials;
        public bool[] hasPulled;
        public string[] keywords;
        public Texture[] fallbackImages;

        public void Start()
        {
            imageLoader.AddPlugin(gameObject);
            //if (!useFallback) return;
            //Texture fallback = imageLoader.fallbackPosterTexture;
            for (int i = 0; i < materialIndex.Length; i++)
            {
                foreach (var keyword in keywords)
                {
                    materials[i].SetTexture(keyword, fallbackImages[i]);
                }
            }
        }

        public override void _UVR_PS_Update()
        {
            for (int i = 0; i < materialIndex.Length; i++)
            {
                if (!hasPulled[i])
                {
                    if (imageLoader.imageContainers[materialIndex[i]].hasDownloaded)
                    {
                        foreach (var keyword in keywords)
                        {
                            materials[i].SetTexture(keyword,imageLoader.imageContainers[materialIndex[i]].mat.mainTexture);
                        }
                    }
                }
            }
        }
    }
}

