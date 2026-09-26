using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbBuffBase
{
	protected zbCombatData combatData;

	public zbSkillBase skillBase;

	public zbSkillBuffCsvRecord buffRecord;

	public int monsterUUID;

	protected float extraTime;

	protected float param1;

	protected float param2;

	private int layer;

	protected float damage;

	private zbBuffEffect bulletComponent;

	private float startTm;

	public float durationTime;

	public Dictionary<EAttrSkillType, float> skillAttrList = new Dictionary<EAttrSkillType, float>();

	public int skillBaseHeroSkillType;

	public virtual void Reset()
	{
		combatData = null;
		skillBase = null;
		buffRecord = null;
		monsterUUID = 0;
		extraTime = 0f;
		layer = 0;
		damage = 0f;
		startTm = zbCombatSystem.Instance.accumTime;
		durationTime = 0f;
		skillAttrList.Clear();
	}

	public virtual void Init(zbSkillBase _skillBase, Dictionary<EAttrSkillType, float> _skillAttrList, int _monsterUUID, zbSkillBuffCsvRecord _buffRecord, float _extraTime = 0f, float _param1 = 0f, float _param2 = 0f)
	{
		skillBase = _skillBase;
		combatData = skillBase.combatData;
		skillBaseHeroSkillType = skillBase.hero.GetSkillType();
		monsterUUID = _monsterUUID;
		buffRecord = _buffRecord;
		extraTime = _extraTime;
		param1 = _param1;
		param2 = _param2;
		layer = 0;
		damage = 0f;
		startTm = zbCombatSystem.Instance.accumTime;
		durationTime = 0f;
		RefreshAttrList(_skillAttrList);
		bool flag = !zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode() || Rand.Range(0, 3) == 1;
		if (zbCombatSystem.Instance.combatFPS.IsVeryLowPerfomanceMode())
		{
			flag = false;
		}
		if ((buffRecord.spinePoint != 0 && (Object)(object)bulletComponent == (Object)null) & flag)
		{
			bulletComponent = combatData.buffEffectPoolMgr.GetBuffEffect(buffRecord.id);
			bulletComponent.InitData(combatData, monsterUUID, buffRecord.id);
		}
		RefreshProperty();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			if (skillBase.isMainSkill)
			{
				if (!monsterById.isEliteBoss)
				{
					int count = skillBase.hero.team.mainSkillBuffExtraDamage.Count;
					for (int i = 0; i < count; i++)
					{
						stMainSkillBuffExtraDamage stMainSkillBuffExtraDamage2 = skillBase.hero.team.mainSkillBuffExtraDamage[i];
						if (stMainSkillBuffExtraDamage2.skillType == skillBaseHeroSkillType && stMainSkillBuffExtraDamage2.srcBuffId == buffRecord.id)
						{
							damage += Math.Min(stMainSkillBuffExtraDamage2.roll0to1 * (float)(long)monsterById.maxHp, (float)(int)skillBase.hero.team.teamAttack * stMainSkillBuffExtraDamage2.limitDamageRatio);
						}
					}
				}
				int count2 = skillBase.hero.team.mainSkillBuffExtraBuff.Count;
				for (int j = 0; j < count2; j++)
				{
					stMainSkillBuffExtraBuff stMainSkillBuffExtraBuff2 = skillBase.hero.team.mainSkillBuffExtraBuff[j];
					if (stMainSkillBuffExtraBuff2.skillType == skillBaseHeroSkillType && stMainSkillBuffExtraBuff2.srcBuffId == buffRecord.id)
					{
						zbCombatSystem.Instance.buffSystem.CreateBuff(skillBase, _skillAttrList, monsterUUID, stMainSkillBuffExtraBuff2.tarBuffId, stMainSkillBuffExtraBuff2.extraTime);
					}
				}
			}
			int count3 = monsterById.monsterIndex10081.Count;
			for (int k = 0; k < count3; k++)
			{
				stMonsterSkillIndex10081 stMonsterSkillIndex40463 = monsterById.monsterIndex10081[k];
				if (buffRecord.id == stMonsterSkillIndex40463.scrBuffId)
				{
					durationTime *= 1f + stMonsterSkillIndex40463.extraTime;
				}
			}
		}
		float num = skillBase.hero.team.ExtraProperties[ETeamExtra.GemMod1047];
		if (num > 0f)
		{
			durationTime *= 1f + num;
		}
	}

	public virtual void RefreshProperty()
	{
	}

	public virtual void AddLayer(int addlayer = 1)
	{
		if (skillBaseHeroSkillType == 4 && skillBase.hero.team.ExtraProperties[ETeamExtra.GemMod1060] > 0f)
		{
			layer += addlayer;
		}
		else
		{
			layer = Math.Min(layer + addlayer, buffRecord.layer);
		}
	}

	public virtual void Free()
	{
		if ((Object)(object)bulletComponent != (Object)null)
		{
			bulletComponent.Free();
		}
		bulletComponent = null;
	}

	public virtual bool IsEnd()
	{
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			return true;
		}
		if ((float)zbCombatSystem.Instance.accumTime >= startTm + durationTime)
		{
			return true;
		}
		return false;
	}

	public virtual void Update()
	{
	}

	public float GetAttrValueByType(EAttrSkillType attrSkillType)
	{
		if (skillAttrList.TryGetValue(attrSkillType, out var value))
		{
			return zbSkillSystem.Instance.SafeCheckAttrNeedDiv100(attrSkillType, value);
		}
		return 0f;
	}

	public void RefreshAttrList(Dictionary<EAttrSkillType, float> _skillAttrList)
	{
		float[][] baseVal = buffRecord.baseVal;
		foreach (float[] array in baseVal)
		{
			skillAttrList[(EAttrSkillType)array[0]] = array[1];
		}
		int[] fatherVal = buffRecord.fatherVal;
		for (int i = 0; i < fatherVal.Length; i++)
		{
			EAttrSkillType eAttrSkillType = (EAttrSkillType)fatherVal[i];
			float num = (_skillAttrList.TryGetValue(eAttrSkillType, out var value) ? value : 0f);
			if (eAttrSkillType == EAttrSkillType.Damage || eAttrSkillType == EAttrSkillType.IceDamage || eAttrSkillType == EAttrSkillType.BurnDamage)
			{
				num *= buffRecord.fatherValRate;
			}
			skillAttrList[eAttrSkillType] = num;
		}
		if (buffRecord.atkValRate > 0f)
		{
			float value2 = (float)(int)skillBase.hero.team.teamAttack * buffRecord.atkValRate;
			skillAttrList[EAttrSkillType.Damage] = value2;
			skillAttrList[EAttrSkillType.IceDamage] = value2;
			skillAttrList[EAttrSkillType.BurnDamage] = value2;
		}
	}

	public virtual void TriggerDieAction()
	{
	}
}
