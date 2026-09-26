using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10431 : zbMonsterSkillBase
{
	private int monsterUUID;

	public stMonsterSkillIndex10431 monsterIndex10431 = new stMonsterSkillIndex10431();

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
		monsterIndex10431 = new stMonsterSkillIndex10431();
		monsterIndex10431.everyTime = monsterSkillRecord.param[0];
		monsterIndex10431.maxCount = (int)monsterSkillRecord.param[1];
		monsterIndex10431.distance = monsterSkillRecord.param[2] / 100f;
		monsterIndex10431.everyTimeKeep = 0f;
		monsterIndex10431.curCount = 0;
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
				monsterIndex10431.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
				monsterIndex10431.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
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
		if (monsterIndex10431.curCount >= monsterIndex10431.maxCount)
		{
			return;
		}
		monsterIndex10431.everyTimeKeep += Time.deltaTime;
		if (monsterIndex10431.everyTimeKeep >= monsterIndex10431.everyTime)
		{
			monsterIndex10431.everyTimeKeep = 0f;
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
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		int index = Rand.Range(0, monsterIndex10431.monsterNumList.Count);
		int num = monsterIndex10431.monsterNumList[index];
		int monsterTid = monsterIndex10431.monsterTidList[index];
		if (num == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monsterTid, ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 = ((num2 >= 8) ? 7 : num2);
				combatData.monsters.SpawnMonsterAtPosition(monsterTid, zbMonsterSkillBase.vector3s[num2] * monsterIndex10431.distance + ((Component)monster).transform.position, ((Component)monster).transform.parent);
				num2++;
			}
		}
		monsterIndex10431.curCount++;
	}

	public void ResetToNormal()
	{
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
		monsterIndex10431.monsterNumList.Clear();
		monsterIndex10431.monsterTidList.Clear();
	}
}
