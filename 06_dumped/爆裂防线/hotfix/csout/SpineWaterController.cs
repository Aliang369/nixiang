using Spine.Unity;
using UnityEngine;

public class SpineWaterController : MonoBehaviour
{
	private int cutPixelsFromBottom;

	private float pixelsPerUnit = 100f;

	private Material _clippedMaterial;

	private SkeletonRenderer _skeletonRenderer;

	private Material _originalMaterial;

	private void Awake()
	{
		_skeletonRenderer = ((Component)this).GetComponent<SkeletonRenderer>();
		if ((Object)(object)_skeletonRenderer == (Object)null)
		{
			Debug.LogError((object)"缺少 SkeletonRenderer 组件！");
		}
	}

	public void ApplyWaterClip()
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_skeletonRenderer == (Object)null)
		{
			return;
		}
		if ((Object)(object)_skeletonRenderer.SkeletonDataAsset == (Object)null)
		{
			Debug.LogWarning((object)"SkeletonDataAsset尚未加载，稍后重试");
			return;
		}
		if (_skeletonRenderer.SkeletonDataAsset.atlasAssets == null || _skeletonRenderer.SkeletonDataAsset.atlasAssets.Length == 0)
		{
			Debug.LogError((object)"未找到Atlas材质！");
			return;
		}
		Material primaryMaterial = _skeletonRenderer.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;
		if ((Object)(object)primaryMaterial == (Object)null)
		{
			Debug.LogError((object)"获取原始材质失败！");
			return;
		}
		if ((Object)(object)_originalMaterial != (Object)null && (Object)(object)_originalMaterial != (Object)(object)primaryMaterial)
		{
			if (_skeletonRenderer.CustomMaterialOverride.ContainsKey(_originalMaterial))
			{
				_skeletonRenderer.CustomMaterialOverride.Remove(_originalMaterial);
			}
			if ((Object)(object)_clippedMaterial != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)_clippedMaterial);
				_clippedMaterial = null;
			}
		}
		if ((Object)(object)_clippedMaterial == (Object)null)
		{
			Shader val = Shader.Find("Custom/SpineWaterClip_URP");
			if ((Object)(object)val == (Object)null)
			{
				Debug.LogError((object)"找不到自定义Shader！请检查Shader名称是否匹配");
				return;
			}
			_clippedMaterial = new Material(val);
			if (primaryMaterial.HasProperty("_MainTex"))
			{
				_clippedMaterial.SetTexture("_MainTex", primaryMaterial.GetTexture("_MainTex"));
			}
			if (primaryMaterial.HasProperty("_Color"))
			{
				_clippedMaterial.SetColor("_Color", primaryMaterial.GetColor("_Color"));
			}
			float num = (float)cutPixelsFromBottom / pixelsPerUnit;
			_clippedMaterial.SetFloat("_CutHeight", num);
		}
		if (_skeletonRenderer.CustomMaterialOverride.ContainsKey(primaryMaterial))
		{
			_skeletonRenderer.CustomMaterialOverride[primaryMaterial] = _clippedMaterial;
		}
		else
		{
			_skeletonRenderer.CustomMaterialOverride.Add(primaryMaterial, _clippedMaterial);
		}
		_originalMaterial = primaryMaterial;
		_skeletonRenderer.LateUpdate();
	}

	public void RefreshAfterSpineChanged()
	{
	}

	public void SetCutPixels(int newPixels)
	{
		cutPixelsFromBottom = newPixels;
		if ((Object)(object)_clippedMaterial != (Object)null)
		{
			float num = (float)cutPixelsFromBottom / pixelsPerUnit;
			_clippedMaterial.SetFloat("_CutHeight", num);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_skeletonRenderer != (Object)null && (Object)(object)_originalMaterial != (Object)null && _skeletonRenderer.CustomMaterialOverride.ContainsKey(_originalMaterial))
		{
			_skeletonRenderer.CustomMaterialOverride.Remove(_originalMaterial);
		}
		if ((Object)(object)_clippedMaterial != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)_clippedMaterial);
		}
	}
}
