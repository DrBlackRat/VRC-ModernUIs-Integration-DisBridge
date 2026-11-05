//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
Shader "DisBridge/PosterDisplay"
{
    Properties
    {
        _MainTex ("Poster A", 2D) = "white" {}
        [NoScaleOffset] _MainTexB ("Poster B", 2D) = "black" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Brightness("Brightness", Range(0,2)) = 1

        [Header(Transition)] [Space]
        [KeywordEnum(Mask, Scroll)] _TransitionMode ("Mode", int) = 0
        [NoScaleOffset] _TransitionMask ("Transition Mask", 2D) = "black" {}
        _Position("Swap Position", Range(0,2)) = 2

        [Header(Other)] [Space]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull [_CullMode]

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow

        #pragma target 3.0

        #pragma  shader_feature _TRANSITIONMODE_MASK _TRANSITIONMODE_SCROLL

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _MainTexB;
        sampler2D _TransitionMask;
        float _Position;
        float _Brightness;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float GetBlendValue(float2 uv)
        {
            #ifdef _TRANSITIONMODE_SCROLL
                return _Position / 2 < uv.x;
            #else
                uv.x -= _Position - 1;

                fixed4 maskSample = tex2D(_TransitionMask, uv);
                return uv.x < 0.0 ? 0.0 : uv.x > 1.0 ? 1.0 : maskSample.r;
            #endif
        }

        fixed4 GetTexA(float2 uv)
        {
            #ifdef _TRANSITIONMODE_SCROLL
                uv.x = uv.x + 1 - _Position / 2;
            #endif
            return tex2D(_MainTex, uv);
        }

        fixed4 GetTexB(float2 uv)
        {
            #ifdef _TRANSITIONMODE_SCROLL
                uv.x = uv.x - _Position / 2;
            #endif
            return tex2D(_MainTexB, uv);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;

            fixed4 colA = GetTexA(uv);
            fixed4 colB = GetTexB(uv);
            float maskValue = GetBlendValue(uv);
            fixed4 col = lerp(colA, colB, maskValue);
            col *= _Color * _Brightness;

            o.Albedo = col.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = col.a;
        }

        ENDCG
    }
}