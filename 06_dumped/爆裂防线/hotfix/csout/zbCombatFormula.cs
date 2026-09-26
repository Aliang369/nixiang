using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbCombatFormula
{
	private Vector3 bubblePosition;

	private List<zbMonster> monsterRandomList;

	private List<zbMonster> monsterRangeList;

	private HashSet<int> eSkillTag;

	private static readonly float[] Pow15Table = new float[20]
	{
		1f, 1.5f, 2.25f, 3.375f, 5.0625f, 7.59375f, 11.390625f, 17.085938f, 25.628906f, 38.44336f,
		57.66504f, 86.49756f, 129.74634f, 194.6195f, 291.92926f, 437.8939f, 656.8408f, 985.2612f, 1477.8918f, 2216.8376f
	};

	public void Init()
	{
	}

	public void Free()
	{
	}

	public stDamageResult CalcMonsterDamage(zbSkillBase skillBase, EAttrSkillType attrSkillType, zbMonster monster, float damage = 0f, int _skillAttackType = 0, bool isAgain = false, bool param = false)
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		stDamageResult result = new stDamageResult
		{
			damageType = EDamageResult.Hit,
			isKilled = false,
			isViolent = false
		};
		if (monster.isGoDestroyed)
		{
			return result;
		}
		if (monster.monsterIndex10101 || monster.monsterSkillIndex3011Penetration)
		{
			skillBase.currPenetration = 99;
		}
		result = CalcMonsterImmuneDamage(skillBase, monster);
		if (result.damageType != EDamageResult.Hit)
		{
			return result;
		}
		long num = monster.curHp;
		monsterRandomList.Clear();
		bool flag = monster.CheckInstantKillRoll(skillBase.hero.GetSkillType());
		float num2 = 0f;
		if (!flag)
		{
			num2 = ((!(damage > 0f)) ? Math.Max(skillBase.GetAttrValueByType(attrSkillType), 1f) : damage);
		}
		float num3 = 0f;
		if (!flag)
		{
			num3 = skillBase.hero.team.combatViolent.CalcViolent(skillBase, attrSkillType, monster);
			result.isViolent = num3 > 0f;
		}
		if (result.isViolent)
		{
			result.damageType = EDamageResult.Violent;
			num2 *= 1f + num3;
			if (monster.monsterIndex20011)
			{
				result.damageType = EDamageResult.Hit;
				result.isViolent = false;
			}
		}
		int skillAttackType = skillBase.getAttackType();
		if (_skillAttackType > 0)
		{
			skillAttackType = _skillAttackType;
		}
		if (!flag)
		{
			num2 = AddMonsterDamage(skillBase, num2, monster, skillAttackType, result.isViolent);
			num2 = DecMonsterDamage(skillBase, num2, monster, skillAttackType, result.isViolent);
			if (num2 <= 0f)
			{
				result.damageType = EDamageResult.Immune;
				result.damageValue = 0L;
				result.isViolent = false;
				Vector3 position = monster.monsterTF.position;
				((Vector3)(ref bubblePosition)).Set(position.x, position.y + 0.4f, position.z);
				skillBase.combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Immune, 0L);
				return result;
			}
			result.damageValue = (long)num2;
			result.damageValue = ReplayAddDamage(result.damageValue);
			result.damageValue = Math.Max(result.damageValue, 1L);
			result.damageValue = Math.Min(result.damageValue, long.MaxValue);
			if (monster.monsterIndex10000.attrPercent > 0f)
			{
				result.damageValue += (long)(monster.monsterIndex10000.attrPercent * (float)(long)monster.maxHp);
			}
		}
		else
		{
			result.damageValue = (long)monster.maxHp + 1;
		}
		result.damageValue = -monster.ChangeHp(skillBase.hero, null, -result.damageValue, result.isViolent, flag);
		result.isKilled = num > 0 && monster.isDead;
		if (result.damageValue > 0)
		{
			skillBase.hitNumber++;
			HitMonsterTodo(skillBase, attrSkillType, monster, skillAttackType, isAgain, param);
			if (result.isKilled)
			{
				KillMonsterTodo(skillBase, attrSkillType, monster, skillAttackType);
				if (skillBase.isMainSkill && skillBase.hero.team.modIndex1068.Contains(skillBase.hero.GetSkillType()))
				{
					monster.unTriggerDieSkill = true;
				}
				if (monster.monsterIndex40402 != null && monster.monsterIndex40402.monsterIndex40402 && skillBase.hero.GetSkillType() == monster.monsterIndex40402.skillAttackType)
				{
					monster.monsterIndex40402.monsterSkill.TriggerDieSkill(null);
				}
			}
			ShowMonsterHitEffect(monster);
		}
		return result;
	}

	private void ShowMonsterHitEffect(zbMonster monster)
	{
		if (!zbCombatSystem.Instance.combatFPS.IsSpriteReplaceMode() && zbCombatSystem.Instance.isWorldBoss() && monster.IsBoss())
		{
			zbMonsterHitEffect monsteHitEffect = monster.combatData.monsterHitEffectPoolMgr.GetMonsteHitEffect();
			if ((Object)(object)monsteHitEffect != (Object)null)
			{
				monsteHitEffect.InitData(monster.combatData, monster.GetUUID());
			}
		}
	}

	public void GetMonsterRandomList(zbSkillBase skillBase, zbMonster monster)
	{
		if (monsterRandomList.Count <= 0)
		{
			skillBase.combatData.monsters.GetRandomMonsters(20, monsterRandomList, monster.GetUUID());
		}
	}

	public void HitMonsterTodo(zbSkillBase skillBase, EAttrSkillType attrSkillType, zbMonster monster, int skillAttackType, bool isAgain = false, bool param = false)
	{
		//IL_0dc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dda: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ddc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0822: Unknown result type (might be due to invalid IL or missing references)
		//IL_0888: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_099b: Unknown result type (might be due to invalid IL or missing references)
		bool isBallistic = skillBase.isBallistic;
		zbCombatTeam team = skillBase.hero.team;
		if (skillBase.isMainSkill)
		{
			int skillType = skillBase.hero.GetSkillType();
			if (team.mainHitToSubSkillBySkill.TryGetValue(skillType, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					stMainHitToSubSkill stMainHitToSubSkill2 = value[i];
					if (stMainHitToSubSkill2.damageType == (int)attrSkillType && zbUtil.Roll0to1(stMainHitToSubSkill2.roll0to1))
					{
						for (int j = 0; j < stMainHitToSubSkill2.subCount; j++)
						{
							GetMonsterRandomList(skillBase, monster);
							zbSkillSystem.Instance.CreateSubSkill(stMainHitToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > j) ? monsterRandomList[j].GetUUID() : 0);
						}
					}
				}
			}
			for (int k = 0; k < team.designSkillHitBuffToSubSkill.Count; k++)
			{
				stDesignSkillHitBuffToSubSkill stDesignSkillHitBuffToSubSkill2 = team.designSkillHitBuffToSubSkill[k];
				if (stDesignSkillHitBuffToSubSkill2.srcSkillId == skillBase.hero.skillRecord.id && stDesignSkillHitBuffToSubSkill2.skillType == skillType && zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(monster.GetUUID(), stDesignSkillHitBuffToSubSkill2.srcBuffId) != null)
				{
					for (int l = 0; l < stDesignSkillHitBuffToSubSkill2.subCount; l++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stDesignSkillHitBuffToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > l) ? monsterRandomList[l].GetUUID() : 0);
					}
				}
			}
			for (int m = 0; m < team.modIndex1025.Count; m++)
			{
				stModIndex1025 stModIndex2146 = team.modIndex1025[m];
				if (stModIndex2146.srcSkillId == skillBase.hero.skillRecord.id && stModIndex2146.skillType == skillType)
				{
					for (int n = 0; n < stModIndex2146.subCount; n++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stModIndex2146.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > n) ? monsterRandomList[n].GetUUID() : 0);
					}
				}
			}
			if (team.mainHitBuffToSubSkillBySkill.TryGetValue(skillType, out var value2))
			{
				for (int num = 0; num < value2.Count; num++)
				{
					stMainHitBuffToSubSkill stMainHitBuffToSubSkill2 = value2[num];
					if (zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(monster.GetUUID(), stMainHitBuffToSubSkill2.srcBuffId) != null && zbUtil.Roll0to1(stMainHitBuffToSubSkill2.roll0to1))
					{
						for (int num2 = 0; num2 < stMainHitBuffToSubSkill2.subCount; num2++)
						{
							GetMonsterRandomList(skillBase, monster);
							zbSkillSystem.Instance.CreateSubSkill(stMainHitBuffToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num2) ? monsterRandomList[num2].GetUUID() : 0);
						}
					}
				}
			}
			if (skillBase.hitNumber == 1 && team.mainFirstHitToSubSkillBySkill.TryGetValue(skillType, out var value3))
			{
				for (int num3 = 0; num3 < value3.Count; num3++)
				{
					stMainFirstHitToSubSkill stMainFirstHitToSubSkill2 = value3[num3];
					if (zbUtil.Roll0to1(stMainFirstHitToSubSkill2.roll0to1))
					{
						for (int num4 = 0; num4 < stMainFirstHitToSubSkill2.subCount; num4++)
						{
							GetMonsterRandomList(skillBase, monster);
							zbSkillSystem.Instance.CreateSubSkill(stMainFirstHitToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num4) ? monsterRandomList[num4].GetUUID() : 0);
						}
					}
				}
			}
			int num5 = 0;
			for (int num6 = 0; num6 < team.mainHitDefaultToSubSkill.Count; num6++)
			{
				stMainHitDefaultToSubSkill stMainHitDefaultToSubSkill2 = team.mainHitDefaultToSubSkill[num6];
				if (stMainHitDefaultToSubSkill2.skillType == skillType && skillBase.mainMonsterUUID == monster.GetUUID())
				{
					for (int num7 = 0; num7 < stMainHitDefaultToSubSkill2.subCount; num7++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stMainHitDefaultToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num5) ? monsterRandomList[num5].GetUUID() : 0);
						num5++;
					}
				}
			}
			for (int num8 = 0; num8 < team.modIndex1040.Count; num8++)
			{
				stModIndex1040 stModIndex2147 = team.modIndex1040[num8];
				if (stModIndex2147.skillType != skillType)
				{
					continue;
				}
				skillBase.hero.hitMonsterList.Add(monster.GetUUID());
				if (skillBase.hero.hitMonsterList.Count >= stModIndex2147.releaseNumber)
				{
					for (int num9 = 0; num9 < stModIndex2147.extraNumber; num9++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stModIndex2147.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num9) ? monsterRandomList[num9].GetUUID() : 0);
					}
					skillBase.hero.hitMonsterList.Clear();
				}
			}
			if (team.mainHitToSubBuffBySkill.TryGetValue(skillType, out var value4))
			{
				for (int num10 = 0; num10 < value4.Count; num10++)
				{
					stMainHitToSubBuff stMainHitToSubBuff2 = value4[num10];
					if (zbUtil.Roll0to1(stMainHitToSubBuff2.roll0to1))
					{
						zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stMainHitToSubBuff2.subBuffId, stMainHitToSubBuff2.extraTime);
					}
				}
			}
			for (int num11 = 0; num11 < team.modIndex1076.Count; num11++)
			{
				stModIndex1076 stModIndex2148 = team.modIndex1076[num11];
				if (stModIndex2148.skillType == skillType && zbUtil.Roll0to1(stModIndex2148.roll0to1))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stModIndex2148.subBuffId, stModIndex2148.extraTime);
				}
			}
			for (int num12 = 0; num12 < skillBase.skillRecord.buff.Length; num12++)
			{
				float[] array = skillBase.skillRecord.buff[num12];
				if (zbUtil.Roll0to1(array[0]))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), (int)array[1], array[2]);
				}
			}
			if (!isAgain)
			{
				for (int num13 = 0; num13 < team.modIndex1034.Count; num13++)
				{
					stModIndex1034 stModIndex2149 = team.modIndex1034[num13];
					if (stModIndex2149.skillType == skillType && zbUtil.Roll0to1(stModIndex2149.roll0to1))
					{
						for (int num14 = 0; num14 < stModIndex2149.extraNumber; num14++)
						{
							CalcMonsterDamage(skillBase, attrSkillType, monster, 0f, skillAttackType, isAgain: true);
						}
					}
				}
			}
			for (int num15 = 0; num15 < zbCombatDefine.mainSkillNeedKnockback.Count; num15++)
			{
				if (zbCombatDefine.mainSkillNeedKnockback[num15] == (ESkillType)skillType && skillBase.GetAttrValueByType(EAttrSkillType.Knockback) > 0f)
				{
					zbSkillSystem.Instance.CreateSubSkill(10203, skillBase.hero, skillBase, skillBase.skillAttrList, skillBase.skillTF.position, Vector2.op_Implicit(skillBase.getDirection()), monster.GetUUID());
				}
			}
			for (int num16 = 0; num16 < zbCombatDefine.mainSkillNeedTow.Count; num16++)
			{
				if (zbCombatDefine.mainSkillNeedTow[num16] == (ESkillType)skillType && skillBase.GetAttrValueByType(EAttrSkillType.Tow) > 0f)
				{
					zbSkillSystem.Instance.CreateSubSkill(11302, skillBase.hero, skillBase, skillBase.skillAttrList, skillBase.skillTF.position, Vector2.up, monster.GetUUID());
				}
			}
			if (!monster.isEliteBoss)
			{
				for (int num17 = 0; num17 < team.modIndex1077.Count; num17++)
				{
					stModIndex1077 stModIndex2150 = team.modIndex1077[num17];
					if (stModIndex2150.skillType == skillType && zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(monster.GetUUID(), stModIndex2150.srcBuffId) != null)
					{
						zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stModIndex2150.tarBuffId, stModIndex2150.extraTime);
					}
				}
				for (int num18 = 0; num18 < team.modIndex1036.Count; num18++)
				{
					stModIndex1036 stModIndex2151 = team.modIndex1036[num18];
					int num19 = 0;
					if (zbUtil.Roll0to1(stModIndex2151.roll0to1))
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stModIndex2151.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, skillBase.skillTF.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num19) ? monsterRandomList[num19].GetUUID() : 0);
						num19++;
					}
				}
			}
		}
		else if (skillBase.isSubSkill)
		{
			for (int num20 = 0; num20 < team.subHitToSubSkill.Count; num20++)
			{
				stSubHitToSubSkill stSubHitToSubSkill2 = team.subHitToSubSkill[num20];
				if (stSubHitToSubSkill2.srcSubSkillId == skillBase.skillSubRecord.id)
				{
					for (int num21 = 0; num21 < stSubHitToSubSkill2.subCount; num21++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(stSubHitToSubSkill2.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, skillBase.skillTF.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num21) ? monsterRandomList[num21].GetUUID() : 0);
					}
				}
			}
			for (int num22 = 0; num22 < team.subSkillHitToSubBuff.Count; num22++)
			{
				stSubSkillHitToSubBuff stSubSkillHitToSubBuff2 = team.subSkillHitToSubBuff[num22];
				if (stSubSkillHitToSubBuff2.subSkillId == skillBase.skillSubRecord.id && zbUtil.Roll0to1(stSubSkillHitToSubBuff2.roll0to1))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stSubSkillHitToSubBuff2.subBuffId, stSubSkillHitToSubBuff2.extraTime);
				}
			}
			for (int num23 = 0; num23 < skillBase.skillSubRecord.skillSub.Length; num23++)
			{
				int[] array2 = skillBase.skillSubRecord.skillSub[num23];
				GetMonsterRandomList(skillBase, monster);
				for (int num24 = 0; num24 < array2[0]; num24++)
				{
					zbSkillSystem.Instance.CreateSubSkill(array2[1], skillBase.hero, skillBase, skillBase.skillAttrList, skillBase.skillTF.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > num24) ? monsterRandomList[num24].GetUUID() : 0);
				}
			}
			for (int num25 = 0; num25 < skillBase.skillSubRecord.skillBuff.Length; num25++)
			{
				float[] array3 = skillBase.skillSubRecord.skillBuff[num25];
				if (zbUtil.Roll0to1(array3[0]))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), (int)array3[1], array3[2]);
				}
			}
			for (int num26 = 0; num26 < team.modIndex1076.Count; num26++)
			{
				stModIndex1076 stModIndex2152 = team.modIndex1076[num26];
				if (stModIndex2152.srcSubSkillId == skillBase.skillSubRecord.id && zbUtil.Roll0to1(stModIndex2152.roll0to1))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stModIndex2152.subBuffId, stModIndex2152.extraTime);
				}
			}
		}
		for (int num27 = 0; num27 < team.modIndex1059.Count; num27++)
		{
			stModIndex1059 stModIndex2153 = team.modIndex1059[num27];
			if (skillAttackType == stModIndex2153.skillAttackType)
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stModIndex2153.buffId, stModIndex2153.extraTime);
			}
		}
		if (!monster.isEliteBoss)
		{
			for (int num28 = 0; num28 < team.modIndex1063.Count; num28++)
			{
				stModIndex1063 stModIndex2154 = team.modIndex1063[num28];
				if (stModIndex2154.skillType == skillBase.hero.GetSkillType() && zbUtil.Roll0to1(stModIndex2154.roll0to1))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), stModIndex2154.subBuffId, stModIndex2154.extraTime);
				}
			}
		}
		if (isBallistic && monster.monsterIndex10172)
		{
			Vector3 direction = skillBase.getDirection();
			direction.y *= -1f;
			skillBase.SetDirection(Vector2.op_Implicit(direction));
		}
		ESkillAttackTypeHitMonsterTodo(skillBase, monster, skillAttackType);
	}

	public void ESkillAttackTypeHitMonsterTodo(zbSkillBase skillBase, zbMonster monster, int skillAttackType)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (monster.monsterIndex10211.monsterIndex10211 && skillAttackType == monster.monsterIndex10211.skillAttackType && monster.monsterIndex10211.durationTimeKeep <= 0f)
		{
			monster.monsterIndex10211.skillBase.PlayTackAnimation(1, flag: true);
			monster.monsterIndex10211.durationTimeKeep = monster.monsterIndex10211.durationTime;
		}
		for (int i = 0; i < monster.monsterIndex10281.Count; i++)
		{
			stMonsterSkillIndex10281 stMonsterSkillIndex40463 = monster.monsterIndex10281[i];
			if (skillAttackType == stMonsterSkillIndex40463.attackType)
			{
				monster.ChangeHp(null, null, (long)(stMonsterSkillIndex40463.attrPercent * (float)(long)monster.maxHp));
			}
		}
		for (int j = 0; j < monster.monsterIndex10332.Count; j++)
		{
			stMonsterSkillIndex10332 stMonsterSkillIndex40464 = monster.monsterIndex10332[j];
			if (skillAttackType == stMonsterSkillIndex40464.attackType && zbUtil.Roll0to1(stMonsterSkillIndex40464.roll0to1))
			{
				for (int k = 0; k < stMonsterSkillIndex40464.subCount; k++)
				{
					zbSkillSystem.Instance.CreateSubSkill(stMonsterSkillIndex40464.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, monster.GetCurPosition(), Vector2.up);
				}
			}
		}
		float[][] buff;
		if (skillBase.isMechaSkill && !skillBase.isSubSkill && (long)monster.curHp > 0)
		{
			buff = skillBase.mechaSkillRecord.buff;
			foreach (float[] array in buff)
			{
				if (zbUtil.Roll0to1(array[0]))
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), (int)array[1], array[2]);
				}
			}
		}
		if (!skillBase.isWallSkill || (long)monster.curHp <= 0)
		{
			return;
		}
		buff = skillBase.wallSkillRecord.buff;
		foreach (float[] array2 in buff)
		{
			if (zbUtil.Roll0to1(array2[0]))
			{
				zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, skillBase.skillAttrList, monster.GetUUID(), (int)array2[1], array2[2]);
			}
		}
	}

	public void KillMonsterTodo(zbSkillBase skillBase, EAttrSkillType attrSkillType, zbMonster monster, int skillAttackType)
	{
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		zbCombatTeam team = skillBase.hero.team;
		if (skillBase.isMainSkill)
		{
			if ((Object)(object)skillBase.hero.pet != (Object)null)
			{
				skillBase.hero.pet.TriggerKill();
			}
			int skillType = skillBase.hero.GetSkillType();
			foreach (stMainKillToSubSkill item in team.mainKillToSubSkill)
			{
				if (item.skillType == skillType && zbUtil.Roll0to1(item.roll0to1))
				{
					for (int i = 0; i < item.subCount; i++)
					{
						GetMonsterRandomList(skillBase, monster);
						zbSkillSystem.Instance.CreateSubSkill(item.subSkillId, skillBase.hero, skillBase, skillBase.skillAttrList, ((Component)skillBase).transform.position, Vector2.up, monster.GetUUID(), (monsterRandomList.Count > i) ? monsterRandomList[i].GetUUID() : 0);
					}
				}
			}
			foreach (stModIndex1035 item2 in team.modIndex1035)
			{
				if (item2.skillType != skillType)
				{
					continue;
				}
				foreach (zbHero hero in team.heroList)
				{
					if (hero.GetSkillType() != 1)
					{
						hero.nextFireTime += item2.nextFireTimeRatio * hero.GetAttrValueByType(EAttrSkillType.Cd);
					}
				}
			}
			foreach (stModIndex1041 item3 in team.modIndex1041)
			{
				if (item3.skillType == skillType)
				{
					item3.killNumber++;
					item3.cdDecRatio = Math.Min((float)item3.killNumber * item3.cdPreRatio, item3.cdMaxRatio);
				}
			}
			foreach (stModIndex1065 item4 in team.modIndex1065)
			{
				if (item4.skillType == skillType)
				{
					skillBase.hero.nextFireTime += item4.cdDecPreTime;
				}
			}
			foreach (stModIndex1079 item5 in team.modIndex1079)
			{
				if (item5.skillType == skillType && skillBase.hero.needDurationProgressMaxTemp < item5.maxExtraTime)
				{
					skillBase.hero.needDurationProgress += item5.addExtraTime;
					skillBase.hero.needDurationProgressMaxTemp = skillBase.hero.needDurationProgressMaxTemp + item5.addExtraTime;
				}
			}
			ESkillAttackTypeKillMonsterTodo(monster, skillAttackType);
		}
		foreach (stModIndex1080 item6 in team.modIndex1080)
		{
			item6.curKillCount++;
			if (item6.curKillCount < item6.killTotleCount)
			{
				continue;
			}
			zbHeroQiang zbHeroQiang2 = (zbHeroQiang)team.GetHeroBySkillType(ESkillType.Qiang);
			if ((Object)(object)zbHeroQiang2 != (Object)null)
			{
				for (int j = 0; j < item6.subCount; j++)
				{
					GetMonsterRandomList(skillBase, monster);
					zbSkillSystem.Instance.CreateSubSkill(item6.subSkillId, zbHeroQiang2, skillBase, skillBase.skillAttrList, zbHeroQiang2.GetGunpointPosition(), Vector2.up, monster.GetUUID(), (monsterRandomList.Count > j) ? monsterRandomList[j].GetUUID() : 0);
				}
			}
			item6.curKillCount = 0;
		}
	}

	public void ESkillAttackTypeKillMonsterTodo(zbMonster monster, int skillAttackType)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		foreach (stMonsterSkillIndex10201 item in monster.monsterIndex10201)
		{
			if (skillAttackType != item.unAttackType)
			{
				for (int i = 0; i < item.monsterNum; i++)
				{
					monster.combatData.monsters.SpawnMonsterAtPosition(item.monsterTid, monster.monsterTF.position, monster.monsterTF.parent);
				}
			}
		}
	}

	public float AddMonsterDamage(zbSkillBase skillBase, float damage, zbMonster monster, int skillAttackType, bool isViolent)
	{
		float num = damage;
		float num2 = 1f;
		num *= skillBase.GetExtraAddRatio(monster);
		num = ESkillAttackTypeAddMonsterDamage(skillBase, num, monster, skillAttackType, isViolent);
		zbCombatTeam team = skillBase.hero.team;
		if (skillBase.isMainSkill)
		{
			int skillType = skillBase.hero.GetSkillType();
			if (skillBase.mainMonsterUUID == monster.GetUUID() && team.mainToDefaultAddDamage.TryGetValue(skillType, out var value))
			{
				num *= 1f + value;
			}
			bool flag = zbCombatSystem.Instance.buffSystem.IsMonsterHaveAnyBuff(monster.GetUUID());
			if (flag)
			{
				num2 = 1f;
				int count = team.modIndex1039.Count;
				for (int i = 0; i < count; i++)
				{
					stModIndex1039 stModIndex2146 = team.modIndex1039[i];
					if (stModIndex2146.skillType == skillType)
					{
						num2 += stModIndex2146.extraDamageRatio;
					}
				}
				num *= num2;
			}
			if (skillBase.ricochetCount > 0)
			{
				num2 = 1f;
				int count2 = team.modIndex1042.Count;
				for (int j = 0; j < count2; j++)
				{
					stModIndex1042 stModIndex2147 = team.modIndex1042[j];
					if (stModIndex2147.skillType == skillType)
					{
						num2 += Math.Min(stModIndex2147.extraPreRatio * (float)skillBase.ricochetCount, stModIndex2147.extraMaxRatio);
					}
				}
				num *= num2;
			}
			num2 = 1f;
			int count3 = team.modIndex1024.Count;
			for (int k = 0; k < count3; k++)
			{
				stModIndex1024 stModIndex2148 = team.modIndex1024[k];
				if (skillType == stModIndex2148.skillType && zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(monster.GetUUID(), stModIndex2148.srcBuffId) != null)
				{
					num2 += stModIndex2148.damagePercent;
				}
			}
			num *= num2;
			if (skillType == 12)
			{
				num2 = 1f;
				float num3 = team.ExtraProperties[ETeamExtra.DianqiuMod2123];
				if (num3 > 0f)
				{
					num2 += num3 * (float)skillBase.ricochetCount;
				}
				num *= num2;
			}
			if ((skillBase.isMainSkill && skillType == 13) & flag)
			{
				num *= 1f + team.ExtraProperties[ETeamExtra.LongjuanMod2133];
			}
			num2 = 1f;
			int count4 = team.modIndex1069.Count;
			for (int l = 0; l < count4; l++)
			{
				stModIndex1069 stModIndex2149 = team.modIndex1069[l];
				if (skillType == stModIndex2149.skillType && zbUtil.Roll0to1(stModIndex2149.roll0to1))
				{
					num2 += stModIndex2149.damagePercent;
				}
			}
			num *= num2;
			if (team.modIndex2144.skillType == skillType && team.modIndex2144.damagePrecent > 0f)
			{
				num *= 1f + team.modIndex2144.damagePrecent;
			}
		}
		else if (skillBase.isSubSkill)
		{
			num *= team.subSkillAddDamage[skillBase.skillSubRecord.id];
		}
		if (zbSelf.Instance.bossSubjectHandler.currDamageAdd > 0f)
		{
			num *= 1f + zbSelf.Instance.bossSubjectHandler.currDamageAdd;
		}
		if (!monster.isEliteBoss)
		{
			num2 = 1f;
			int count5 = team.modIndex1043.Count;
			for (int m = 0; m < count5; m++)
			{
				stModIndex1043 stModIndex2150 = team.modIndex1043[m];
				if (stModIndex2150.hpGreaterPresent <= (float)(long)monster.curHp / (float)(long)monster.maxHp)
				{
					num2 += stModIndex2150.hpGreaterDamageAdd;
				}
			}
			num *= num2;
			num2 = 1f;
			int num4 = (int)zbCombatSystem.Instance.combatRecorder.curRound + 1;
			int count6 = team.modIndex1044.Count;
			for (int n = 0; n < count6; n++)
			{
				stModIndex1044 stModIndex2151 = team.modIndex1044[n];
				if (stModIndex2151.firstNumberWave >= num4)
				{
					num2 += stModIndex2151.damageRatio;
				}
			}
			num *= num2;
		}
		return num;
	}

	public float ESkillAttackTypeAddMonsterDamage(zbSkillBase skillBase, float damage, zbMonster monster, int skillAttackType, bool isViolent)
	{
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		float num = damage;
		float num2 = 1f;
		zbCombatTeam team = skillBase.hero.team;
		bool isEliteBoss = monster.isEliteBoss;
		if (isEliteBoss)
		{
			num2 = 1f;
			for (int i = 0; i < team.modIndex1031.Count; i++)
			{
				stModIndex1031 stModIndex2146 = team.modIndex1031[i];
				num2 += stModIndex2146.extraDamageRatio;
			}
			num *= num2;
			for (int j = 0; j < skillBase.combatData.wallSkillIndex5071DataList.Count; j++)
			{
				stWallSkillIndex5071 stWallSkillIndex5082 = skillBase.combatData.wallSkillIndex5071DataList[j];
				if ((float)stWallSkillIndex5082.damageBossAddPrecent > 0f)
				{
					num *= 1f + (float)stWallSkillIndex5082.damageBossAddPrecent;
				}
			}
		}
		if (zbCombatSystem.Instance.buffSystem.IsMonsterHaveAnyBuff(monster.GetUUID()))
		{
			num2 = 1f;
			for (int k = 0; k < team.modIndex1033.Count; k++)
			{
				stModIndex1033 stModIndex2147 = team.modIndex1033[k];
				num2 += stModIndex2147.extraDamageRatio;
			}
			num *= num2;
		}
		num2 = 1f;
		for (int l = 0; l < team.modIndex1051.Count; l++)
		{
			stModIndex1051 stModIndex2148 = team.modIndex1051[l];
			int count = team.heroList.Count;
			num2 += stModIndex2148.extraDamageRatio * (float)count;
		}
		num *= num2;
		num2 = 1f;
		for (int m = 0; m < team.modIndex1053.Count; m++)
		{
			stModIndex1053 stModIndex2149 = team.modIndex1053[m];
			if (!skillBase.combatData.monsters.IsInAlertRange(monster.monsterTF.position.y, stModIndex2149.distance))
			{
				num2 += stModIndex2149.extraDamageRatio;
			}
		}
		num *= num2;
		num2 = 1f;
		for (int n = 0; n < team.modIndex1055.Count; n++)
		{
			stModIndex1055 stModIndex2150 = team.modIndex1055[n];
			zbCombatDefense defense = skillBase.combatData.defense;
			if ((float)(long)defense.curHp <= stModIndex2150.defenseHpLess * (float)(long)defense.maxHp)
			{
				num2 += stModIndex2150.extraDamageRatio;
			}
		}
		num *= num2;
		num2 = 1f;
		for (int num3 = 0; num3 < monster.monsterIndex10261.Count; num3++)
		{
			stMonsterSkillIndex10261 stMonsterSkillIndex40463 = monster.monsterIndex10261[num3];
			if (skillBase.hero.GetSkillType() == stMonsterSkillIndex40463.skillType)
			{
				num2 += stMonsterSkillIndex40463.damagePercent;
			}
		}
		num *= num2;
		num2 = 1f;
		for (int num4 = 0; num4 < monster.monsterIndex10311.Count; num4++)
		{
			stMonsterSkillIndex10311Extra stMonsterSkillIndex10311Extra2 = monster.monsterIndex10311[num4];
			if (skillAttackType == stMonsterSkillIndex10311Extra2.attackType)
			{
				num2 += stMonsterSkillIndex10311Extra2.attrPercent;
			}
		}
		num *= num2;
		num2 = 1f;
		for (int num5 = 0; num5 < monster.monsterIndex10321.Count; num5++)
		{
			stMonsterSkillIndex10321 stMonsterSkillIndex40464 = monster.monsterIndex10321[num5];
			if (skillAttackType == stMonsterSkillIndex40464.attackType && zbUtil.Roll0to1(stMonsterSkillIndex40464.roll0to1))
			{
				num2 += stMonsterSkillIndex40464.damagePercent;
			}
		}
		num *= num2;
		float num6 = team.ExtraProperties[ETeamExtra.GemMod1057_2];
		if (num6 > 0f)
		{
			num *= 1f + Rand.Range(team.ExtraProperties[ETeamExtra.GemMod1057_1], num6);
		}
		if ((long)skillBase.combatData.defense.shieldHp > 0)
		{
			int count2 = skillBase.combatData.wallSkillIndex5031DataList.Count;
			for (int num7 = 0; num7 < count2; num7++)
			{
				stWallSkillIndex5031 stWallSkillIndex5083 = skillBase.combatData.wallSkillIndex5031DataList[num7];
				num *= 1f + (float)stWallSkillIndex5083.damageAddPrecent;
			}
		}
		if (skillBase.combatData.defense.curHp < skillBase.combatData.defense.maxHp)
		{
			int count3 = skillBase.combatData.wallSkillIndex5051DataList.Count;
			for (int num8 = 0; num8 < count3; num8++)
			{
				stWallSkillIndex5051 stWallSkillIndex5084 = skillBase.combatData.wallSkillIndex5051DataList[num8];
				if ((float)stWallSkillIndex5084.damageAddPrecent > 0f)
				{
					num *= 1f + (float)stWallSkillIndex5084.damageAddPrecent;
				}
			}
		}
		if (team.battleTeamInfo.FootholdInfo != null && team.battleTeamInfo.FootholdInfo.Damage > 0f)
		{
			num *= team.battleTeamInfo.FootholdInfo.Damage;
		}
		num *= 1f + (float)monster.beDamageUpPercent;
		if (!isEliteBoss)
		{
			int count4 = team.modIndex1029.Count;
			for (int num9 = 0; num9 < count4; num9++)
			{
				stModIndex1029 stModIndex2151 = team.modIndex1029[num9];
				num += Math.Min(stModIndex2151.extraDamageRatio * (float)(long)monster.maxHp, (float)(int)team.teamAttack * stModIndex2151.limitDamageRatio);
			}
			if (isViolent)
			{
				int count5 = team.modIndex1030.Count;
				for (int num10 = 0; num10 < count5; num10++)
				{
					stModIndex1030 stModIndex2152 = team.modIndex1030[num10];
					num += Math.Min(stModIndex2152.extraDamageRatio * (float)(long)monster.maxHp, (float)(int)team.teamAttack * stModIndex2152.limitDamageRatio);
				}
			}
		}
		return num;
	}

	public float DecMonsterDamage(zbSkillBase skillBase, float damage, zbMonster monster, int skillAttackType, bool isViolent)
	{
		float num = damage;
		bool flag = false;
		eSkillTag.Clear();
		if (skillBase.isMainSkill)
		{
			int[] tag = skillBase.hero.skillRecord.tag;
			foreach (int item in tag)
			{
				eSkillTag.Add(item);
			}
			foreach (stModIndex1062 item3 in skillBase.hero.team.modIndex1062)
			{
				if (item3.skillType == skillBase.hero.GetSkillType() && zbUtil.Roll0to1(item3.roll0to1))
				{
					flag = true;
					break;
				}
			}
		}
		else if (skillBase.isSubSkill)
		{
			int[] tag = skillBase.skillSubRecord.tag;
			foreach (int item2 in tag)
			{
				eSkillTag.Add(item2);
			}
		}
		float[][] skillTagWeak = monster.monsterRecord.skillTagWeak;
		foreach (float[] array in skillTagWeak)
		{
			if (eSkillTag.Contains((int)array[0]) && (!flag || !(array[1] < 0f)))
			{
				num *= 1f - array[1];
			}
		}
		return ESkillAttackTypeDecMonsterDamage(skillBase, num, monster, skillAttackType);
	}

	public float ESkillAttackTypeDecMonsterDamage(zbSkillBase skillBase, float damage, zbMonster monster, int skillAttackType)
	{
		float num = damage;
		bool flag = false;
		int count = skillBase.hero.team.modIndex1032.Count;
		for (int i = 0; i < count; i++)
		{
			if (zbUtil.Roll0to1(skillBase.hero.team.modIndex1032[i].roll0to1))
			{
				flag = true;
				break;
			}
		}
		float[][] attackTypeWeak = monster.monsterRecord.attackTypeWeak;
		foreach (float[] array in attackTypeWeak)
		{
			if (skillAttackType == (int)array[0] && (!flag || !(array[1] < 0f)))
			{
				num *= 1f - array[1];
			}
		}
		TriggerBigBOSSDamage(skillBase, num, monster, skillAttackType);
		return num * monster.GetAttrValueByType(EAttrMonsterType.DamageTaken);
	}

	public float TriggerBigBOSSDamage(zbSkillBase skillBase, float damage, zbMonster monster, int skillAttackType)
	{
		float num = damage;
		if (monster.monsterSkillIndex3011 == 2 && (long)monster.shieldHp > 0)
		{
			num = ((!eSkillTag.Contains(3)) ? (num * (1f - monster.monsterSkillIndex30112.jianshangPre)) : (num * (1f + monster.monsterSkillIndex30112.dimianPre)));
		}
		if (monster.monsterSkillIndex3011 == 3)
		{
			num *= 1f + monster.monsterSkillIndex30113.hitDamagePre;
		}
		if (monster.monsterSkillIndex3011BreakFlag)
		{
			num *= 1f + monster.monsterSkillIndex3011BreakHitDam;
		}
		if (monster.monsterSkillIndex30122.trigger30123Num > 0)
		{
			num *= 1f + monster.monsterSkillIndex30123.zengshangPre * (float)monster.monsterSkillIndex30122.trigger30123Num;
		}
		if (monster.monsterSkillIndex3013 == 2)
		{
			if (monster.monster30132Class.rockPingzi && skillAttackType == 5 && monster.monsterSkillIndex30132.energyAttackCdKeep >= monster.monsterSkillIndex30132.energyAttackCd)
			{
				stMonsterSkillIndex30132 monsterSkillIndex = monster.monsterSkillIndex30132;
				int energyAttackNumKeep = monsterSkillIndex.energyAttackNumKeep + 1;
				monsterSkillIndex.energyAttackNumKeep = energyAttackNumKeep;
			}
		}
		else if (monster.monsterSkillIndex3013 == 3 && monster.monster30133Class.rockPingzi && skillAttackType == 5 && monster.monsterSkillIndex30133.energyAttackCdKeep >= monster.monsterSkillIndex30133.energyAttackCd)
		{
			stMonsterSkillIndex30133 monsterSkillIndex2 = monster.monsterSkillIndex30133;
			int energyAttackNumKeep = monsterSkillIndex2.energyAttackNumKeep + 1;
			monsterSkillIndex2.energyAttackNumKeep = energyAttackNumKeep;
		}
		if (monster.monsterSkillIndex3014 == 2 && monster.monster3014DazeDam <= monster.monsterSkillIndex30142.dazeTime)
		{
			num *= 1f + monster.monsterSkillIndex30142.hitDamagePre;
		}
		else if (monster.monsterSkillIndex3014 == 3 && monster.monster3014DazeDam <= monster.monsterSkillIndex30143.dazeTime)
		{
			num *= 1f + monster.monsterSkillIndex30143.hitDamagePre;
		}
		if (monster.monster30111Class != null)
		{
			monster.monster30111Class.TriggerBreakSkill(skillAttackType);
		}
		if (monster.monster30151Class != null)
		{
			monster.monster30151Class.TriggerBreakSkill(skillAttackType);
		}
		if (monster.monster30161Class != null)
		{
			monster.monster30161Class.TriggerBreakSkill(skillAttackType);
		}
		if (monster.monsterSkillIndex3015BreakFlag)
		{
			num *= 1f + monster.monsterSkillIndex3015BreakHitDam;
		}
		if (monster.monsterSkillIndex3016BreakFlag)
		{
			num *= 1f + monster.monsterSkillIndex3016BreakHitDam;
		}
		return num;
	}

	public stDamageResult CalcBuffMonsterDamage(zbSkillBase skillBase, zbMonster monster, float damage = 0f, int _skillAttackType = 0, bool ignoreAttackTypeAdd = false)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		long num = monster.curHp;
		float damage2 = damage;
		stDamageResult stDamageResult2 = new stDamageResult
		{
			damageType = EDamageResult.Hit,
			isKilled = false,
			isViolent = false
		};
		stDamageResult2 = CalcMonsterImmuneDamage(skillBase, monster);
		if (stDamageResult2.damageType != EDamageResult.Hit)
		{
			return stDamageResult2;
		}
		if (!ignoreAttackTypeAdd)
		{
			damage2 = ESkillAttackTypeAddMonsterDamage(skillBase, damage2, monster, _skillAttackType, stDamageResult2.isViolent);
		}
		damage2 = ESkillAttackTypeDecMonsterDamage(skillBase, damage2, monster, _skillAttackType);
		if (damage2 <= 0f)
		{
			stDamageResult2.damageType = EDamageResult.Immune;
			stDamageResult2.damageValue = 0L;
			stDamageResult2.isViolent = false;
			Vector3 position = monster.monsterTF.position;
			((Vector3)(ref bubblePosition)).Set(position.x, position.y + 0.4f, position.z);
			skillBase.combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Immune, 0L);
			return stDamageResult2;
		}
		stDamageResult2.damageValue = (long)damage2;
		stDamageResult2.damageValue = ReplayAddDamage(stDamageResult2.damageValue);
		stDamageResult2.damageValue = Math.Max(stDamageResult2.damageValue, 1L);
		stDamageResult2.damageValue = Math.Min(stDamageResult2.damageValue, long.MaxValue);
		stDamageResult2.damageValue = -monster.ChangeHp(skillBase.hero, null, -stDamageResult2.damageValue, stDamageResult2.isViolent);
		stDamageResult2.isKilled = num > 0 && monster.isDead;
		if (stDamageResult2.damageValue > 0)
		{
			ESkillAttackTypeHitMonsterTodo(skillBase, monster, _skillAttackType);
			if (stDamageResult2.isKilled)
			{
				ESkillAttackTypeKillMonsterTodo(monster, _skillAttackType);
			}
		}
		return stDamageResult2;
	}

	public stDamageResult CalcMonsterImmuneDamage(zbSkillBase skillBase, zbMonster monster)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		stDamageResult result = new stDamageResult
		{
			damageType = EDamageResult.Hit,
			isKilled = false,
			isViolent = false
		};
		if (monster.monsterSkillIndexBianShen)
		{
			result.damageValue = 0L;
			result.isViolent = false;
			return result;
		}
		if (monster.monsterRecord.moveType == 2 && skillBase.IsSkillTag(3))
		{
			result.damageType = EDamageResult.Unhit;
			result.damageValue = 0L;
			result.isViolent = false;
		}
		List<int> immeTagList = monster.monsterIndex10322.immeTagList;
		int i = 0;
		for (int count = immeTagList.Count; i < count; i++)
		{
			if (skillBase.IsSkillTag(immeTagList[i]))
			{
				result.damageType = EDamageResult.Immune;
				result.damageValue = 0L;
				result.isViolent = false;
				break;
			}
		}
		if (result.damageType == EDamageResult.Unhit)
		{
			result.damageValue = 0L;
			result.isViolent = false;
			Vector3 position = monster.monsterTF.position;
			((Vector3)(ref bubblePosition)).Set(position.x, position.y + 0.4f, position.z);
			skillBase.combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Unhit, 0L);
			return result;
		}
		if (result.damageType == EDamageResult.Immune)
		{
			result.damageValue = 0L;
			result.isViolent = false;
			Vector3 position2 = monster.monsterTF.position;
			((Vector3)(ref bubblePosition)).Set(position2.x, position2.y + 0.4f, position2.z);
			skillBase.combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Immune, 0L);
			return result;
		}
		if (monster.monsterIndex10373.monsterIndex10373)
		{
			monster.monsterIndex10373.skillBase.PlayTackAnimation(1, flag: true);
			monster.monsterIndex10373.monsterIndex10373 = false;
			zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
			if (monster.monsterTF.position.x > monster.combatData.nodeRootPositonX)
			{
				zbExtraDirection2.direction = Vector2.op_Implicit(Vector2.left);
				monster.monsterTF.localScale = new Vector3(0f - Math.Abs(monster.monsterTF.localScale.x), monster.monsterTF.localScale.y, monster.monsterTF.localScale.z);
			}
			else
			{
				zbExtraDirection2.direction = Vector2.op_Implicit(Vector2.right);
				monster.monsterTF.localScale = new Vector3(Math.Abs(monster.monsterTF.localScale.x), monster.monsterTF.localScale.y, monster.monsterTF.localScale.z);
			}
			zbExtraDirection2.duration = monster.monsterIndex10373.monsterIndex10373Distance / monster.monsterIndex10373.monsterIndex10373Speed;
			zbExtraDirection2.speed = monster.monsterIndex10373.monsterIndex10373Speed;
			monster.AddExtraDirectionList(zbExtraDirection2);
			result.damageValue = 0L;
			return result;
		}
		if (!monster.monsterIndex10373.monsterIndex10373 && monster.monsterIndex10373.monsterIndex10373Keep > 0f)
		{
			result.damageValue = 0L;
			return result;
		}
		if (monster.monsterSkillIndex3011Immune)
		{
			result.damageValue = 0L;
			return result;
		}
		if (monster.monsterSkillIndex3013 == 3 && monster.monsterSkillIndex30133.wudiTimeKeep > 0f)
		{
			result.damageValue = 0L;
			return result;
		}
		if (monster.monsterBlockCount > 0)
		{
			monster.monsterBlockCount--;
			result.damageValue = 0L;
			result.isViolent = false;
			Vector3 position3 = monster.monsterTF.position;
			((Vector3)(ref bubblePosition)).Set(position3.x, position3.y + 0.4f, position3.z);
			skillBase.combatData.combatBubble.ShowBubble(bubblePosition, ECombatBubble.Block, 0L);
			return result;
		}
		return result;
	}

	public stDamageResult CalcWallDamage(zbMonster attacker, zbCombatDefense target, float damage = 0f, float positionX = 0f, float hitRatePercent = 1f)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		float damage2 = damage;
		stDamageResult result = default(stDamageResult);
		_ = (long)target.curHp;
		result.damageType = EDamageResult.Hit;
		result.damageValue = 0L;
		result.isKilled = false;
		result.isViolent = false;
		if (Rand.Range(0f, 1f) > hitRatePercent)
		{
			result.damageType = EDamageResult.Unhit;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(Object.op_Implicit((Object)(object)attacker) ? ((Component)attacker).transform.position.x : positionX, target.GetAlertPositionY() + 0.45f, ((Component)target).transform.position.z);
			target.combatData.combatBubble.ShowBubble(val, ECombatBubble.Unhit, 0L);
			return result;
		}
		if (target.immunityDuration > 0f)
		{
			result.damageType = EDamageResult.Immune;
		}
		else if ((int)target.damageImmunity > 0)
		{
			result.damageType = EDamageResult.Immune;
			target.SetDamageImmunity((int)target.damageImmunity - 1);
			if ((Object)(object)attacker != (Object)null)
			{
				float num = target.combatData.masterTeam.ExtraProperties[ETeamExtra.WallDefenseFanji];
				if (num > 0f)
				{
					int num2 = target.combatData.masterTeam.teamAttack;
					zbCombatSystem.Instance.combatFormula.WallAttackMonster(target, attacker, (float)num2 * num);
				}
				if (target.combatData.helperTeam != null)
				{
					num = target.combatData.helperTeam.ExtraProperties[ETeamExtra.WallDefenseFanji];
					if (num > 0f)
					{
						int num3 = target.combatData.helperTeam.teamAttack;
						zbCombatSystem.Instance.combatFormula.WallAttackMonster(target, attacker, (float)num3 * num);
					}
				}
			}
		}
		if (result.damageType == EDamageResult.Immune)
		{
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(Object.op_Implicit((Object)(object)attacker) ? ((Component)attacker).transform.position.x : positionX, target.GetAlertPositionY() + 0.45f, ((Component)target).transform.position.z);
			target.combatData.combatBubble.ShowBubble(val2, ECombatBubble.Immune, 0L);
			return result;
		}
		if ((Object)(object)attacker != (Object)null)
		{
			if (damage <= 0f)
			{
				damage2 = attacker.GetAttrValueByType(EAttrMonsterType.Damage);
			}
			target.combatData.TriggerAction5051(attacker);
			if (attacker.isEliteBoss)
			{
				target.combatData.TriggerAction5071();
			}
		}
		damage2 = ChangeMonsterDamage(damage2, target);
		result.damageValue = (long)damage2;
		if (result.damageValue > 0)
		{
			if ((Object)(object)attacker != (Object)null && attacker.combatData.masterTeam != null)
			{
				foreach (zbHero hero in attacker.combatData.masterTeam.heroList)
				{
					if ((Object)(object)hero.pet != (Object)null)
					{
						hero.pet.TriggerAttacked();
					}
				}
			}
			if ((Object)(object)attacker != (Object)null && attacker.combatData.helperTeam != null)
			{
				foreach (zbHero hero2 in attacker.combatData.helperTeam.heroList)
				{
					if ((Object)(object)hero2.pet != (Object)null)
					{
						hero2.pet.TriggerAttacked();
					}
				}
			}
		}
		target.ChangeHp(Object.op_Implicit((Object)(object)attacker) ? ((Component)attacker).transform.position.x : positionX, -result.damageValue);
		return result;
	}

	public float ChangeMonsterDamage(float damage, zbCombatDefense target)
	{
		float num = damage;
		num = Math.Max(num - (float)(int)target.damageDecr, 1f);
		if ((long)target.shieldHp > 0)
		{
			foreach (stWallSkillIndex5021 wallSkillIndex5021Data in target.combatData.wallSkillIndex5021DataList)
			{
				num *= 1f - (float)wallSkillIndex5021Data.defenseDamageDecPre;
			}
		}
		return Math.Max(num, 1f);
	}

	public stDamageResult WallAttackMonster(zbCombatDefense defense, zbMonster monster, float damage)
	{
		long num = monster.curHp;
		stDamageResult result = default(stDamageResult);
		result.damageType = EDamageResult.Hit;
		result.damageValue = 0L;
		result.isKilled = false;
		result.isViolent = false;
		result.damageValue = (long)damage;
		monster.ChangeHp(null, defense, -result.damageValue);
		result.isKilled = num > 0 && (long)monster.curHp <= 0;
		return result;
	}

	public long ReplayAddDamage(long curDamage)
	{
		return curDamage;
	}

	public zbCombatFormula()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bubblePosition = Vector3.zero;
		monsterRandomList = new List<zbMonster>(20);
		monsterRangeList = new List<zbMonster>();
		eSkillTag = new HashSet<int>();
		base._002Ector();
	}
}
