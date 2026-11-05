//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UdonSharpEditor;
using UdonVR.disbridge;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonVR.DisBridge.Editors
{
    [CustomEditor(typeof(DisBridgeEditor))]
    public class DisBridgeEditingTool_Editor : Editor
    {
        private PluginManager disBridge;
        private DisBridgeEditor myScript;
        
        private bool DrawRoleContainers_Open = true;
        private bool[] DrawCreatorEconomy_Open;
        private bool[] DrawManualUserlist_Open;
        
        private bool[] roles_Open;
        private RoleContainer[] roles;
        private bool isNotLocal = false;
        
        private readonly float indentSize = 20f;
        private readonly Color lightBlue = new (.294f, .424f, .784f, 1f); 
        private readonly Color brightLightBlue = new (.5f, .6f, 1f, 1f);
        
        private Vector2[] scrollPos;
        private float[] helpboxSizes;
        
        private PackageInfo packageInfo;
        
        void OnEnable()
        {
            myScript = (DisBridgeEditor)target;
            disBridge = myScript.disBridge;
            packageInfo = JsonConvert.DeserializeObject<PackageInfo>(System.IO.File.ReadAllText("Packages/com.udonvr.disbridge/package.json"));
        }

        public override void OnInspectorGUI()
        {
            if (myScript == null)
            {
                EditorGUILayout.HelpBox("UhOh Stinky!!\nSomething bad broke, you should never be seeing this message!!", MessageType.Error);
                return;
            }
            Undo.RecordObject(myScript,$"Undo on {myScript.name}");
            EditorFunctions.DrawDisBridgeLogo();
            DrawSDKVersionCheck();
            DrawPluginManagerCheck();
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You are in Playmode. Changes made will not be saved.", MessageType.Warning);
            }
            
            InitRoleContainers();
            
            DrawServerCheck();
            DrawDuplicateIdCheck();
            DrawRoleContainers();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Version: {packageInfo.version}", GUILayout.MinWidth(0f));
                EditorGUILayout.LabelField($"Debug Logs:", GUILayout.MinWidth(0f), GUILayout.MaxWidth(75f));
                disBridge.debugLevel = (debugLogLevel)EditorGUILayout.EnumPopup(disBridge.debugLevel, GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
            }
            
            
            
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawPluginManagerCheck()
        {
            if (disBridge == null)
            {
                EditorGUILayout.HelpBox("DisBridge Not Connected!!", MessageType.Error);
                myScript.disBridge = (PluginManager)EditorGUILayout.ObjectField(myScript.disBridge, typeof(PluginManager), true);
                if (myScript.disBridge != null) { disBridge = myScript.disBridge; }
            }
        }

        private float _DrawServerCheck_height = 1f;
        private bool _DrawServerCheck_dismiss = false;
        private void DrawServerCheck()
        {
            if (isNotLocal)
            {
                if (_DrawServerCheck_dismiss) return;
                if (disBridge._keyUrl == null || disBridge._keyUrl.Length == 0)
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.HelpBox("It looks like you're trying to Import roles, but have no servers importing.\nPlease import a server.\n\n If you intend to use DisBridge in Offline Mode, then ignore this.", MessageType.Error);
                        _DrawServerCheck_height = (GUILayoutUtility.GetLastRect().height > 1f)? GUILayoutUtility.GetLastRect().height : _DrawServerCheck_height;
                        GUILayout.BeginVertical(GUILayout.Width(100f));
                        if (GUILayout.Button("Import\nServer Key", GUILayout.Width(100f),GUILayout.Height(_DrawServerCheck_height - 10f)))
                        {
                            if (DisBridgeEdiitingToolFunctions.GetJson(out DisbridgeKey _key))
                            {
                                DisBridgeEdiitingToolFunctions.ParseServerKey(_key,disBridge);
                            }
                        }
                        if (GUILayout.Button("Dismiss", GUILayout.Width(100f)))
                        {
                            _DrawServerCheck_dismiss = true;
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
        }

        private float _DrawDuplicateIdCheck_height = 1f;
        private void DrawDuplicateIdCheck()
        {
            List<string> roleIDs = new List<string>();
            bool isDuplicate = false;
            foreach (var role in roles)
            {
                if (roleIDs.Contains(role.roleId))
                {
                    isDuplicate = true;
                }
                else
                {
                    roleIDs.Add(role.roleId);
                }
            }

            if (isDuplicate)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("You have Duplicate RoleIDs. Please make your RoleIDs unique!", MessageType.Error);
                    _DrawDuplicateIdCheck_height = (GUILayoutUtility.GetLastRect().height > 1f) ? GUILayoutUtility.GetLastRect().height : _DrawDuplicateIdCheck_height;
                    if (GUILayout.Button("Auto Fix", GUILayout.Width(100f), GUILayout.Height(_DrawDuplicateIdCheck_height)))
                    {
                        roleIDs = new List<string>();
                        for (int i = 0; i < roles.Length; i++)
                        {
                            if (roleIDs.Contains(roles[i].roleId))
                            {
                                Undo.RecordObject(roles[i], $"Undo on {roles[i].name}");
                                roles[i].roleId = $"{roles[i].roleId}[{i}]";
                                PrefabUtility.RecordPrefabInstancePropertyModifications(roles[i]);
                                roleIDs.Add(roles[i].roleId);
                            }
                            else
                            {
                                roleIDs.Add(roles[i].roleId);
                            }
                        }
                    }
                }
            }
        }

        private void InitRoleContainers()
        {
            if (disBridge == null) { return; }
            roles = disBridge.roles.GetComponentsInChildren<RoleContainer>();
            if (roles_Open == null || roles_Open.Length != roles.Length)
            {
                roles_Open = new bool[roles.Length];
            }
            if (DrawCreatorEconomy_Open == null || DrawCreatorEconomy_Open.Length != roles.Length)
            {
                DrawCreatorEconomy_Open = new bool[roles.Length];
                for (int i = 0; i < DrawCreatorEconomy_Open.Length; i++)
                {
                    DrawCreatorEconomy_Open[i] = true;
                }
            }
            if (DrawManualUserlist_Open == null || DrawManualUserlist_Open.Length != roles.Length)
            {
                DrawManualUserlist_Open = new bool[roles.Length];
                for (int i = 0; i < DrawManualUserlist_Open.Length; i++)
                {
                    DrawManualUserlist_Open[i] = true;
                }
            }
            
            if (scrollPos == null || scrollPos.Length != roles.Length)
            {
                scrollPos = new Vector2[roles.Length];
            }
            if (helpboxSizes == null || helpboxSizes.Length != roles.Length)
            {
                helpboxSizes = new float[roles.Length];
            }
            
            isNotLocal = false;
            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i].roleId.IsNullOrWhitespace())
                {
                    roles[i].roleId = $"Local_{roles[i].roleName}[{i}]";
                }
                if (!roles[i].roleId.Contains("Local_")) isNotLocal = true;
            }
        }
        
        private void DrawRoleContainers()
        {
            if (disBridge == null) { return; }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Role Containers", "Your full list of Role Containers."), GUILayout.MinWidth(0f));
                    if (GUILayout.Button(DrawRoleContainers_Open? "Close" : "Open", GUILayout.Width(100f)))
                    {
                        DrawRoleContainers_Open = !DrawRoleContainers_Open;
                    }
                }
                
                if (!DrawRoleContainers_Open) return;

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                    if (GUILayout.Button("Add Role", GUILayout.Width(100f)))
                    {
                        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.udonvr.disbridge/Runtime/Prefabs/Parts/RoleContainer.prefab", typeof(GameObject));
                        GameObject role = (GameObject)PrefabUtility.InstantiatePrefab(prefab, disBridge.roles);
                        Undo.RegisterCreatedObjectUndo(role,"Create Role Container");
                        RoleContainer rc = role.GetComponent<RoleContainer>();
                        Undo.RecordObject(role,$"Undo on {role.name}");
                        role.name = $"New Role {role.transform.GetSiblingIndex()}";
                        PrefabUtility.RecordPrefabInstancePropertyModifications(role);
                        Undo.RecordObject(rc,$"Undo on {role.name}");
                        rc.roleName = $"New Role {role.transform.GetSiblingIndex()}";
                        PrefabUtility.RecordPrefabInstancePropertyModifications(rc);
                    }
                    if (GUILayout.Button("Remove Role", GUILayout.Width(100f)))
                    {
                        if (disBridge.roles.childCount == 0) return;
                        DeleteRoleContainer(disBridge.roles.GetChild(disBridge.roles.childCount - 1).GetComponent<RoleContainer>());
                    }

                    EditorGUILayout.LabelField("", GUILayout.MinWidth(0f));
                    if (GUILayout.Button("Import Json", GUILayout.Width(100f)))
                    {
                        if (DisBridgeEdiitingToolFunctions.GetJson(out DisbridgeKey _key))
                        {
                            DisBridgeEdiitingToolFunctions.ParseJsonForRoles(_key,disBridge);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                
                for (int i = 0; i < roles.Length; i++)
                {
                    Undo.RecordObject(roles[i],$"Undo on {roles[i].name}");
                    
                    GUI.backgroundColor = roles[i].roleColor;
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            GUI.backgroundColor = Color.white;
                            if (GUILayout.Button(new GUIContent(string.IsNullOrWhiteSpace(roles[i].roleName)?roles[i].roleAltName:roles[i].roleName, $"Index: {i}" +
                                        $"\nName: {roles[i].roleName}" +
                                        $"\nAlt Name: {roles[i].roleAltName}" +
                                        $"\nColor: {roles[i].roleColor}" +
                                        $"\nStaff?: {roles[i].IsStaff}" +
                                        $"\nSupporter?: {roles[i].IsSupporter}" +
                                        $"\nLocal Usernames: {roles[i].manualMembers.Length}" +
                                        $"\nCreator Eco Products: {roles[i].ceProducts.Length}"),
                                    EditorStyles.label, GUILayout.MinWidth(0f)))
                            {
                                roles_Open[i] = !roles_Open[i];
                            }
                            
                            //EditorGUILayout.LabelField(new GUIContent($"{roles[i].roleName}", $"Name: {roles[i].roleName}\nIndex: {i}"), GUILayout.MinWidth(0f));

                            if (EditorApplication.isPlayingOrWillChangePlaymode)
                            {
                                UdonSharpEditorUtility.CopyUdonToProxy(roles[i]);
                                EditorGUILayout.LabelField($"[{roles[i].members.Length}]", GUILayout.MinWidth(0f),GUILayout.MaxWidth(50f));
                            }
                            
                            EditorGUILayout.LabelField(new GUIContent($"Role ID:", $"Used as an identifier for importing from the DisBridge servers.\nThis needs to be unique and not blank.\n\nIf you're not importing from the DisBridge servers, then this just needs to be filled with *something*."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(50f));
                            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                            roles[i].roleId = EditorGUILayout.TextField("",roles[i].roleId,GUILayout.MinWidth(0f), GUILayout.MaxWidth(100f));
                            EditorGUI.EndDisabledGroup();
                            
                            GUI.backgroundColor = roles_Open[i] ? brightLightBlue : lightBlue;
                            if (GUILayout.Button(roles_Open[i] ? "Close" : "Open", GUILayout.Width(100f)))
                            {
                                roles_Open[i] = !roles_Open[i];
                            }
                            GUI.backgroundColor = Color.white;
                            
                            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                            DrawMoveRoleUp(roles[i],i);
                            DrawMoveRoleDown(roles[i],i);
                            EditorGUI.EndDisabledGroup();
                            
                            if (!roles_Open[i]) continue;
                        }

                        if (EditorGUIUtility.currentViewWidth >= 600f)
                        {
                            EditorGUILayout.BeginHorizontal();
                            DrawCurrentUserlist(roles[i], true, i);
                        }
                        else
                        {
                            EditorGUILayout.BeginVertical();
                            DrawCurrentUserlist(roles[i], false, i);
                        }
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            DrawIndent();
                            using (new EditorGUILayout.VerticalScope())
                            {
                                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                                DrawRoleNamePlural(roles[i]);
                                DrawRoleName(roles[i]);
                                DrawRoleColor(roles[i]);
                                DrawRoleIcon(roles[i]);
                                DrawRoleIsStaff(roles[i]);
                                DrawRoleIsSupporter(roles[i]);
                                DrawRoleIsBooster(roles[i]);
                                EditorGUI.EndDisabledGroup();
                                DrawManualUserlist(roles[i],i);
                                DrawCreatorEconomy(roles[i],i);
                                DrawCustomNamelist(roles[i]);
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField("", GUILayout.MinWidth(0f));
                                    DrawDeleteRole(roles[i]);
                                }
                                RoleNameCheck(roles[i]);
                                PrefabUtility.RecordPrefabInstancePropertyModifications(roles[i]);
                            }
                        }
                        helpboxSizes[i] = GUILayoutUtility.GetLastRect().height >1 ? GUILayoutUtility.GetLastRect().height : helpboxSizes[i];
                        if (EditorGUIUtility.currentViewWidth >= 600f)
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.EndVertical();
                        }
                        
                    }
                }
            }

        }

        private void DrawIndent()
        {
            EditorGUILayout.LabelField(new GUIContent("", ""), GUILayout.MinWidth(0f), GUILayout.MaxWidth(indentSize));
        }

        private void DrawSDKVersionCheck()
        {
            try
            {
                PackageInfo packageInfo = JsonConvert.DeserializeObject<PackageInfo>(System.IO.File.ReadAllText("Packages/com.vrchat.base/package.json"));
                if (packageInfo.version == "3.10.1")
                {
                    GUI.backgroundColor = Color.yellow;
                    EditorGUILayout.HelpBox("\nVRC SDK 3.10.2 Detected.\n\n3.10.2 has a known bug in it that stops URLs from loading in ClientSim.\nWhile this doesn't stop DisBridge from working, it will stop testing in playmode from working. Downgrade your SDK to 3.10.1 to fix this.\n", MessageType.Warning);
                    GUI.backgroundColor = Color.white;
                }
            }
            catch (Exception e)
            {
                
            }
        }
        
        #region RoleContainerParts


        private void DrawCurrentUserlist(RoleContainer role,bool isOnSide, int index)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            GUI.backgroundColor = role.roleColor;
            if (isOnSide)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox,GUILayout.Width(200),GUILayout.Height(helpboxSizes[index])))
                {
                    _DrawCurrentUserList();
                }
            }
            else
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(100f)))
                {
                    _DrawCurrentUserList();
                }
            }
            GUI.backgroundColor = Color.white;

            void _DrawCurrentUserList()
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("Users:", "The current list of Usernames."), GUILayout.MinWidth(0f));
                    EditorGUILayout.LabelField($"User Count: [{role.members.Length}]", GUILayout.MinWidth(0f));
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    scrollPos[index] = EditorGUILayout.BeginScrollView(scrollPos[index]);
                    for (int i = 0; i < role.members.Length; i++)
                    {
                        EditorGUILayout.LabelField($"[{i}] {role.members[i]}", GUILayout.MinWidth(0f));
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }
        private void DrawRoleName(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Role Name (single):", "Used for when the Role Name is used in plugins.\nExample `Moderator`"), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.roleAltName = EditorGUILayout.TextField(role.roleAltName);
            }
        }
        private void DrawRoleNamePlural(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Role Name (plural):", "Used for when the Role Name is used in plugins.\nExample `Moderators`"), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.roleName = EditorGUILayout.TextField(role.roleName);
            }
        }
        private void DrawRoleColor(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Role Color:", "Role Color used for plugins."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.roleColor = EditorGUILayout.ColorField(role.roleColor);
            }
        }
        private void DrawRoleIsStaff(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Staff Role:", "Used by plugins for generic 'Staff' checks."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.IsStaff = EditorGUILayout.Toggle(role.IsStaff, GUILayout.Width(13f));
            }
        }
        private void DrawRoleIsSupporter(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Supporter Role:", "Used by plugins for generic 'Supporter' checks."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.IsSupporter = EditorGUILayout.Toggle(role.IsSupporter, GUILayout.Width(13f));
            }
        }
        private void DrawRoleIsBooster(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Booster Role:", "Used by plugins for generic 'Booster' checks."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.isBooster = EditorGUILayout.Toggle(role.isBooster, GUILayout.Width(13f));
            }
        }
        private void DrawRoleIcon(RoleContainer role)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Role Icon:", "Role Icon used by Plugins."), GUILayout.MinWidth(0f), GUILayout.MaxWidth(150f));
                role.roleIcon = (Sprite)EditorGUILayout.ObjectField(role.roleIcon, typeof(Sprite), false);
            }
        }
        private void DrawCustomNamelist(RoleContainer role)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                using (new EditorGUILayout.HorizontalScope())
                {
                    //DrawIndent();
                    EditorGUILayout.LabelField(new GUIContent("Allow Custom Namelist URL:", "Enables or Disables the URL for custom namelists. This should generally be off."), GUILayout.MinWidth(0f),
                        GUILayout.MaxWidth(200f));
                    if (GUILayout.Button(role.AllowCustomURL? "Disable" : "Enable"))
                    {
                        role.AllowCustomURL = !role.AllowCustomURL;
                    }
                }
               
                if (!role.AllowCustomURL)
                {
                    EditorGUI.EndDisabledGroup();
                    return;
                }
                
                EditorGUILayout.HelpBox("This is not supported. Please do not ask for help from the official Discord about custom URLs.\nThis option is here for advanced users who know what they're doing.", MessageType.Warning);
                EditorGUILayout.HelpBox("This is a Security Risk!!\nUsing non-encrypted name lists opens up a vulnerability for DNS Redirect attacks on your world and may allow users to add themselves to the role.\nIt is NOT recommended to have this enabled.", MessageType.Error);
                EditorGUILayout.HelpBox("Having this enabled will add 5 secconds of startup time to DisBridge.", MessageType.Warning);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(new GUIContent("URL:", "Url of your custom namelist."), GUILayout.MinWidth(0f),
                        GUILayout.MaxWidth(100f));
                    role.customUrl = new VRCUrl(EditorGUILayout.TextField(role.customUrl.ToString()));
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        private void DrawCreatorEconomy(RoleContainer role, int target)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    if (GUILayout.Button(new GUIContent("Creator Economy:", "Used for assigning Creator Economy products to this RoleContainer."),
                            EditorStyles.label, GUILayout.MinWidth(0f)))
                    {
                        DrawCreatorEconomy_Open[target] = !DrawCreatorEconomy_Open[target];
                    }
                    if (GUILayout.Button(DrawCreatorEconomy_Open[target]? "Close":"Open", GUILayout.Width(100f)))
                    {
                        DrawCreatorEconomy_Open[target] = !DrawCreatorEconomy_Open[target];
                    }
                }
                
                if (!DrawCreatorEconomy_Open[target]) return;
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIndent();
                    if (GUILayout.Button("Add"))
                    {
                        AddCeProduct(role);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        RemoveCeProduct(role,role.ceProducts.Length - 1);
                    }
                }
                for (int i = 0; i < role.ceProducts.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawIndent();
                        Undo.RecordObject(role,$"Undo on {role.name}");
                        role.ceProducts[i] = (UdonProduct)EditorGUILayout.ObjectField(role.ceProducts[i], typeof(UdonProduct), false);
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("-", GUILayout.Width(25f)))
                        {
                            RemoveCeProduct(role,i);
                        }
                        GUI.backgroundColor = Color.white;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(role);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        private void DrawManualUserlist(RoleContainer role, int target)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    if (GUILayout.Button(new GUIContent("Local Usernames:", "This is for hardcoding names to the world."),
                            EditorStyles.label, GUILayout.MinWidth(0f)))
                    {
                        DrawManualUserlist_Open[target] = !DrawManualUserlist_Open[target];
                    }
                    if (GUILayout.Button(DrawManualUserlist_Open[target]? "Close":"Open", GUILayout.Width(100f)))
                    {
                        DrawManualUserlist_Open[target] = !DrawManualUserlist_Open[target];
                    }
                }
                if (!DrawManualUserlist_Open[target]) return;
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIndent();
                    if (GUILayout.Button("Add"))
                    {
                        AddName(role);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        RemoveName(role,role.manualMembers.Length - 1);
                    }
                }
                for (int i = 0; i < role.manualMembers.Length; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawIndent();
                        role.manualMembers[i] = EditorGUILayout.TextField(role.manualMembers[i]);
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("-", GUILayout.Width(25f)))
                        {
                            RemoveName(role,i);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        
        private void DrawDeleteRole(RoleContainer role)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete", GUILayout.Width(100f)))
            {
                DeleteRoleContainer(role);
            }
            GUI.backgroundColor = Color.white;
        }
        private void DeleteRoleContainer(RoleContainer role)
        {
            if (EditorUtility.DisplayDialog($"Confirm Delete [{role.roleName}]", $"Are you sure you want to delete [{role.roleName}]?\n\nYou can Undo this.", $"Yes", "No"))
            {
                Undo.DestroyObjectImmediate(role.gameObject);
            }
        }

        private void AddCeProduct(RoleContainer role)
        {
            Undo.RecordObject(role, $"Undo on {role.name}");
            Array.Resize(ref role.ceProducts, role.ceProducts.Length + 1);
            PrefabUtility.RecordPrefabInstancePropertyModifications(role);
        }
        private void RemoveCeProduct(RoleContainer role, int index)
        {
            Undo.RecordObject(target, $"Undo on {target.name}");

            UdonProduct[] newArray = new UdonProduct[role.ceProducts.Length - 1];
            int j = 0;
            for (int i = 0; i < role.ceProducts.Length; i++)
            {
                if (i == index) continue;
                newArray[j] = role.ceProducts[i];
                j++;
            }
            role.ceProducts = newArray;
            PrefabUtility.RecordPrefabInstancePropertyModifications(role);
        }
        
        private void AddName(RoleContainer role)
        {
            Undo.RecordObject(role, $"Undo on {role.name}");
            Array.Resize(ref role.manualMembers, role.manualMembers.Length + 1);
            PrefabUtility.RecordPrefabInstancePropertyModifications(role);
        }
        private void RemoveName(RoleContainer role, int index)
        {
            Undo.RecordObject(target, $"Undo on {target.name}");

            String[] newArray = new String[role.manualMembers.Length - 1];
            int j = 0;
            for (int i = 0; i < role.manualMembers.Length; i++)
            {
                if (i == index) continue;
                newArray[j] = role.manualMembers[i];
                j++;
            }
            role.manualMembers = newArray;
            PrefabUtility.RecordPrefabInstancePropertyModifications(role);
        }

        private void DrawMoveRoleUp(RoleContainer role, int index)
        {
            EditorGUI.BeginDisabledGroup(index == 0);
            if (GUILayout.Button("Up", GUILayout.Width(50f)))
            {
                Undo.SetSiblingIndex(role.transform,index - 1,"Moved RoleContainer Up.");
            }
            EditorGUI.EndDisabledGroup();
        }
        private void DrawMoveRoleDown(RoleContainer role, int index)
        {
            EditorGUI.BeginDisabledGroup(index >= roles.Length - 1);
            if (GUILayout.Button("Down", GUILayout.Width(50f)))
            {
                Undo.SetSiblingIndex(role.transform,index + 1,"Moved RoleContainer Down.");
            }
            EditorGUI.EndDisabledGroup();
        }

        private void RoleNameCheck(RoleContainer role)
        {
            if (role.name != role.roleName)
            {
                Undo.RecordObject(role.gameObject, $"Updating name of gameobject. {role.name} => {role.roleName}");
                role.gameObject.name = role.roleName;
                PrefabUtility.RecordPrefabInstancePropertyModifications(role.gameObject);
            }
        }

        #endregion
    }
}

