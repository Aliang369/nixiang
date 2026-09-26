using Proto.Enums;
using Spine.Unity;
using UnityEngine;

public class zbMonsterDieFx : MonoBehaviour
{
	private zbCombatData m_combatData;

	private SkeletonAnimation m_skeletonAnimation;

	private ESortLayerType m_sortType = ESortLayerType.MonsterDie;

	private float m_fAccumTime;

	private float m_fLifeTime = 1.6f;

	private void ReturnToPool(float delayTime = 0f)
	{
		if (((Component)this).gameObject.activeSelf)
		{
			m_combatData.monsterDieFxPool.ReturnDieFx(this);
		}
	}

	public void Init(zbMonster monster)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		m_combatData = monster.combatData;
		m_skeletonAnimation = ((Component)this).GetComponentInChildren<SkeletonAnimation>();
		m_skeletonAnimation.AnimationState.SetAnimation(0, monster.monsterAvatarRecord.dieColor, false);
		((Component)this).transform.SetParent(((Component)monster).transform.parent);
		((Component)this).transform.position = ((Component)monster).transform.position;
		float attrValueByType = monster.GetAttrValueByType(EAttrMonsterType.BodyScale);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		m_fAccumTime = 0f;
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)0;
		}
		else
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)1;
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
		if (m_fAccumTime < m_fLifeTime)
		{
			((Component)this).transform.Translate(Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime, (Space)0);
			m_fAccumTime += Time.deltaTime;
		}
		else
		{
			ReturnToPool();
		}
	}
}
