using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbCombatDefense : zbAttackerBase
{
	public bool beAttacked;

	[NonSerialized]
	public EBattleResultScore lastResultScore = EBattleResultScore.Win3;

	public zbSafeLong curHp = new zbSafeLong(0L, "zbCombatDefense.curHp");

	public zbSafeLong maxHp = new zbSafeLong(0L, "zbCombatSystem.maxHp");

	public zbSafeLong shieldHp = new zbSafeLong(0L, "zbCombatDefense.shieldHp");

	public zbSafeLong maxShieldHp = new zbSafeLong(0L, "zbCombatDefense.maxShieldHp");

	[NonSerialized]
	public zbSafeInt damageDecr = new zbSafeInt(0, "zbCombatDefense.damageDecr");

	public zbSafeInt damageImmunity = new zbSafeInt(0, "zbCombatDefense.damageImmunity");

	[NonSerialized]
	public int energyRegen;

	[NonSerialized]
	public float failureRate;

	[NonSerialized]
	public int maxMileage;

	[NonSerialized]
	public int visionRange;

	[NonSerialized]
	public float immunityDuration;

	private zbWallAvatar wallAvatar;

	private zbSpineColorChange spineColorChange;

	private GameObject dummyAlert;

	private float dummyAlertY;

	private float dummyOffsetY;

	private BattleSideTeamInfo battleSideTeamInfo;

	private BattleDefenseDetail defenseInfo;

	public void InitData(zbCombatData _combatData, BattleSideTeamInfo _battleSideTeamInfo)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Wall;
		combatData = _combatData;
		battleSideTeamInfo = _battleSideTeamInfo;
		team = combatData.masterTeam;
		defenseInfo = battleSideTeamInfo.MasterTeamInfo.DefenseInfo;
		heroAttr = new zbHeroAttr();
		heroAttr.Init(this);
		heroAttr.RefreshWallAttr();
		wallAvatar = ((Component)((Component)this).transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		if (defenseInfo.DefenseType != EDefenseType.CarDefense)
		{
			((Component)wallAvatar).gameObject.SetActive(true);
			RefreshAvatar();
		}
		else
		{
			((Component)wallAvatar).gameObject.SetActive(false);
		}
		spineColorChange = new zbSpineColorChange();
		if (defenseInfo.DefenseType == EDefenseType.WallDefense)
		{
			spineColorChange.Init(((Component)wallAvatar).GetComponentInChildren<SkeletonRenderer>(), _useSlot: true);
		}
		else if (defenseInfo.DefenseType != EDefenseType.CarDefense)
		{
			spineColorChange.Init(((Component)wallAvatar).GetComponentInChildren<SkeletonRenderer>());
		}
		dummyAlert = ((Component)((Component)this).transform.Find("GameObjectAlert")).gameObject;
		dummyAlertY = dummyAlert.transform.position.y;
		CaclProperty();
		SetCurHp((int)((float)(long)maxHp * defenseInfo.DefensePct / 100f));
		SetShieldHp(maxShieldHp);
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void Restore(zbCombatRecorder restoreData)
	{
		SetCurHp(restoreData.loadedData.recordDefense.curHp);
		SetShieldHp(restoreData.loadedData.recordDefense.shieldHp);
		SetDamageImmunity(restoreData.loadedData.recordDefense.damageImmunity);
	}

	public void RefreshAvatar()
	{
		string spinePath = "animation/scene/scene_wall/wall_self1/wall_self1";
		dummyOffsetY = -0.8f;
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			spinePath = ((combatData.dataIndex != 1) ? "animation/scene/scene_wall/wall_enemy1/wall_enemy1" : "animation/scene/scene_wall/wall_self1/wall_self1");
			dummyOffsetY = -0.8f;
		}
		else if (defenseInfo.DefenseType == EDefenseType.WallDefense)
		{
			int wallId = defenseInfo.WallId;
			if (battleSideTeamInfo.HelperTeamInfo.SkillList.Count > 0)
			{
				wallId = battleSideTeamInfo.HelperTeamInfo.DefenseInfo.WallId;
			}
			zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
			if (recordByKey == null)
			{
				recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(1);
			}
			if (recordByKey != null)
			{
				dummyOffsetY = (float)recordByKey.offset / 100f;
			}
			spinePath = recordByKey.animation;
		}
		else if (defenseInfo.DefenseType == EDefenseType.CarDefense)
		{
			dummyOffsetY = -0.8f;
		}
		wallAvatar.RefreshSpine(spinePath, "1", loop: true);
		wallAvatar.SetSuperWallInAnimation(isFirst: true);
	}

	private void CaclProperty()
	{
		foreach (BattleDefenseAttrDetail attr in defenseInfo.AttrList)
		{
			float num = attr.Value;
			foreach (stAttrTeamTypeData attrTeamTypeAttr in team.teamAttr.attrTeamTypeAttrList)
			{
				if (team.teamAttr.AttrTeamTypeToDefenseType.TryGetValue(attrTeamTypeAttr.eAttrTeamType, out var value) && attr.Type == value)
				{
					num = ((attrTeamTypeAttr.eAttrAddType != EAttrAddType.Percent) ? (num + attrTeamTypeAttr.eValue) : (num * (1f + attrTeamTypeAttr.eValue)));
				}
			}
			switch (attr.Type)
			{
			case EAttrDefenseType.DefenseHp:
				maxHp.Value = (int)num;
				break;
			case EAttrDefenseType.ShieldHp:
				maxShieldHp.Value = (int)num;
				break;
			case EAttrDefenseType.DamageDecr:
				damageDecr.Value = (int)num;
				break;
			case EAttrDefenseType.DamageImmunity:
				SetDamageImmunity((int)num);
				break;
			case EAttrDefenseType.EnergyRegen:
				energyRegen = (int)num;
				break;
			case EAttrDefenseType.FailureRate:
				failureRate = num;
				break;
			case EAttrDefenseType.MaxMileage:
				maxMileage = (int)num;
				break;
			case EAttrDefenseType.VisionRange:
				visionRange = (int)num;
				break;
			}
		}
	}

	protected override void Update()
	{
		if (defenseInfo.DefenseType != EDefenseType.CarDefense)
		{
			spineColorChange.DeltaUpdate();
		}
		if (immunityDuration > 0f)
		{
			immunityDuration -= Time.deltaTime;
		}
		else
		{
			immunityDuration = 0f;
		}
	}

	private ECombatBubble GetHpBubbleType(long value)
	{
		if (value > 0)
		{
			return ECombatBubble.WallAddHp;
		}
		return ECombatBubble.WallNormalDecHp;
	}

	public long ChangeHp(float positionX, long value)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.fightFinish)
		{
			return 0L;
		}
		if (zbCombatSystem.Instance.m_isCelebrating && value < 0)
		{
			return 0L;
		}
		Vector3 bubblePosition = Vector2.op_Implicit(combatData.defenseHp.GetProgressPosition());
		ECombatBubble hpBubbleType = GetHpBubbleType(value);
		combatData.combatBubble.ShowBubble(bubblePosition, hpBubbleType, value);
		value = UseShield(value);
		SetCurHp((long)curHp + value);
		if (value < 0)
		{
			beAttacked = true;
			combatData.TriggerAction5061();
			if (defenseInfo.DefenseType != EDefenseType.CarDefense)
			{
				spineColorChange.LerpChangeColor();
			}
			else
			{
				float num = 999f;
				zbHero zbHero2 = null;
				foreach (zbHero hero in combatData.masterTeam.heroList)
				{
					float x = ((Component)hero).transform.position.x;
					float num2 = Math.Abs(positionX - x);
					if (num > num2)
					{
						num = num2;
						zbHero2 = hero;
					}
				}
				if (combatData.helperTeam != null)
				{
					foreach (zbHero hero2 in combatData.helperTeam.heroList)
					{
						float x2 = ((Component)hero2).transform.position.x;
						float num3 = Math.Abs(positionX - x2);
						if (num > num3)
						{
							num = num3;
							zbHero2 = hero2;
						}
					}
				}
				if ((Object)(object)zbHero2 != (Object)null)
				{
					zbHero2.LerpChangeColor();
				}
			}
			List<stModIndex1049> modIndex = combatData.masterTeam.modIndex1049;
			for (int num4 = modIndex.Count - 1; num4 >= 0; num4--)
			{
				stModIndex1049 stModIndex2146 = modIndex[num4];
				stModIndex2146.lastLostHp -= value;
				zbHero heroBySkillType = combatData.masterTeam.GetHeroBySkillType((ESkillType)stModIndex2146.skillType);
				if ((Object)(object)heroBySkillType != (Object)null)
				{
					int num5 = (int)(stModIndex2146.preHpPercent * (float)(long)maxHp);
					while (stModIndex2146.lastLostHp >= num5)
					{
						for (int i = 0; i < stModIndex2146.subCount; i++)
						{
							zbMonster randomMonster = combatData.monsters.GetRandomMonster();
							if ((Object)(object)randomMonster != (Object)null)
							{
								zbSkillSystem.Instance.CreateSkill(heroBySkillType, ((Component)randomMonster).transform.position, Vector2.up, randomMonster.GetUUID());
							}
						}
						stModIndex2146.lastLostHp -= num5;
					}
				}
			}
			if (combatData.helperTeam != null)
			{
				modIndex = combatData.helperTeam.modIndex1049;
				for (int num6 = modIndex.Count - 1; num6 >= 0; num6--)
				{
					stModIndex1049 stModIndex2147 = modIndex[num6];
					stModIndex2147.lastLostHp -= value;
					zbHero heroBySkillType2 = combatData.helperTeam.GetHeroBySkillType((ESkillType)stModIndex2147.skillType);
					if ((Object)(object)heroBySkillType2 != (Object)null)
					{
						int num7 = (int)(stModIndex2147.preHpPercent * (float)(long)maxHp);
						while (stModIndex2147.lastLostHp >= num7)
						{
							for (int j = 0; j < stModIndex2147.subCount; j++)
							{
								zbMonster randomMonster2 = combatData.monsters.GetRandomMonster();
								if ((Object)(object)randomMonster2 != (Object)null)
								{
									zbSkillSystem.Instance.CreateSkill(heroBySkillType2, ((Component)randomMonster2).transform.position, Vector2.up, randomMonster2.GetUUID());
								}
							}
							stModIndex2147.lastLostHp -= num7;
						}
					}
				}
			}
		}
		return value;
	}

	public long UseShield(long preDamage)
	{
		if ((long)shieldHp <= 0)
		{
			return preDamage;
		}
		_ = (long)shieldHp;
		if (preDamage >= 0)
		{
			return preDamage;
		}
		long num = -preDamage;
		if ((long)shieldHp > num)
		{
			SetShieldHp((long)shieldHp - num);
			return 0L;
		}
		long num2 = num - (long)shieldHp;
		SetShieldHp(0L);
		combatData.TriggerAction5021();
		return -num2;
	}

	public void AddShieldHp(int value)
	{
		if (value > 0)
		{
			if (shieldHp.Value + value > (long)maxShieldHp)
			{
				maxShieldHp.Value = shieldHp.Value + value;
			}
			SetShieldHp(shieldHp.Value + value);
		}
	}

	public void SetShieldHp(long value)
	{
		shieldHp.Value = Math.Clamp(value, 0L, maxShieldHp);
	}

	public void SetCurHp(long value)
	{
		curHp.Value = Math.Clamp(value, 0L, maxHp);
		EBattleResultScore eBattleResultScore = CalcResultScore();
		if (eBattleResultScore != lastResultScore)
		{
			wallAvatar.SetAnimation(0, ((int)(4 - eBattleResultScore)).ToString(), loop: true);
			lastResultScore = eBattleResultScore;
		}
		List<stModIndex1048> modIndex = combatData.masterTeam.modIndex1048;
		for (int num = modIndex.Count - 1; num >= 0; num--)
		{
			stModIndex1048 stModIndex2146 = modIndex[num];
			if (stModIndex2146.firstBelowHp >= (float)(long)curHp / (float)(long)maxHp)
			{
				immunityDuration += stModIndex2146.immunityDuration;
				modIndex.RemoveAt(num);
			}
		}
	}

	public void SetDamageImmunity(int value)
	{
		damageImmunity.Value = value;
	}

	public EBattleResultScore CalcResultScore()
	{
		float num = (float)(long)curHp / (float)(long)maxHp;
		if (num >= 1f)
		{
			return EBattleResultScore.Win3;
		}
		if (num >= 0.5f)
		{
			return EBattleResultScore.Win2;
		}
		if (num > 0f)
		{
			return EBattleResultScore.Win1;
		}
		return EBattleResultScore.Fail;
	}

	public float GetAlertPositionY()
	{
		return dummyAlertY;
	}

	public float GetAlertOffsetY()
	{
		return dummyOffsetY;
	}

	public override int GetSkillType()
	{
		return 201;
	}
}
