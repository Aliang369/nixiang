using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10120 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		((Component)m_skeletonAnimation).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.defense.GetAlertPositionY() + combatData.nodeRootPositonY, 0f);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		List<zbMonster> list = new List<zbMonster>(20);
		combatData.monsters.GetRandomMonsters(20, list);
		int[][] skillSub = skillSubRecord.skillSub;
		foreach (int[] array in skillSub)
		{
			for (int j = 0; j < array[0]; j++)
			{
				zbSkillSystem.Instance.CreateSubSkill(array[1], hero, this, skillAttrList, ((Component)m_skeletonAnimation).transform.position, Vector2.up, (list.Count > j) ? list[j].GetUUID() : 0);
			}
		}
		ReturnToPool(GetAttrValueByType(EAttrSkillType.Duration));
	}
}
