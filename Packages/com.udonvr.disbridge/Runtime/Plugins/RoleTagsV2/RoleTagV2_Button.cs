//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UdonVR.DisBridge.Editor")]
namespace UdonVR.DisBridge.Plugins.RoleTagsV2
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoleTagV2_Button : UdonSharpBehaviour
    {
        public RoleTagsV2_Controller tagController;
        public TextMeshProUGUI buttonText;
        public Image buttonIcon;
        [SerializeField]internal int thisTagIndex;
        public GameObject selector;
        public Button button;

        private bool hasInit = false;
        public GameObject disabledIndicator;

        public void _InitButton(int _index,bool _hideButton)
        {
            //Debug.LogError($"{gameObject.name} {transform.GetSiblingIndex()}");
            hasInit = true;
            //Debug.LogError(_roleContainer.name);
            thisTagIndex = _index;
            if (buttonIcon != null && tagController.c_Icons[thisTagIndex] != null)
            {
                buttonIcon.enabled = true;
                buttonIcon.sprite = tagController.c_Icons[thisTagIndex];
            }
            if (buttonText != null) buttonText.text = tagController.c_Names[thisTagIndex];



            if (tagController._CanUseTag(thisTagIndex))
            {
                if (disabledIndicator != null) disabledIndicator.SetActive(false);
                if (button != null) button.interactable = true;
            }
            else
            {
                if (_hideButton)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    if (button != null) button.interactable = false;
                    if (disabledIndicator != null) disabledIndicator.SetActive(true);
                }
                
            }
            //Debug.LogError("_InitButton End");
        }

        public void _SetTag()
        {
            if (thisTagIndex == -1)
            {
                if (tagController.localTag != null)
                {
                    tagController.localTag._ResetTag();
                    return;
                }
                else
                {
                    return;
                }
            }
            //Debug.LogError("help me");
            if (tagController._CanUseTag(thisTagIndex))
            {
                tagController._SetTag(thisTagIndex);
            }
        }
    }
}