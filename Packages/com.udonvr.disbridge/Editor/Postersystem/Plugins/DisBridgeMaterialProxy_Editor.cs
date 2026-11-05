//Copyright (c) 2025 UdonVR LLC - All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UdonVR.DisBridge.Editors;
using UdonVR.DisBridge.PosterSystem;
using UnityEditor;
using UnityEngine;

namespace UdonVR.DisBridge.PosterSystem.Editors
{

    [CustomEditor(typeof(DisBridgeMaterialProxy))]
    public class DisBridgeMaterialProxy_Editor : Editor
    {
        private void OnEnable()
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;
            for (int i = 0; i < myScript.materials.Length; i++)
            {
                foreach (var _keyword in myScript.keywords)
                {
                    if (myScript.materials[i].GetTexture(_keyword) == null)
                        myScript.materials[i].SetTexture(_keyword, myScript.fallbackImages[i]);
                }
            }
        }

        private void matchMaterials()
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;

            Undo.RecordObject(myScript, "matchMaterials");

            Texture _tex;
            for (int i = 0; i < myScript.materials.Length; i++)
            {
                foreach (var _keyword in myScript.keywords)
                {
                    _tex = myScript.materials[i].GetTexture(_keyword);
                    if (_tex == null)
                    {
                        myScript.materials[i].SetTexture(_keyword, myScript.fallbackImages[i]);
                    }
                    else
                    {
                        myScript.fallbackImages[i] = _tex;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;
            EditorFunctions.PlaymodeWarning(myScript);

            EditorGUILayout.HelpBox(
                "This Plugin is for passing a loaded in texture into a material for you to use in your scene.",
                MessageType.Info);

            EditorFunctions.DrawImageloaderCheck(myScript);

            //bool _bool = EditorGUILayout.Toggle("Use Fallback Image",myScript.useFallback);
            //if (myScript.useFallback != _bool)
            //{
            //    Undo.RecordObject(myScript,$"Changed Use Fallback Image: {myScript.useFallback} => {_bool}");
            //    myScript.useFallback = _bool;
            //}

            //EditorFunctions.GuiLine();

            BuildStringArray();

            EditorFunctions.GuiLine();

            EditorGUILayout.BeginHorizontal();

            BuildMaterialArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        #region materals

        private void BuildMaterialArray()
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;
            if (GUILayout.Button("Add Material"))
            {
                Undo.RecordObject(myScript, $"Add Material");
                int _i = myScript.materialIndex.Length + 1;

                Array.Resize(ref myScript.materialIndex, _i);
                Array.Resize(ref myScript.materials, _i);
                Array.Resize(ref myScript.hasPulled, _i);
                Array.Resize(ref myScript.fallbackImages, _i);
            }

            if (GUILayout.Button("Remove Material"))
            {
                RemoveIndex();
            }

            GUILayout.EndHorizontal();

            for (int i = 0; i < myScript.materialIndex.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                int _matIndex = EditorGUILayout.IntField($"", myScript.materialIndex[i], GUILayout.Width(100),
                    GUILayout.ExpandWidth(false));
                if (myScript.materialIndex[i] != _matIndex)
                {
                    Undo.RecordObject(myScript,
                        $"Changed materialIndex[{i}]: {myScript.materialIndex[i]} => {_matIndex}");
                    myScript.materialIndex[i] = _matIndex;
                }

                if (myScript.imageLoader != null)
                {
                    if (myScript.materialIndex[i] >= myScript.imageLoader.urls.Length &&
                        myScript.imageLoader.urls.Length != 0)
                    {
                        myScript.materialIndex[i] = myScript.imageLoader.urls.Length - 1;
                        Debug.LogError("Index outside of bound of the URL array, clamping.");
                    }

                    if (myScript.materialIndex[i] < 0)
                    {
                        myScript.materialIndex[i] = 0;
                        Debug.LogError("Index outside of bound of the URL array, clamping.");
                    }
                }

                Material _mat =
                    (Material)EditorGUILayout.ObjectField("", myScript.materials[i], typeof(Material), false);
                if (myScript.materials[i] != _mat)
                {
                    Undo.RecordObject(myScript, $"Changed materials[{i}]: {myScript.materials[i]} => {_mat}");
                    myScript.materials[i] = _mat;
                }

                Texture _tex = (Texture)EditorGUILayout.ObjectField(myScript.fallbackImages[i], typeof(Texture), false);
                if (myScript.fallbackImages[i] != _tex)
                {
                    Undo.RecordObject(myScript, $"Updated texture on slot {i}");
                    myScript.fallbackImages[i] = _tex;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    RemoveIndex(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (myScript.imageLoader != null)
            {
                int uwu = myScript.imageLoader.urls.Length == 0 ? 0 : myScript.imageLoader.urls.Length - 1;
                EditorGUILayout.HelpBox($"index bounds = 0 - {uwu}", MessageType.None);
            }

            if (GUILayout.Button("Match Materials", GUILayout.Width(175)))
            {
                matchMaterials();
            }

            GUILayout.EndHorizontal();
        }

        private void RemoveIndex(int _index = -1)
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;

            Undo.RecordObject(myScript, $"RemoveIndex");

            int _p = myScript.materialIndex.Length - 1;
            int[] tempInt = new int[_p];
            Material[] tempMaterials = new Material[_p];
            bool[] tempBools = new bool[_p];
            Texture[] _tempFallback = new Texture[_p];

            int _offset = 0;
            for (int i = 0; i < _p; i++)
            {
                if (i == _index) _offset += 1;
                tempInt[i] = myScript.materialIndex[i + _offset];
                tempMaterials[i] = myScript.materials[i + _offset];
                tempBools[i] = myScript.hasPulled[i + _offset];
                _tempFallback[i] = myScript.fallbackImages[i + _offset];
            }

            myScript.materialIndex = new int[_p];
            myScript.materials = new Material[_p];
            myScript.hasPulled = new bool[_p];
            myScript.fallbackImages = new Texture[_p];

            for (int i = 0; i < _p; i++)
            {
                myScript.materialIndex[i] = tempInt[i];
                myScript.materials[i] = tempMaterials[i];
                myScript.hasPulled[i] = tempBools[i];
                myScript.fallbackImages[i] = _tempFallback[i];
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Keywords

        private void BuildStringArray()
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Keyword"))
            {
                Undo.RecordObject(myScript, $"Add Keyword");
                int _p = myScript.keywords.Length;
                string[] tempKeywords = new string[_p];

                for (int i = 0; i < _p; i++)
                {
                    tempKeywords[i] = myScript.keywords[i];
                }

                myScript.keywords = new string[_p + 1];
                myScript.keywords[myScript.keywords.Length - 1] = "_MainTex";
                for (int i = 0; i < _p; i++)
                {
                    myScript.keywords[i] = tempKeywords[i];
                }
            }

            if (GUILayout.Button("Remove Keyword"))
            {
                RemoveKeyword();
            }

            GUILayout.EndHorizontal();

            //Debug.Log($"myScript.keywords.Length: {myScript.keywords.Length}");
            for (int i = 0; i < myScript.keywords.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                myScript.keywords[i] = EditorGUILayout.TextField($"", myScript.keywords[i]);

                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    RemoveKeyword(i);
                }

                GUILayout.EndHorizontal();
            }
        }

        private void RemoveKeyword(int _index = -1)
        {
            DisBridgeMaterialProxy myScript = (DisBridgeMaterialProxy)target;

            Undo.RecordObject(myScript, $"RemoveKeyword {_index}");

            int _p = myScript.keywords.Length - 1;
            string[] tempKeywords = new string[_p];

            int _offset = 0;
            for (int i = 0; i < _p; i++)
            {
                if (i == _index) _offset += 1;
                tempKeywords[i] = myScript.keywords[i + _offset];
            }

            myScript.keywords = new string[_p];

            for (int i = 0; i < _p; i++)
            {
                myScript.keywords[i] = tempKeywords[i];
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////

    }
}