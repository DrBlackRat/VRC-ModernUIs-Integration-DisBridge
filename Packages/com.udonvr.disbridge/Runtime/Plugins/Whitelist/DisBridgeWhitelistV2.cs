//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DisBridgeWhitelistV2 : DisBridgeWhitelist_Class
    {
        [Tooltip("Defines if the whitelist is global toggleable or not.")]
        [SerializeField] internal bool autoUnlock = true;
        [SerializeField] internal bool isGlobal = false;
        [UdonSynced] private bool isUnlocked = false;
        
        public bool[] TargetsDefaultOff_EnforceStart;
        public bool[] TargetsDefaultOn_EnforceStart;
        
        [SerializeField] internal bool alwaysEnforceLockState = false;
        [SerializeField] internal bool neverEnforceLockState = false;
        
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private string animatorValue;

        [SerializeField] internal bool canMasterUnlock = false;
        [SerializeField] internal bool canOwnerUnlock = false;

        private void Start()
        {
            disBridge.AddPlugin(gameObject);
            if (neverEnforceLockState) return;

            for (int i = 0; i < TargetsDefaultOn.Length; i++)
            {
                if (TargetsDefaultOn[i] != null)
                {
                    TargetsDefaultOn[i].SetActive(alwaysEnforceLockState || (TargetsDefaultOn_EnforceStart[i] || TargetsDefaultOn[i].activeSelf));
                }
            }

            for (int i = 0; i < TargetsDefaultOff.Length; i++)
            {
                if (TargetsDefaultOff[i] != null)
                {
                    TargetsDefaultOff[i].SetActive(!alwaysEnforceLockState && (!TargetsDefaultOff_EnforceStart[i] && TargetsDefaultOff[i].activeSelf));
                }
            }
        }

        public override void _UVR_Init()
        {
            if (!autoUnlock) return;
            if (UnlockCheck()) UnlockWhitelist();
        }

        public override void _UVR_Update()
        {
            _UVR_Init();
        }

        public override void _ForceUnlock(string _code)
        {
            if (_code == gameObject.name)
                UnlockWhitelist();
        }

        public override void _ForceLock(string _code)
        {
            if (_code == gameObject.name)
                LockWhitelist();
        }

        private bool UnlockCheck()
        {
            if (canOwnerUnlock && Networking.IsInstanceOwner) return true;
            if (canMasterUnlock && Networking.IsMaster) return true;
            return _IsMemberInRoles(Networking.LocalPlayer, false);
        }
        
        private void UnlockWhitelist()
        {
            if (keypads != null && keypads.Length != 0)
            {
                foreach (var _keypad in keypads)
                {
                    _keypad.SetUnlock(true);
                }
            }
            foreach (GameObject _obj in TargetsDefaultOn)
            {
                if (_obj != null)
                    _obj.SetActive(false);
            }
            foreach (GameObject _obj in TargetsDefaultOff)
            {
                if (_obj != null)
                    _obj.SetActive(true);
            }
            if (targetAnimator != null) targetAnimator.SetBool(animatorValue,true);

            if (isGlobal && !isUnlocked)
            {
                Networking.SetOwner(Networking.LocalPlayer,gameObject);
                isUnlocked = true;
                RequestSerialization();
            }
            
        }
        private void LockWhitelist()
        {
            if (keypads != null && keypads.Length != 0)
            {
                foreach (var _keypad in keypads)
                {
                    _keypad.SetUnlock(false);
                }
            }
            foreach (GameObject _obj in TargetsDefaultOn)
            {
                if (_obj != null)
                    _obj.SetActive(true);
            }
            foreach (GameObject _obj in TargetsDefaultOff)
            {
                if (_obj != null)
                    _obj.SetActive(false);
            }
            if (targetAnimator != null) targetAnimator.SetBool(animatorValue,false);
        }

        public override void OnDeserialization()
        {
            if (!isGlobal) return;
            if (isUnlocked) UnlockWhitelist();
            else LockWhitelist();
        }
        
    }
}
