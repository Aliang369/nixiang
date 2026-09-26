using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10151 : zbMonsterSkillBase
{
	private int monsterUUID;

	public stMonsterSkillIndex10151 monsterIndex10151 = new stMonsterSkillIndex10151();

	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	private bool m_bExecReturn2;

	private float m_DurationTime2;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterUUID = monster.GetUUID();
		loopStart = 3;
		loopNumber = 2;
		monsterIndex10151 = new stMonsterSkillIndex10151();
		monsterIndex10151.everyTime = monsterSkillRecord.param[0];
		monsterIndex10151.maxCount = (int)monsterSkillRecord.param[1];
		monsterIndex10151.distance = monsterSkillRecord.param[2] / 100f;
		monsterIndex10151.everyTimeKeep = 0f;
		monsterIndex10151.curCount = 0;
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
		m_bExecReturn2 = false;
		m_DurationTime2 = 0f;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 < monsterSkillRecord.param.Count())
			{
				monsterIndex10151.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
				monsterIndex10151.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
				num++;
				continue;
			}
			break;
		}
	}

	public override void Update()
	{
		if (IsBuffStop() || (Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			return;
		}
		if (m_bExecReturn2)
		{
			m_DurationTime2 += Time.deltaTime;
			if (m_DurationTime2 >= m_DurationMax / 2f)
			{
				m_bExecReturn2 = false;
				ResetToNormal();
			}
		}
		if (monsterIndex10151.curCount >= monsterIndex10151.maxCount)
		{
			return;
		}
		monsterIndex10151.everyTimeKeep += Time.deltaTime;
		if (monsterIndex10151.everyTimeKeep >= monsterIndex10151.everyTime)
		{
			monsterIndex10151.everyTimeKeep = 0f;
			monster.canMoveOrAttack = false;
			TrackEntry val = PlayTackAnimation(1, flag: false);
			if (val != null)
			{
				m_DurationMax = val.AnimationEnd * 2f / 3f - 1f / 15f;
			}
			else
			{
				m_DurationMax = 2f / 3f;
			}
			m_bExecReturn = true;
			m_DurationTime = 0f;
		}
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				m_bExecReturn2 = true;
				m_DurationTime2 = 0f;
				ActionCoroutine();
			}
		}
	}

	public void ActionCoroutine()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		int num = 0;
		foreach (int monsterNum in monsterIndex10151.monsterNumList)
		{
			num += monsterNum;
		}
		if (num == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monsterIndex10151.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			foreach (int monsterNum2 in monsterIndex10151.monsterNumList)
			{
				for (int i = 0; i < monsterNum2; i++)
				{
					num2 = ((num2 >= 8) ? 7 : num2);
					combatData.monsters.SpawnMonsterAtPosition(monsterIndex10151.monsterTidList[num3], zbMonsterSkillBase.vector3s[num2] * monsterIndex10151.distance + ((Component)monster).transform.position, ((Component)monster).transform.parent);
					num2++;
				}
				num3++;
			}
		}
		monsterIndex10151.curCount++;
	}

	public void ResetToNormal()
	{
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
		monsterIndex10151.monsterNumList.Clear();
		monsterIndex10151.monsterTidList.Clear();
	}
}
