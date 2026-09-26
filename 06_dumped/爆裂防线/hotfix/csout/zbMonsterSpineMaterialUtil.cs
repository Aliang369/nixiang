using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbMonsterSpineMaterialUtil
{
	private SkeletonAnimation skeletonAnimation;

	private Material lastOriginalMaterial;

	private const float cutHeight = 0.1f;

	private const float coverStrength = 0.95f;

	private const float waterBrightness = 0.7f;

	private const float blendWidth = 0.03f;

	private static readonly Color waterColor;

	private static readonly Dictionary<Material, Material> s_waterMaterialCache;

	public zbMonsterSpineMaterialUtil(SkeletonAnimation skeletonAnimation)
	{
		this.skeletonAnimation = skeletonAnimation;
	}

	public void Initialize()
	{
		ApplyWaterEffect();
	}

	public void ApplyWaterEffect()
	{
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			return;
		}
		Material val = ResolveOriginalMaterial();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if ((Object)(object)lastOriginalMaterial != (Object)null && (Object)(object)lastOriginalMaterial != (Object)(object)val)
		{
			RemoveWaterEffect();
		}
		Material orCreateWaterMaterial = GetOrCreateWaterMaterial(val);
		if (!((Object)(object)orCreateWaterMaterial == (Object)null))
		{
			if (((SkeletonRenderer)skeletonAnimation).CustomMaterialOverride.TryGetValue(val, out var value) && (Object)(object)value == (Object)(object)orCreateWaterMaterial)
			{
				lastOriginalMaterial = val;
				return;
			}
			((SkeletonRenderer)skeletonAnimation).CustomMaterialOverride[val] = orCreateWaterMaterial;
			lastOriginalMaterial = val;
			((SkeletonRenderer)skeletonAnimation).LateUpdate();
		}
	}

	public void RemoveWaterEffect()
	{
		if (!((Object)(object)skeletonAnimation == (Object)null))
		{
			Material val = (((Object)(object)lastOriginalMaterial != (Object)null) ? lastOriginalMaterial : ResolveOriginalMaterial());
			if ((Object)(object)val != (Object)null && ((SkeletonRenderer)skeletonAnimation).CustomMaterialOverride.ContainsKey(val))
			{
				((SkeletonRenderer)skeletonAnimation).CustomMaterialOverride.Remove(val);
			}
			lastOriginalMaterial = null;
		}
	}

	public void Cleanup()
	{
		RemoveWaterEffect();
	}

	public static void ClearCache()
	{
		foreach (KeyValuePair<Material, Material> item in s_waterMaterialCache)
		{
			if ((Object)(object)item.Value != (Object)null)
			{
				Object.Destroy((Object)(object)item.Value);
			}
		}
		s_waterMaterialCache.Clear();
	}

	private Material ResolveOriginalMaterial()
	{
		if ((Object)(object)skeletonAnimation == (Object)null || (Object)(object)((SkeletonRenderer)skeletonAnimation).SkeletonDataAsset == (Object)null)
		{
			return null;
		}
		AtlasAssetBase[] atlasAssets = ((SkeletonRenderer)skeletonAnimation).SkeletonDataAsset.atlasAssets;
		if (atlasAssets != null && atlasAssets.Length != 0 && (Object)(object)atlasAssets[0] != (Object)null)
		{
			return atlasAssets[0].PrimaryMaterial;
		}
		return null;
	}

	private static Material GetOrCreateWaterMaterial(Material originalMaterial)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (s_waterMaterialCache.TryGetValue(originalMaterial, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Material val = zbAssetLoader.Instance.LoadResouce<Material>("effect/material/mt_spinewater.mat");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		Material val2 = new Material(val);
		if (originalMaterial.HasProperty("_MainTex"))
		{
			val2.SetTexture("_MainTex", originalMaterial.GetTexture("_MainTex"));
		}
		if (originalMaterial.HasProperty("_Color"))
		{
			val2.SetColor("_Color", originalMaterial.GetColor("_Color"));
		}
		val2.SetFloat("_CutHeight", 0.1f);
		val2.SetFloat("_CoverStrength", 0.95f);
		val2.SetFloat("_WaterBrightness", 0.7f);
		val2.SetFloat("_BlendWidth", 0.03f);
		val2.SetColor("_WaterColor", waterColor);
		s_waterMaterialCache[originalMaterial] = val2;
		return val2;
	}

	static zbMonsterSpineMaterialUtil()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		waterColor = new Color(0.15f, 0.6f, 0.8f, 1f);
		s_waterMaterialCache = new Dictionary<Material, Material>();
	}
}
