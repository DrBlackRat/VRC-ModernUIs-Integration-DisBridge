//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UdonVR.DisBridge;
using UdonVR.DisBridge.Plugins.RoleTagsV2;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RoleTagsV2_Tag : UdonSharpBehaviour
{
    private bool isUsed;
    public RoleTagsV2_Controller tagController;

    public GameObject thisTag;

    //[UdonSynced] public bool isUsed = false;
    [UdonSynced] public int playerId = -1;
    [UdonSynced] public int thisTagIndex = -1;
    [UdonSynced] public bool hideMyTagGlobal = false;
    //private RoleContainer _roleContainer;
    [SerializeField] private GameObject[] objectToggles;
    private VRCPlayerApi thisOwner;
    private int lastPlayer = -1;

    public Image[] tagTintables;

    public GameObject boosterObject;
    
    public GameObject iconObject;
    public Image tagRoleIcon;
    public TextMeshProUGUI tagText;

    public LookAtConstraint lookAt;

    public bool isLocalTag = false;

    private Color __tmpColor;
    public bool _TryGetTag(int __roleContainer)
    {
        tagController.disBridge.log($"tag {name} -- _TryGetTag");
        if (isUsed && !isLocalTag) return false;
        if (VRC.SDKBase.Utilities.IsValid(VRCPlayerApi.GetPlayerById(playerId)))
        {
            //tagController.disBridge.logError($"[RoleTagsV2_Tag.33][{gameObject.name}]TryGetTag return False ");
            return false;
        }
        else
        {
            //tagController.disBridge.logError($"[RoleTagsV2_Tag.38][{gameObject.name}]TryGetTag return True ");
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            thisTagIndex = __roleContainer;
            thisOwner = Networking.LocalPlayer;
            playerId = thisOwner.playerId;
            tagController.localTag = this;
            isLocalTag = true;
            RequestSerialization();
            SetupTag();
        }
        return true;
    }

    public void UpdateTagDisplay()
    {
        RefreshTag();
    }
    
    public void _ToggleTag()
    {
        tagController.disBridge.log($"tag {name} -- _ToggleTag");
        if (Networking.LocalPlayer != thisOwner) return;
        hideMyTagGlobal = !hideMyTagGlobal;
        RefreshTag();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        tagController.disBridge.log($"tag {name} - [{playerId}] [{thisTagIndex}]");
        if (playerId == -1 || thisTagIndex == -1)
        {
            _ResetTag();
            return;
        }
        thisOwner = VRCPlayerApi.GetPlayerById(playerId);
        if (!VRC.SDKBase.Utilities.IsValid(thisOwner))
        {
            _ResetTag();
            return;
        }

        //if (lastPlayer != playerId)
        //{
        //    lastPlayer = playerId;
        //    
        //}
        SetupTag();
        //thisTag.SetActive(ShowTag());
    }

    public void MoveToOwner()
    {
        if (!isUsed) return;
        if (!VRC.SDKBase.Utilities.IsValid(thisOwner)) { _ResetTag(); return; }
        
        transform.position = thisOwner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position + tagController.tagOffset;
    }

    public void _SetRole(int _role)
    {
        tagController.disBridge.log($"tag {name} - _SetRole");
        if (Networking.LocalPlayer != thisOwner) return;
        thisTagIndex = _role;
        RequestSerialization();
        SetupTag();
    }

    private bool ShowTag(bool isObject = false)
    {
        if (!isUsed)
        {
            //Debug.Log($"[{gameObject.name}][tag]False - 1");
            return false;
        }

        if (tagController.hideAllTags_bool)
        {
            //Debug.Log($"[{gameObject.name}][tag]False - 2");
            return false;
        }
        if (thisTagIndex != -1 && !isObject){
            //Debug.Log($"[{gameObject.name}][tag]Check - 1");
            if (tagController.c_useUiTag[thisTagIndex] == false)
            {
                //Debug.Log($"[{gameObject.name}][tag]False - 3");
                return false;
            }
        }

        //bool helpMe = isLocalTag ? !tagController.hideMyTagLocal_bool : !hideMyTagGlobal;
        //Debug.Log($"[{gameObject.name}][tag]Return {helpMe}");
        return isLocalTag ? !tagController.hideMyTagLocal_bool : !hideMyTagGlobal;
    }
    public void _ResetTag()
    {
        tagController.disBridge.log($"tag {name} - _ResetTag");
        if (!isUsed) return;
        isUsed = false;
        thisTag.SetActive(false);
        iconObject.SetActive(false);
        boosterObject.SetActive(false);
        lookAt.constraintActive = false;
        if (isLocalTag)
        {
            tagController._ResetTagSelectors();
            tagController.localTag = null;
        }
        if (Networking.IsOwner(gameObject))
        {
            playerId = -1;
            thisTagIndex = -1;
            RequestSerialization();
        }

        foreach (var _obj in objectToggles)
        {
            if (Utilities.IsValid(_obj)) _obj.SetActive(false);
        }
    }

    private void SetupTag()
    {
        isUsed = true;
        RefreshTag();
    }
    private void RefreshTag()
    {
        if (!isUsed) return;
        tagController.disBridge.log($"tag {name} - SetupTag");
        thisTag.SetActive(ShowTag());
        __LookAtSetup();
        if (tagController.c_Names != null)
        {
            tagController.disBridge.log($"tag {name} - c_Names.legnth{tagController.c_Names.Length}");
            if (tagController.c_Names.Length != 0)
                tagText.text = tagController.c_Names[Mathf.Clamp(thisTagIndex,0,tagController.c_Names.Length)];
        }
        
        if (tagController.c_Colors != null)
        {
            tagController.disBridge.log($"tag {name} - c_Colors.legnth{tagController.c_Colors.Length}");
            if (tagController.c_Colors.Length != 0)
            {
                __tmpColor = tagController.c_Colors[Mathf.Clamp(thisTagIndex,0,tagController.c_Colors.Length)];
            }
        }

        for (int i = 0; i < tagTintables.Length; i++)
        {
            tagTintables[i].color = __tmpColor;
            //tagTintables[i].GraphicUpdateComplete();
        }

        if (tagController.c_Icons[thisTagIndex] != null)
        {
            tagController.disBridge.log($"SetupTag {name} -- iconObject.SetActive(true);");
            iconObject.SetActive(true);
            tagRoleIcon.sprite = tagController.c_Icons[thisTagIndex];
        }
        else
        {
            tagController.disBridge.log($"SetupTag {name} -- iconObject.SetActive(false);");
            iconObject.SetActive(false);
        }
        boosterObject.SetActive(tagController.disBridge.IsBooster(thisOwner));
        
        for (int i = 0; i < objectToggles.Length; i++)
        {

            if (objectToggles[i] != null)
            {
                objectToggles[i].SetActive(i == thisTagIndex && ShowTag(true));
            }
        }
    }

    private void __LookAtSetup()
    {
        if (!tagController.c_DontUseLookAt[thisTagIndex] && !Networking.IsOwner(gameObject))
        {
            if (tagController.c_LookAtConstraints[thisTagIndex] == null)
            {
                if (lookAt.sourceCount == 0)
                {
                    lookAt.AddSource(tagController.headTrackerSrc);
                }
                lookAt.constraintActive = true;
            }
            else
            {
                lookAt.constraintActive = false;
                tagController.c_LookAtConstraints[thisTagIndex].enabled = true;
                if (tagController.c_LookAtConstraints[thisTagIndex].sourceCount == 0)
                {
                    tagController.c_LookAtConstraints[thisTagIndex].AddSource(tagController.headTrackerSrc);
                }
                tagController.c_LookAtConstraints[thisTagIndex].constraintActive = true;
            }
        }
        else
        {
            if (tagController.c_LookAtConstraints[thisTagIndex] != null) tagController.c_LookAtConstraints[thisTagIndex].constraintActive = false;
            lookAt.constraintActive = false;
            lookAt.transform.localRotation = Quaternion.identity;
        }
    }
}
