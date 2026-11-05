// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DisBridge/ScreenFXAnimated"
{
	Properties
	{
		_FadeImage("Fade Image", 2DArray) = "white" {}
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		_FadeImageOpacity("Fade Image Opacity", Range( 0 , 1)) = 0
		[Header(Filter)][Space]_FilterStrength("Filter Strength", Range( 0 , 1)) = 0
		_WhiteStrength("White Strength", Range( 0 , 1)) = 0
		_NoiseStrength("Noise Strength", Range( 0 , 1)) = 0
		_NoiseScale("Noise Scale", Float) = 5
		[Header(Flipbook)][Space]_FlipbookTime("Flipbook Time", Range( 0 , 1)) = 0
		_SpriteCount("Sprite Count", Float) = 4
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
		#define ASE_VERSION 19701
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D_ARRAY(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D_ARRAY(tex,samplertex,coord) tex2DArray(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half4 _ColorTint;
		uniform half _NoiseScale;
		uniform half _NoiseStrength;
		uniform half _WhiteStrength;
		uniform half _FilterStrength;
		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_FadeImage);
		uniform float4 _FadeImage_ST;
		uniform half _FlipbookTime;
		uniform half _SpriteCount;
		SamplerState sampler_FadeImage;
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
			half mulTime22 = _Time.y * 0.5;
			half2 appendResult48 = (half2(( mulTime22 * 97.89 ) , ( mulTime22 * 80.59 )));
			float2 uv_TexCoord10 = i.uv_texcoord * half2( 30,45 ) + appendResult48;
			half simplePerlin2D3 = snoise( uv_TexCoord10*_NoiseScale );
			simplePerlin2D3 = simplePerlin2D3*0.5 + 0.5;
			half StaticNoise82 = ( simplePerlin2D3 * _NoiseStrength );
			half FilterStrength49 = _FilterStrength;
			half4 BackgroundColor84 = ( _ColorTint * saturate( ( StaticNoise82 + ( _WhiteStrength * FilterStrength49 ) ) ) );
			float2 uv_FadeImage = i.uv_texcoord * _FadeImage_ST.xy + _FadeImage_ST.zw;
			half FrameTime72 = ( ( _FlipbookTime * _SpriteCount ) % _SpriteCount );
			half4 tex2DArrayNode53 = SAMPLE_TEXTURE2D_ARRAY( _FadeImage, sampler_FadeImage, float3(uv_FadeImage,FrameTime72) );
			half4 lerpResult59 = lerp( BackgroundColor84 , tex2DArrayNode53 , ( tex2DArrayNode53.a * _FadeImageOpacity ));
			o.Emission = lerpResult59.rgb;
			o.Alpha = FilterStrength49;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.CommentaryNode;28;-1584,-1888;Inherit;False;2208.713;494.901;Static Generation;14;33;82;47;45;21;22;46;44;48;13;30;25;3;10;Static Generation;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1536,-1664;Inherit;False;Constant;_StaticSpeed;Static Speed;2;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-1280,-1664;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1280,-1600;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;97.89;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1280,-1536;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;80.59;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-1024,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1024,-1536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;13;-768,-1792;Inherit;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;0;False;0;False;30,45;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;48;-768,-1664;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-512,-1792;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-512,-1664;Inherit;False;Property;_NoiseScale;Noise Scale;7;0;Create;True;0;0;0;False;0;False;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;3;-256,-1792;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-256,-1536;Inherit;False;Property;_NoiseStrength;Noise Strength;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;1408,-2176;Inherit;False;Property;_FilterStrength;Filter Strength;4;1;[Header];Create;True;1;Filter;0;0;False;1;Space;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;86;-1073.479,-1120;Inherit;False;1702.85;527.84;;9;83;50;37;51;84;43;42;81;38;Background;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;79;848,-1888;Inherit;False;1185.759;336.9376;;5;72;77;75;71;69;Frame Index;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;128,-1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;1792,-2176;Inherit;False;FilterStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-1024,-704;Inherit;False;49;FilterStrength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-1024,-768;Inherit;False;Property;_WhiteStrength;White Strength;5;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;82;384,-1792;Inherit;False;StaticNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;1024,-1664;Inherit;False;Property;_SpriteCount;Sprite Count;9;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;896,-1792;Inherit;False;Property;_FlipbookTime;Flipbook Time;8;1;[Header];Create;True;1;Flipbook;0;0;False;1;Space;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-640,-768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;83;-640,-896;Inherit;False;82;StaticNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;1280,-1792;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;62;841.2515,-1120.394;Inherit;False;1387.547;403.3113;;8;59;85;67;66;53;78;55;54;Fade Image;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-384,-768;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;77;1536,-1792;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;54;896,-1024;Inherit;True;Property;_FadeImage;Fade Image;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;LockedToTexture2DArray;Texture2DArray;-1;0;2;SAMPLER2DARRAY;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SaturateNode;81;-128,-768;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-256,-1024;Inherit;False;Property;_ColorTint;Color Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;1792,-1792;Inherit;False;FrameTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;55;1152,-960;Inherit;False;0;-1;2;3;2;SAMPLER2DARRAY;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;78;1152,-832;Inherit;False;72;FrameTime;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;128,-1024;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;53;1408,-1024;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2DArray;8;0;SAMPLER2DARRAY;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;66;1408,-832;Inherit;False;Property;_FadeImageOpacity;Fade Image Opacity;2;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;384,-1024;Inherit;False;BackgroundColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;1792,-896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;1792,-1024;Inherit;False;84;BackgroundColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;2304,-768;Inherit;False;49;FilterStrength;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;59;2048,-960;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;2688,-1024;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;UdonVR/ScreenFX;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;1;False;;7;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;False;0;True;Transparent;;Overlay;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;3;1;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;3;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;21;0
WireConnection;44;0;22;0
WireConnection;44;1;45;0
WireConnection;46;0;22;0
WireConnection;46;1;47;0
WireConnection;48;0;44;0
WireConnection;48;1;46;0
WireConnection;10;0;13;0
WireConnection;10;1;48;0
WireConnection;3;0;10;0
WireConnection;3;1;25;0
WireConnection;33;0;3;0
WireConnection;33;1;30;0
WireConnection;49;0;35;0
WireConnection;82;0;33;0
WireConnection;50;0;37;0
WireConnection;50;1;51;0
WireConnection;75;0;69;0
WireConnection;75;1;71;0
WireConnection;38;0;83;0
WireConnection;38;1;50;0
WireConnection;77;0;75;0
WireConnection;77;1;71;0
WireConnection;81;0;38;0
WireConnection;72;0;77;0
WireConnection;55;2;54;0
WireConnection;43;0;42;0
WireConnection;43;1;81;0
WireConnection;53;0;54;0
WireConnection;53;1;55;0
WireConnection;53;6;78;0
WireConnection;84;0;43;0
WireConnection;67;0;53;4
WireConnection;67;1;66;0
WireConnection;59;0;85;0
WireConnection;59;1;53;0
WireConnection;59;2;67;0
WireConnection;2;2;59;0
WireConnection;2;9;52;0
ASEEND*/
//CHKSM=878241BCC558A6335EB8A8863D70FAF2EB571FF0