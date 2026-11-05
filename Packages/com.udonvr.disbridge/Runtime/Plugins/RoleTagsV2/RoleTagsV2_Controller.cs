//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR.DisBridge.Plugins.RoleTagsV2
{
    public class RoleTagsV2_Controller : DisBridgePlugin
    {
        public RoleTagsV2_Tag[] tags;
        public bool useHeadtracker;
        public Transform headTracker;

        public byte roleMode;

        public byte tagTransparency = 255;

        public RoleTagsV2_Tag localTag;

        //UI
        public Button hideMyTagLocal_Toggle;
        public GameObject hideMyTagLocal;
        public bool hideMyTagLocal_bool;
        private bool hideMyTagLocal_bool_local;

        public Button hideMyTagGlobal_Toggle;
        public GameObject hideMyTagGlobal;

        public GameObject hideAllTags;
        public bool hideAllTags_bool;

        public Color[] tagColors;

        public Transform tagButtonParent;
        public RoleTagV2_Button[] tagButtons;
        public GridLayoutGroup buttonGrid;
        public RectTransform buttonRect;
        public GameObject buttonParent;
        public GameObject loadingParent;

        public RoleContainer[] admins;
        private bool isAdmin = false;
        public bool hideButtons = true;

        #region customTagMode

        public bool[] c_DefaultAssign;
        public string[] c_Names;
        public Sprite[] c_Icons;
        public Color[] c_Colors;
        public RoleContainer[] c_RoleContainers;
        public UdonProduct[] c_UdonProducts;
        public PermissionGroup[] c_PermissionGroups;
        public bool[] c_DontUseLookAt;
        public LookAtConstraint[] c_LookAtConstraints;
        public bool[] c_useUiTag;

        public Vector3 tagOffset = new Vector3(0,.75f,0);

        #endregion

        public bool dontImportFromRoles = false;
        
        public ConstraintSource headTrackerSrc;

        public RoleTagV2_Button noTagButton;

        private bool hasStart = false;
        
        public byte menuMode = 0;
        // 0 = no menu
        // 1 = auto generate
        // 2 = custom menu

        // TODO write in editor system for distributing gameobjects
        // write in system to turn off tags if object is perfered
        //
        // tie bool array to object array and a string array for hash keeping
        // needs to work like the RoleContainers in the new plugin system
        
        void Start()
        {
            //Debug.LogError($"Start {gameObject.name}");
            disBridge.AddPlugin(gameObject);
            ControllerSetup();
        }

        private void AutoSetupTags()
        {
            //Debug.LogError($"AutoSetupTags {gameObject.name}");
            if (roleMode == 4) return;
            int _i = _roleContainers.Length;
            c_DefaultAssign = new bool[0];
            c_Names = new string[_i];
            c_Icons = new Sprite[_i];
            c_Colors = new Color[_i];
            c_RoleContainers = new RoleContainer[_i];
            c_UdonProducts = new UdonProduct[_i];
            c_PermissionGroups = new PermissionGroup[_i];
            c_DontUseLookAt = new bool[_i];
            c_LookAtConstraints = new LookAtConstraint[_i];
            c_useUiTag =  new bool[_i];

            RoleContainer tmp;
            for (int i = 0; i < _i; i++)
            {
                tmp = _roleContainers[i];
                c_Names[i] = string.IsNullOrWhiteSpace(tmp.roleAltName) ? tmp.name : tmp.roleAltName;
                c_Icons[i] = tmp.roleIcon;
                c_Colors[i] = tmp.roleColor;
                c_RoleContainers[i] = tmp;
                c_useUiTag[i] = true;
            }
        }

        private void ManualSetupTags()
        {
            if (dontImportFromRoles) { return; }
            RoleContainer tmp;
            for (int i = 0; i < c_Names.Length; i++)
            {
                tmp = c_RoleContainers[i];
                if (tmp == null) continue;
                c_Names[i] = string.IsNullOrWhiteSpace(c_Names[i])
                    ? string.IsNullOrWhiteSpace(tmp.roleAltName) ? tmp.name : tmp.roleAltName
                    : c_Names[i];
                c_Icons[i] = c_Icons[i] == null ? tmp.roleIcon : c_Icons[i];
                c_Colors[i] = c_Colors[i] == Color.white ? (Color)tmp.roleColor : c_Colors[i];
            }
        }

        public void _UpdateGrid()
        {
            return;
            if (buttonGrid == null) return;
            buttonGrid.enabled = true;
            SendCustomEventDelayedFrames(nameof(_UpdateGrid2), 1);
        }

        public void _UpdateGrid2()
        {
            if (buttonGrid == null) return;
            buttonGrid.enabled = false;
        }

        public override void _UVR_Init()
        {
            Debug.LogError($"[RoleTagsV2_Controller] _UVR_Init {gameObject.name}");
            if (!hasStart)
            {
                ControllerSetup();
            }

            foreach (var _role in admins)
            {
                if (_role == null) continue;
                if (_role.IsMember(Networking.LocalPlayer))
                {
                    isAdmin = true;
                    break;
                }
            }
            
            if (Networking.LocalPlayer.isMaster)
            {
                _Init_Delay();
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(_Init_Delay),5f);
            }
            Debug.LogError($"[RoleTagsV2_Controller] _UVR_Init -- TagButton Init {gameObject.name}");
            for (int i = 0; i < tagButtons.Length; i++)
            {
                if (tagButtons[i] != null)
                    tagButtons[i]._InitButton(i,hideButtons);
            }

            if (_IsMemberInRoles(Networking.LocalPlayer) && hideMyTagLocal_Toggle != null)
            {
                Debug.LogError($"[RoleTagsV2_Controller] _UVR_Init -- Member [{Networking.LocalPlayer.displayName}] is in Roles");
                hideMyTagLocal_Toggle.interactable = hideMyTagGlobal_Toggle.interactable = true;
            }
            else
            {
                
            }
        }

        public override void _UVR_Update()
        {
            _UVR_Init();
        }

        private void ControllerSetup()
        {
            if (hasStart) return;
            hasStart = true;
            //Debug.LogError($"ControllerSetup {gameObject.name}");
            if (headTracker != null)
            {
                headTrackerSrc = new ConstraintSource();
                headTrackerSrc.sourceTransform = headTracker;
                headTrackerSrc.weight = 1f;
            }
            
            if (buttonParent != null) buttonParent.SetActive(true);
            if (loadingParent != null) loadingParent.SetActive(false);

            switch (roleMode)
            {
                case 0: //all Roles
                    _roleContainers = disBridge.GetRoles();
                    AutoSetupTags();
                    break;
                case 1: //Staff
                    _roleContainers = disBridge.GetStaff();
                    AutoSetupTags();
                    break;
                case 2: //Supporters
                    _roleContainers = disBridge.GetSupporter();
                    AutoSetupTags();
                    break;
                case 3: // Selector
                    AutoSetupTags();
                    break;
                case 4: // Custom Tags
                    ManualSetupTags();
                    break;
            }
            if (c_UdonProducts.Length < c_Names.Length)
            {
                c_UdonProducts = new UdonProduct[c_Names.Length];
            }
            if (c_PermissionGroups.Length < c_Names.Length)
            {
                c_PermissionGroups = new PermissionGroup[c_Names.Length];
            }
            

            Color _tmpColor;
            tagColors = new Color[c_Names.Length];
            for (int i = 0; i < c_Names.Length; i++)
            {
                _tmpColor = c_Colors[i];
                tagColors[i] = new Color(_tmpColor.r, _tmpColor.g, _tmpColor.b, tagTransparency);
            }

            switch (menuMode)
            {
                default:
                    // do nothing
                    break;
                case 1:
                    //Debug.LogError($"AutoGen ButtonSetup {gameObject.name}");
                    // auto generate
                    tagButtons = new RoleTagV2_Button[c_Names.Length];
                    for (int i = 0; i < tagButtons.Length - 1; i++)
                    {
                        Instantiate(tagButtonParent.GetChild(0).gameObject, tagButtonParent);
                    }

                    tagButtons = tagButtonParent.GetComponentsInChildren<RoleTagV2_Button>();
                    if (buttonGrid != null)
                    {
                        buttonRect.sizeDelta = new Vector2(buttonGrid.cellSize.x,buttonGrid.cellSize.y * c_Names.Length);
                    }
                    else
                    {
                        buttonRect.sizeDelta = new Vector2(buttonRect.rect.width,buttonRect.rect.height * c_Names.Length);
                    }

                    SendCustomEventDelayedFrames(nameof(_UpdateGrid), 5);
                    break;
                case 2:
                    break;
            }
        }

        public void _Init_Delay()
        {
            bool skipCheck = false;
            if (c_DefaultAssign == null) skipCheck = true;
            if (c_DefaultAssign.Length == 0) skipCheck = true;

            for (int i = 0; i < c_Names.Length; i++)
            {
                if (skipCheck == false)
                {
                    if (c_DefaultAssign[i] == false) continue;
                }
                if (_SetTag(i))
                {
                    break;
                }
            }
            
            //if (TryGetPlayerRoleContainer(Networking.LocalPlayer, out RoleContainer _tmpRoleContainer))
            //{
            //    for (int i = 0; i < tags.Length; i++)
            //    {
            //        if (tags[i]._TryGetTag(_tmpRoleContainer)) { break; }
            //    }
            //}
        }

        public override void PostLateUpdate()
        {
            if (useHeadtracker)
            {
                headTracker.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            }
            for (int i = 0; i < tags.Length; i ++)
            {
                tags[i].MoveToOwner();
            }
        }
        
        //UI Events
        public void _Toggle_hideMyTagLocal()
        {
            if (localTag == null) return;
            hideMyTagLocal_bool = !hideMyTagLocal_bool;
            hideMyTagLocal_bool_local = hideMyTagLocal_bool;
            localTag.UpdateTagDisplay();
            hideMyTagLocal.SetActive(hideMyTagLocal_bool);
        }

        public void _Toggle_hideMyTagGlobal()
        {
            if (localTag == null) return;
            localTag._ToggleTag();
            hideMyTagGlobal.SetActive(localTag.hideMyTagGlobal);
            
            hideMyTagLocal_bool = localTag.hideMyTagGlobal || hideMyTagLocal_bool_local;
            localTag.UpdateTagDisplay();
            hideMyTagLocal.SetActive(hideMyTagLocal_bool);
        }
        
        public void _Toggle_hideAllTags()
        {
            hideAllTags_bool = !hideAllTags_bool;
            foreach (var _var in tags)
            {
                _var.UpdateTagDisplay();
            }
            hideAllTags.SetActive(hideAllTags_bool);

            hideMyTagLocal_Toggle.interactable = hideMyTagGlobal_Toggle.interactable = !hideAllTags_bool;
        }

        public bool _SetTag(int _role)
        {
            if (!_CanUseTag(_role))
            {
                //if you cant use the tag, return
                return false;
            }
            
            if (localTag == null)
            {
                for (int j = 0; j < tags.Length; j++)
                    // looking to assign this player a Tag
                {
                    if (tags[j]._TryGetTag(_role))
                    {
                        // try to get a tag, when it finds one, break the loop
                        break;
                    }
                }
            }
            else
            // the player has a tag, assign the role to their tag
            {
                localTag._SetRole(_role);
            }
            for (int i = 0; i < tagButtons.Length; i++)
                // set the Selectors of all the tag buttons
            {
                if (tagButtons[i] == null) continue;
                if (_role == i)
                {
                    if (tagButtons[i].selector != null)
                        tagButtons[i].selector.SetActive(true);
                    continue;
                }
                if (tagButtons[i].selector != null)
                    tagButtons[i].selector.SetActive(false);
            }

            if (noTagButton != null)
            {
                noTagButton.selector.SetActive(false);
            }

            return true;
        }

        public void _ResetTagSelectors()
        {
            for (int i = 0; i < tagButtons.Length; i++)
            {
                if (tagButtons[i].selector != null)
                    tagButtons[i].selector.SetActive(false);
            }
            if (noTagButton != null)
            {
                noTagButton.selector.SetActive(true);
            }
        }

        public bool _CanUseTag(int _index)
        {
            disBridge.log($"[RoleTagsV2_Controller] _CanUseTag: {_index}");
            if (isAdmin)
            {
                disBridge.log($"[RoleTagsV2_Controller] _CanUseTag: User is Admin. [True]");
                return true;
            }

            if (c_RoleContainers[_index] == null && c_UdonProducts[_index] == null)
            {
                disBridge.log($"[RoleTagsV2_Controller] both null [True]");
                return true;
            }
            
            if (c_RoleContainers[_index] != null)
            {
                if (c_RoleContainers[_index].IsMember())
                {
                    disBridge.log($"[RoleTagsV2_Controller] member of RoleContainer [True]");
                    return true;
                }
            }
            else
            {
                disBridge.log($"[RoleTagsV2_Controller] c_RoleContainers[{_index}] is NULL]");
            }
            
            if (c_UdonProducts[_index] != null)
            {
                if (Store.DoesPlayerOwnProduct(Networking.LocalPlayer,c_UdonProducts[_index]))
                {
                    disBridge.log($"[RoleTagsV2_Controller] member of UdonProduct [True]");
                    return true;
                }
            }
            else
            {
                disBridge.log($"[RoleTagsV2_Controller] c_UdonProducts[{_index}] is NULL]");
            }

            if (c_PermissionGroups != null)
            {
                if (c_PermissionGroups[_index] != null)
                {
                    if (c_PermissionGroups[_index]._IsMember(Networking.LocalPlayer)) return true;
                }
            }
            else
            {
                disBridge.log($"[RoleTagsV2_Controller] c_PermissionGroups is NULL");
            }
            disBridge.log($"[RoleTagsV2_Controller] member can not use tag [False]");
            return false;
        }
        
        public override void OnPurchaseConfirmed(IProduct product, VRCPlayerApi player, bool purchasedNow)
        {
            disBridge.log($"OnPurchaseConfirmed {tagButtons.Length}");
            for (int i = 0; i < tagButtons.Length; i++)
            {
                if (tagButtons[i] == null) continue;
                tagButtons[i]._InitButton(i,hideButtons);
            }
        }

        public override void OnPurchaseExpired(IProduct product, VRCPlayerApi player)
        {
            disBridge.log($"OnPurchaseExpired {tagButtons.Length}");
            for (int i = 0; i < tagButtons.Length; i++)
            {
                if (tagButtons[i] == null) continue;
                tagButtons[i]._InitButton(i,hideButtons);
            }
        }
    }
}