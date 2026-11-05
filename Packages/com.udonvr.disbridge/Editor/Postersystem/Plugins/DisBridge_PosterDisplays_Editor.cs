//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UdonVR.DisBridge.Editors;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonVR.DisBridge.PosterSystem.Editors
{
    [CustomEditor(typeof(DisBridge_PosterDisplays))]
    public class DisBridge_PosterDisplays_Editor : Editor
    {
        
        bool useLargeImage = false;
        private bool collapseTexture = false;
        private bool collapseMesh = false;

        //////////////////////////////////////////////////////////////////////////
        #region GUI
        
            public override void OnInspectorGUI()
            {
                DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
                EditorFunctions.PlaymodeWarning(myScript);
                DrawDefaultInspector();
                EditorFunctions.GuiLine();
                bool _useLoader = EditorGUILayout.Toggle("Use ImageLoader:",myScript.useLoader);
                if (myScript.useLoader != _useLoader)
                {
                    Undo.RecordObject(myScript,$"Changed useLoader: {myScript.useLoader} => {_useLoader}");
                    myScript.useLoader = _useLoader;
                }

                if (_useLoader)
                {
                    EditorFunctions.DrawImageloaderCheck(myScript, false, true);
                    if (myScript.posterTextures != null && myScript.posterTextures.Length > 0)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Imageloader chosen but Displaycontroller still has Textures assigned to it. This wont break anything but may allow un-used Textures to be included in Build.\nIt is recommended to clear these",MessageType.Warning);
                        if (GUILayout.Button("Clear textures",GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                        {
                            Undo.RecordObject(myScript,$"Clearing Textures");
                            Array.Resize(ref myScript.posterTextures,0);
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorFunctions.GuiLine();
                }
                else
                {
                    DrawTextures();
                }
                DrawDisplays();
                DrawDebugMode();
                PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            }

        #endregion
        //////////////////////////////////////////////////////////////////////////

        #region Textures

        private void DrawTextures()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add texture"))
            {
                AddTexture();
            }
            if (GUILayout.Button("Remove texture"))
            {
                RemoveTexture();
            }
            if (GUILayout.Button(useLargeImage ? "Large" : "Small",GUILayout.Width(100)))
            {
                collapseTexture = false;
                useLargeImage = !useLargeImage;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            
            EditorGUILayout.IntField("textureSize: (readonly)", myScript.posterTextures.Length);
            
            if (GUILayout.Button(collapseTexture ? "Expand" : "Collapse",GUILayout.Width(125)))
            {
                collapseTexture = !collapseTexture;
            }
            
            GUILayout.EndHorizontal();
            
            int _texLegth = myScript.posterTextures.Length;
            Texture _tex;
            if (!collapseTexture)
            {
                for (int i = 0; i < _texLegth; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (useLargeImage)
                    {
                        _tex = (Texture)EditorGUILayout.ObjectField($"Texture {i}:",myScript.posterTextures[i],typeof(Texture),false);
                    }
                    else
                    {
                        _tex = (Texture)EditorGUILayout.ObjectField($"Texture {i}:",myScript.posterTextures[i],typeof(Texture),false,GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }
                
                    if (myScript.posterTextures[i] != _tex)
                    {
                        Undo.RecordObject(myScript,$"Changed Texture {i}");
                        myScript.posterTextures[i] = _tex;
                    }
               
                    if (GUILayout.Button("Remove",GUILayout.Width(100)))
                    {
                        RemoveTexture(i);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            EditorFunctions.GuiLine();
        }
        
        private void AddTexture()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
            
            Undo.RecordObject(myScript,$"Added Texture");
            
            int _i = myScript.posterTextures.Length + 1;
            Array.Resize(ref myScript.posterTextures, _i);

            collapseTexture = false;
        }
        
        private void RemoveTexture(int _index = -1)
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;

            Undo.RecordObject(myScript,$"RemoveDisplay: {_index}");
            
            int _i = myScript.posterTextures.Length;
            Texture[] tmp_1 = new Texture[_i];

            int _p = myScript.posterTextures.Length - 1;

            int _offset = 0;
            for (int i = 0; i < _p; i++)
            {
                if (i == _index) _offset += 1;
                tmp_1[i] = myScript.posterTextures[i + _offset];
            }
        
            myScript.posterTextures = new Texture[_p];

            for (int i = 0; i < _p; i++)
            {
                myScript.posterTextures[i] = tmp_1[i];
            }
            
            collapseTexture = false;
        }

        #endregion
        
        //////////////////////////////////////////////////////////////////////////
        #region Displays

        private void DrawDisplays()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Display")) AddDisplay();
            if (GUILayout.Button("Remove Display")) RemoveDisplay();
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.IntField("display amount: (readonly)", myScript.displays.Length);
            
            if (GUILayout.Button(collapseMesh ? "Expand" : "Collapse",GUILayout.Width(125)))
            {
                collapseMesh = !collapseMesh;
            }
            
            GUILayout.EndHorizontal();
            if (!collapseMesh)
            {
                int _displays = myScript.displays.Length;
                for (int i = 0; i < _displays; i++)
                {
                    GUILayout.BeginHorizontal();
                    int _val = EditorGUILayout.IntField(myScript.displayOffsets[i],GUILayout.Width(50));
                    if (myScript.displayOffsets[i] != _val)
                    {
                        Undo.RecordObject(myScript,$"Changed type: {myScript.displayOffsets[i]} => {_val}");
                        myScript.displayOffsets[i] = _val;
                    }
                    MeshRenderer _rend = (MeshRenderer)EditorGUILayout.ObjectField( myScript.displays[i], typeof(MeshRenderer), true);
                    if (myScript.displays[i] != _rend)
                    {
                        Undo.RecordObject(myScript,$"Changed type: {myScript.displays[i]} => {_val}");
                        myScript.displays[i] = _rend;
                    }

                    if (myScript.displays[i] == null)
                    {
                        if (GUILayout.Button("Create",GUILayout.Width(75)))
                        {
                            myScript.displays[i] = EditorFunctions.CreatePrefab(@"Packages\com.udonvr.disbridge\Runtime\PosterSystem\Scripts\plugins\Displays\UdonPoster.prefab",myScript.gameObject, true, false, false).GetComponent<MeshRenderer>();
                            myScript.displays[i].name = $"UdonVRPoster ({i})";
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Select",GUILayout.Width(50)))
                        {
                            Selection.activeObject = myScript.displays[i].gameObject;
                        }
                    }
                
                    if (GUILayout.Button("Remove",GUILayout.Width(100)))
                    {
                        RemoveDisplay(i);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"Child count does not match Display count: {myScript.displays.Length}/{myScript.transform.childCount}", MessageType.None);
            if (myScript.transform.childCount <= myScript.displays.Length)
            {
                if (GUILayout.Button("Get Displays",GUILayout.Width(125)))
                {
                    CollectTheChildren();
                }
            }
            else
            {
                if (GUILayout.Button("Match Children",GUILayout.Width(125)))
                {
                    Undo.RecordObject(myScript,$"Match Children");
                    
                    myScript.displays = new MeshRenderer[myScript.transform.childCount];
                    Array.Resize(ref myScript.displayOffsets, myScript.transform.childCount);
                    for (int i = 0; i < myScript.transform.childCount; i++)
                    {
                        myScript.displays[i] = myScript.transform.GetChild(i).GetComponent<MeshRenderer>();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void AddDisplay()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
            
            Undo.RecordObject(myScript,$"Added Display");
            
            int _i = myScript.displays.Length + 1;
            Array.Resize(ref myScript.displays, _i);
            Array.Resize(ref myScript.displayOffsets, _i);
        }
        
        private void RemoveDisplay(int _index = -1)
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;

            Undo.RecordObject(myScript,$"RemoveDisplay: {_index}");
            
            int _i = myScript.displays.Length;
            MeshRenderer[] tmp_1 = new MeshRenderer[_i];
            int[] tmp_2 = new int[_i];
            Material[] tmp_3 = new Material[_i];

            int _p = myScript.displays.Length - 1;

            int _offset = 0;
            for (int i = 0; i < _p; i++)
            {
                if (i == _index) _offset += 1;
                tmp_1[i] = myScript.displays[i + _offset];
                tmp_2[i] = myScript.displayOffsets[i + _offset];
            }
        
            myScript.displays = new MeshRenderer[_p];
            myScript.displayOffsets = new int[_p];

            for (int i = 0; i < _p; i++)
            {
                myScript.displays[i] = tmp_1[i];
                myScript.displayOffsets[i] = tmp_2[i];
            }
        }
        #endregion
        //////////////////////////////////////////////////////////////////////////
        #region Functions

        private void CollectTheChildren()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
            int l = myScript.displays.Length;
            Undo.RecordObject(myScript,$"CollectTheChildren");
            myScript.displays = myScript.GetComponentsInChildren<MeshRenderer>(true);
            Array.Resize(ref myScript.displays, l);
        }

        public void DrawDebugMode()
        {
            DisBridge_PosterDisplays myScript = (DisBridge_PosterDisplays)target;
            bool _isDebug = EditorGUILayout.Toggle(myScript.isDebug);
            if (myScript.isDebug != _isDebug)
            {
                Undo.RecordObject(myScript,$"Changed type: {myScript.isDebug} => {myScript}");
                myScript.isDebug = _isDebug;
            }
            
            if (myScript.isDebug)
            {
                EditorGUILayout.HelpBox("Debugmode is on -- DO NOT EDIT THESE !!!",MessageType.Error);
                for (int i = 0; i < myScript.displays.Length; i++)
                {
                    myScript.displays[i] = (MeshRenderer)EditorGUILayout.ObjectField($"Display {i}:", myScript.displays[i], typeof(MeshRenderer), true);
                }
                EditorFunctions.GuiLine();
                for (int i = 0; i < myScript._materials.Length; i++)
                {
                    myScript._materials[i] = (Material)EditorGUILayout.ObjectField($"Material {i}:", myScript._materials[i], typeof(Material), false);
                }
                /*
                EditorFunctions.GuiLine();
                for (int i = 0; i < myScript.posterTextures.Length; i++)
                {
                    myScript.posterTextures[i] = (Texture)EditorGUILayout.ObjectField($"Texture {i}:", myScript.posterTextures[i], typeof(Texture));
                }
                */
            }
        }
        #endregion
        
    }
}

