//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

public enum popupMode
{
    Group,
    Store,
    Listing
}
public class CE_CustomPopup : UdonSharpBehaviour
{
    [UdonSynced] public popupMode mode = 0;
    [UdonSynced] private bool isPickup = false;
    [UdonSynced] public string customID;

    public TMP_InputField feild;
    public Toggle pickupToggle;
    public VRC_Pickup pickup;

    public TMP_Dropdown modeSelect;

    public CE_PopupPickup popupPickup;

    private void Start()
    {
        OnDeserialization();
    }

    public void _Use()
    {
        switch (mode)
        {
            case popupMode.Listing:
                _Popup_Listing();
                break;
            case popupMode.Group:
                _Popup_Group();
                break;
            case popupMode.Store:
                _Popup_GroupStore();
                break;
        }
    }

    public void _Popup_Listing()
    {
        if (!string.IsNullOrWhiteSpace(customID))
        {
            Store.OpenGroupListing(customID);
            return;
        }
    }

    public void _Popup_Group()
    {
        if (!string.IsNullOrWhiteSpace(customID))
        {
            Store.OpenGroupPage(customID);
            return;
        }
    }

    public void _Popup_GroupStore()
    {
        if (!string.IsNullOrWhiteSpace(customID))
        {
            Store.OpenGroupStorePage(customID);
            return;
        }
    }

    public void _UpdateID()
    {
        if (feild == null) return;
        Networking.SetOwner(Networking.LocalPlayer,gameObject);
        customID = feild.text;
        isPickup = pickupToggle.isOn;
        pickup.pickupable = isPickup;
        popupPickup._SetDisableInteractive(isPickup);
        mode = (popupMode)modeSelect.value;
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        pickupToggle.SetIsOnWithoutNotify(isPickup);
        feild.SetTextWithoutNotify(customID);
        pickup.pickupable = isPickup;
        popupPickup._SetDisableInteractive(isPickup);
        modeSelect.SetValueWithoutNotify((int)mode);
    }
}
