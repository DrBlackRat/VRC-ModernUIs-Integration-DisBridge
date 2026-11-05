//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using UdonSharp;
using UnityEditor;
using UnityEngine;
//using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon;
//using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = System.Object;
using UdonVR.DisBridge.PosterSystem;

namespace UdonVR.DisBridge.Editors.Postersystem
{

    
    [CustomEditor(typeof(DisBridgeImageLoader))]
    public class DisBridgeImageLoader_Editor : Editor
    {
        //private string imgUrl = "https://udonvr-db.github.io/---/Image";
    //////////////////////////////////////////////////////////////////////////
    #region GUI
        private string[] urlTypes = new string[2]
        { "DisBridge", "Custom" };
        
        public override void OnInspectorGUI()
        {
            DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
            EditorFunctions.PlaymodeWarning(myScript);
            
            ///////////////////////////////////////////////////////////////////////////
            #region Variables
            DrawPluginmanager();
            EditorFunctions.GuiLine();
            DrawPlugins();
            Material _mat;
            _mat = (Material)EditorGUILayout.ObjectField("Fallback Image material", myScript.fallbackPoster, typeof(Material), false);
            if (myScript.fallbackPoster != _mat)
            {
                Undo.RecordObject(myScript,$"Changed fallbackPoster");
                myScript.fallbackPoster = _mat;
            }
            
            _mat = (Material)EditorGUILayout.ObjectField("Loading", myScript.loading, typeof(Material), false);
            if (myScript.loading != _mat)
            {
                Undo.RecordObject(myScript,$"Changed Loading material");
                myScript.loading = _mat;
            }
            
            EditorFunctions.GuiLine();
            _mat = (Material)EditorGUILayout.ObjectField("Unknown", myScript.unknown, typeof(Material), false);
            if (myScript.unknown != _mat)
            {
                Undo.RecordObject(myScript,$"Changed Unknown material");
                myScript.unknown = _mat;
            }
            
            _mat = (Material)EditorGUILayout.ObjectField("Access Denied", myScript.accessDenied, typeof(Material), false);
            if (myScript.accessDenied != _mat)
            {
                Undo.RecordObject(myScript,$"Changed Access Denied material");
                myScript.accessDenied = _mat;
            }

            _mat = (Material)EditorGUILayout.ObjectField("Download Error", myScript.downloadError, typeof(Material), false);
            if (myScript.downloadError != _mat)
            {
                Undo.RecordObject(myScript,$"Changed downloadError material");
                myScript.downloadError = _mat;
            }
            
            _mat = (Material)EditorGUILayout.ObjectField("Invalid Image", myScript.invalidImage, typeof(Material), false);
            if (myScript.invalidImage != _mat)
            {
                Undo.RecordObject(myScript,$"Changed Invalid Image material");
                myScript.invalidImage = _mat;
            }
            
            _mat = (Material)EditorGUILayout.ObjectField("Invalid URL", myScript.invalidURL, typeof(Material), false);
            if (myScript.invalidURL != _mat)
            {
                Undo.RecordObject(myScript,$"Changed invalidURL material");
                myScript.invalidURL = _mat;
            }

            
            #endregion
            EditorFunctions.GuiLine();

            
            DrawURLs();

            if (!Application.isPlaying)
            {
                DrawImageContainers();
            }

            DrawDebugMode();
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        } 
    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    #region URLs

    private void DrawPluginmanager()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        
        PluginManager _pluginManager = (PluginManager)EditorGUILayout.ObjectField("DisBridge Plugin Manager (optional)", myScript.pluginManager, typeof(PluginManager), true);
        if (myScript.pluginManager != _pluginManager)
        {
            Undo.RecordObject(myScript,$"Changed type: {myScript.pluginManager} => {_pluginManager}");
            myScript.pluginManager = _pluginManager;
        }
        
        if (myScript.pluginManager == null)
        {
            PluginManager[] managers = FindObjectsOfType<PluginManager>();
            if (managers == null || managers.Length > 1)
            {
                
                EditorGUILayout.HelpBox("!!! No Plugin Manager bound !!!\n\n multiple Plugin Managers were found, please bind one manually or below.", MessageType.Error);

                for (int i = 0; i < managers.Length; i++)
                {
                    if(GUILayout.Button($"{managers[i].name} on parent {managers[i].transform.parent.name}"))
                    {
                        Undo.RecordObject(myScript,$"Changed type: {myScript.pluginManager} => {managers[i]}");
                        myScript.pluginManager = managers[i];
                    }
                }
            }
            else if (managers.Length == 0)
            {
                EditorGUILayout.HelpBox("!!! No Plugin Manager bound !!!\n\n DisBridge URLs not available when there is no Plugin Manager.",MessageType.Error);
            }
            else
            {
                Undo.RecordObject(myScript,$"Changed type: {myScript.pluginManager} => {managers[0]}");
                myScript.pluginManager = managers[0];
            }
        }
        else if (myScript.pluginManager._keyUrl.ToString().IsNullOrWhitespace())
        {
            EditorGUILayout.HelpBox("!!! Plugin manager has no key !!!\n\n DisBridge URLs not available when there is no key attached to your Plugin Manager. Please import your server settings into DisBridge.",MessageType.Error);
            if(GUILayout.Button($"Go to DisBridge"))
            {
                Selection.activeObject = myScript.pluginManager.transform.parent;
            }
        }
    }

    private void DrawPlugins()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        if (myScript.plugins != null && myScript.plugins.Length > 0)
        {
            EditorGUILayout.HelpBox("Loaded Plugins",MessageType.None);
            for (int i = 0; i < myScript.plugins.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField($"Plugin {i}:",myScript.plugins[i], typeof(GameObject), true);
                if (GUILayout.Button("Select",GUILayout.Width(50)))
                {
                    Selection.activeObject = myScript.plugins[i];
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorFunctions.GuiLine();
        }
    }
    private void DrawURLs()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("URLs are not editable in Playmode.",MessageType.Error);
        }
        
        
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        
        EditorGUILayout.BeginHorizontal();
        
        if(GUILayout.Button("Add Url"))
        {
            AddUrl();
        }
        
        if(GUILayout.Button("Remove Url"))
        {
            RemoveUrl();
        }
        
        GUILayout.EndHorizontal();
        
        for (int i = 0; i < myScript.urls.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            Undo.RecordObject(myScript,$"Changed sel:");
            myScript.urlTypes[i] = EditorGUILayout.Popup(myScript.urlTypes[i], urlTypes, GUILayout.Width(100));
            if (myScript.urlTypes[i] == 0)
            {
                int _urlSlot = EditorGUILayout.IntField(myScript.urlSlots[i],GUILayout.Width(25));
                if (myScript.urlSlots[i] != _urlSlot)
                {
                    Undo.RecordObject(myScript,$"Changed urlSlots[{i}]: {myScript.urlSlots[i]} => {_urlSlot}");
                    myScript.urlSlots[i] = _urlSlot;
                }
                if (myScript.pluginManager != null && myScript.pluginManager._keyUrl != null && myScript.pluginManager._keyUrl.Length != 0 && !String.IsNullOrWhiteSpace(myScript.pluginManager._keyUrl[0].ToString()) && myScript.pluginManager._keyUrl[0].ToString().Length >= ("https://udonvr-db.github.io/DB/").Length)
                {
                    string uwu = myScript.pluginManager._keyUrl[0].ToString().Replace("https://udonvr-db.github.io/DB/","");
                    uwu = uwu.Replace("https://udonvr-db.github.io/","");
                    uwu = uwu.Replace("/String","");
                    uwu = uwu.Replace("/Json","");
                    myScript.urls[i] = new VRCUrl($"https://api.disbridge.com/api/serve/{uwu}/images/{myScript.urlSlots[i]}.png");
                    
                }
                else
                {
                    myScript.urls[i] = new VRCUrl($"https://api.disbridge.com/api/serve/---/images/{myScript.urlSlots[i]}.png");
                }
                EditorGUILayout.TextField(myScript.urls[i].ToString());
            }
            else
            {
                string _urlCustom = EditorGUILayout.TextField( $"", myScript.urlCustoms[i] );
                if (myScript.urlCustoms[i] != _urlCustom)
                {
                    Undo.RecordObject(myScript,$"Changed urlCustoms[{i}]: {myScript.urlCustoms[i]} => {_urlCustom}");
                    myScript.urlCustoms[i] = _urlCustom;
                    myScript.urls[i] = new VRC.SDKBase.VRCUrl(myScript.urlCustoms[i]);
                }
            }

            if(GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
            {
                RemoveUrl(i);
            }
            
            GUILayout.EndHorizontal();
        }
    }

    private void AddUrl()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        
        Undo.RecordObject(myScript,"Add Url");
        
        int _p = myScript.urls.Length;
        
        int[] tempUrlTypes = new int[_p];
        int[] tmpUrlSlots = new int[_p];
        string[] tmpurlCustoms = new string[_p];
        VRCUrl[] tempUrls = new VRCUrl[_p];
        

        for (int i = 0; i < _p; i++)
        {
            tempUrlTypes[i] = myScript.urlTypes[i];
            tmpUrlSlots[i] = myScript.urlSlots[i];
            tmpurlCustoms[i] = myScript.urlCustoms[i];
            tempUrls[i] = myScript.urls[i];
        }
            
        myScript.urlTypes = new int[_p + 1];
        myScript.urlTypes[myScript.urlTypes.Length - 1] = 0;
        
        myScript.urlSlots = new int[_p + 1];
        

        myScript.urlCustoms = new string[_p + 1];
        
        myScript.urls = new VRCUrl[_p + 1];
        

        for (int i = 0; i < _p; i++)
        {
            myScript.urlTypes[i] = tempUrlTypes[i];
            myScript.urlSlots[i] = tmpUrlSlots[i];
            myScript.urlCustoms[i] = tmpurlCustoms[i];
            myScript.urls[i] = tempUrls[i];
        }
        
        try
        {
            //Debug.Log("uwu me harder");
            //Debug.Log($"{myScript.urlSlots[myScript.urlTypes.Length - 1]} = {myScript.urlSlots[_p] + 1}");
            myScript.urlSlots[myScript.urlTypes.Length - 1] = myScript.urlSlots[myScript.urlTypes.Length - 2] + 1;
            //Debug.Log("i've been uwu'ed hard enough father please spare me");
        }
        catch
        {
            //Debug.Log("Please father, i have not had enough uwu and require more");
            myScript.urlSlots[myScript.urlTypes.Length - 1] = 0;
        }
        
    }

    private void RemoveUrl(int _index = -1)
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        
        Undo.RecordObject(myScript,"Remove Url");
        
        int _p = myScript.urls.Length - 1;
        
        int[] tempUrlTypes = new int[_p];
        int[] tmpUrlSlots = new int[_p];
        string[] tmpurlCustoms = new string[_p];
        VRCUrl[] tempUrls = new VRCUrl[_p];

        int _offset = 0;
        for (int i = 0; i < _p; i++)
        {
            if (i == _index) _offset += 1;
            tempUrlTypes[i] = myScript.urlTypes[i + _offset];
            tmpUrlSlots[i] = myScript.urlSlots[i + _offset];
            tmpurlCustoms[i] = myScript.urlCustoms[i + _offset];
            tempUrls[i] = myScript.urls[i + _offset];
        }
        
        myScript.urls = new VRCUrl[_p];
        myScript.urlSlots = new int[_p];
        myScript.urlCustoms = new string[_p];
        myScript.urlTypes = new int[_p];
        for (int i = 0; i < _p; i++)
        {
            myScript.urlTypes[i] = tempUrlTypes[i];
            myScript.urlSlots[i] = tmpUrlSlots[i];
            myScript.urlCustoms[i] = tmpurlCustoms[i];
            myScript.urls[i] = tempUrls[i];
        }
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Functions

    private void DrawImageContainers()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        Transform tmpTrans = myScript.transform;
        
        if (tmpTrans.childCount != myScript.urls.Length)
        {
            int howManySouls = (tmpTrans.childCount - myScript.urls.Length);

            if (howManySouls > 0)
            {
                //Create more children
                for (int i = 0; i < howManySouls; i++)
                {
                    DestroyImmediate(tmpTrans.GetChild(i).gameObject);
                }
            }
            else
            {
                //Consume the children
                howManySouls = Mathf.Abs(howManySouls);
                for (int i = 0; i < howManySouls; i++)
                {
                    EditorFunctions.CreatePrefab("Packages/com.udonvr.disbridge/Editor/Resources/Prefabs/DisBridge ImageContainer.prefab", myScript.gameObject, false, false);
                    //Instantiate(tmpTrans.GetChild(0).gameObject, myScript.transform);
                }
            }
        }
        //myScript.imageContainers = myScript.GetComponentsInChildren<DisBridgeImageContainer>();
        DisBridgeImageContainer[] containers = myScript.GetComponentsInChildren<DisBridgeImageContainer>();
        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].gameObject.name = $"ImageContainer ({i})";
            containers[i].loader = myScript;
            containers[i].renderer = containers[i].GetComponent<MeshRenderer>();
            containers[i].imageUrl = myScript.urls[i];
        }
    }
    
    public void ResetUrls()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;

        myScript.urls = Array.Empty<VRCUrl>();
        myScript.urlTypes = Array.Empty<int>();
        myScript.urlTypes = Array.Empty<int>();
        myScript.urlCustoms = Array.Empty<string>();
    }
    
    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////////////
        
    public void DrawDebugMode()
    {
        DisBridgeImageLoader myScript = (DisBridgeImageLoader)target;
        bool _isDebug = EditorGUILayout.Toggle(myScript.isDebug);
        if (myScript.isDebug != _isDebug)
        {
            Undo.RecordObject(myScript,$"Changed type: {myScript.isDebug} => {myScript}");
            myScript.isDebug = _isDebug;
        }
        if (myScript.isDebug)
        {
            EditorGUILayout.HelpBox("Debugmode is on -- DO NOT EDIT THESE !!!",MessageType.Error);
            DrawDefaultInspector();
        }
    }
            
    }
}
