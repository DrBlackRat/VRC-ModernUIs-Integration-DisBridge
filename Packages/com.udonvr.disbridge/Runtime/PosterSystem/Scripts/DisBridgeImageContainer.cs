//Copyright (c) 2024 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonVR.DisBridge.PosterSystem
{
    public class DisBridgeImageContainer : UdonSharpBehaviour
    {
        public DisBridgeImageLoader loader;

        public MeshRenderer renderer;

        public Material mat;
        public VRCUrl imageUrl;
        public TextureInfo textureinfo;

        private VRCImageDownloader vRCImageDownloader;

        private bool _hasDownloaded = false;

        public bool hasDownloaded => _hasDownloaded;


        public GameObject updatingText;


        public void Initialize()
        {
            mat = renderer.material;
            StartDownload();
        }

        public void StartDownload()
        {
            Debug.Log($"DisBridgeImageContainer - StartDownload on {gameObject.name}");
            if (vRCImageDownloader != null)
            {
                if (updatingText != null)
                    updatingText.SetActive(true);
            }
            else
            {
                vRCImageDownloader = new VRCImageDownloader();
                mat.mainTexture = loader.loading.mainTexture;
            }

            //TODO set URL with editor script
            imageUrl = loader.urls[transform.GetSiblingIndex()];
            vRCImageDownloader.DownloadImage(imageUrl, mat, (IUdonEventReceiver)this, textureinfo);
        }

        public void RetryDownload()
        {
            if (_hasDownloaded == false) StartDownload();
        }

        private string err;

        public override void OnImageLoadError(IVRCImageDownload result)
        {
            _hasDownloaded = false;
            if (updatingText != null)
                updatingText.SetActive(false);
            switch (result.Error)
            {
                case VRCImageDownloadError.Unknown:
                    err = "Unknown";
                    renderer.material = loader.unknown;
                    break;
                case VRCImageDownloadError.AccessDenied:
                    err = "AccessDenied";
                    renderer.material = loader.accessDenied;
                    break;
                case VRCImageDownloadError.DownloadError:
                    err = "DownloadError";
                    renderer.material = loader.downloadError;
                    break;
                case VRCImageDownloadError.InvalidImage:
                    err = "InvalidImage";
                    renderer.material = loader.invalidImage;
                    break;
                case VRCImageDownloadError.InvalidURL:
                    err = "InvalidURL";
                    renderer.material = loader.invalidURL;
                    break;
            }

            Debug.LogError($"DisBridgeImageContainer on {gameObject.name} - {err} - {result.ErrorMessage}");
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            Debug.Log($"ImageLoad Success on {gameObject.name}");
            _hasDownloaded = true;
            renderer.material = mat;
            loader.textures[transform.GetSiblingIndex()] = mat.mainTexture;
            loader.loadedPosters++;
            loader.ImagesUpdated();
            if (updatingText != null)
                updatingText.SetActive(false);
        }

        public bool TryGetMaterial(out Material _mat)
        {
            if (_hasDownloaded)
            {
                _mat = mat;
                return true;
            }

            _mat = null;
            return false;
        }

        /*
        public override void Interact()
        {
            vRCImageDownloader.Dispose();
            vRCImageDownloader = new VRCImageDownloader();
            vRCImageDownloader.DownloadImage(imageUrl, mat, (IUdonEventReceiver)this, textureinfo);
            if (updatingText != null)
                updatingText.SetActive(true);
        }
        */
    }
}