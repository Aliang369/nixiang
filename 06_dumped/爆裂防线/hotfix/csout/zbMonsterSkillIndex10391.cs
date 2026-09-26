using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class zbMonsterSkillIndex10391 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10391 data = new stMonsterSkillIndex10391();

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	public override void RefreshProperty()
	{
		loopStart = 1;
		loopNumber = 1;
		base.RefreshProperty();
		data.distance = monsterSkillRecord.param[0] / 100f;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 >= monsterSkillRecord.param.Count())
			{
				break;
			}
			data.buffIdList.Add((int)monsterSkillRecord.param[num2]);
			num++;
		}
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
		foreach (zbMonster item in m_monstersInRange)
		{
			foreach (int buffId in data.buffIdList)
			{
				item.immBuffList.Add(buffId);
			}
		}
	}

	public void ClearIndexData()
	{
		foreach (zbMonster item in m_monstersInRange)
		{
			foreach (int buffId in data.buffIdList)
			{
				item.immBuffList.Remove(buffId);
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
