using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex10311 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10311 data = new stMonsterSkillIndex10311();

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.distance = monsterSkillRecord.param[0] / 100f;
		data.attackType = (int)monsterSkillRecord.param[1];
		data.attrPercent = monsterSkillRecord.param[2];
		ClearIndexData();
	}

	public override float GetDiameter()
	{
		return data.distance * 2f;
	}

	public override void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		ClearIndexData();
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, m_monstersInRange, data.distance);
		foreach (zbMonster item2 in m_monstersInRange)
		{
			stMonsterSkillIndex10311Extra item = new stMonsterSkillIndex10311Extra
			{
				attackType = data.attackType,
				attrPercent = data.attrPercent,
				monsterSkillUUID = GetUUID(),
				srcMonsterTid = monster.GetUUID()
			};
			item2.monsterIndex10311.Add(item);
		}
	}

	public void ClearIndexData()
	{
		foreach (zbMonster item in m_monstersInRange)
		{
			for (int num = item.monsterIndex10311.Count - 1; num >= 0; num--)
			{
				stMonsterSkillIndex10311Extra stMonsterSkillIndex10311Extra2 = item.monsterIndex10311[num];
				if (stMonsterSkillIndex10311Extra2.monsterSkillUUID == GetUUID() && stMonsterSkillIndex10311Extra2.srcMonsterTid == monster.GetUUID())
				{
					item.monsterIndex10311.RemoveAt(num);
				}
			}
		}
		m_monstersInRange.Clear();
	}

	public override void ClearList()
	{
		base.ClearList();
		ClearIndexData();
	}
}
