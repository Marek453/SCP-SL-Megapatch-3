using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace LlockhamIndustries.Decals
{
	[ExecuteInEditMode]
	public class DynamicDecals : MonoBehaviour
	{
		private static DynamicDecals system;

		private DynamicDecalSettings settings;

		public SystemPath renderingPath;

		private bool shaderReplacement = true;

		internal RenderTextureFormat depthFormat;

		internal RenderTextureFormat normalFormat;

		internal RenderTextureFormat maskFormat;

		private List<ProjectionData> Projections;

		private bool renderersMarked;

		private Mesh cube;

		private Shader depthShader;

		private Shader normalShader;

		private Shader maskShader;

		private Shader depthNormalShader;

		private Shader normalMaskShader;

		private Shader depthNormalMaskShader;

		private Shader depthNormalMaskShader_Packed;

		private Material stereoBlitLeft;

		private Material stereoBlitRight;

		private Material stereoDepthBlitLeft;

		private Material stereoDepthBlitRight;

		internal Dictionary<Camera, CameraData> cameraData = new Dictionary<Camera, CameraData>();

		private Camera customCamera;

		private Dictionary<int, ProjectionPool> Pools;

		public static bool Initialized
		{
			get
			{
				return system != null;
			}
		}

		public static DynamicDecals System
		{
			get
			{
				if (system == null)
				{
					GameObject gameObject = new GameObject("Dynamic Decals");
					gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
					gameObject.AddComponent<DynamicDecals>();
				}
				return system;
			}
		}

		public DynamicDecalSettings Settings
		{
			get
			{
				if (settings == null)
				{
					settings = Resources.Load<DynamicDecalSettings>("Settings");
				}
				if (settings == null)
				{
					settings = ScriptableObject.CreateInstance<DynamicDecalSettings>();
				}
				return settings;
			}
		}

		private bool FireInCulling
		{
			get
			{
				return false;
			}
		}

		public SystemPath SystemPath
		{
			get
			{
				return renderingPath;
			}
		}

		public bool ShaderReplacement
		{
			get
			{
				return Projections.Count > 0 && shaderReplacement;
			}
			set
			{
				shaderReplacement = value;
			}
		}

		public bool Instanced
		{
			get
			{
				return SystemInfo.supportsInstancing;
			}
		}

		public int ProjectionCount
		{
			get
			{
				return Projections.Count;
			}
		}

		public int RendererCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < Projections.Count; i++)
				{
					num += Projections[i].instances.Count;
				}
				return num;
			}
		}

		public Mesh Cube
		{
			get
			{
				if (cube == null)
				{
					cube = Resources.Load<Mesh>("Decal");
					cube.name = "Projection";
				}
				return cube;
			}
		}

		public Shader DepthShader
		{
			get
			{
				if (depthShader == null)
				{
					depthShader = Shader.Find("Projection/Internal/Depth");
				}
				return depthShader;
			}
		}

		public Shader NormalShader
		{
			get
			{
				if (normalShader == null)
				{
					normalShader = Shader.Find("Projection/Internal/Normal");
				}
				return normalShader;
			}
		}

		public Shader MaskShader
		{
			get
			{
				if (maskShader == null)
				{
					maskShader = Shader.Find("Projection/Internal/Mask");
				}
				return maskShader;
			}
		}

		public Shader NormalMaskShader
		{
			get
			{
				if (normalMaskShader == null)
				{
					normalMaskShader = Shader.Find("Projection/Internal/NormalMask");
				}
				return normalMaskShader;
			}
		}

		public Shader DepthNormalMaskShader
		{
			get
			{
				if (depthNormalMaskShader == null)
				{
					depthNormalMaskShader = Shader.Find("Projection/Internal/DepthNormalMask");
				}
				return depthNormalMaskShader;
			}
		}

		public Shader DepthNormalMaskShader_Packed
		{
			get
			{
				if (depthNormalMaskShader_Packed == null)
				{
					depthNormalMaskShader_Packed = Shader.Find("Projection/Internal/DepthNormalMask_Packed");
				}
				return depthNormalMaskShader_Packed;
			}
		}

		public Material StereoBlitLeft
		{
			get
			{
				if (stereoBlitLeft == null)
				{
					stereoBlitLeft = new Material(Shader.Find("Projection/Internal/StereoBlitLeft"));
				}
				return stereoBlitLeft;
			}
		}

		public Material StereoBlitRight
		{
			get
			{
				if (stereoBlitRight == null)
				{
					stereoBlitRight = new Material(Shader.Find("Projection/Internal/StereoBlitRight"));
				}
				return stereoBlitRight;
			}
		}

		public Material StereoDepthBlitLeft
		{
			get
			{
				if (stereoDepthBlitLeft == null)
				{
					stereoDepthBlitLeft = new Material(Shader.Find("Projection/Internal/StereoDepthBlitLeft"));
				}
				return stereoDepthBlitLeft;
			}
		}

		public Material StereoDepthBlitRight
		{
			get
			{
				if (stereoDepthBlitRight == null)
				{
					stereoDepthBlitRight = new Material(Shader.Find("Projection/Internal/StereoDepthBlitRight"));
				}
				return stereoDepthBlitRight;
			}
		}

		public Camera CustomCamera
		{
			get
			{
				if (customCamera == null)
				{
					GameObject gameObject = new GameObject("Custom Camera");
					customCamera = gameObject.AddComponent<Camera>();
					gameObject.AddComponent<ProjectionBlocker>();
					gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
					gameObject.SetActive(false);
					if (Application.isPlaying)
					{
						UnityEngine.Object.DontDestroyOnLoad(gameObject);
					}
				}
				return customCamera;
			}
		}

		public ProjectionPool DefaultPool
		{
			get
			{
				return PoolFromInstance(Settings.pools[0]);
			}
		}

		public static string DebugLog
		{
			get
			{
				string text = "Debug Information (Copy and Paste) \r\n";
				text += "\r\nGeneral\r\n";
				text = text + "OS : " + SystemInfo.operatingSystem + "\r\n";
				text = text + "Graphics device : " + SystemInfo.graphicsDeviceName + "\r\n";
				string text2 = text;
				text = string.Concat(text2, "Graphics API : ", SystemInfo.graphicsDeviceType, "\r\n");
				Camera main = Camera.main;
				if (main != null)
				{
					text += "\r\nCamera\r\n";
					text2 = text;
					text = string.Concat(text2, "Rendering path : ", main.actualRenderingPath, "\r\n");
					text2 = text;
					text = text2 + "Is orthographic : " + main.orthographic + "\r\n";
					text += "\r\nShader Replacement\r\n";
					text = text + "Method : " + System.GetData(main).replacement.ToString() + "\r\n";
				}
				else
				{
					text += "\r\nMain camera not found\r\nPlease tag your main camera\r\n";
				}
				if (XRSettings.enabled)
				{
					text2 = text;
					text = text2 + "\r\nVirtualReality : " + XRSettings.isDeviceActive + "\r\n";
					text = text + "VR API : " + XRSettings.loadedDeviceName + "\r\n";
					text = text + "VR device : " + "\r\n";
				}
				return text;
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void OnEnable()
		{
			if (system == null)
			{
				system = this;
			}
			else if (system != this)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(base.gameObject, true);
				}
				return;
			}
			Initialize();
		}

		private void OnDisable()
		{
			Terminate();
		}

		public static void ApplySettings()
		{
			system.settings = Resources.Load<DynamicDecalSettings>("Settings");
		}

		private void UpdateSystemPath()
		{
			Camera camera = null;
			if (Camera.main != null)
			{
				camera = Camera.main;
			}
			else if (Camera.current != null)
			{
				camera = Camera.current;
			}
			if (!(camera != null))
			{
				return;
			}
			if (camera.actualRenderingPath == RenderingPath.Forward || camera.actualRenderingPath == RenderingPath.DeferredShading)
			{
				SystemPath systemPath = SystemPath.Forward;
				if (camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					systemPath = SystemPath.Deferred;
				}
				if (renderingPath != systemPath)
				{
					renderingPath = systemPath;
					UpdateRenderers();
				}
			}
			else
			{
				Debug.LogWarning("Current Rendering Path not supported! Please use either Forward or Deferred");
			}
		}

		public void RestoreDepthTextureModes()
		{
			for (int i = 0; i < cameraData.Count; i++)
			{
				Camera key = cameraData.ElementAt(i).Key;
				if (key != null)
				{
					cameraData.ElementAt(i).Value.RestoreDepthTextureMode(key);
				}
			}
		}

		private ProjectionData GetProjectionData(Projection Projection)
		{
			for (int i = 0; i < Projections.Count; i++)
			{
				if (Projections[i].projection == Projection)
				{
					return Projections[i];
				}
			}
			return null;
		}

		private void UpdateProjectionData()
		{
			for (int i = 0; i < Projections.Count; i++)
			{
				Projections[i].Update();
			}
		}

		public bool Register(ProjectionRenderer Instance)
		{
			if (Instance != null)
			{
				Projection projection = Instance.Projection;
				ProjectionData projectionData = GetProjectionData(projection);
				if (projectionData != null)
				{
					projectionData.Add(Instance);
					return base.isActiveAndEnabled;
				}
				projectionData = new ProjectionData(projection);
				projectionData.Add(Instance);
				for (int i = 0; i < Projections.Count; i++)
				{
					if (projection.Priority < Projections[i].projection.Priority)
					{
						Projections.Insert(i, projectionData);
						return true;
					}
				}
				Projections.Add(projectionData);
				return base.isActiveAndEnabled;
			}
			return false;
		}

		public void Deregister(ProjectionRenderer Instance)
		{
			if (!(Instance != null))
			{
				return;
			}
			Projection projection = Instance.Projection;
			for (int i = 0; i < Projections.Count; i++)
			{
				if (Projections[i].projection == projection)
				{
					Projections[i].Remove(Instance);
					if (Projections[i].instances.Count == 0)
					{
						Projections.RemoveAt(i);
					}
					break;
				}
			}
		}

		public void Reorder(Projection Projection)
		{
			ProjectionData projectionData = GetProjectionData(Projection);
			if (projectionData == null)
			{
				return;
			}
			Projections.Remove(projectionData);
			for (int i = 0; i < Projections.Count; i++)
			{
				if (Projection.Priority < Projections[i].projection.Priority)
				{
					Projections.Insert(i, projectionData);
					return;
				}
			}
			Projections.Add(projectionData);
			OrderRenderers();
		}

		public void OrderRenderers()
		{
			if (!renderersMarked || Projections == null)
			{
				return;
			}
			int Order = 1;
			foreach (ProjectionData projection in Projections)
			{
				projection.AssertOrder(ref Order);
			}
		}

		public void MarkRenderers()
		{
			renderersMarked = true;
		}

		public void UpdateRenderers()
		{
			if (Projections != null)
			{
				for (int i = 0; i < Projections.Count; i++)
				{
					Projections[i].UpdateRenderers();
				}
			}
		}

		public void UpdateRenderers(Projection Projection)
		{
			if (Projections == null)
			{
				return;
			}
			for (int i = 0; i < Projections.Count; i++)
			{
				if (Projections[i].projection == Projection)
				{
					Projections[i].UpdateRenderers();
					break;
				}
			}
		}

		private void SetupMaskedMaterials()
		{
			foreach (Material material in Settings.Materials)
			{
				material.renderQueue = 2999;
			}
		}

		internal CameraData GetData(Camera Camera)
		{
			CameraData value = null;
			if (!cameraData.TryGetValue(Camera, out value))
			{
				value = new CameraData();
				cameraData[Camera] = value;
			}
			if (value != null)
			{
				if (!value.initialized && Camera.GetComponent<ProjectionBlocker>() == null)
				{
					value.Initialize(Camera, this);
				}
				else if (value.initialized && Camera.GetComponent<ProjectionBlocker>() != null)
				{
					value.Terminate(Camera);
				}
			}
			return value;
		}

		internal ProjectionPool PoolFromInstance(PoolInstance Instance)
		{
			if (Pools == null)
			{
				Pools = new Dictionary<int, ProjectionPool>();
			}
			ProjectionPool value;
			if (!Pools.TryGetValue(Instance.id, out value))
			{
				value = new ProjectionPool(Instance);
				Pools.Add(Instance.id, value);
			}
			return value;
		}

		public ProjectionPool GetPool(string Title)
		{
			for (int i = 0; i < Settings.pools.Length; i++)
			{
				if (settings.pools[i].title == Title)
				{
					return PoolFromInstance(settings.pools[i]);
				}
			}
			Debug.LogWarning("No valid pool with the title : " + Title + " found. Returning default pool");
			return PoolFromInstance(settings.pools[0]);
		}

		public ProjectionPool GetPool(int ID)
		{
			for (int i = 0; i < Settings.pools.Length; i++)
			{
				if (settings.pools[i].id == ID)
				{
					return PoolFromInstance(settings.pools[i]);
				}
			}
			Debug.LogWarning("No valid pool with the ID : " + ID + " found. Returning default pool");
			return PoolFromInstance(settings.pools[0]);
		}

		private void Initialize()
		{
			depthFormat = RenderTextureFormat.Depth;
			normalFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB2101010) ? RenderTextureFormat.ARGB2101010 : RenderTextureFormat.ARGB32);
			maskFormat = ((!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGB32);
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(SuperLateUpdate));
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(PreRender));
			if (Projections == null)
			{
				Projections = new List<ProjectionData>();
			}
			else
			{
				for (int i = 0; i < Projections.Count; i++)
				{
					Projections[i].EnableRenderers();
				}
			}
			SetupMaskedMaterials();
		}

		private void Terminate()
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(SuperLateUpdate));
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(PreRender));
			foreach (KeyValuePair<Camera, CameraData> cameraDatum in cameraData)
			{
				cameraDatum.Value.Terminate(cameraDatum.Key);
			}
			cameraData.Clear();
			if (Projections != null)
			{
				for (int i = 0; i < Projections.Count; i++)
				{
					Projections[i].DisableRenderers();
				}
			}
		}

		private void LateUpdate()
		{
			UpdateSystemPath();
			UpdateProjectionData();
			OrderRenderers();
		}

		private void SuperLateUpdate(Camera Camera)
		{
			if (FireInCulling)
			{
				CameraData data = GetData(Camera);
				if (data.initialized && (Camera.cameraType == CameraType.SceneView || Camera.cameraType == CameraType.Preview || Camera.isActiveAndEnabled))
				{
					data.Update(Camera, this);
				}
			}
		}

		private void PreRender(Camera Camera)
		{
			CameraData data = GetData(Camera);
			if (data.initialized && (Camera.cameraType == CameraType.SceneView || Camera.cameraType == CameraType.Preview || Camera.isActiveAndEnabled))
			{
				if (!FireInCulling)
				{
					data.Update(Camera, this);
				}
				data.AssignGlobalProperties(Camera);
			}
		}

		public static void DebugInDevelopmentBuild()
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log(DebugLog);
			}
		}
	}
}
