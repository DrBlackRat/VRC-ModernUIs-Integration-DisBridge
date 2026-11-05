//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DisBridge_Spawn : DisBridgePlugin
    {
        public MeshRenderer fadeCube;
        
        public string matProperty = "_FilterStrength";
        public string matProperty2 = "_FadeImageOpacity";
        public float internalFade = 1f;
        public float fadeLength = 1f;
        public Transform[] spawns;
        public bool startFade = false;

        public AnimationCurve fadeCurve;
        public AnimationCurve iconCurve;

        private Material fadeMat;
        private float _internalLerp;
    
        private void Start()
        {
            fadeCube.gameObject.SetActive(true);
            fadeMat = fadeCube.material;
            fadeMat.SetFloat(matProperty,fadeCurve.Evaluate(internalFade));
            fadeMat.SetFloat(matProperty2,iconCurve.Evaluate(internalFade));
            disBridge.AddPlugin(gameObject);
            SendCustomEventDelayedFrames(nameof(_FadeStep),0);
        }
    
        public override void _UVR_Init()
        {
            startFade = true;
            for (int i = 0; i < _roleContainers.Length; i++)
            {
                if (spawns[i] == null) continue;
                if (_roleContainers[i] == null) continue;
                if (_roleContainers[i].IsMember()) Networking.LocalPlayer.TeleportTo(spawns[i].position,spawns[i].rotation);
            }
        }
    
        public void _FadeStep()
        {
            fadeCube.transform.SetPositionAndRotation(VRCCameraSettings.ScreenCamera.Position,VRCCameraSettings.ScreenCamera.Rotation);
            if (startFade)
            {
                //_internalLerp = _internalLerp + (Time.deltaTime / fadeLength);
                //internalFade = Mathf.Lerp(1,0,_internalLerp);
                internalFade = Mathf.Clamp01(internalFade - (Time.deltaTime / fadeLength));
                fadeMat.SetFloat(matProperty,Mathf.Clamp01(fadeCurve.Evaluate(internalFade)));
                fadeMat.SetFloat(matProperty2,Mathf.Clamp01(iconCurve.Evaluate(internalFade)));
            }
            //Debug.Log($"[Spawn] internalFade {internalFade}");
            if (internalFade <= 0)
            {
                //Debug.Log($"[Spawn] internalFade return");
                fadeCube.gameObject.SetActive(false);
                return;
            }
            SendCustomEventDelayedFrames(nameof(_FadeStep),0);
        }
    }
}