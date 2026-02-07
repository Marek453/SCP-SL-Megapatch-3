Shader "Projection/Decal/Standard/Additive" {
	Properties {
		_Color ("Albedo", Vector) = (1,1,1,1)
		_MainTex ("Albedo Map", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
		_NormalCutoff ("Normal Cutoff", Range(0, 1)) = 0.5
		_TilingOffset ("Tiling / Offset", Vector) = (1,1,0,0)
		_MaskBase ("Mask Base", Range(0, 1)) = 0
		_MaskLayers ("Layers", Vector) = (0.5,0.5,0.5,0.5)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}