using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

[Serializable]
public class EnemySlotEffectDisplayModule : IEnemyDisplayModule
{
	private class EffectData
	{
		public bool forceStop;

		public float timeSinceStart;
	}

	[SerializeField]
	public SkeletonRendererCustomMaterials customMaterials;

	[SerializeField]
	public AnimationCurve particleFailureAdditionColorCurve;

	[SerializeField]
	public AnimationCurve particleFailureAlphaCurve;

	[SerializeField]
	public AnimationCurve flashAdditionColorCurve;

	[SerializeField]
	public float flashTime;

	[NonSerialized]
	private EnemyDisplay enemyDisplay;

	[NonSerialized]
	private Dictionary<Material, EffectData> runningActions;

	public override void Initialize(EnemyDisplay enemyDisplay)
	{
		this.enemyDisplay = enemyDisplay;
		runningActions = new Dictionary<Material, EffectData>();
	}

	private IEnumerator ParticleFailure(Material targetMaterial, EffectData data)
	{
		Color color;
		while (data.timeSinceStart < flashTime && !data.forceStop)
		{
			data.timeSinceStart += Time.deltaTime;
			if (flashTime <= 0f || data.timeSinceStart > flashTime)
			{
				targetMaterial.SetColor("_AdditionalColor", Color.black);
				color = targetMaterial.GetColor("_Color");
				color.a = 1f;
				targetMaterial.SetColor("_Color", color);
				yield break;
			}
			float num = particleFailureAdditionColorCurve.Evaluate(data.timeSinceStart / flashTime);
			Color value = new Color(num, num, num);
			targetMaterial.SetColor("_AdditionalColor", value);
			num = particleFailureAlphaCurve.Evaluate(data.timeSinceStart / flashTime);
			color = targetMaterial.GetColor("_Color");
			color.a = num;
			targetMaterial.SetColor("_Color", color);
			yield return null;
		}
		runningActions.Remove(targetMaterial);
		targetMaterial.SetColor("_AdditionalColor", Color.black);
		color = targetMaterial.GetColor("_Color");
		color.a = 1f;
		targetMaterial.SetColor("_Color", color);
	}

	public void ParticleFailure(string slotGroupName)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Material material = customMaterials.customSlotGroupMaterials.Find(delegate(SlotGroupMaterialOverride group)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return group.groupName == slotGroupName;
		}).material;
		if (!(material == null))
		{
			if (runningActions.ContainsKey(material))
			{
				runningActions[material].forceStop = true;
				runningActions.Remove(material);
			}
			EffectData effectData = new EffectData
			{
				timeSinceStart = 0f,
				forceStop = false
			};
			runningActions.Add(material, effectData);
			enemyDisplay.StartCoroutine(ParticleFailure(material, effectData));
		}
	}

	private IEnumerator Flash(Material targetMaterial, EffectData data)
	{
		while (data.timeSinceStart < flashTime && !data.forceStop)
		{
			data.timeSinceStart += Time.deltaTime;
			if (flashTime <= 0f || data.timeSinceStart > flashTime)
			{
				targetMaterial.SetColor("_AdditionalColor", Color.black);
				yield break;
			}
			float num = flashAdditionColorCurve.Evaluate(data.timeSinceStart / flashTime);
			Color value = new Color(num, num, num);
			targetMaterial.SetColor("_AdditionalColor", value);
			yield return null;
		}
		runningActions.Remove(targetMaterial);
		targetMaterial.SetColor("_AdditionalColor", Color.black);
	}

	public void Flash(string slotGroupName)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Material material = customMaterials.customSlotGroupMaterials.Find(delegate(SlotGroupMaterialOverride group)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return group.groupName == slotGroupName;
		}).material;
		if (!(material == null))
		{
			if (runningActions.ContainsKey(material))
			{
				runningActions[material].forceStop = true;
				runningActions.Remove(material);
			}
			EffectData effectData = new EffectData
			{
				timeSinceStart = 0f,
				forceStop = false
			};
			runningActions.Add(material, effectData);
			enemyDisplay.StartCoroutine(Flash(material, effectData));
		}
	}
}
