// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PICO_Hand"
{
	Properties
	{
		_T_PICO_Hand_AlbedoTransparency("T_PICO_Hand_AlbedoTransparency", 2D) = "white" {}
		_T_PICO_Hand_Normal("T_PICO_Hand_Normal", 2D) = "bump" {}
		_T_PICO_Hand_MetallicSmoothness("T_PICO_Hand_MetallicSmoothness", 2D) = "white" {}
		_T_PICO_Hand_AO("T_PICO_Hand_AO", 2D) = "white" {}
		_FadeMult("Fade Mult", Float) = 1
		_FadeSub("Fade Sub", Float) = 0
		_DiffuseTint("Diffuse Tint", Color) = (1,1,1,1)
		_VertexOffset("VertexOffset", Range( 0 , 0.01)) = 0
		_FresCol("FresCol", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+1500" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float _VertexOffset;
		uniform sampler2D _T_PICO_Hand_AlbedoTransparency;
		uniform float4 _T_PICO_Hand_AlbedoTransparency_ST;
		uniform sampler2D _T_PICO_Hand_Normal;
		uniform float4 _T_PICO_Hand_Normal_ST;
		uniform float4 _DiffuseTint;
		uniform float _FadeMult;
		uniform float _FadeSub;
		uniform float4 _FresCol;
		uniform sampler2D _T_PICO_Hand_MetallicSmoothness;
		uniform float4 _T_PICO_Hand_MetallicSmoothness_ST;
		uniform sampler2D _T_PICO_Hand_AO;
		uniform float4 _T_PICO_Hand_AO_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 uv_T_PICO_Hand_AlbedoTransparency = v.texcoord * _T_PICO_Hand_AlbedoTransparency_ST.xy + _T_PICO_Hand_AlbedoTransparency_ST.zw;
			float4 tex2DNode1 = tex2Dlod( _T_PICO_Hand_AlbedoTransparency, float4( uv_T_PICO_Hand_AlbedoTransparency, 0, 0.0) );
			v.vertex.xyz += ( ase_vertexNormal * ( _VertexOffset * tex2DNode1.a ) );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_T_PICO_Hand_Normal = i.uv_texcoord * _T_PICO_Hand_Normal_ST.xy + _T_PICO_Hand_Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _T_PICO_Hand_Normal, uv_T_PICO_Hand_Normal ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV31 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode31 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV31, 5.0 ) );
			float2 uv_T_PICO_Hand_AlbedoTransparency = i.uv_texcoord * _T_PICO_Hand_AlbedoTransparency_ST.xy + _T_PICO_Hand_AlbedoTransparency_ST.zw;
			float4 tex2DNode1 = tex2D( _T_PICO_Hand_AlbedoTransparency, uv_T_PICO_Hand_AlbedoTransparency );
			float4 temp_output_21_0 = ( _DiffuseTint * tex2DNode1 );
			float temp_output_18_0 = saturate( ( ( tex2DNode1.a * _FadeMult ) - _FadeSub ) );
			o.Albedo = ( ( ( fresnelNode31 * temp_output_21_0 ) + temp_output_21_0 ) * temp_output_18_0 ).rgb;
			o.Emission = ( _FresCol * fresnelNode31 ).rgb;
			o.Metallic = 0.0;
			float2 uv_T_PICO_Hand_MetallicSmoothness = i.uv_texcoord * _T_PICO_Hand_MetallicSmoothness_ST.xy + _T_PICO_Hand_MetallicSmoothness_ST.zw;
			o.Smoothness = ( temp_output_18_0 * tex2D( _T_PICO_Hand_MetallicSmoothness, uv_T_PICO_Hand_MetallicSmoothness ).a );
			float2 uv_T_PICO_Hand_AO = i.uv_texcoord * _T_PICO_Hand_AO_ST.xy + _T_PICO_Hand_AO_ST.zw;
			o.Occlusion = tex2D( _T_PICO_Hand_AO, uv_T_PICO_Hand_AO ).g;
			o.Alpha = temp_output_18_0;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
440.6667;602.6667;1782;653;1954.4;844.4085;2.165;True;True
Node;AmplifyShaderEditor.CommentaryNode;19;-628.2742,-208.7978;Float;False;763;278;fade mask controls;5;15;14;17;16;18;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-578.2742,-48.79795;Float;False;Property;_FadeMult;Fade Mult;5;0;Create;True;0;0;False;0;1;5.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1224.534,-60.3982;Float;True;Property;_T_PICO_Hand_AlbedoTransparency;T_PICO_Hand_AlbedoTransparency;0;0;Create;True;0;0;False;0;c778733a930fa1f42987db58f0820903;c778733a930fa1f42987db58f0820903;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-1393.342,-503.5251;Float;False;Property;_DiffuseTint;Diffuse Tint;7;0;Create;True;0;0;False;0;1,1,1,1;0.8301887,0.9327163,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-333.2739,-45.79795;Float;False;Property;_FadeSub;Fade Sub;6;0;Create;True;0;0;False;0;0;3.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-411.2739,-158.7978;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-647.2117,-352.4131;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;31;-547.8575,-609.6586;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;16;-207.274,-151.7978;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-133.6578,-419.6586;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-166.4159,744.4561;Float;False;Property;_VertexOffset;VertexOffset;8;0;Create;True;0;0;False;0;0;0.002;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-704.8533,421.6894;Float;True;Property;_T_PICO_Hand_MetallicSmoothness;T_PICO_Hand_MetallicSmoothness;3;0;Create;True;0;0;False;0;772c619c1d8e6474eb780e73ee5d73aa;772c619c1d8e6474eb780e73ee5d73aa;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;32;50.64221,-353.1586;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;144.5969,815.5389;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;18;-40.27399,-154.7978;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;25;-131.3158,533.8565;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;36;85.02856,-751.3129;Float;False;Property;_FresCol;FresCol;9;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-686.4223,215.318;Float;True;Property;_T_PICO_Hand_Normal;T_PICO_Hand_Normal;2;0;Create;True;0;0;False;0;c6b2114490a7f0043a387d9453d8b496;c6b2114490a7f0043a387d9453d8b496;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-613.1292,118.5484;Float;False;Constant;_Metalness;Metalness;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;220.9842,620.9559;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;5;-679.5561,642.074;Float;True;Property;_T_PICO_Hand_AO;T_PICO_Hand_AO;4;0;Create;True;0;0;False;0;db01d532ab0b16846a4bb16a99c48f4a;db01d532ab0b16846a4bb16a99c48f4a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;323.1973,191.0389;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;377.3037,-599.7631;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;329.2821,-242.816;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;982.6251,-16.88;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;PICO_Hand;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.3;True;True;1500;True;Transparent;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;1;4
WireConnection;14;1;15;0
WireConnection;21;0;23;0
WireConnection;21;1;1;0
WireConnection;16;0;14;0
WireConnection;16;1;17;0
WireConnection;33;0;31;0
WireConnection;33;1;21;0
WireConnection;32;0;33;0
WireConnection;32;1;21;0
WireConnection;27;0;26;0
WireConnection;27;1;1;4
WireConnection;18;0;16;0
WireConnection;24;0;25;0
WireConnection;24;1;27;0
WireConnection;28;0;18;0
WireConnection;28;1;3;4
WireConnection;37;0;36;0
WireConnection;37;1;31;0
WireConnection;30;0;32;0
WireConnection;30;1;18;0
WireConnection;0;0;30;0
WireConnection;0;1;2;0
WireConnection;0;2;37;0
WireConnection;0;3;4;0
WireConnection;0;4;28;0
WireConnection;0;5;5;2
WireConnection;0;9;18;0
WireConnection;0;11;24;0
ASEEND*/
//CHKSM=C86B2E7AA14CD3706AC645CE5326D47DEFD34361