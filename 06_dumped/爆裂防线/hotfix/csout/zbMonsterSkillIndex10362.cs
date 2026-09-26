using System;
using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10362 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		loopStart = 1;
		loopNumber = 2;
		monster.monsterIndex10362.distance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex10362.skillIndexFlag = true;
		monster.monsterIndex10362.monsterSkill = this;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 < monsterSkillRecord.param.Count())
			{
				monster.monsterIndex10362.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
				monster.monsterIndex10362.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
				num++;
				continue;
			}
			break;
		}
	}

	public override void TriggerDieSkill(Action callback)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (monster.monsterIndex10362.distance < ((Component)monster).transform.position.y - monster.combatData.defense.GetAlertPositionY())
		{
			callback?.Invoke();
			return;
		}
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
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (int monsterNum in monster.monsterIndex10362.monsterNumList)
		{
			num += monsterNum;
		}
		if (num == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex10362.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			foreach (int monsterNum2 in monster.monsterIndex10362.monsterNumList)
			{
				for (int i = 0; i < monsterNum2; i++)
				{
					num2 = ((num2 >= 8) ? 7 : num2);
					combatData.monsters.SpawnMonsterAtPosition(monster.monsterIndex10362.monsterTidList[num3], ((Component)monster).transform.position, ((Component)monster).transform.parent);
					num2++;
				}
				num3++;
			}
		}
		m_callback?.Invoke();
	}

	public override void ClearList()
	{
		monster.monsterIndex10362.monsterNumList.Clear();
		monster.monsterIndex10362.monsterTidList.Clear();
	}
}
