Shader "DisBridge/ScreenFX"
{
    Properties
    {
       _ColorTint("Color Tint", Color) = (1,1,1,0)
       _NoiseStrength("Noise Strength", Range( 0 , 1)) = 0
       _FilterStrength("Filter Strength", Range( 0 , 1)) = 0
       _NoiseScale("Noise Scale", Float) = 5
       _WhiteStrength("White Strength", Range( 0 , 1)) = 0
       _FadeImage("Fade Image", 2D) = "white" {}
       _FadeImageOpacity("Fade Image Opacity", Range( 0 , 1)) = 0
       [HideInInspector] _texcoord( "", 2D ) = "white" {}
       [HideInInspector] __dirty( "", Int ) = 1
    }

    SubShader
    {
       Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay+0" "IsEmissive" = "true"  }
       Cull Back
       ZWrite On
       ZTest Always
       Blend One OneMinusSrcAlpha
       
       CGPROGRAM
       #include "UnityShaderVariables.cginc"
       #pragma target 3.0
       #pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
       struct Input
       {
          float2 uv_texcoord;
       };

       uniform half4 _ColorTint;
       uniform half _WhiteStrength;
       uniform half _FilterStrength;
       uniform half _NoiseScale;
       uniform half _NoiseStrength;
       uniform sampler2D _FadeImage;
       uniform float4 _FadeImage_ST;
       uniform half _FadeImageOpacity;


       float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

       float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

       float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

       float snoise( float2 v )
       {
          const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
          float2 i = floor( v + dot( v, C.yy ) );
          float2 x0 = v - i + dot( i, C.xx );
          float2 i1;
          i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
          float4 x12 = x0.xyxy + C.xxzz;
          x12.xy -= i1;
          i = mod2D289( i );
          float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
          float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
          m = m * m;
          m = m * m;
          float3 x = 2.0 * frac( p * C.www ) - 1.0;
          float3 h = abs( x ) - 0.5;
          float3 ox = floor( x + 0.5 );
          float3 a0 = x - ox;
          m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
          float3 g;
          g.x = a0.x * x0.x + h.x * x0.y;
          g.yz = a0.yz * x12.xz + h.yz * x12.yw;
          return 130.0 * dot( m, g );
       }


       inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
       {
          return half4 ( 0, 0, 0, s.Alpha );
       }

       void surf( Input i , inout SurfaceOutput o )
       {
          half FilterStrength49 = _FilterStrength;
          half mulTime22 = _Time.y * 0.5;
          half2 appendResult48 = (half2(( 97.89 * mulTime22 ) , ( 80.59 * mulTime22 )));
          float2 uv_TexCoord10 = i.uv_texcoord * half2( 30,45 ) + appendResult48;
          half simplePerlin2D3 = snoise( uv_TexCoord10*_NoiseScale );
          simplePerlin2D3 = simplePerlin2D3*0.5 + 0.5;
          half clampResult41 = clamp( ( ( _WhiteStrength * FilterStrength49 ) + ( simplePerlin2D3 * _NoiseStrength ) ) , 0.0 , 1.0 );
          float2 uv_FadeImage = i.uv_texcoord * _FadeImage_ST.xy + _FadeImage_ST.zw;
          half4 tex2DNode53 = tex2D( _FadeImage, uv_FadeImage );

          // --- MODIFICATION START ---
          // Create a mask that is 1 inside the [0,1] UV rect and 0 outside.
          half border = step(uv_FadeImage.x, 1.0) * step(0.0, uv_FadeImage.x) * step(uv_FadeImage.y, 1.0) * step(0.0, uv_FadeImage.y);
          // Multiply the texture's color by the mask to make it black outside the border.
          // The alpha is preserved for the lerp calculation below.
          tex2DNode53.rgb *= border;
          // --- MODIFICATION END ---
          
          half4 lerpResult59 = lerp( ( _ColorTint * clampResult41 ) , tex2DNode53 , ( _FadeImageOpacity * tex2DNode53.a ));
          o.Emission = lerpResult59.rgb;
          o.Alpha = FilterStrength49;
       }

       ENDCG
    }
}