using System;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMonsterSkillBase
{
	public zbCombatData combatData;

	public zbMonster monster;

	public zbMonsterSkillCsvRecord monsterSkillRecord;

	public zbMonsterSkillIndexCsvRecord monsterSkillIndexRecord;

	private static int sAutoId;

	private int m_UUID = -1;

	protected int loopStart;

	protected int loopNumber = 1;

	protected Action m_callback;

	public List<zbMonster> monsterRangeAllList = new List<zbMonster>();

	protected static readonly List<Vector3> vector3s;

	private zbMonsterEffect bulletComponent { get; set; }

	public virtual void Init(zbMonster _monster, int _monsterSkillId)
	{
		m_UUID = ++sAutoId;
		monster = _monster;
		monsterSkillRecord = zbDataConfig.Instance.monsterSkillCsvTable.GetRecordByKey(_monsterSkillId);
		monsterSkillIndexRecord = zbDataConfig.Instance.monsterSkillIndexCsvTable.GetRecordByKey(monsterSkillRecord.skillIndex);
		combatData = monster.combatData;
		RefreshProperty();
		if (monsterSkillIndexRecord != null && monsterSkillIndexRecord.spineShow == 1)
		{
			CreateEffectSpine();
		}
	}

	public virtual void RefreshProperty()
	{
	}

	public int GetUUID()
	{
		return m_UUID;
	}

	public virtual zbMonsterEffect CreateEffectSpine()
	{
		FreeEffectSpine();
		if (monsterSkillRecord != null && monsterSkillRecord.effectId > 0)
		{
			zbMonsterEffectCsvRecord recordByKey = zbDataConfig.Instance.monsterEffectCsvTable.GetRecordByKey(monsterSkillRecord.effectId);
			if (recordByKey != null)
			{
				bulletComponent = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(monsterSkillRecord.effectId);
				bulletComponent.InitData(combatData, monster.GetUUID(), monsterSkillRecord.effectId, GetDiameter());
				if ((Object)(object)bulletComponent != (Object)null)
				{
					bulletComponent.PlayEffectAnim(recordByKey.sequenceName, loop: true);
				}
			}
		}
		return bulletComponent;
	}

	public virtual void FreeEffectSpine()
	{
		if ((Object)(object)bulletComponent != (Object)null)
		{
			bulletComponent.Free();
		}
		bulletComponent = null;
	}

	public virtual float GetDiameter()
	{
		return 1f;
	}

	public TrackEntry PlayTackAnimation(int tackIndex, bool flag)
	{
		if (monsterSkillRecord.animationName != "")
		{
			return monster.anim.spineAnimation.PlayTackAnimation(tackIndex, monsterSkillRecord.animationName, flag);
		}
		return null;
	}

	public virtual void Save(stMonsterBoss monsterBoss)
	{
	}

	public virtual void RestoreData(stMonsterBoss monsterBoss)
	{
	}

	public virtual void TriggerDieSkill(Action callback)
	{
	}

	public virtual void CallBackLostHp(long value)
	{
	}

	public virtual void Update()
	{
	}

	public virtual void ClearList()
	{
		FreeEffectSpine();
	}

	public virtual bool IsBuffStop()
	{
		if (monsterSkillIndexRecord.stopTrigger > 0 && monster.CalBuffStopBool())
		{
			return true;
		}
		return false;
	}

	static zbMonsterSkillBase()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		sAutoId = 0;
		vector3s = new List<Vector3>
		{
			new Vector3(1f, 0f),
			new Vector3(-1f, 0f),
			new Vector3(0f, 1f),
			new Vector3(0f, -1f),
			new Vector3(1f, -1f),
			new Vector3(-1f, 1f),
			new Vector3(1f, 1f),
			new Vector3(-1f, -1f)
		};
	}
}
