using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class EffectModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private List<Image> effectImages;

	[SerializeField]
	private Image outlineEffect;

	[SerializeField]
	private Material effectMaterialPrefab;

	[NonSerialized]
	private Material effectMaterial;

	[NonSerialized]
	private List<Material> effectMaterials;

	public override void Init()
	{
		effectMaterial = new Material(effectMaterialPrefab);
		foreach (Image effectImage in effectImages)
		{
			effectImage.material = effectMaterial;
		}
	}

	public override void Setup()
	{
		foreach (Image effectImage in effectImages)
		{
			effectImage.materialForRendering.DisableKeyword("ENABLE_DISSOLVE");
		}
	}

	public void AppendEffectTween(Sequence sequence, float value, float time)
	{
		RefreshMaterials();
		sequence.AppendCallback(delegate
		{
			foreach (Material effectMaterial in effectMaterials)
			{
				effectMaterial.EnableKeyword("ENABLE_DISSOLVE");
			}
			foreach (Material effectMaterial2 in effectMaterials)
			{
				effectMaterial2.SetFloat("_DissolveFactor", 1f);
			}
		});
		foreach (Material effectMaterial3 in effectMaterials)
		{
			sequence.Join(effectMaterial3.DOFloat(value, "_DissolveFactor", time));
		}
		sequence.AppendInterval(time);
		sequence.AppendCallback(delegate
		{
			base.slotDisplay.gameObject.SetActive(value: false);
			foreach (Material effectMaterial4 in effectMaterials)
			{
				effectMaterial4.DisableKeyword("ENABLE_DISSOLVE");
			}
		});
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			base.slotDisplay.gameObject.SetActive(value: false);
			foreach (Material effectMaterial5 in effectMaterials)
			{
				effectMaterial5.DisableKeyword("ENABLE_DISSOLVE");
			}
		});
	}

	private void EnableDis()
	{
		foreach (Image effectImage in effectImages)
		{
			effectImage.materialForRendering.EnableKeyword("ENABLE_DISSOLVE");
		}
	}

	private void DisableDis()
	{
		foreach (Image effectImage in effectImages)
		{
			effectImage.materialForRendering.DisableKeyword("ENABLE_DISSOLVE");
		}
	}

	private void SetVal(float i)
	{
		foreach (Image effectImage in effectImages)
		{
			effectImage.materialForRendering.SetFloat("_DissolveFactor", i);
		}
	}

	private void RefreshMaterials()
	{
		if (effectMaterials == null)
		{
			effectMaterials = new List<Material>();
		}
		effectMaterials.Clear();
		effectMaterials.Add(effectMaterial);
		effectMaterials.Add(outlineEffect.materialForRendering);
		EffectMarkModule module = base.slotDisplay.GetModule<EffectMarkModule>();
		if (module != null)
		{
			module.AppendMaterial(effectMaterials);
		}
	}
}
