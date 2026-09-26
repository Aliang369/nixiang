using System;
using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10161 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		loopStart = 1;
		loopNumber = 2;
		monster.monsterIndex10161.monsterSkill = this;
		monster.monsterIndex10161.distance = monsterSkillRecord.param[0] / 100f;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 >= monsterSkillRecord.param.Count())
			{
				break;
			}
			monster.monsterIndex10161.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
			monster.monsterIndex10161.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
			num++;
		}
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override void TriggerDieSkill(Action callback)
	{
		m_callback = callback;
		TrackEntry val = PlayTackAnimation(0, flag: false);
		if (val != null)
		{
			m_DurationMax = val.AnimationEnd - 0.5f;
		}
		else
		{
			m_DurationMax = 0.5f;
		}
		m_bExecReturn = true;
		m_DurationTime = 0f;
	}

	public override void Update()
	{
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine();
			}
		}
	}

	public void ActionCoroutine()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (int monsterNum in monster.monsterIndex10161.monsterNumList)
		{
			num += monsterNum;
		}
		if (num == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex10161.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			foreach (int monsterNum2 in monster.monsterIndex10161.monsterNumList)
			{
				for (int i = 0; i < monsterNum2; i++)
				{
					num2 = ((num2 >= 8) ? 7 : num2);
					combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex10161.monsterTidList[num3], zbMonsterSkillBase.vector3s[num2] * monster.monsterIndex10161.distance + ((Component)monster).transform.position, ((Component)monster).transform.parent);
					num2++;
				}
				num3++;
			}
		}
		m_callback?.Invoke();
	}

	public override void ClearList()
	{
		monster.monsterIndex10161.monsterNumList.Clear();
		monster.monsterIndex10161.monsterTidList.Clear();
	}
}
