//Copyright (c) 2024 UdonVR LLC - All Rights Reserved
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.PosterSystem
{
    public abstract class DisBridgeImageLoaderPlugin : UdonSharpBehaviour
    {
        [HideInInspector] public DisBridgeImageLoader imageLoader;
        [HideInInspector] public bool isDebug = false;

        public virtual void _UVR_PS_Init()
        {
            
        }
        public virtual void _UVR_PS_Update()
        {
            
        }
    }
}