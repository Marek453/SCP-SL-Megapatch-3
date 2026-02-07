using System.Collections.Generic;
using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class DynamicDecalSettings : ScriptableObject
	{
		public PoolInstance[] pools;

		public DecalMaskMethod maskMethod;

		[SerializeField]
		private ProjectionLayer[] layers;

		[SerializeField]
		private List<ReplacementPass> passes;

		[SerializeField]
		private ShaderReplacementType replacement;

		[SerializeField]
		private List<Material> materials;

		[SerializeField]
		private List<int> materialQueues;

		[SerializeField]
		private bool singlepassVR;

		public bool UseMaskLayers
		{
			get
			{
				return maskMethod == DecalMaskMethod.Layer || maskMethod == DecalMaskMethod.Both;
			}
		}

		public ProjectionLayer[] Layers
		{
			get
			{
				return layers;
			}
			set
			{
				if (layers != value)
				{
					layers = value;
					CalculatePasses();
				}
			}
		}

		public List<ReplacementPass> Passes
		{
			get
			{
				return passes;
			}
		}

		public List<Material> Materials
		{
			get
			{
				return materials;
			}
		}

		public ShaderReplacementType Replacement
		{
			get
			{
				return replacement;
			}
			set
			{
				if (replacement != value)
				{
					replacement = value;
					DynamicDecals.System.UpdateRenderers();
				}
			}
		}

		public bool SinglePassVR
		{
			get
			{
				return singlepassVR;
			}
		}

		public DynamicDecalSettings()
		{
			ResetPools();
			ResetMasking();
		}

		public void ResetSettings()
		{
			DynamicDecals.System.UpdateRenderers();
		}

		public void ResetPools()
		{
			pools = new PoolInstance[1]
			{
				new PoolInstance("Default", null)
			};
		}

		public void ResetMasking()
		{
			maskMethod = DecalMaskMethod.Both;
			layers = new ProjectionLayer[4]
			{
				new ProjectionLayer("Layer 1"),
				new ProjectionLayer("Layer 2"),
				new ProjectionLayer("Layer 3"),
				new ProjectionLayer("Layer 4")
			};
			CalculatePasses();
			if (materials == null)
			{
				materials = new List<Material>();
				materialQueues = new List<int>();
			}
			else
			{
				ClearMaterials();
			}
		}

		public void ResetGeneral()
		{
			replacement = ShaderReplacementType.Standard;
		}

		public void CalculatePasses()
		{
			if (passes == null)
			{
				passes = new List<ReplacementPass>();
			}
			else
			{
				passes.Clear();
			}
			for (int i = 0; i < 32; i++)
			{
				Vector4 layerVector = LayerVector(i);
				AddToPasses(i, layerVector);
			}
		}

		private Vector4 LayerVector(int LayerIndex)
		{
			Vector4 result = new Vector4(0f, 0f, 0f, 0f);
			if (layers[0].layers.Contains(LayerIndex))
			{
				result.x = 1f;
			}
			if (layers[1].layers.Contains(LayerIndex))
			{
				result.y = 1f;
			}
			if (layers[2].layers.Contains(LayerIndex))
			{
				result.z = 1f;
			}
			if (layers[3].layers.Contains(LayerIndex))
			{
				result.w = 1f;
			}
			return result;
		}

		private void AddToPasses(int LayerIndex, Vector4 LayerVector)
		{
			for (int i = 0; i < passes.Count; i++)
			{
				if (passes[i].vector == LayerVector)
				{
					passes[i].layers = passes[i].layers.Add(LayerIndex);
					return;
				}
			}
			passes.Add(new ReplacementPass(LayerIndex, LayerVector));
		}

		public void AddMaterial(Material p_Material)
		{
			if (p_Material.renderQueue < 2999 && !materials.Contains(p_Material))
			{
				materials.Add(p_Material);
				materialQueues.Add(p_Material.renderQueue);
				p_Material.renderQueue = 2999;
			}
		}

		public void RemoveMaterial(Material p_Material)
		{
			int num = -1;
			for (int i = 0; i < materials.Count; i++)
			{
				if (materials[i] == p_Material)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				RemoveMaterial(num);
			}
		}

		public void RemoveMaterial(int p_Index)
		{
			materials[p_Index].renderQueue = materialQueues[p_Index];
			materials.RemoveAt(p_Index);
			materialQueues.RemoveAt(p_Index);
		}

		public void ClearMaterials()
		{
			for (int num = materials.Count - 1; num >= 0; num--)
			{
				materials[num].renderQueue = materialQueues[num];
				materials.RemoveAt(num);
				materialQueues.RemoveAt(num);
			}
		}

		public void CalculateVR()
		{
		}
	}
}
