using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub20005 : zbSkillBase
{
	private List<int> m_monstersInRange = new List<int>();

	protected override void OnInited()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		m_skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
		m_skeletonAnimation.AnimationState.AddAnimation(0, "loop", true, 0f);
		float attrValueByType = GetAttrValueByType(EAttrSkillType.DamageRange);
		((Component)this).transform.localScale = new Vector3(attrValueByType, attrValueByType, 1f);
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y, ((Component)this).transform.position.z + ((Component)this).transform.position.x / 100f + ((Component)this).transform.position.y / 10f);
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			component.sub20005.sub20005 = true;
			component.sub20005.attrMonsterType1 = 5;
			component.sub20005.attrPercent1 = skillSubRecord.param[0];
			component.sub20005.attrMonsterType2 = 3;
			component.sub20005.attrPercent2 = skillSubRecord.param[1];
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			component.sub20005.sub20005 = false;
			component.sub20005.attrMonsterType1 = 5;
			component.sub20005.attrPercent1 = skillSubRecord.param[0];
			component.sub20005.attrMonsterType2 = 3;
			component.sub20005.attrPercent2 = skillSubRecord.param[1];
			m_monstersInRange.Remove(component.GetUUID());
		}
	}
}
