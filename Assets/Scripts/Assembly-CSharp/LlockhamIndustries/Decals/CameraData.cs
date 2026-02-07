using System.Collections.Generic;
using LlockhamIndustries.Misc;
using UnityEngine;
using UnityEngine.XR;

namespace LlockhamIndustries.Decals
{
	internal class CameraData
	{
		public DepthTextureMode? originalDTM;

		public DepthTextureMode? desiredDTM;

		public ShaderReplacement replacement;

		private RenderTexture depthBuffer;

		private RenderTexture normalBuffer;

		private RenderTexture maskBuffer;

		private RenderTexture depthEye;

		private RenderTexture normalEye;

		private RenderTexture maskEye;

		public bool initialized;

		public void Initialize(Camera Camera, DynamicDecals System)
		{
			initialized = true;
		}

		public void Terminate(Camera Camera)
		{
			RestoreDepthTextureMode(Camera);
			ReleaseTextures();
			initialized = false;
		}

		public void Update(Camera Camera, DynamicDecals System)
		{
			UpdateRenderingMethod(Camera, System);
			UpdateRenderTextures(Camera, System);
			UpdateShaderReplacement(Camera, System);
		}

		public void AssignGlobalProperties(Camera Camera)
		{
			if (replacement == ShaderReplacement.Null)
			{
				return;
			}
			switch (replacement)
			{
			case ShaderReplacement.Classic:
				maskBuffer.SetGlobalShaderProperty("_MaskBuffer_0");
				Shader.DisableKeyword("_PrecisionDepthNormals");
				Shader.DisableKeyword("_CustomDepthNormals");
				Shader.DisableKeyword("_PackedDepthNormals");
				break;
			case ShaderReplacement.DoublePass:
			case ShaderReplacement.TriplePass:
				if (Camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					depthBuffer.SetGlobalShaderProperty("_CustomDepthTexture");
					normalBuffer.SetGlobalShaderProperty("_CustomNormalTexture");
					Shader.DisableKeyword("_PrecisionDepthNormals");
					Shader.EnableKeyword("_CustomDepthNormals");
					Shader.DisableKeyword("_PackedDepthNormals");
				}
				else
				{
					normalBuffer.SetGlobalShaderProperty("_CustomNormalTexture");
					Shader.EnableKeyword("_PrecisionDepthNormals");
					Shader.DisableKeyword("_CustomDepthNormals");
					Shader.DisableKeyword("_PackedDepthNormals");
				}
				maskBuffer.SetGlobalShaderProperty("_MaskBuffer_0");
				break;
			case ShaderReplacement.SinglePass:
				depthBuffer.SetGlobalShaderProperty("_CustomDepthTexture");
				normalBuffer.SetGlobalShaderProperty("_CustomNormalTexture");
				maskBuffer.SetGlobalShaderProperty("_MaskBuffer_0");
				Shader.DisableKeyword("_PrecisionDepthNormals");
				Shader.EnableKeyword("_CustomDepthNormals");
				Shader.DisableKeyword("_PackedDepthNormals");
				break;
			case ShaderReplacement.SingleTarget:
				depthBuffer.SetGlobalShaderProperty("_CustomDepthNormalMaskTexture");
				Shader.DisableKeyword("_PrecisionDepthNormals");
				Shader.DisableKeyword("_CustomDepthNormals");
				Shader.EnableKeyword("_PackedDepthNormals");
				break;
			}
		}

		private ShaderReplacement Standard(bool VR)
		{
			if (VR)
			{
				return ShaderReplacement.TriplePass;
			}
			if (SystemInfo.supportedRenderTargetCount < 3)
			{
				return ShaderReplacement.DoublePass;
			}
			return ShaderReplacement.SinglePass;
		}

		private bool VRCamera(Camera Source)
		{
			if (Source.cameraType == CameraType.SceneView || Source.cameraType == CameraType.Preview)
			{
				return false;
			}
			return XRSettings.enabled && Source.stereoTargetEye != StereoTargetEyeMask.None;
		}

		private void UpdateRenderingMethod(Camera Camera, DynamicDecals System)
		{
			ShaderReplacement shaderReplacement = Standard(XRSettings.enabled && Camera.stereoTargetEye != StereoTargetEyeMask.None);
			switch (System.Settings.Replacement)
			{
			case ShaderReplacementType.VR:
				shaderReplacement = ShaderReplacement.SingleTarget;
				break;
			case ShaderReplacementType.Mobile:
				shaderReplacement = ShaderReplacement.Classic;
				break;
			}
			if (depthBuffer != null)
			{
				DebugManager.Log("Depth Format", depthBuffer.format.ToString());
			}
			if (normalBuffer != null)
			{
				DebugManager.Log("Normal Format", normalBuffer.format.ToString());
			}
			if (maskBuffer != null)
			{
				DebugManager.Log("Mask Format", maskBuffer.format.ToString());
			}
			DebugManager.Log("Shader Replacement", shaderReplacement.ToString());
			DebugManager.Log("API", SystemInfo.graphicsDeviceType.ToString());
			if (replacement != shaderReplacement)
			{
				replacement = shaderReplacement;
				SwitchRenderingMethod(Camera);
				UpdateRenderTextures(Camera, System, true);
			}
		}

		private void SwitchRenderingMethod(Camera Camera)
		{
			switch (replacement)
			{
			case ShaderReplacement.Classic:
				desiredDTM = DepthTextureMode.DepthNormals;
				SetDepthTextureMode(Camera);
				break;
			case ShaderReplacement.DoublePass:
			case ShaderReplacement.TriplePass:
				if (Camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					RestoreDepthTextureMode(Camera);
					break;
				}
				desiredDTM = DepthTextureMode.Depth;
				SetDepthTextureMode(Camera);
				break;
			case ShaderReplacement.SingleTarget:
			case ShaderReplacement.SinglePass:
				RestoreDepthTextureMode(Camera);
				break;
			}
		}

		private void SetDepthTextureMode(Camera Camera)
		{
			if (desiredDTM.HasValue)
			{
				if (Camera.depthTextureMode != desiredDTM)
				{
					if (!originalDTM.HasValue)
					{
						originalDTM = Camera.depthTextureMode;
					}
					else
					{
						Camera.depthTextureMode = originalDTM.Value;
					}
					Camera.depthTextureMode |= desiredDTM.Value;
				}
			}
			else
			{
				RestoreDepthTextureMode(Camera);
			}
		}

		public void RestoreDepthTextureMode(Camera Camera)
		{
			if (originalDTM.HasValue && Camera != null)
			{
				Camera.depthTextureMode = originalDTM.Value;
			}
		}

		private void UpdateRenderTextures(Camera Camera, DynamicDecals System, bool ForceNewTextures = false)
		{
			int num = Camera.pixelWidth;
			int num2 = Camera.pixelHeight;
			if (VRCamera(Camera))
			{
				num = ((!System.Settings.SinglePassVR) ? XRSettings.eyeTextureWidth : (XRSettings.eyeTextureWidth * 2));
				num2 = XRSettings.eyeTextureHeight;
			}
			if (maskBuffer == null || maskBuffer.width != num || maskBuffer.height != num2 || ForceNewTextures)
			{
				ReleaseTextures();
				GetTextures(Camera, System, num, num2);
			}
		}

		private void GetTextures(Camera Camera, DynamicDecals System, int Width, int Height)
		{
			switch (replacement)
			{
			case ShaderReplacement.SingleTarget:
				depthBuffer = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.RGFloat);
				if (VRCamera(Camera) && System.Settings.SinglePassVR)
				{
					depthEye = RenderTexture.GetTemporary(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 24, RenderTextureFormat.RGFloat);
				}
				break;
			case ShaderReplacement.SinglePass:
				depthBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.depthFormat);
				normalBuffer = RenderTexture.GetTemporary(Width, Height, 0, System.normalFormat);
				maskBuffer = RenderTexture.GetTemporary(Width, Height, 0, System.maskFormat);
				break;
			case ShaderReplacement.DoublePass:
				if (Camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					depthBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.depthFormat);
				}
				normalBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.normalFormat);
				maskBuffer = RenderTexture.GetTemporary(Width, Height, 0, System.maskFormat);
				break;
			case ShaderReplacement.TriplePass:
				if (Camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					depthBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.depthFormat);
				}
				normalBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.normalFormat);
				maskBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.maskFormat);
				if (VRCamera(Camera) && System.Settings.SinglePassVR)
				{
					if (Camera.actualRenderingPath == RenderingPath.DeferredShading)
					{
						depthEye = RenderTexture.GetTemporary(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 24, System.depthFormat);
					}
					normalEye = RenderTexture.GetTemporary(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 24, System.normalFormat);
					maskEye = RenderTexture.GetTemporary(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 24, System.maskFormat);
				}
				break;
			case ShaderReplacement.Classic:
				maskBuffer = RenderTexture.GetTemporary(Width, Height, 24, System.maskFormat);
				break;
			}
		}

		private void ReleaseTextures()
		{
			if (depthBuffer != null && depthBuffer.IsCreated())
			{
				RenderTexture.ReleaseTemporary(depthBuffer);
				depthBuffer = null;
				if (depthEye != null && depthEye.IsCreated())
				{
					RenderTexture.ReleaseTemporary(depthEye);
					depthEye = null;
				}
			}
			if (normalBuffer != null && normalBuffer.IsCreated())
			{
				RenderTexture.ReleaseTemporary(normalBuffer);
				normalBuffer = null;
				if (normalEye != null && normalEye.IsCreated())
				{
					RenderTexture.ReleaseTemporary(normalEye);
					normalEye = null;
				}
			}
			if (maskBuffer != null && maskBuffer.IsCreated())
			{
				RenderTexture.ReleaseTemporary(maskBuffer);
				maskBuffer = null;
				if (maskEye != null && maskEye.IsCreated())
				{
					RenderTexture.ReleaseTemporary(maskEye);
					maskEye = null;
				}
			}
		}

		private void UpdateShaderReplacement(Camera Source, DynamicDecals System)
		{
			if (!System.ShaderReplacement)
			{
				return;
			}
			Camera customCamera = System.CustomCamera;
			SetupReplacementCamera(Source, customCamera);
			if (VRCamera(Source) && System.Settings.SinglePassVR)
			{
				if (Source.stereoTargetEye == StereoTargetEyeMask.Both || Source.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					if (Source.transform.parent != null)
					{
						customCamera.transform.position = Source.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.LeftEye));
					}
					else
					{
						customCamera.transform.position = InputTracking.GetLocalPosition(XRNode.LeftEye);
					}
					customCamera.transform.rotation = Source.transform.rotation * InputTracking.GetLocalRotation(XRNode.LeftEye);
					customCamera.projectionMatrix = Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					customCamera.worldToCameraMatrix = Source.worldToCameraMatrix;
					RenderToTextures(Source, customCamera, System, depthEye, normalEye, maskEye);
					StereoBlit(Source, System, true);
				}
				if (Source.stereoTargetEye == StereoTargetEyeMask.Both || Source.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					if (Source.transform.parent != null)
					{
						customCamera.transform.position = Source.transform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.RightEye));
						Matrix4x4 worldToCameraMatrix = Source.worldToCameraMatrix;
						worldToCameraMatrix.m03 -= Source.stereoSeparation * Source.transform.parent.localScale.x;
						customCamera.worldToCameraMatrix = worldToCameraMatrix;
					}
					else
					{
						customCamera.transform.position = InputTracking.GetLocalPosition(XRNode.RightEye);
						Matrix4x4 worldToCameraMatrix2 = Source.worldToCameraMatrix;
						worldToCameraMatrix2.m03 -= Source.stereoSeparation;
						customCamera.worldToCameraMatrix = worldToCameraMatrix2;
					}
					customCamera.transform.rotation = Source.transform.rotation * InputTracking.GetLocalRotation(XRNode.RightEye);
					customCamera.projectionMatrix = Source.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					RenderToTextures(Source, customCamera, System, depthEye, normalEye, maskEye);
					StereoBlit(Source, System, false);
				}
			}
			else
			{
				RenderToTextures(Source, customCamera, System, depthBuffer, normalBuffer, maskBuffer);
			}
		}

		private void RenderToTextures(Camera Source, Camera Renderer, DynamicDecals System, RenderTexture depth, RenderTexture normal, RenderTexture mask)
		{
			switch (replacement)
			{
			case ShaderReplacement.Classic:
				Renderer.targetTexture = mask;
				DrawSplitPass(Source, Renderer, System, System.MaskShader, false);
				break;
			case ShaderReplacement.TriplePass:
				Renderer.cullingMask = Source.cullingMask;
				if (Source.actualRenderingPath == RenderingPath.DeferredShading)
				{
					Renderer.targetTexture = depth;
					DrawRegualarPass(Renderer, System.DepthShader);
				}
				Renderer.targetTexture = normal;
				DrawRegualarPass(Renderer, System.NormalShader);
				Renderer.targetTexture = mask;
				DrawSplitPass(Source, Renderer, System, System.MaskShader);
				break;
			case ShaderReplacement.DoublePass:
			{
				if (Source.actualRenderingPath == RenderingPath.DeferredShading)
				{
					Renderer.cullingMask = Source.cullingMask;
					Renderer.targetTexture = depth;
					DrawRegualarPass(Renderer, System.DepthShader);
				}
				RenderBuffer[] colorBuffer2 = new RenderBuffer[2] { normal.colorBuffer, mask.colorBuffer };
				Renderer.SetTargetBuffers(colorBuffer2, normal.depthBuffer);
				DrawSplitPass(Source, Renderer, System, System.NormalMaskShader);
				break;
			}
			case ShaderReplacement.SinglePass:
			{
				RenderBuffer[] colorBuffer = new RenderBuffer[3] { mask.colorBuffer, normal.colorBuffer, depth.colorBuffer };
				Renderer.SetTargetBuffers(colorBuffer, depth.depthBuffer);
				DrawSplitPass(Source, Renderer, System, System.DepthNormalMaskShader);
				break;
			}
			case ShaderReplacement.SingleTarget:
				Renderer.targetTexture = depth;
				DrawSplitPass(Source, Renderer, System, System.DepthNormalMaskShader_Packed);
				break;
			}
			Renderer.targetTexture = null;
		}

		private void DrawRegualarPass(Camera Renderer, Shader ReplacementShader)
		{
			Renderer.clearFlags = CameraClearFlags.Color;
			Renderer.backgroundColor = Color.clear;
			Renderer.RenderWithShader(ReplacementShader, "RenderType");
		}

		private void DrawSplitPass(Camera Source, Camera Renderer, DynamicDecals System, Shader ReplacementShader, bool RenderInvalid = true)
		{
			List<ReplacementPass> passes = System.Settings.Passes;
			Renderer.clearFlags = CameraClearFlags.Color;
			Renderer.backgroundColor = Color.clear;
			if (System.Settings.UseMaskLayers)
			{
				for (int i = 0; i < passes.Count; i++)
				{
					if (passes[i].vector != Vector4.zero || RenderInvalid)
					{
						Renderer.cullingMask = (int)passes[i].layers & Source.cullingMask;
						Shader.SetGlobalVector("_MaskWrite", passes[i].vector);
						Renderer.RenderWithShader(ReplacementShader, "RenderType");
						Renderer.clearFlags = CameraClearFlags.Nothing;
					}
				}
			}
			else if (RenderInvalid)
			{
				Renderer.cullingMask = -1;
				Shader.SetGlobalVector("_MaskWrite", Vector4.zero);
				Renderer.RenderWithShader(ReplacementShader, "RenderType");
				Renderer.clearFlags = CameraClearFlags.Nothing;
			}
		}

		private void StereoBlit(Camera Source, DynamicDecals System, bool Left)
		{
			switch (replacement)
			{
			case ShaderReplacement.SingleTarget:
				Graphics.Blit(depthEye, depthBuffer, (!Left) ? System.StereoBlitRight : System.StereoBlitLeft);
				break;
			case ShaderReplacement.TriplePass:
				if (Source.actualRenderingPath == RenderingPath.DeferredShading)
				{
					Material material = ((!Left) ? System.StereoDepthBlitRight : System.StereoDepthBlitLeft);
					material.SetTexture("_DepthTex", depthEye);
					Graphics.Blit(depthEye, depthBuffer, material);
				}
				Graphics.Blit(normalEye, normalBuffer, (!Left) ? System.StereoBlitRight : System.StereoBlitLeft);
				Graphics.Blit(maskEye, maskBuffer, (!Left) ? System.StereoBlitRight : System.StereoBlitLeft);
				break;
			}
		}

		private void SetupReplacementCamera(Camera Source, Camera Target)
		{
			Target.CopyFrom(Source);
			Target.renderingPath = RenderingPath.Forward;
			Target.depthTextureMode = DepthTextureMode.None;
			Target.useOcclusionCulling = false;
			Target.allowMSAA = false;
			Target.allowHDR = false;
			Target.rect = new Rect(0f, 0f, 1f, 1f);
		}

		private void SetupReplacementCameraExperimental(Camera Source, Camera Target)
		{
			Target.transform.position = Source.transform.position;
			Target.transform.rotation = Source.transform.rotation;
			if (!XRSettings.enabled)
			{
				Target.fieldOfView = Source.fieldOfView;
			}
			Target.nearClipPlane = Source.nearClipPlane;
			Target.farClipPlane = Source.farClipPlane;
			Target.rect = new Rect(0f, 0f, 1f, 1f);
			Target.orthographic = Source.orthographic;
			Target.orthographicSize = Source.orthographicSize;
			Target.ResetProjectionMatrix();
			Target.ResetWorldToCameraMatrix();
			Target.renderingPath = RenderingPath.Forward;
			Target.depthTextureMode = DepthTextureMode.None;
			Target.useOcclusionCulling = false;
			Target.allowMSAA = false;
			Target.allowHDR = false;
			Target.eventMask = 0;
		}
	}
}
