using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex20211 : zbMonsterSkillBase
{
	private int monsterUUID;

	public stMonsterSkillIndex20211 monsterIndex20211 = new stMonsterSkillIndex20211();

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
		monsterIndex20211 = new stMonsterSkillIndex20211();
		monsterIndex20211.everyTime = monsterSkillRecord.param[0];
		monsterIndex20211.maxCount = (int)monsterSkillRecord.param[1];
		monsterIndex20211.defenseDistance = monsterSkillRecord.param[2] / 100f;
		monsterIndex20211.everyTimeKeep = 0f;
		monsterIndex20211.curCount = 0;
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
				monsterIndex20211.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
				monsterIndex20211.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
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
		if (monsterIndex20211.curCount >= monsterIndex20211.maxCount)
		{
			return;
		}
		monsterIndex20211.everyTimeKeep += Time.deltaTime;
		if (monsterIndex20211.everyTimeKeep >= monsterIndex20211.everyTime)
		{
			monsterIndex20211.everyTimeKeep = 0f;
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
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		int num = 0;
		Vector3 spawnPosition = default(Vector3);
		foreach (int monsterNum in monsterIndex20211.monsterNumList)
		{
			int monsterTid = monsterIndex20211.monsterTidList[num];
			for (int i = 0; i < monsterNum; i++)
			{
				((Vector3)(ref spawnPosition))._002Ector(Rand.Range(monster.combatData.monsterSpawner.GetMinX(), monster.combatData.monsterSpawner.GetMaxX()), monsterIndex20211.defenseDistance + monster.combatData.defense.GetAlertPositionY(), 0f);
				combatData.monsters.SpawnMonsterAtPosition(monsterTid, spawnPosition, ((Component)monster).transform.parent);
			}
			num++;
		}
		monsterIndex20211.curCount++;
	}

	public void ResetToNormal()
	{
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
		monsterIndex20211.monsterNumList.Clear();
		monsterIndex20211.monsterTidList.Clear();
	}
}
