Shader "Projection/Decal/Packed/Dry" {
	Properties {
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_GlossMap ("Gloss Map", 2D) = "white" {}
		_NormalCutoff ("Normal Cutoff", Range(0, 1)) = 0.5
		_MaskBase ("Mask Base", Range(0, 1)) = 0
		_MaskLayers ("Layers", Vector) = (0.5,0.5,0.5,0.5)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}