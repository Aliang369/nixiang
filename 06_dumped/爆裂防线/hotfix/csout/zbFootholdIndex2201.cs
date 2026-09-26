using System;
using UnityEngine;

public class zbFootholdIndex2201 : zbFootholdSkillBase
{
	public stFootholdIndex2201 footholdIndex2201Data;

	[NonSerialized]
	public GameObject nodeHeroAvatar;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Wall;
		base.OnInited();
		nodeHeroAvatar = zbUtil.FindGameObjectRecursive(((Component)this).transform, "Gunpoint");
		canOutWindow = true;
		if (combatData.dataIndex == 2)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		ReturnToPool(99999f);
	}

	public override void SetFootholdAtr(stFootholdIndex _footholdIndex, zbFootholdEffectCsvRecord _footholdEffectRecord)
	{
		base.SetFootholdAtr(_footholdIndex, _footholdEffectRecord);
		footholdIndex2201Data = (stFootholdIndex2201)_footholdIndex;
		m_damageIntervalPre = 0f;
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "act", true, 0f);
		ReturnToPool(99999f);
	}

	protected override void Update()
	{
		base.Update();
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= 1.1333333f)
		{
			m_damageIntervalPre = -9999f;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = 360f / (float)footholdIndex2201Data.skillNumber;
		for (int i = 0; i < footholdIndex2201Data.skillNumber; i++)
		{
			zbFootholdIndex2201_2 obj = (zbFootholdIndex2201_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/foothold/footholdIndex2201_2.prefab", hero, nodeHeroAvatar.transform.position, Vector2.up);
			obj.SetFootholdAtr(footholdIndex2201Data, footholdEffectRecord);
			((Component)obj).transform.Rotate(0f, 0f, num * (float)i);
			m_skeletonAnimation.AnimationState.AddAnimation(0, "out", false, footholdIndex2201Data.durationTime);
			ReturnToPool(footholdIndex2201Data.durationTime + 1.2666667f);
		}
	}
}
