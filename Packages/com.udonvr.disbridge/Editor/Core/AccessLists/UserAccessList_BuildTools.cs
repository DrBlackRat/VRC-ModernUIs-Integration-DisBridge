using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace UdonVR.DisBridge.Editors
{

    public class UserAccessList_BuildTools : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -9999;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return true;
        }

        [PostProcessScene(-9999)]
        private static void ProcessScene()
        {
            UserAccessList_Button[] _buttons = Object.FindObjectsOfType<UserAccessList_Button>(true);
            UserAccessList[] accessLists = Object.FindObjectsOfType<UserAccessList>(true);
            UserAccessList_Controller[] _buttonLists = Object.FindObjectsOfType<UserAccessList_Controller>(true);
            UserAccessList_Class[] UserAccess_plugins =  Object.FindObjectsOfType<UserAccessList_Class>(true);
            
            foreach (var buttonList in _buttonLists)
            {
                buttonList._Menu_Close();
                buttonList.buttons = buttonList.buttonParent.GetComponentsInChildren<UserAccessList_Button>();
                foreach (var button in buttonList.buttons)
                {
                    button.userAccessList_Button_List = buttonList;
                    for (int i = 0; i < button.userAccessList_Button_List.userAccessLists.Length; i++)
                    {
                        button.buttons[i].userAccessList =  button.userAccessList_Button_List.userAccessLists[i];
                    }
                }

                foreach (var accessList in buttonList.userAccessLists)
                {
                    accessList.controller = buttonList;
                }
            }
            
            foreach (var button in _buttons)
            {
                foreach (var __button in button.buttons)
                {
                    __button.hasAccessList = (__button.userAccessList != null);
                    if (__button.hasAccessList) __button.toggleText.text = __button.userAccessList.accessListName;
                }
                button._rectTransform =  ((RectTransform)button.transform);
                button.openSize = new Vector2(button._rectTransform.rect.width, button._rectTransform.rect.height);
                button.closeSize = new Vector2(button.openSize.x, 0f);
                button.__ResetButton();
            }

            BuildPlugins(accessLists,UserAccess_plugins);
        }

        private static void BuildPlugins(UserAccessList[] accessLists, UserAccessList_Class[] UserAccess_plugins)
        {
            foreach (var accessList in accessLists)
            {
                List<UserAccessList_Class> newPlugins = new List<UserAccessList_Class>();
                foreach (var plugin in UserAccess_plugins)
                {
                    if (plugin.userAccessList == accessList)
                    {
                        newPlugins.Add(plugin);
                    }
                }
                accessList.plugins = newPlugins.ToArray();
            }
            
            
        }
    }
}