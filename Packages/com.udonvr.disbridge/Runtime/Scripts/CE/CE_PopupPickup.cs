//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CE_PopupPickup : UdonSharpBehaviour
{
    public CE_CustomPopup popup;
    public override void Interact()
    {
        popup._Use();
    }

    public void _SetDisableInteractive(bool _state)
    {
        DisableInteractive = _state;
    }
}
