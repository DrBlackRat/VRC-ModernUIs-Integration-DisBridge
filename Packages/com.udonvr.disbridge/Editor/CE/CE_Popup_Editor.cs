//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using UdonVR.DisBridge.Ce;
using UnityEditor;
using UnityEngine;
using VRC.Economy;
using VRC.Editor;
using UdonVR.DisBridge.Editors;

//prod_d6dcc254-7b41-4f68-af4e-7fcd3ef01eb9 - product ID
//prod_40ebac89-d30d-46ed-91f3-74463ae0ab9c - listing ID
//grp_ad2d850f-7e50-4632-9d92-4aa1ac83fce5 - Group ID

namespace UdonVR.DisBridge.Editors.Ce
{
    [CustomEditor(typeof(CE_Popup))]
    public class CE_Popup_Editor : Editor
    {
        private string[] labels = new string[5] { "Udon Listing", "Group", "Group Store","World Store", "Custom" };
        private CE_Popup myScript;

        public override void OnInspectorGUI()
        {
            myScript = (CE_Popup)target;
            Undo.RecordObject(myScript, $"Undo Event");

            EditorFunctions.PlaymodeWarning(myScript);
            byte _roleMode = (byte)EditorGUILayout.Popup("Mode:", myScript.mode, labels);
            myScript.mode = _roleMode;
            switch (_roleMode)
            {
                case 0:
                    EditorGUILayout.HelpBox(
                        $"Interact with this script or call '{nameof(myScript._Popup_Listing)}' to display the UdonListing page",
                        MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(nameof(myScript.Interact));
                    EditorGUILayout.TextField(nameof(myScript._Popup_Group));
                    EditorGUILayout.EndHorizontal();
                    break;
                case 1:
                    EditorGUILayout.HelpBox(
                        $"Interact with this script or call '{nameof(myScript._Popup_Group)}' to display the Group page", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(nameof(myScript.Interact));
                    EditorGUILayout.TextField(nameof(myScript._Popup_Group));
                    EditorGUILayout.TextField(nameof(myScript._Popup_GroupStore));
                    EditorGUILayout.EndHorizontal();
                    break;
                case 2:
                    EditorGUILayout.HelpBox(
                        $"Interact with this script or call '{nameof(myScript._Popup_GroupStore)}' to display the Group Store page",
                        MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(nameof(myScript.Interact));
                    EditorGUILayout.TextField(nameof(myScript._Popup_Group));
                    EditorGUILayout.TextField(nameof(myScript._Popup_GroupStore));
                    EditorGUILayout.EndHorizontal();
                    break;
                case 3:
                        EditorGUILayout.HelpBox(
                        $"Interact with this script to display the World Store page",
                        MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.EndHorizontal();
                    break;

                case 4:
                    EditorGUILayout.HelpBox(
                        $"Interact is disabled. you must call '{nameof(myScript._Popup_Listing)}' '{nameof(myScript._Popup_Group)}' or '{nameof(myScript._Popup_GroupStore)}' to display respective popup",
                        MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(nameof(myScript._Popup_Group));
                    EditorGUILayout.TextField(nameof(myScript._Popup_GroupStore));
                    EditorGUILayout.TextField(nameof(myScript._Popup_Listing));
                    EditorGUILayout.EndHorizontal();
                    break;
            }
            
            if (myScript.groupId == null) myScript.groupId = String.Empty;
            if (myScript.listingId == null) myScript.listingId = String.Empty;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Groups Page"))
            {
                Application.OpenURL("https://vrchat.com/home/groups");
            }

            EditorGUI.BeginDisabledGroup(!myScript.groupId.Contains("grp_"));
            if (GUILayout.Button("Open My Group Page"))
            {
                Application.OpenURL($"https://vrchat.com/home/group/{myScript.groupId}");
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Open Listings Page"))
            {
                Application.OpenURL("https://vrchat.com/home/marketplace/storefront/listings");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorFunctions.GuiLine();

            // bool hasProduct = false;
            // if (myScript.udonProduct != null)
            // {
            //     myScript.listingId = myScript.udonProduct.ID;
            //     hasProduct = true;
            // }
            
            switch (_roleMode)
            {
                case 0: // Listing
                    EditorGUILayout.BeginHorizontal();
                    //myScript.udonProduct = (UdonProduct)EditorGUILayout.ObjectField(myScript.udonProduct,typeof(UdonProduct), false);
                    //EditorGUI.BeginDisabledGroup(hasProduct);
                    myScript.listingId = EditorGUILayout.TextField("Listing ID",myScript.listingId);
                    //EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    ListingCheck();
                    
                    break;
                case 1: // Group
                    myScript.groupId = EditorGUILayout.TextField("Group ID", myScript.groupId);
                    GroupCheck();
                    break;
                case 2: // Group
                    myScript.groupId = EditorGUILayout.TextField("Group ID", myScript.groupId);
                    GroupCheck();
                    break;
                case 3: // World Store - has no parameters, we just call Interact()
                    break;
                case 4: // Custom
                    EditorGUILayout.BeginHorizontal();
                    //myScript.udonProduct = (UdonProduct)EditorGUILayout.ObjectField(myScript.udonProduct,typeof(UdonProduct), false);
                    //EditorGUI.BeginDisabledGroup(hasProduct);
                    myScript.listingId = EditorGUILayout.TextField("Listing ID",myScript.listingId);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    myScript.groupId = EditorGUILayout.TextField("Group ID", myScript.groupId);
                    
                    ListingCheck();
                    GroupCheck();
                    break;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }

        private void ListingCheck()
        {
            if (!myScript.listingId.Contains("prod_"))
            {   
                EditorGUILayout.HelpBox(
                    $"It looks like your Listing ID is not a Listing. Please make sure your ID starts with `prod_`",
                    MessageType.Error); 
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Your Listing ID looks good. However, if it does not show up in-game, double check that it is a 'LISTING' and not a 'PRODUCT'.\nThese IDs follow the same format so they can not be told apart from one another.",
                    MessageType.Info); 
            }
        }

        private void GroupCheck()
        {
            if (!myScript.groupId.Contains("grp_"))
            {   
                EditorGUILayout.HelpBox(
                    $"It looks like your Group ID is not a Listing. Please make sure your ID starts with `grp_`",
                    MessageType.Error);
            }
        }
    }
}
