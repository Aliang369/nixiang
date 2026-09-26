using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbMonsterBuffBase
{
	protected zbCombatData combatData;

	public zbMonsterBuffCsvRecord buffRecord;

	public int monsterUUID;

	protected float extraTime;

	private int layer;

	protected float damage;

	private zbBuffEffect bulletComponent;

	private float startTm;

	public float durationTime;

	public Dictionary<EAttrSkillType, float> skillAttrList = new Dictionary<EAttrSkillType, float>();

	public virtual void Reset()
	{
		combatData = null;
		buffRecord = null;
		monsterUUID = 0;
		extraTime = 0f;
		layer = 0;
		damage = 0f;
		startTm = zbCombatSystem.Instance.accumTime;
		durationTime = 0f;
		skillAttrList.Clear();
	}

	public virtual void Init(zbMonster pMonster, int _monsterUUID, zbMonsterBuffCsvRecord _buffRecord, float _extraTime = 0f)
	{
		combatData = pMonster.combatData;
		monsterUUID = _monsterUUID;
		buffRecord = _buffRecord;
		extraTime = _extraTime;
		layer = 0;
		damage = 0f;
		startTm = zbCombatSystem.Instance.accumTime;
		durationTime = 0f;
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
	}

	public virtual void RefreshProperty()
	{
	}

	public virtual void AddLayer(int addlayer = 1)
	{
		layer = Math.Min(layer + addlayer, buffRecord.layer);
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

	public virtual void TriggerDieAction()
	{
	}
}
