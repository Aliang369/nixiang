using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbMonsterSkillIndex10353 : zbMonsterSkillBase
{
	private int monsterUUID;

	private int targetMonsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterUUID = monster.GetUUID();
		monster.monsterIndex10353.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex10353.attrPercent = monsterSkillRecord.param[1];
		monster.monsterIndex10353.durationTime = monsterSkillRecord.param[2];
		targetMonsterUUID = 0;
		monster.monsterIndex10353.everyTimeKeep = 0f;
		monster.monsterIndex10353.durationTimeKeep = 0f;
	}

	public override void Update()
	{
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		if (monster.fsm.GetStateId() != EMonsterState.Move || IsBuffStop())
		{
			return;
		}
		monster.monsterIndex10353.everyTimeKeep += Time.deltaTime;
		if (monster.monsterIndex10353.everyTimeKeep >= monster.monsterIndex10353.everyTime)
		{
			monster.monsterIndex10353.everyTimeKeep = 0f - monster.monsterIndex10353.durationTime;
			monster.monsterIndex10353.durationTimeKeep = monster.monsterIndex10353.durationTime;
			PlayTackAnimation(1, flag: true);
		}
		monster.monsterIndex10353.durationTimeKeep -= Time.deltaTime;
		if (monster.monsterIndex10353.durationTimeKeep <= 0f)
		{
			monster.monsterIndex10353.durationTimeKeep = 0f;
			zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
			if ((Object)(object)monsterById != (Object)null)
			{
				monsterById.monsterIndex10353Stop = false;
			}
			monster.anim.spineAnimation.StopAnimation(1);
			targetMonsterUUID = 0;
			return;
		}
		if (targetMonsterUUID == 0)
		{
			foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
			{
				if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) > 0.5f && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < 1f && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 100f)
				{
					targetMonsterUUID = monster.Value.GetUUID();
					monster.Value.monsterIndex10353Stop = true;
					break;
				}
			}
		}
		zbMonster monsterById2 = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById2 != (Object)null)
		{
			((Component)monsterById2).transform.position = new Vector3(((Component)base.monster).transform.position.x, ((Component)base.monster).transform.position.y - 0.15f, ((Component)monsterById2).transform.position.z);
		}
	}

	public override void ClearList()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.monsterIndex10353Stop = false;
		}
		targetMonsterUUID = 0;
	}
}
