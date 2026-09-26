using System.Collections.Generic;
using UnityEngine;

public class zbMonsterSkillIndex10131 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10131 data = new stMonsterSkillIndex10131();

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	private static readonly List<stMonsterSkillIndex10131> _tempSkillList = new List<stMonsterSkillIndex10131>();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.distance = monsterSkillRecord.param[0] / 100f;
		data.attrMonsterType = (int)monsterSkillRecord.param[1];
		data.attrPercent = monsterSkillRecord.param[2];
		data.srcMonsterTid = monster.GetTid();
		data.monsterSkillUUID = GetUUID();
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
			_tempSkillList.Clear();
			foreach (stMonsterSkillIndex10131 item2 in item.monsterIndex10131)
			{
				if (item2.srcMonsterTid == monster.GetTid())
				{
					_tempSkillList.Add(item2);
				}
			}
			if (_tempSkillList.Count == 0)
			{
				item.monsterIndex10131.Add(data);
			}
		}
	}

	public void ClearIndexData()
	{
		foreach (zbMonster item in m_monstersInRange)
		{
			for (int num = item.monsterIndex10131.Count - 1; num >= 0; num--)
			{
				stMonsterSkillIndex10131 stMonsterSkillIndex40463 = item.monsterIndex10131[num];
				if (stMonsterSkillIndex40463.srcMonsterTid == monster.GetTid() && stMonsterSkillIndex40463.monsterSkillUUID == GetUUID())
				{
					item.monsterIndex10131.RemoveAt(num);
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
