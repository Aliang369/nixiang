using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex20192 : zbMonsterSkillBase
{
	private int monsterUUID;

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterUUID = monster.GetUUID();
		monster.monsterIndex20192.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex20192.distance = monsterSkillRecord.param[1] / 100f;
		monster.monsterIndex20192.attrMonsterType = (int)monsterSkillRecord.param[2];
		monster.monsterIndex20192.attrPercent = monsterSkillRecord.param[3];
		monster.monsterIndex20192.durationTime = monsterSkillRecord.param[4];
		monster.monsterIndex20192.everyTimeKeep = 0f;
	}

	public override float GetDiameter()
	{
		return monster.monsterIndex20192.distance * 2f;
	}

	public override void Update()
	{
		if (!IsBuffStop())
		{
			monster.monsterIndex20192.everyTimeKeep += Time.deltaTime;
			if (monster.monsterIndex20192.everyTimeKeep >= monster.monsterIndex20192.everyTime)
			{
				monster.monsterIndex20192.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(ActionCoroutine());
			}
		}
	}

	public IEnumerator ActionCoroutine()
	{
		CreateEffectSpine();
		TrackEntry val = PlayTackAnimation(1, flag: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		else
		{
			yield return (object)new WaitForSeconds(1f);
		}
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			yield break;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, m_monstersInRange, monster.monsterIndex20192.distance);
		foreach (zbMonster item2 in m_monstersInRange)
		{
			zbExtraProperty item = new zbExtraProperty
			{
				attrMonsterType = monster.monsterIndex20192.attrMonsterType,
				attrPercent = monster.monsterIndex20192.attrPercent,
				durationTime = monster.monsterIndex20192.durationTime
			};
			item2.extraPropertiesList.Add(item);
		}
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}
}
