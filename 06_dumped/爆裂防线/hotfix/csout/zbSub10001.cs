using Proto.Enums;
using UnityEngine;

public class zbSub10001 : zbSkillBase
{
	private float m_returnTime;

	private float m_returnTimePre;

	protected override void OnInited()
	{
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById == (Object)null || monsterById.isEliteBoss || monsterById.GetAttrValueByType(EAttrMonsterType.Mass) > skillSubRecord.param[0])
		{
			ReturnToPool(0.1f);
			m_returnTime = 30f;
			m_returnTimePre = 0f;
		}
		else
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
			m_returnTime = 0.5f;
			m_returnTimePre = 0f;
			ReturnToPool(0.5f);
		}
	}

	protected override void Update()
	{
		base.Update();
		m_returnTimePre += Time.deltaTime;
		if (m_returnTimePre >= m_returnTime)
		{
			m_returnTimePre = 0f;
			ActionReturn();
		}
	}

	private void ActionReturn()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			((Component)monsterById).transform.position = new Vector3(((Component)monsterById).transform.position.x, 7.4f, ((Component)monsterById).transform.position.z);
		}
	}
}
