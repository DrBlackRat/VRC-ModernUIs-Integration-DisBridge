//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using TMPro;
using UdonSharp;
using UdonVR.DisBridge;
using UdonVR.DisBridge.Plugins;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Plugins.Editor")]
public class ToggleV2 : DisBridgePlugin
{
    [SerializeField] internal GameObject[] defaultOn;
    [SerializeField] internal GameObject[] defaultOff;
    [SerializeField] internal bool[] defaultOn_EnforceStart;
    [SerializeField] internal bool[] defaultOff_EnforceStart;
    [SerializeField] internal Animator[] targetAnimators;
    [SerializeField] internal string[] targetAnimatorStrings;
    [SerializeField] internal bool[] targetAnimator_EnforceStart;
    [SerializeField] internal bool alwaysEnforceLockState;
    [SerializeField] internal bool neverEnforceLockState;

    [UdonSynced]private bool lockState = false;
    [SerializeField] internal bool isGlobal = false;

    [SerializeField] internal bool offIfNotUsable = true;
    
    [SerializeField] internal bool disallowUnlock = false;
    [SerializeField] internal bool canOwnerUnlock = false;
    [SerializeField] internal bool canMasterUnlock = false;

    [SerializeField] internal DisBridgeWhitelist_Class[] whitelists;

    [SerializeField] internal GameObject button;
    [SerializeField] internal GameObject buttonIndicator;
    [SerializeField] internal Collider buttonCollider;
    [SerializeField] internal bool isPhysical = false;
    [SerializeField] internal TextMeshProUGUI globalIndicator;
    
    
    
    
    void Start()
    {
        disBridge.AddPlugin(gameObject);
        
        DisableInteractive = !isPhysical;
        
        __UpdateIndicators();
        _EnforceLockState();

        if (globalIndicator)
        {
            if (globalIndicator != null) globalIndicator.text = isGlobal ? "(Global)" : "(Local)";
        }
    }

    void _EnforceLockState()
    {
        if (neverEnforceLockState) return;
        if (alwaysEnforceLockState)
        {
            __UpdateState();
            return;
        }
        for (int i = 0; i < defaultOn.Length; i++)
        {
            if (defaultOn[i] == null) continue;
            defaultOn[i].SetActive(defaultOn_EnforceStart[i] || defaultOn[i].activeSelf);
        }
        for (int i = 0; i < defaultOff.Length; i++)
        {
            if (defaultOff[i] == null) continue;
            defaultOff[i].SetActive(!defaultOn_EnforceStart[i] && defaultOff[i].activeSelf);
        }
        for (int i = 0; i < targetAnimators.Length; i++)
        {
            if (targetAnimators[i] == null) continue;
            targetAnimators[i].SetBool(targetAnimatorStrings[i],!targetAnimator_EnforceStart[i] && targetAnimators[i].GetBool(targetAnimatorStrings[i]));
        }
    }

    public override void Interact()
    {
        _Use();
    }

    public void _Use()
    {
        if (__FlipState()) __UpdateState();
        __UpdateIndicators();
    }

    internal void _Editor_Use()
    {
        lockState = !lockState;
        __UpdateState();
        __UpdateIndicators();
    }

    public override void _UVR_Init()
    {
        __UpdateIndicators();
    }

    public override void _UVR_Update()
    {
        __UpdateIndicators();
    }

    private void __UpdateState()
    {
        for (int i = 0; i < defaultOn.Length; i++)
        {
            if (defaultOn[i] == null) continue;
            defaultOn[i].SetActive(!lockState);
        }
        for (int i = 0; i < defaultOff.Length; i++)
        {
            if (defaultOff[i] == null) continue;
            defaultOff[i].SetActive(lockState);
        }

        for (int i = 0; i < targetAnimators.Length; i++)
        {
            if (targetAnimators[i] == null) continue;
            targetAnimators[i].SetBool(targetAnimatorStrings[i],lockState);
        }

        for (int i = 0; i < whitelists.Length; i++)
        {
            if (whitelists[i] == null) continue;
            if (lockState)
            {
                whitelists[i]._ForceLock(whitelists[i].name);
            }
            else
            {
                whitelists[i]._ForceUnlock(whitelists[i].name);
            }
        }
    }

    private void __UpdateIndicators()
    {
        if (buttonIndicator != null) buttonIndicator.SetActive(lockState);
        if (button != null && offIfNotUsable)
        {
            button.SetActive(_IsMemberInRoles(Networking.LocalPlayer));
        } 
        if (buttonCollider != null) buttonCollider.enabled = (!offIfNotUsable);
    }

    private bool __FlipState()
    {
        if (__UnlockCheck())
        {
            lockState = !lockState || disallowUnlock;
            if (isGlobal)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
            return true;
        }
        return false;
    }
    
    private bool __UnlockCheck()
    {
        if (canOwnerUnlock && Networking.IsInstanceOwner) return true;
        if (canMasterUnlock && Networking.IsMaster) return true;
        return _IsMemberInRoles(Networking.LocalPlayer);
    }
    public override void OnDeserialization()
    {
        if (!isGlobal) return;
        __UpdateIndicators();
        __UpdateState();
    }
}
