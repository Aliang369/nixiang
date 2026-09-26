using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex20081 : zbMonsterSkillBase
{
	public float distance;

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		distance = monsterSkillRecord.param[0] / 100f;
		ClearIndexData();
	}

	public override float GetDiameter()
	{
		return distance * 2f;
	}

	public override void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		ClearIndexData();
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, m_monstersInRange, distance);
		foreach (zbMonster item in m_monstersInRange)
		{
			item.immAllBuff++;
		}
	}

	public void ClearIndexData()
	{
		foreach (zbMonster item in m_monstersInRange)
		{
			item.immAllBuff--;
		}
		m_monstersInRange.Clear();
	}

	public override void ClearList()
	{
		base.ClearList();
		ClearIndexData();
	}
}
