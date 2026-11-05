//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerMover : UdonSharpBehaviour
    {
        public MeshRenderer fadeCube;
        
        public string matProperty = "_FilterStrength";
        public string matProperty2 = "_FadeImageOpacity";
        public float internalFade = 1f;
        public float fadeLength = 1f;
        public bool startFade = false;

        public AnimationCurve fadeCurve_In;
        public AnimationCurve iconCurve_In;
        public AnimationCurve fadeCurve_Out;
        public AnimationCurve iconCurve_Out;

        public NoClip noClip; 

        [HideInInspector] public Material fadeMat;
        private float _internalLerp;

        private VRCPlayerApi localPlayer;
        private Transform location;
        private bool useRespawn = false;
        private bool isMoving = false;
        private void Start()
        {
            fadeCube.gameObject.SetActive(false);
            fadeMat = fadeCube.material;
            localPlayer = Networking.LocalPlayer;
        }

        public bool _MovePlayer(Transform _location, bool _forceMove = false)
        {
            if (isMoving && !_forceMove)
            {
                return false;
            }
            isMoving = true;
            location = _location;
            internalFade = 1f;
            fadeMat.SetFloat(matProperty,fadeCurve_In.Evaluate(internalFade));
            fadeMat.SetFloat(matProperty2,iconCurve_In.Evaluate(internalFade));
            fadeCube.gameObject.SetActive(true);
            startFade = true;
            _FadeStepIn();
            return true;
        }

        public bool _MovePlayer(bool _forceMove = false)
        {
            if (isMoving && !_forceMove)
            {
                return false;
            }
            isMoving = true;
            useRespawn = true;
            internalFade = 1f;
            fadeMat.SetFloat(matProperty,fadeCurve_In.Evaluate(internalFade));
            fadeMat.SetFloat(matProperty2,iconCurve_In.Evaluate(internalFade));
            fadeCube.gameObject.SetActive(true);
            startFade = true;
            _FadeStepIn();
            return true;
        }
        private void _MovePlayerMiddle()
        {
            if (useRespawn)
            {
                useRespawn = false;
                localPlayer.Respawn();
            }
            else
            {
                
                localPlayer.TeleportTo(location.position, location.rotation);
            }
            startFade = true;
            internalFade = 1f;
            fadeMat.SetFloat(matProperty,Mathf.Clamp01(fadeCurve_Out.Evaluate(internalFade)));
            fadeMat.SetFloat(matProperty2,Mathf.Clamp01(iconCurve_Out.Evaluate(internalFade)));
            _FadeStepOut();
            
        }
        public void _FadeStepOut()
        {
            fadeCube.transform.SetPositionAndRotation(VRCCameraSettings.ScreenCamera.Position,VRCCameraSettings.ScreenCamera.Rotation);

            if (noClip != null)
            {
                noClip._DisableNoclip();
            }
            
            if (startFade)
            {
                internalFade = Mathf.Clamp01(internalFade - (Time.deltaTime / fadeLength));
                fadeMat.SetFloat(matProperty,Mathf.Clamp01(fadeCurve_Out.Evaluate(internalFade)));
                fadeMat.SetFloat(matProperty2,Mathf.Clamp01(iconCurve_Out.Evaluate(internalFade)));
            }
            if (internalFade <= 0)
            {
                fadeCube.gameObject.SetActive(false);
                isMoving = false;
                return;
            }
            SendCustomEventDelayedFrames(nameof(_FadeStepOut),0);
        }
        public void _FadeStepIn()
        {
            fadeCube.transform.SetPositionAndRotation(VRCCameraSettings.ScreenCamera.Position,VRCCameraSettings.ScreenCamera.Rotation);
            if (startFade)
            {
                internalFade = Mathf.Clamp01(internalFade - (Time.deltaTime / fadeLength));
                fadeMat.SetFloat(matProperty,Mathf.Clamp01(fadeCurve_In.Evaluate(internalFade)));
                fadeMat.SetFloat(matProperty2,Mathf.Clamp01(iconCurve_In.Evaluate(internalFade)));
            }
            if (internalFade <= 0)
            {
                _MovePlayerMiddle();
                return;
            }
            SendCustomEventDelayedFrames(nameof(_FadeStepIn),0);
        }
    }
}