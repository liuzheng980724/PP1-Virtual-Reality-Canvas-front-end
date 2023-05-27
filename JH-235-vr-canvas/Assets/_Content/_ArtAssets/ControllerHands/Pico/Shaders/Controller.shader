// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Controller"
{
	Properties
	{
		_ControllerFade("ControllerFade", Range( 0 , 1)) = 0
		_Spec("Spec", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		_FresnelCol("FresnelCol", Color) = (0,0,0,0)
		_FresMin("FresMin", Float) = 0
		_FresMax("FresMax", Float) = 0
		_FresnelVal("FresnelVal", Vector) = (0,0,0,0)
		_Texture("Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 4.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Texture;
		uniform float4 _Texture_ST;
		uniform float3 _FresnelVal;
		uniform float _ControllerFade;
		uniform float4 _FresnelCol;
		uniform float _FresMin;
		uniform float _FresMax;
		uniform sampler2D _Spec;
		uniform float4 _Spec_ST;
		uniform sampler2D _Occlusion;
		uniform float4 _Occlusion_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
			float4 tex2DNode29 = tex2D( _Texture, uv_Texture );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV3 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode3 = ( _FresnelVal.x + _FresnelVal.y * pow( 1.0 - fresnelNdotV3, _FresnelVal.z ) );
			o.Albedo = ( ( ( tex2DNode29 * fresnelNode3 ) + tex2DNode29 ) * _ControllerFade ).rgb;
			o.Emission = saturate( ( _FresnelCol * (_FresMin + (fresnelNode3 - 0.0) * (_FresMax - _FresMin) / (1.0 - 0.0)) ) ).rgb;
			float2 uv_Spec = i.uv_texcoord * _Spec_ST.xy + _Spec_ST.zw;
			float4 tex2DNode6 = tex2D( _Spec, uv_Spec );
			o.Metallic = tex2DNode6.r;
			float clampResult26 = clamp( ( ( tex2DNode6.a * 0.8 ) * _ControllerFade ) , 0.0 , 1.0 );
			o.Smoothness = clampResult26;
			float2 uv_Occlusion = i.uv_texcoord * _Occlusion_ST.xy + _Occlusion_ST.zw;
			o.Occlusion = tex2D( _Occlusion, uv_Occlusion ).r;
			float clampResult8 = clamp( _ControllerFade , 0.0 , 1.0 );
			o.Alpha = clampResult8;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
440.6667;602.6667;1782;653;1626.179;911.9605;1.517642;True;True
Node;AmplifyShaderEditor.Vector3Node;25;-1529.049,-154.6573;Float;False;Property;_FresnelVal;FresnelVal;9;0;Create;True;0;0;False;0;0,0,0;0,1.3,2.14;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;23;-1392.461,68.43607;Float;False;Property;_FresMin;FresMin;7;0;Create;True;0;0;False;0;0;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-872.8455,161.0494;Float;True;Property;_Spec;Spec;3;0;Create;True;0;0;False;0;None;663aaa2b7cc9d8946868a4de63e51f40;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-693.2251,386.3059;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-768.7112,-801.1723;Float;True;Property;_Texture;Texture;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;3;-1223.5,-129.8332;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1393.979,173.1534;Float;False;Property;_FresMax;FresMax;8;0;Create;True;0;0;False;0;0;0.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-1124.351,-346.6914;Float;False;Property;_FresnelCol;FresnelCol;6;0;Create;True;0;0;False;0;0,0,0,0;0.5793876,0.8393353,0.990566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-433.0958,-366.154;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-464.226,268.1601;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;22;-726.2158,-122.7867;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-621.5768,76.36419;Float;False;Property;_ControllerFade;ControllerFade;2;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-148.0384,-387.0291;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-376.5295,-220.7339;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;47.45612,-66.05595;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-204.9231,-645.7041;Float;True;Property;_Occlusion;Occlusion;5;0;Create;True;0;0;False;0;None;8a4d28d2169e8064d830476ed527189b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;445.0883,-682.6571;Float;True;Property;_Normal;Normal;4;0;Create;True;0;0;False;0;None;4f0f2fe6192a84444982ce5b5677e68c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;223.9033,-252.8386;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;26;325.5092,-106.0927;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;8;22.47906,190.9525;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;-137.372,-163.763;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;5;-742.3741,-531.6585;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;0,0,0,0;0.2803488,0.3681123,0.4245282,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1007.997,-195.8265;Float;False;True;4;Float;ASEMaterialInspector;0;0;Standard;Controller;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;1;25;1
WireConnection;3;2;25;2
WireConnection;3;3;25;3
WireConnection;11;0;29;0
WireConnection;11;1;3;0
WireConnection;17;0;6;4
WireConnection;17;1;18;0
WireConnection;22;0;3;0
WireConnection;22;3;23;0
WireConnection;22;4;24;0
WireConnection;12;0;11;0
WireConnection;12;1;29;0
WireConnection;13;0;16;0
WireConnection;13;1;22;0
WireConnection;9;0;17;0
WireConnection;9;1;1;0
WireConnection;10;0;12;0
WireConnection;10;1;1;0
WireConnection;26;0;9;0
WireConnection;8;0;1;0
WireConnection;28;0;13;0
WireConnection;0;0;10;0
WireConnection;0;1;7;0
WireConnection;0;2;28;0
WireConnection;0;3;6;1
WireConnection;0;4;26;0
WireConnection;0;5;14;1
WireConnection;0;9;8;0
ASEEND*/
//CHKSM=890B2A7E63267417E07CA48AD67A41B93919472C