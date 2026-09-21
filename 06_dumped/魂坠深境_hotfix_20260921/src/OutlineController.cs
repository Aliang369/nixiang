using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity.Extension;
using UnityEngine;

public class OutlineController
{
	private DisplayUnit displayUnit;

	public readonly Renderer outline;

	private Sequence sequence;

	private readonly List<Material> visiableMaterials;

	public OutlineController(DisplayUnit displayUnit, Renderer outline)
	{
		this.displayUnit = displayUnit;
		this.outline = outline;
		if (!(outline == null))
		{
			RenderExistingMesh component = outline.GetComponent<RenderExistingMesh>();
			visiableMaterials = new List<Material>();
			for (int i = 0; i < component.replacementMaterials.Length; i++)
			{
				Material material = new Material(component.replacementMaterials[i].replacementMaterial);
				component.replacementMaterials[i].replacementMaterial = material;
				visiableMaterials.Add(material);
			}
			component.InitializeDict();
		}
	}

	public void DoFade(float value, float time)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		if (visiableMaterials != null)
		{
			foreach (Material visiableMaterial in visiableMaterials)
			{
				if (visiableMaterial.HasColor(ShaderFields._OutlineColor))
				{
					sequence.Append(visiableMaterial.DOShaderColorAlpha(ShaderFields._OutlineColor, value, time));
				}
			}
		}
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
	}
}
