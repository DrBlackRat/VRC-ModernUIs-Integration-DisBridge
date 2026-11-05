//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace UdonVR.DisBridge.Editors
{

    struct ShowBool
    {
        public bool Key;
        public bool Url;
        public bool UrlUntrust;
    }
    [CustomEditor(typeof(PluginManager))]
    public class PluginManager_Editor : Editor
    {
        private ShowBool[] show;
        private const string baseurl = @"https://udonvr-db.github.io";

        private DisBridgePlugin[] detectedPlugins;
        private PluginManager pm;
        
        SerializedProperty githubURLs;
        SerializedProperty keys;
        SerializedProperty cloudflareURLs;
        SerializedProperty serverNames;
        SerializedProperty backend;
        private void OnValidate()
        {
            return;
            if (pm != null && pm._keyUrl != null && pm._keyUrl.Length != 0)
                // Validate the Github URLs
            {
                for (int i = 0; i < pm._keyUrl.Length; i++)
                {
                    var url = pm._keyUrl[i].Get();
                    if (!url.Contains("Json"))
                    {
                        // Define a regular expression pattern to match the ID number
                        string pattern = @"\b(\d+)\b";

                        // Use Regex.Match to find the first match in the input string
                        Match match = Regex.Match(url, pattern);

                        // Check if a match was found
                        if (match.Success)
                        {
                            // Extract the ID number from the match
                            string guildId = match.Groups[1].Value;

                            // Print the extracted ID number
                            Debug.Log("Extracted ID Number: " + guildId);
                            pm._keyUrl[i] = new VRC.SDKBase.VRCUrl($"{baseurl}/{guildId}/Json");
                            PrefabUtility.RecordPrefabInstancePropertyModifications(pm);
                        }
                        else
                        {
                            Debug.Log("No ID number found in the string.");
                        }
                    }
                }
            }
        }

        private void OnEnable()
        {
            pm = (PluginManager)target;
            githubURLs = serializedObject.FindProperty(nameof(pm._keyUrl));
            keys = serializedObject.FindProperty(nameof(pm._key));
            cloudflareURLs = serializedObject.FindProperty(nameof(pm._primaryUrl));
            serverNames = serializedObject.FindProperty(nameof(pm._serverNames));
            OnValidate();
            detectedPlugins = GameObject.FindObjectsOfType<DisBridgePlugin>();
        }


        public override void OnInspectorGUI()
        {
            EditorFunctions.DrawDisBridgeLogo();
            githubURLs = serializedObject.FindProperty(nameof(pm._keyUrl));
            keys = serializedObject.FindProperty(nameof(pm._key));
            cloudflareURLs = serializedObject.FindProperty(nameof(pm._primaryUrl));
            serverNames = serializedObject.FindProperty(nameof(pm._serverNames));
            backend = serializedObject.FindProperty(nameof(pm.backend));
            pm = (PluginManager)target;
            
            if (githubURLs == null) Debug.Log($"keyUrls Null");
            if (keys == null) Debug.Log($"Keys Null");
            if (cloudflareURLs == null) Debug.Log($"untrustedUrls Null");
            if (serverNames == null) Debug.Log($"serverNames Null");

            DrawKeyInfo();
            DrawPluginInfo();

            PrefabUtility.RecordPrefabInstancePropertyModifications(pm);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKeyInfo()
        {
            CheckForBackend();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
               // EditorGUILayout.PropertyField(keyUrls);
                GUILayout.Label("Keys:");
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    //if (GUILayout.Button("Add new key"))
                    //{
                    //    if (DisBridgeEdiitingToolFunctions.GetJson(out DisbridgeKey _key))
                    //    {
                    //        DisBridgeEdiitingToolFunctions.ParseServerKey(_key,pm);
                    //    }
                        //AddKeyArrays();
                        //Array.Resize(ref myScript.keyUrl, tmp);
                        //Array.Resize(ref myScript.key, tmp);
                        //Array.Resize(ref myScript.untrustedUrl, tmp);
                    //}
                    if (pm.backend == null) return;
                    if (pm._key == null) return;
                    bool isEmpty = pm._key.Length == 0;
                    EditorGUI.BeginDisabledGroup(isEmpty);
                    //if (GUILayout.Button($"Remove Key {(isEmpty ? " " : $"[{keys.arraySize - 1}] {pm._serverNames[keys.arraySize - 1]}")}"))
                    //{
                    //    int tmp = pm._keyUrl.Length - 1;
                    //    keyUrls.arraySize = tmp;
                    //    keys.arraySize = tmp;
                    //    untrustedUrls.arraySize = tmp;
                    //    serverNames.arraySize = tmp;
                        //Array.Resize(ref myScript.keyUrl, tmp);
                        //Array.Resize(ref myScript.key, tmp);
                        //Array.Resize(ref myScript.untrustedUrl, tmp);
                    //}
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("Add new server from JSON"))
                    {
                        if (DisBridgeEdiitingToolFunctions.GetJson(out DisbridgeKey _key))
                        {
                            DisBridgeEdiitingToolFunctions.ParseServerKey(_key,pm);
                        }
                    }
                }

                if (show == null || show.Length != pm._key.Length)
                {
                    show = new ShowBool[pm._key.Length];
                }


                if (pm._key == null || pm._keyUrl == null || pm._primaryUrl == null) return;

                for (int i = 0; i < pm._key.Length; i++)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label($"Key [{i}] {pm._serverNames[i]}");
                        EditorGUILayout.Space(1, true);
                        if (GUILayout.Button("Parse", GUILayout.Width(60)))
                        {
                            ReadJson(i);
                        }
                        EditorGUILayout.Space(10, false);
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            serverNames.DeleteArrayElementAtIndex(i);
                            githubURLs.DeleteArrayElementAtIndex(i);
                            keys.DeleteArrayElementAtIndex(i);
                            cloudflareURLs.DeleteArrayElementAtIndex(i);

                            show = new ShowBool[githubURLs.arraySize];
                            // return to pervent using stale array data
                            return;
                        }
                        if (i != pm._key.Length - 1)
                        {
                            if (GUILayout.Button("D", GUILayout.Width(20)))
                            {
                                serverNames.MoveArrayElement(i, i + 1);
                                githubURLs.MoveArrayElement(i, i + 1);
                                keys.MoveArrayElement(i, i + 1);
                                cloudflareURLs.MoveArrayElement(i, i + 1);
                                //using tupple to swap values
                                (show[i], show[i + 1]) = (show[i + 1], show[i]);
                            }
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Button("D", GUILayout.Width(20));
                            EditorGUI.EndDisabledGroup();
                        }
                        if (i != 0)
                        {
                            if (GUILayout.Button("⬆️", GUILayout.Width(20)))
                            {
                                serverNames.MoveArrayElement(i, i - 1);
                                githubURLs.MoveArrayElement(i, i - 1);
                                keys.MoveArrayElement(i, i - 1);
                                cloudflareURLs.MoveArrayElement(i, i - 1);
                                //using tupple to swap values
                                (show[i], show[i - 1]) = (show[i - 1], show[i]);
                            }
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Button("⬆️", GUILayout.Width(20));
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(show[i].Key ? "Hide Key" : "Show Key", GUILayout.Width(150)))
                            {
                                show[i].Key = !show[i].Key;
                            }

                            {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.TextField(show[i].Key ? pm._key[i] : "hidden");
                                EditorGUI.EndDisabledGroup();
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(show[i].UrlUntrust ? "Hide DisBridge Url" : "Show DisBridge Url", GUILayout.Width(150)))
                            {
                                show[i].UrlUntrust = !show[i].UrlUntrust;
                            }

                            {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.TextField(show[i].UrlUntrust ? pm._primaryUrl[i].ToString() : "hidden");
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(show[i].Url ? "Hide Github Url" : "Show Github Url", GUILayout.Width(150)))
                            {
                                show[i].Url = !show[i].Url;
                            }

                            {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.TextField(show[i].Url ? pm._keyUrl[i].ToString() : "hidden");
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawPluginInfo()
        {
            if (detectedPlugins == null) detectedPlugins = GameObject.FindObjectsOfType<DisBridgePlugin>();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Detected Plugins: ", GUILayout.MinWidth(0));
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Used by this Plugin Manager: ", GUILayout.MinWidth(0));
                    foreach (var _plugin in detectedPlugins)
                    {
                        if (_plugin.disBridge == pm)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(_plugin, typeof(DisBridgePlugin), true);
                            if (GUILayout.Button("Remove", GUILayout.Width(100f)))
                            {
                                Undo.RecordObject(_plugin, $"Undo on {_plugin.name}");
                                _plugin.disBridge = null;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(_plugin);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Not Used: ", GUILayout.MinWidth(0));
                    foreach (var _plugin in detectedPlugins)
                    {
                        if (_plugin.disBridge != pm)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(_plugin, typeof(DisBridgePlugin), true);
                            if (GUILayout.Button("Add", GUILayout.Width(100f)))
                            {
                                Undo.RecordObject(_plugin, $"Undo on {_plugin.name}");
                                _plugin.disBridge = pm;
                                PrefabUtility.RecordPrefabInstancePropertyModifications(_plugin);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void AddKeyArrays()
        {
            int tmp = 0;
            if (pm._keyUrl is null)
            {
                EditorUtility.DisplayDialog("Error", "Please Import First!", "OK");
            } 
            else
            {
                tmp = pm._keyUrl.Length + 1;
            }
            
            githubURLs.arraySize = tmp;
            keys.arraySize = tmp;
            cloudflareURLs.arraySize = tmp;
            serverNames.arraySize = tmp;
            githubURLs.GetArrayElementAtIndex(tmp - 1).boxedValue = VRCUrl.Empty;
            cloudflareURLs.GetArrayElementAtIndex(tmp - 1).boxedValue = VRCUrl.Empty;
            keys.GetArrayElementAtIndex(tmp - 1).stringValue = string.Empty;
            serverNames.GetArrayElementAtIndex(tmp - 1).stringValue = string.Empty;
            serializedObject.ApplyModifiedProperties();
        }
        private bool ReadJson(int index)
        {
            if (pm._key == null || pm._key.Length <= index)
            {
                EditorUtility.DisplayDialog("Error", "Error message", "OK");
                return false; //TODO Prompt user of error
            }
            if (pm._keyUrl == null || pm._keyUrl.Length <= index)
            {
                EditorUtility.DisplayDialog("Error", "Error message", "OK");
                return false; //TODO Prompt user of error
            }
            if (pm._serverNames == null || pm._serverNames.Length <= index)
            {
                EditorUtility.DisplayDialog("Error", "Error message", "OK");
                return false; //TODO Prompt user of error
            }
            if (pm._primaryUrl == null || pm._primaryUrl.Length <= index)
            {
                EditorUtility.DisplayDialog("Error", "Error message", "OK");
                return false; //TODO Prompt user of error
            }

            string path = EditorUtility.OpenFilePanel("Pick Json file", "Downloads", "json");


            //File.Copy(path, $"{_folderpath}/Runtime/EditorScript/Roles.json", true);
            //AssetDatabase.ImportAsset($"{_folderpath}/Runtime/EditorScript/Roles.json");

            if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".json"))
            {
                EditorUtility.DisplayDialog("Select Json", "Error: You did not select a file, Or you selected something that was not a .json", "OK");
                return false;
            }
            else
            {
                string jsonStr = File.ReadAllText(path);
                //TextAsset ta = (TextAsset)AssetDatabase.LoadAssetAtPath($"{_folderpath}/Runtime/EditorScript/Roles.json", typeof(TextAsset));

                DisbridgeKey keyInfo = JsonConvert.DeserializeObject<DisbridgeKey>(jsonStr);
                if (!string.IsNullOrWhiteSpace(keyInfo.key) && index < pm._key.Length)
                    pm._key[index] = keyInfo.key;
                
                // cloudflare URL -- assigned as primary URL
                if (!string.IsNullOrWhiteSpace(keyInfo.keyJsonUrl) && index < pm._primaryUrl.Length)
                    pm._primaryUrl[index] = new(keyInfo.keyJsonUrl);
                
                // github URL -- assigned as legacy KeyURL used as fallback
                if (!string.IsNullOrWhiteSpace(keyInfo.fallbackUrl) && index < pm._keyUrl.Length)
                    pm._keyUrl[index] = new(keyInfo.fallbackUrl);
                
                if (!string.IsNullOrWhiteSpace(keyInfo.serverName) && index < pm._serverNames.Length)
                    pm._serverNames[index] = keyInfo.serverName;

                return true;
            }
        }
        
        private bool CheckForBackend()
        {
            if (backend.objectReferenceValue != null) return true;
            Debug.LogError("Backend is not set!");
            DisBridgeBackend uwu = pm.transform.GetComponentInChildren<DisBridgeBackend>();
            if (uwu == null)
            {
                EditorGUILayout.HelpBox("No Backend Found", MessageType.Error);
                return false;
            }
            else
            {
                backend.objectReferenceValue = uwu;
                return true;
            }
        }
    }
}