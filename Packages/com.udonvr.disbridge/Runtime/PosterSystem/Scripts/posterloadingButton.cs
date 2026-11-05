//Copyright (c) 2023 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.PosterSystem
{
    public class posterloadingButton : UdonSharpBehaviour
    {
        [Range(0, 1)] [Tooltip("1 - Retry failed downloads.\n2 - Force download all posters.")]
        public int mode = 0;

        public DisBridgeImageLoader loader;

        public override void Interact()
        {
            switch (mode)
            {
                case 0:
                    loader.RetryFailed();
                    break;
                case 1:
                    loader.ForceUpdate();
                    break;
            }

        }
    }
}