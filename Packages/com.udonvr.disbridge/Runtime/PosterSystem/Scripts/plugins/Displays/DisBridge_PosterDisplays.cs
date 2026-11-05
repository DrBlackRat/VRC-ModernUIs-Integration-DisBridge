//Copyright (c) 2024 UdonVR LLC - All Rights Reserved
using System;
using TMPro;
using UdonSharp;
using UnityEngine;  
using UnityEngine.UI;
using VRC.SDKBase;
using UdonVR.DisBridge;
using VRC.Udon.Wrapper.Modules;

namespace UdonVR.DisBridge.PosterSystem
{
	public class DisBridge_PosterDisplays : DisBridgeImageLoaderPlugin
	{

		[Header("Displays")] 
		[HideInInspector]public MeshRenderer[] displays;
		[HideInInspector]public Material[] _materials;
		[HideInInspector][Tooltip("This is a great tooltip")]public int[] displayOffsets;
		[HideInInspector]public bool useLoader = true;

		private int _texIndex;
		[UdonSynced] private int N_texIndex = 1;

		[HideInInspector] [Header("Posters")] public Texture[] posterTextures;

		[Header("Transition Settings")] public string keyword = "_Position";
		public float delay = 0.7f;
		public float speed = 10f;

		private float startPoint = 3f;
		private float endPoint = 0f;

		[UdonSynced] private float N_timeCounterLoop = 0;
		//[UdonSynced] private float N_timeCounter = 0;
		private float _timeCounterLoop = 0;
		//private float _timeCounter = 0;

		private float _speed = 1;

		private bool hasinit = false;
		private bool hasLoader = false;

		//public TextMeshPro debugOut;


		void Start()
		{
			if (useLoader && imageLoader != null)
			{
				hasLoader = true;
				
				imageLoader.AddPlugin(gameObject);
				
				for (int i = 0; i < posterTextures.Length; i++)
				{
					posterTextures[i] = imageLoader.fallbackPosterTexture;
				}
				InitMaterials();

				_speed = (endPoint - startPoint);
			}
			else
			{
				InitMaterials();
				_speed = (endPoint - startPoint);
				hasinit = true;
				changetexture();
			}
		}

		private void InitMaterials()
		{
			if (_materials != null && _materials.Length == displays.Length) return;

			int j = 0;
			for (int i = 0; i < displays.Length; i++)
			{ if (displays[i] != null) j++; }

			if (j > 0)
			{
				MeshRenderer[] tmpArray = new MeshRenderer[j];
				j = 0;
				for (int i = 0; i < displays.Length; i++)
				{
					if (displays[i] != null)
					{
						tmpArray[j] = displays[i];
						j++;
					}
				}

				displays = tmpArray;
			}
			
			_materials = new Material[displays.Length];
			for (int i = 0; i < displays.Length; i++)
			{
				_materials[i] = displays[i].material;
			} 
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////

		#region Loop
		
		void Update()
		{
			_timeCounterLoop += Time.deltaTime * speed / 100;
			//_timeCounter = _timeCounterLoop;

			if (_timeCounterLoop > 1 + delay)
			{
				_timeCounterLoop = 0;
				//_timeCounter = 0;
				changetexture();
			}


			float _preCal = Mathf.Clamp(startPoint + Mathf.Clamp01(_timeCounterLoop) * _speed, 0, 2);
			foreach (var _material in _materials)
			{
				_material.SetFloat(keyword,_preCal);
			}

		}

		private int _i;

		void changetexture()
		{
			if (isDebug)
			{
				Debug.LogError($"changetexture");
				Debug.LogError($"posterTextures.Length = {posterTextures.Length}");
			}

			if (!hasinit)
			{
				Debug.LogError($"hasinit is false");
				return;
			}
			if (posterTextures == null)
			{
				Debug.LogError($"is null");
				return;
			}
			if (posterTextures.Length == 0)
			{
				Debug.LogError($"is 0");
				return;
			}
			
			_texIndex = (_texIndex < 0 || _texIndex >= posterTextures.Length) ? 0 : _texIndex;

			InitMaterials();
			if (isDebug) Debug.LogError($"_materials.Length = {_materials.Length}");
			for (int i = 0; i < _materials.Length; i++)
			{
				_i = _texIndex + displayOffsets[i];
				if (isDebug) Debug.LogError($"_i = {_i}");
				while (_i >= posterTextures.Length && _i != 0)
					//rollover poster slots so index isn't out of bounds
				{
					_i -= posterTextures.Length;
				}

				if (isDebug) Debug.LogError($"_i = {_i} : i = {i}");
				_materials[i].SetTexture("_MainTex", _materials[i].GetTexture("_MainTexB"));
				_materials[i].SetTexture("_MainTexB", posterTextures[_i]);
			}

			if (_texIndex < posterTextures.Length - 1)
			{
				_texIndex++;
			}
			else
			{
				_texIndex = 0;
			}

			UpdateNetworking();
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////////////////////////////

		#region Networking

		public override void OnDeserialization()
		{
			if (_texIndex == N_texIndex) return;
			_texIndex = N_texIndex;
			//_timeCounter = N_timeCounter;
			_timeCounterLoop = N_timeCounterLoop;
		}

		private void UpdateNetworking()
		{
			if (!Networking.LocalPlayer.IsOwner(gameObject)) return;
			N_texIndex = _texIndex;
			//N_timeCounter = _timeCounter;
			N_timeCounterLoop = _timeCounterLoop;
			RequestSerialization();
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////////////////////////////

		#region DisBridge

		public override void _UVR_PS_Init()
		{
			if (isDebug)Debug.LogError("DisBridge_PosterDisplays - _UVR_PS_Init - Started");
			hasinit = true;
			posterTextures = new Texture[imageLoader.urls.Length];
			for (int t = 0; t < posterTextures.Length; t++)
			{
				posterTextures[t] = imageLoader.fallbackPosterTexture;
			}
			changetexture();
		}

		public override void _UVR_PS_Update()
		{
			if (!hasinit)
			{
				Debug.LogError($"DisBridge_PosterDisplays on {gameObject}\nUPDATE CALLED BEFORE INIT, RETURNING");
				return;
			}
			//if (posterTextures == null) {Debug.LogError("NULL NULL NULL");}
			for (int t = 0; t < imageLoader.urls.Length; t++)
			{
				posterTextures[t] = imageLoader.textures[t];
			}
		}

		#endregion
	}
}