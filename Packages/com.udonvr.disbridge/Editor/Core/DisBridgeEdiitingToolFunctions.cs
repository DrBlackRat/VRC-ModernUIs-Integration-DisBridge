//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using Newtonsoft.Json;
using UdonVR.DisBridge;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonVR.DisBridge.Editors
{
    public static class DisBridgeEdiitingToolFunctions
    {
        private static readonly int expectedVersion = 2;
        public static bool GetJson(out DisbridgeKey json)
        {
            string path = EditorUtility.OpenFilePanel("Pick Roles.json", "Downloads", "json");
                        
            if (path.Length != 0)
            {
                if (string.IsNullOrEmpty(path) || !path.Contains(".json"))
                {
                    EditorUtility.DisplayDialog("Json Failure", "Error: You did not select a file, Or you selected something that was not a .json", "OK");
                    json = null;
                    return false;
                }
            }

            try
            {
                json = JsonConvert.DeserializeObject<DisbridgeKey>(System.IO.File.ReadAllText(path));
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Json Failure", "Error: JSON failed to parse!\nPlease export a new json from your server.", "OK");
                json = null;
                return false;
            }
            

            if (json.version != expectedVersion)
            {
                if (json.version == 0)
                {
                    EditorUtility.DisplayDialog("Json Failure", "Error: version number not found!\nPlease export a new json from your server.", "OK");
                }
                else if (json.version > expectedVersion)
                {
                    EditorUtility.DisplayDialog("Json Failure", $"Error: version number greater than expected!\nexpected: [{expectedVersion}]\nreceived: [{json.version}]\n\nPlease update DisBridge.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Json Failure", $"Error: version number less than expected!\nexpected: [{expectedVersion}]\nreceived: [{json.version}]\n\nPlease update DisBridge.", "OK");
                }
                return false;
            }
            return true;
        }
        
        public static void ParseJsonForRoles(DisbridgeKey myKey, PluginManager _disBridge)
        {
            //if (PrefabUtility.IsPartOfAnyPrefab(_disBridge.gameObject))
            //{
            //    PrefabUtility.UnpackPrefabInstance(_disBridge.gameObject,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
            //}
            //if (PrefabUtility.IsPartOfAnyPrefab(_disBridge.gameObject))
            //{
            //    PrefabUtility.UnpackPrefabInstance(_disBridge.gameObject,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
            //}

            if (myKey.roles.Length == 0)
            {
                EditorUtility.DisplayDialog("No Roles Found",
                    "Error: It looks like you didnt setup your server yet!\nPlease add some roles to the bot to track, then export a new Json.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Success",
                    "Import Has completed!",
                    "OK");
            }

            ParseServerKey(myKey, _disBridge);

            bool _roleFound = false;
            foreach (RoleModal _role in myKey.roles)
            {
                _roleFound = false;
                RoleContainer rc;
                for (int i = 0; i < _disBridge.roles.childCount; i++)
                {
                    rc = _disBridge.roles.GetChild(i).GetComponent<RoleContainer>();
                    if (rc.roleId == _role.roleId)
                    {

                        switch (EditorUtility.DisplayDialogComplex($"Existing Role Found: {rc.roleName}",
                                    $"{rc.roleName} Already exists!\n\nDo you want to Skip this role or Replace it?",
                                    "Replace", "Skip", "Fill Missing"))
                        {
                            case 0: // ok
                                Undo.RecordObject(rc,$"");
                                rc.roleName = _role.roleName;
                                //TODO alt name
                                //TODO icon
                                rc.roleColor = _role.GetColor();
                                rc.isBooster = _role.isBooster;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(rc);
                                break;

                            case 1: // cancel

                                break;

                            case 2: // alt
                                Undo.RecordObject(rc,$"");
                                if (rc.roleName.IsNullOrWhitespace()) rc.roleName = _role.roleName;
                                //TODO alt name
                                //TODO icon
                                if (rc.roleColor == Color.white) rc.roleColor = _role.GetColor();
                                rc.isBooster = _role.isBooster;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(rc);
                                break;

                            default:
                                break;
                        }


                        //if (EditorUtility.DisplayDialog($"Existing Role Found: {rc.roleName}", $"{rc.roleName} Already exists!\n\nDo you want to Skip this role or Replace it?", "Replace", "Skip"))
                        //{
                        //    
                        //}
                        _roleFound = true;
                        break;
                    }
                }

                if (_roleFound == false)
                {
                    //Packages\com.udonvr.disbridge\Runtime\Prefabs\Parts
                    GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.udonvr.disbridge/Runtime/Prefabs/Parts/RoleContainer.prefab", typeof(GameObject));
                    GameObject role = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _disBridge.roles);
                    Undo.RegisterCreatedObjectUndo(role,"Create Role Container");
                    rc = role.GetComponent<RoleContainer>();
                    Undo.RecordObject(role,$"Undo on {role.name}");
                    role.name = _role.roleName;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(role);
                    Undo.RecordObject(rc,$"Undo on {role.name}");
                    rc.roleId = _role.roleId;
                    rc.roleName = _role.roleName;
                    rc.roleColor = _role.GetColor();
                    rc.isBooster = _role.isBooster;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(rc);
                }
            }

        }

        private static string[] SplitKeyString(string key, int chunkSize = 8) // unsure how long keys are however 8 seems to be a good number.
        {
            List<string> chunks = new List<string>();
            for (int i = 0; i < key.Length; i += chunkSize)
            {
                string chunk = key.Substring(i, Math.Min(chunkSize, key.Length - i));
                chunks.Add(chunk);
            }

            return chunks.ToArray();
        }

        /// <summary>
        /// Parses and adds a server to a Plugin Manager
        /// </summary>
        /// <param name="_key"> - json of the import, use GetJson to get the import/</param>
        /// <param name="_disBridge"></param>
        /// <returns> int<br></br>'-1' user canceled<br></br>'>=0' index of the imported server</returns>
        public static int ParseServerKey(DisbridgeKey _key,PluginManager _disBridge)
        {
            string __serverName = _key.serverName;
            string __keyUrl = _key.fallbackUrl;
            string __key = _key.key;
            string __primaryUrl = _key.keyJsonUrl;

            if (_disBridge._key == null || _disBridge._key.Length == 0)
            {
                Undo.RecordObject(_disBridge,$"Undo on {_disBridge.name}");
                _disBridge._key = new string[1];
                _disBridge._serverNames = new string[1];
                _disBridge._primaryUrl = new VRCUrl[1];
                _disBridge._keyUrl = new VRCUrl[1];

                _disBridge._key[0] = __key;
                _disBridge._serverNames[0] = __serverName;
                _disBridge._keyUrl[0] = new VRCUrl(__keyUrl);
                _disBridge._primaryUrl[0] = new VRCUrl(__primaryUrl);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_disBridge);
                return 0;
            }
            
            for (int i = 0; i < _disBridge._serverNames.Length; i++)
            {
                if (_disBridge._serverNames[i] == __serverName)
                {
                    bool selection = EditorUtility.DisplayDialog("Existing Server Found.",
                        "Would you like to overwrite this server?" +
                        "\n" +
                        "\n" +
                        $"\n{__serverName}" +
                        $"\n======================================" +
                        $"\nServer Key:" +
                        $"\n{_disBridge._key[i]}\n{__key}" +
                        $"\n======================================" +
                        $"\nServer Primary URL:" +
                        $"\n{_disBridge._primaryUrl[i]}\n{__primaryUrl}" +
                        $"\n======================================" +
                        $"\nServer Fallback URL:" +
                        $"\n{_disBridge._keyUrl[i]}\n{__keyUrl}", "Replace", "Skip");
                    if (selection)
                    {
                        Undo.RecordObject(_disBridge,$"Undo on {_disBridge.name}");
                        _disBridge._key[i] = __key;
                        _disBridge._serverNames[i] = __serverName;
                        _disBridge._keyUrl[i] = new VRCUrl(__keyUrl);
                        _disBridge._primaryUrl[i] = new VRCUrl(__primaryUrl);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_disBridge);
                        return i;
                    }
                    return -1;
                }
            }
            int newLength = _disBridge._serverNames.Length + 1;
            Undo.RecordObject(_disBridge,$"Undo on {_disBridge.name}");
            Array.Resize(ref _disBridge._key, newLength);
            Array.Resize(ref _disBridge._serverNames, newLength);
            Array.Resize(ref _disBridge._primaryUrl, newLength);
            Array.Resize(ref _disBridge._keyUrl, newLength);

            _disBridge._key[^1] = __key;
            _disBridge._serverNames[^1] = __serverName;
            _disBridge._keyUrl[^1] = new VRCUrl(__keyUrl);
            _disBridge._primaryUrl[^1] = new VRCUrl(__primaryUrl);
            PrefabUtility.RecordPrefabInstancePropertyModifications(_disBridge);
            
            return -1;
        }
    }
}