using Proto.Enums;
using UnityEngine;

public class zbSub11207 : zbSkillBase
{
	private zbBoundsCircle2D boundsCircle2D;

	protected override void OnInited()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		}
		ReturnToPool(0.4f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		boundsCircle2D.center.x = ((Component)this).transform.position.x;
		boundsCircle2D.center.y = ((Component)this).transform.position.y;
		boundsCircle2D.radius = attrValueByType / 2f;
		UpdateMonsterCollision();
	}

	private void UpdateMonsterCollision()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsters.monsterGrid.QueryFirst(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			zbSkillSystem.Instance.CreateSubSkill(11302, hero, this, skillAttrList, ((Component)this).transform.position, Vector2.up, overlapResult.GetUUID());
		}
	}
}
