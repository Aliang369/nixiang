using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbCombatHandler
{
	private static float modAttrvalueTotal = 0f;

	private static float monsterSkillParamTotal = 0f;

	private static float skillSubParamTotal = 0f;

	private static float skillBuffParamTotal = 0f;

	private static float wallSkillParamTotal = 0f;

	private static float mechaSkillParamTotal = 0f;

	private static float foothSkillParamTotal = 0f;

	private static bool bCalled = false;

	private float battleInfoValueTotal;

	public float teamBaseAttrTotal;

	private long battleMonsterHpTotal;

	private readonly int battleMonsterHpTotal_i = 30;

	private string outString = "";

	private string m_configBubbleNumber = "combatBubbleNumber";

	private float m_maxBubbleNumber = 1f;

	public int roleFightPlayerId;

	private static readonly EBattleFuncType[] ElementTrialTypes = new EBattleFuncType[6]
	{
		EBattleFuncType.ElementTrialElec,
		EBattleFuncType.ElementTrialFire,
		EBattleFuncType.ElementTrialIce,
		EBattleFuncType.ElementTrialPhy,
		EBattleFuncType.ElementTrialEnergy,
		EBattleFuncType.ElementTrialWind
	};

	private static readonly EBattleFuncType[] WorldLairTypes = new EBattleFuncType[2]
	{
		EBattleFuncType.WorldLair,
		EBattleFuncType.WorldLairHelp
	};

	private static readonly EBattleFuncType[] LegendaryTypes = new EBattleFuncType[1] { EBattleFuncType.Legendary };

	private static readonly EBattleFuncType[] EliteQuestTypes = new EBattleFuncType[1] { EBattleFuncType.EliteQuest };

	private Dictionary<EBattleFuncType, string> eBattleFuncTypeSaveKey = new Dictionary<EBattleFuncType, string>();

	private string saveWorldLairKey = "";

	private string saveElementTrialKey = "";

	private string saveLegendaryKey = "";

	private string saveEliteQuestKey = "";

	private string lastBattleUUID = "";

	public void Init()
	{
		if (!bCalled)
		{
			foreach (KeyValuePair<int, zbModCsvRecord> record in zbDataConfig.Instance.modCsvTable.GetRecords().records)
			{
				float[] valueList = record.Value.valueList;
				foreach (float num in valueList)
				{
					modAttrvalueTotal += num;
				}
			}
			foreach (KeyValuePair<int, zbMonsterSkillCsvRecord> record2 in zbDataConfig.Instance.monsterSkillCsvTable.GetRecords().records)
			{
				float[] valueList = record2.Value.param;
				foreach (float num2 in valueList)
				{
					monsterSkillParamTotal += num2;
				}
			}
			foreach (KeyValuePair<int, zbSkillSubCsvRecord> record3 in zbDataConfig.Instance.skillSubCsvTable.GetRecords().records)
			{
				skillSubParamTotal += record3.Value.atkValRate;
				skillSubParamTotal += record3.Value.fatherValRate;
				int[] fatherVal = record3.Value.fatherVal;
				foreach (int num3 in fatherVal)
				{
					skillSubParamTotal += num3;
				}
				float[][] baseVal = record3.Value.baseVal;
				foreach (float[] valueList in baseVal)
				{
					foreach (float num4 in valueList)
					{
						skillSubParamTotal += num4;
					}
				}
			}
			foreach (KeyValuePair<int, zbSkillBuffCsvRecord> record4 in zbDataConfig.Instance.skillBuffCsvTable.GetRecords().records)
			{
				skillBuffParamTotal += record4.Value.atkValRate;
				skillBuffParamTotal += record4.Value.fatherValRate;
				skillBuffParamTotal += record4.Value.param;
				int[] fatherVal = record4.Value.fatherVal;
				foreach (int num5 in fatherVal)
				{
					skillBuffParamTotal += num5;
				}
				float[][] baseVal = record4.Value.baseVal;
				foreach (float[] valueList in baseVal)
				{
					foreach (float num6 in valueList)
					{
						skillBuffParamTotal += num6;
					}
				}
			}
			foreach (KeyValuePair<int, zbWallSkillCsvRecord> record5 in zbDataConfig.Instance.wallSkillCsvTable.GetRecords().records)
			{
				float[] valueList = record5.Value.param;
				foreach (float num7 in valueList)
				{
					wallSkillParamTotal += num7;
				}
			}
			foreach (KeyValuePair<int, zbMechaSkillCsvRecord> record6 in zbDataConfig.Instance.mechaSkillCsvTable.GetRecords().records)
			{
				float[] valueList = record6.Value.param;
				foreach (float num8 in valueList)
				{
					mechaSkillParamTotal += num8;
				}
			}
			foreach (KeyValuePair<int, zbFootholdEffectCsvRecord> record7 in zbDataConfig.Instance.footholdEffectCsvTable.GetRecords().records)
			{
				float[] valueList = record7.Value.param;
				foreach (float num9 in valueList)
				{
					foothSkillParamTotal += num9;
				}
			}
		}
		bCalled = true;
		m_maxBubbleNumber = zbGameSave.Instance.GetFloat(m_configBubbleNumber, 1f);
		InitSaveKey();
	}

	public float GetMaxBubbleNumber()
	{
		return m_maxBubbleNumber;
	}

	public zbCombatData GetPvpCombatData(zbCombatData _combatData)
	{
		if (_combatData.dataIndex == 1)
		{
			return zbCombatSystem.Instance.combatData2;
		}
		return zbCombatSystem.Instance.combatData;
	}

	public void SetMaxBubbleNumber(float volume)
	{
		m_maxBubbleNumber = Mathf.Clamp(volume, 0f, 1f);
		zbGameSave.Instance.SetFloat(m_configBubbleNumber, volume);
	}

	public BattleServantDetail GetServantDetail(BattleTeamInfo _battleTeamInfo, int SkillType)
	{
		if (_battleTeamInfo.ServantList == null)
		{
			return null;
		}
		foreach (BattleServantDetail servant in _battleTeamInfo.ServantList)
		{
			if (servant.SkillType == SkillType && servant.ServantTid > 0)
			{
				return servant;
			}
		}
		return null;
	}

	public void StartShowCombat(BattleInfo battleInfo, int paramId = 0, zbCombatRecorder restoreData = null, BattleResultInfo dataResult = null)
	{
		if (battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp)
		{
			zbUIMgr.Instance.ShowCombatVSWnd(battleInfo, paramId, restoreData, dataResult, battleInfo.DefenderTeamInfo.MasterTeamInfo.RoleBaseInfo);
			return;
		}
		zbUIMgr.Instance.ShowCombat(battleInfo, paramId, restoreData, dataResult);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_CommFight_Ack);
	}

	public void RecordBattleInfo(BattleInfo _battleInfo)
	{
		battleInfoValueTotal = 0f;
		battleMonsterHpTotal = 0L;
		foreach (BattleSkillDetail skill in _battleInfo.AttackerTeamInfo.MasterTeamInfo.SkillList)
		{
			foreach (BattleSkillAttrDetail attr in skill.AttrList)
			{
				battleInfoValueTotal += attr.Value;
			}
		}
		foreach (BattleTeamAttrDetail attr2 in _battleInfo.AttackerTeamInfo.MasterTeamInfo.AttrList)
		{
			battleInfoValueTotal += attr2.Value;
		}
		foreach (BattleMonsterDetail monster in _battleInfo.MonsterInfo.MonsterList)
		{
			foreach (BattleMonsterAttrDetail attr3 in monster.AttrList)
			{
				battleInfoValueTotal += attr3.Value;
				if (attr3.Type == EAttrMonsterType.Hp)
				{
					battleMonsterHpTotal += (long)attr3.Value * battleMonsterHpTotal_i;
				}
			}
		}
	}

	public void RecordTeamBaseAttr(List<stTeamAttrData> teamBaseAttrList)
	{
		foreach (stTeamAttrData teamBaseAttr in teamBaseAttrList)
		{
			teamBaseAttrTotal += teamBaseAttr.eValue;
		}
	}

	public zbItem CreatezbItemByAwardPreview(zbItemData itemData)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		zbItem obj = new zbItem();
		VisualElement spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)obj, "frame", (string)null);
		VisualElement spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)obj, "icon", (string)null);
		Label val = UQueryExtensions.Q<Label>((VisualElement)(object)obj, "lbName", (string)null);
		Label lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)obj, "lbCount", (string)null);
		VisualElement obj2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)obj, "frag", (string)null);
		zbResourceHelper.RefreshResourceItem(itemData.type, itemData.id, itemData.count, spriteFrame, spriteIcon, val, lbCount);
		obj2.visible = itemData.type == EResourceType.HeroFrag;
		((VisualElement)val).visible = false;
		((VisualElement)obj).style.scale = StyleScale.op_Implicit(new Vector2(0.8f, 0.8f));
		return obj;
	}

	public void InitSaveKey()
	{
		saveElementTrialKey = "autoElementTrialKey" + zbSelf.Instance.GetRoleId();
		saveWorldLairKey = "autoWorldLairKey" + zbSelf.Instance.GetRoleId();
		saveLegendaryKey = "autoLegendaryKey" + zbSelf.Instance.GetRoleId();
		saveEliteQuestKey = "autoEliteQuestKey" + zbSelf.Instance.GetRoleId();
		eBattleFuncTypeSaveKey.Clear();
		EBattleFuncType[] elementTrialTypes = ElementTrialTypes;
		foreach (EBattleFuncType key in elementTrialTypes)
		{
			eBattleFuncTypeSaveKey[key] = saveElementTrialKey;
		}
		elementTrialTypes = WorldLairTypes;
		foreach (EBattleFuncType key2 in elementTrialTypes)
		{
			eBattleFuncTypeSaveKey[key2] = saveWorldLairKey;
		}
		elementTrialTypes = LegendaryTypes;
		foreach (EBattleFuncType key3 in elementTrialTypes)
		{
			eBattleFuncTypeSaveKey[key3] = saveLegendaryKey;
		}
		elementTrialTypes = EliteQuestTypes;
		foreach (EBattleFuncType key4 in elementTrialTypes)
		{
			eBattleFuncTypeSaveKey[key4] = saveEliteQuestKey;
		}
	}

	public bool IsCanAutoSelectMod(EBattleFuncType eBattleFuncType)
	{
		return eBattleFuncTypeSaveKey.ContainsKey(eBattleFuncType);
	}

	public bool IsAutoSelectMod(EBattleFuncType eBattleFuncType)
	{
		eBattleFuncTypeSaveKey.TryGetValue(eBattleFuncType, out var value);
		if (value == "")
		{
			return false;
		}
		if (zbGameSave.Instance.GetBool(value))
		{
			return zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) > 0;
		}
		return false;
	}

	public void SetAutoSelectMod(EBattleFuncType eBattleFuncType)
	{
		eBattleFuncTypeSaveKey.TryGetValue(eBattleFuncType, out var value);
		if (!(value == ""))
		{
			if (zbSelf.Instance.combatHandler.IsAutoSelectMod(eBattleFuncType))
			{
				zbGameSave.Instance.SetBool(value, value: false);
			}
			else
			{
				zbGameSave.Instance.SetBool(value, value: true);
			}
		}
	}

	public void SetAutoOKDisplay(EBattleFuncType eBattleFuncType, VisualElement m_spriteOk)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.combatHandler.IsAutoSelectMod(eBattleFuncType);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public bool IsZuobi()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		EBattleFuncType battleType = zbCombatSystem.Instance.GetBattleType();
		outString = "UUID:" + zbCombatSystem.Instance.battleInfo.BattleBaseInfo.UUID + ", battleFuncType=" + battleType;
		foreach (KeyValuePair<int, zbModCsvRecord> record in zbDataConfig.Instance.modCsvTable.GetRecords().records)
		{
			float[] valueList = record.Value.valueList;
			foreach (float num10 in valueList)
			{
				num += num10;
			}
		}
		foreach (KeyValuePair<int, zbMonsterSkillCsvRecord> record2 in zbDataConfig.Instance.monsterSkillCsvTable.GetRecords().records)
		{
			float[] valueList = record2.Value.param;
			foreach (float num11 in valueList)
			{
				num2 += num11;
			}
		}
		foreach (KeyValuePair<int, zbSkillSubCsvRecord> record3 in zbDataConfig.Instance.skillSubCsvTable.GetRecords().records)
		{
			num3 += record3.Value.atkValRate;
			num3 += record3.Value.fatherValRate;
			int[] fatherVal = record3.Value.fatherVal;
			foreach (int num12 in fatherVal)
			{
				num3 += (float)num12;
			}
			float[][] baseVal = record3.Value.baseVal;
			foreach (float[] valueList in baseVal)
			{
				foreach (float num13 in valueList)
				{
					num3 += num13;
				}
			}
		}
		foreach (KeyValuePair<int, zbSkillBuffCsvRecord> record4 in zbDataConfig.Instance.skillBuffCsvTable.GetRecords().records)
		{
			num4 += record4.Value.atkValRate;
			num4 += record4.Value.fatherValRate;
			num4 += record4.Value.param;
			int[] fatherVal = record4.Value.fatherVal;
			foreach (int num14 in fatherVal)
			{
				num4 += (float)num14;
			}
			float[][] baseVal = record4.Value.baseVal;
			foreach (float[] valueList in baseVal)
			{
				foreach (float num15 in valueList)
				{
					num4 += num15;
				}
			}
		}
		foreach (KeyValuePair<int, zbWallSkillCsvRecord> record5 in zbDataConfig.Instance.wallSkillCsvTable.GetRecords().records)
		{
			float[] valueList = record5.Value.param;
			foreach (float num16 in valueList)
			{
				num5 += num16;
			}
		}
		foreach (KeyValuePair<int, zbMechaSkillCsvRecord> record6 in zbDataConfig.Instance.mechaSkillCsvTable.GetRecords().records)
		{
			float[] valueList = record6.Value.param;
			foreach (float num17 in valueList)
			{
				num6 += num17;
			}
		}
		foreach (KeyValuePair<int, zbFootholdEffectCsvRecord> record7 in zbDataConfig.Instance.footholdEffectCsvTable.GetRecords().records)
		{
			float[] valueList = record7.Value.param;
			foreach (float num18 in valueList)
			{
				num7 += num18;
			}
		}
		foreach (BattleSkillDetail skill in zbCombatSystem.Instance.battleInfo.AttackerTeamInfo.MasterTeamInfo.SkillList)
		{
			foreach (BattleSkillAttrDetail attr in skill.AttrList)
			{
				num8 += attr.Value;
			}
		}
		foreach (BattleTeamAttrDetail attr2 in zbCombatSystem.Instance.battleInfo.AttackerTeamInfo.MasterTeamInfo.AttrList)
		{
			num8 += attr2.Value;
		}
		long num19 = 0L;
		foreach (BattleMonsterDetail monster in zbCombatSystem.Instance.battleInfo.MonsterInfo.MonsterList)
		{
			foreach (BattleMonsterAttrDetail attr3 in monster.AttrList)
			{
				num8 += attr3.Value;
				if (attr3.Type == EAttrMonsterType.Hp)
				{
					num19 += (long)attr3.Value;
				}
			}
		}
		if (Math.Abs(battleMonsterHpTotal / battleMonsterHpTotal_i - num19) > 10)
		{
			outString = outString + "monsterHp:" + battleMonsterHpTotal / battleMonsterHpTotal_i + ":" + num19;
			Abnormal_ClientReportOnce(EFuncType.MainQuest, battleMonsterHpTotal / battleMonsterHpTotal_i, num19, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		foreach (stTeamAttrData teamBaseAttr in zbCombatSystem.Instance.combatData.masterTeam.teamAttr.teamBaseAttrList)
		{
			num9 += teamBaseAttr.eValue;
		}
		if (zbCombatSystem.Instance.combatData.helperTeam != null)
		{
			foreach (stTeamAttrData teamBaseAttr2 in zbCombatSystem.Instance.combatData.helperTeam.teamAttr.teamBaseAttrList)
			{
				num9 += teamBaseAttr2.eValue;
			}
		}
		if (zbCombatSystem.Instance.combatData2 != null)
		{
			foreach (stTeamAttrData teamBaseAttr3 in zbCombatSystem.Instance.combatData2.masterTeam.teamAttr.teamBaseAttrList)
			{
				num9 += teamBaseAttr3.eValue;
			}
			if (zbCombatSystem.Instance.combatData2.helperTeam != null)
			{
				foreach (stTeamAttrData teamBaseAttr4 in zbCombatSystem.Instance.combatData2.helperTeam.teamAttr.teamBaseAttrList)
				{
					num9 += teamBaseAttr4.eValue;
				}
			}
		}
		float num20 = 0f;
		foreach (zbHero hero in zbCombatSystem.Instance.combatData.masterTeam.heroList)
		{
			foreach (KeyValuePair<EAttrSkillType, float> skillAttr in hero.heroAttr.skillAttrList)
			{
				num20 += skillAttr.Value;
			}
		}
		if (battleType == EBattleFuncType.MainQuest)
		{
			int num21 = 0;
			foreach (zbModBase mod in zbCombatSystem.Instance.combatData.masterTeam.modMgr.modList)
			{
				if (mod.modRecord.skillLvUp == 1 && mod.isUserSelect)
				{
					num21++;
				}
			}
			if (num21 > zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel + 5)
			{
				outString = outString + "选择MOD 超过上限:" + zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel + "战斗等级：" + zbCombatSystem.Instance.combatData.combatExp.curLevel.ToString();
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num21, num21, zbSdkHelper.GetResVersion(), outString);
				return true;
			}
		}
		if (Math.Abs(zbCombatSystem.Instance.combatData.masterTeam.teamAttr.attrSum - num20) > 10f)
		{
			outString = outString + "heroAttr:战斗等级：" + zbCombatSystem.Instance.combatData.combatExp.curLevel.ToString();
			Abnormal_ClientReportOnce(EFuncType.MainQuest, zbCombatSystem.Instance.combatData.masterTeam.teamAttr.attrSum, num20, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		if (CheckTeamAttackAllTeams(out var snapValue, out var liveValue))
		{
			outString = outString + "teamAttack:" + snapValue + ":" + liveValue;
			Abnormal_ClientReportOnce(EFuncType.MainQuest, snapValue, liveValue, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		if (CheckHeroAttrAllTeams(out var attrType, out var expected, out var live))
		{
			outString = outString + "attrItem:" + attrType.ToString() + ":" + expected + ":" + live;
			Abnormal_ClientReportOnce(EFuncType.MainQuest, expected, live, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		if (CheckTeamAttrTablesAllTeams(out var tableMask, out var snapHash, out var liveHash))
		{
			outString = outString + "teamAttrTable:" + tableMask + ":" + snapHash + ":" + liveHash;
			Abnormal_ClientReportOnce(EFuncType.MainQuest, snapHash, liveHash, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		if ((float)(zbSelf.Instance.timeHandler.GetServerTime() - (long)zbCombatSystem.Instance.startServerTime) < (float)zbCombatSystem.Instance.accumTime / zbCombatSpeed.speed3 - 10f)
		{
			outString = outString + "时间加速:" + Time.timeScale;
			Abnormal_ClientReportOnce(EFuncType.MainQuest, (float)zbCombatSystem.Instance.accumTime / zbCombatSpeed.speed3 - 10f, zbSelf.Instance.timeHandler.GetServerTime() - (long)zbCombatSystem.Instance.startServerTime, zbSdkHelper.GetResVersion(), outString);
			return true;
		}
		if (Math.Abs(num - modAttrvalueTotal) > 10f || Math.Abs(num2 - monsterSkillParamTotal) > 10f || Math.Abs(num8 - battleInfoValueTotal) > 10f || Math.Abs(num9 - teamBaseAttrTotal) > 10f)
		{
			if (Math.Abs(num - modAttrvalueTotal) > 10f)
			{
				outString = outString + "t_mod:" + num + ":" + modAttrvalueTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num, modAttrvalueTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_modAttrvalueTotal:" + num + ", modAttrvalueTotal:" + modAttrvalueTotal;
			}
			if (Math.Abs(num2 - monsterSkillParamTotal) > 10f)
			{
				outString = outString + "t_monster:" + num2 + ":" + monsterSkillParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num2, monsterSkillParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_monsterSkillParamTotal:" + num2 + ", monsterSkillParamTotal:" + monsterSkillParamTotal;
			}
			if (Math.Abs(num8 - battleInfoValueTotal) > 10f)
			{
				outString = outString + "t_battle:" + num8 + ":" + battleInfoValueTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num8, battleInfoValueTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_battleInfoValueTotal:" + num8 + ", battleInfoValueTotal:" + battleInfoValueTotal;
			}
			if (Math.Abs(num9 - teamBaseAttrTotal) > 10f)
			{
				outString = outString + "t_teamBase:" + num9 + ":" + teamBaseAttrTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num9, teamBaseAttrTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_teamBaseAttrTotal:" + num9 + ", teamBaseAttrTotal:" + teamBaseAttrTotal;
			}
			return true;
		}
		if (Math.Abs(num3 - skillSubParamTotal) > 10f || Math.Abs(num4 - skillBuffParamTotal) > 10f || Math.Abs(num5 - wallSkillParamTotal) > 10f || Math.Abs(num6 - mechaSkillParamTotal) > 10f || Math.Abs(num7 - foothSkillParamTotal) > 10f)
		{
			if (Math.Abs(num3 - skillSubParamTotal) > 10f)
			{
				outString = outString + "t_skillSub:" + num3 + ":" + skillSubParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num3, skillSubParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_skillSubParamTotal:" + num3 + ", skillSubParamTotal:";
			}
			if (Math.Abs(num4 - skillBuffParamTotal) > 10f)
			{
				outString = outString + "t_skillBuff:" + num4 + ":" + skillBuffParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num4, skillBuffParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_skillBuffParamTotal:" + num4 + ",skillBuffParamTotal:" + skillBuffParamTotal;
			}
			if (Math.Abs(num5 - wallSkillParamTotal) > 10f)
			{
				outString = outString + "t_skillBuff:" + num5 + ":" + wallSkillParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num5, wallSkillParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_wallSkillParamTotal:" + num5 + ",wallSkillParamTotal:" + wallSkillParamTotal;
			}
			if (Math.Abs(num6 - mechaSkillParamTotal) > 10f)
			{
				outString = outString + "t_skillBuff:" + num6 + ":" + mechaSkillParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num6, mechaSkillParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_mechaSkillParamTotal:" + num6 + ",mechaSkillParamTotal:" + mechaSkillParamTotal;
			}
			if (Math.Abs(num7 - foothSkillParamTotal) > 10f)
			{
				outString = outString + "t_skillBuff:" + num7 + ":" + foothSkillParamTotal;
				Abnormal_ClientReportOnce(EFuncType.MainQuest, num7, foothSkillParamTotal, zbSdkHelper.GetResVersion(), outString);
				_ = "t_foothSkillParamTotal:" + num7 + ",foothSkillParamTotal:" + foothSkillParamTotal;
			}
			return true;
		}
		return false;
	}

	private static bool CheckTeamAttackAllTeams(out int snapValue, out int liveValue)
	{
		snapValue = 0;
		liveValue = 0;
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (instance == null || instance.combatData == null)
		{
			return false;
		}
		if (instance.combatData.masterTeam != null && instance.combatData.masterTeam.CheckTeamAttack(out snapValue, out liveValue))
		{
			return true;
		}
		if (instance.combatData.helperTeam != null && instance.combatData.helperTeam.CheckTeamAttack(out snapValue, out liveValue))
		{
			return true;
		}
		if (instance.combatData2 != null)
		{
			if (instance.combatData2.masterTeam != null && instance.combatData2.masterTeam.CheckTeamAttack(out snapValue, out liveValue))
			{
				return true;
			}
			if (instance.combatData2.helperTeam != null && instance.combatData2.helperTeam.CheckTeamAttack(out snapValue, out liveValue))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckHeroAttrAllTeams(out EAttrSkillType attrType, out float expected, out float live)
	{
		attrType = EAttrSkillType.EmptyValue;
		expected = 0f;
		live = 0f;
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (instance == null || instance.combatData == null)
		{
			return false;
		}
		if (CheckHeroAttrOneTeam(instance.combatData.masterTeam, ref attrType, ref expected, ref live))
		{
			return true;
		}
		if (CheckHeroAttrOneTeam(instance.combatData.helperTeam, ref attrType, ref expected, ref live))
		{
			return true;
		}
		if (instance.combatData2 != null)
		{
			if (CheckHeroAttrOneTeam(instance.combatData2.masterTeam, ref attrType, ref expected, ref live))
			{
				return true;
			}
			if (CheckHeroAttrOneTeam(instance.combatData2.helperTeam, ref attrType, ref expected, ref live))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckTeamAttrTablesAllTeams(out int tableMask, out uint snapHash, out uint liveHash)
	{
		tableMask = 0;
		snapHash = 0u;
		liveHash = 0u;
		zbCombatSystem instance = zbCombatSystem.Instance;
		if (instance == null || instance.combatData == null)
		{
			return false;
		}
		if (instance.combatData.masterTeam != null && instance.combatData.masterTeam.teamAttr != null && instance.combatData.masterTeam.teamAttr.CheckAttrTableSnapshot(out tableMask, out snapHash, out liveHash))
		{
			return true;
		}
		if (instance.combatData.helperTeam != null && instance.combatData.helperTeam.teamAttr != null && instance.combatData.helperTeam.teamAttr.CheckAttrTableSnapshot(out tableMask, out snapHash, out liveHash))
		{
			return true;
		}
		if (instance.combatData2 != null)
		{
			if (instance.combatData2.masterTeam != null && instance.combatData2.masterTeam.teamAttr != null && instance.combatData2.masterTeam.teamAttr.CheckAttrTableSnapshot(out tableMask, out snapHash, out liveHash))
			{
				return true;
			}
			if (instance.combatData2.helperTeam != null && instance.combatData2.helperTeam.teamAttr != null && instance.combatData2.helperTeam.teamAttr.CheckAttrTableSnapshot(out tableMask, out snapHash, out liveHash))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckHeroAttrOneTeam(zbCombatTeam team, ref EAttrSkillType attrType, ref float expected, ref float live)
	{
		if (team == null)
		{
			return false;
		}
		foreach (zbHero hero in team.heroList)
		{
			if ((Object)(object)hero != (Object)null && hero.heroAttr != null && hero.heroAttr.CheckAttrByRecompute(out attrType, out expected, out live))
			{
				return true;
			}
		}
		if ((Object)(object)team.mecha != (Object)null && team.mecha.heroAttr != null && team.mecha.heroAttr.CheckAttrByRecompute(out attrType, out expected, out live))
		{
			return true;
		}
		if ((Object)(object)team.foothold != (Object)null && team.foothold.heroAttr != null && team.foothold.heroAttr.CheckAttrByRecompute(out attrType, out expected, out live))
		{
			return true;
		}
		return false;
	}

	public void Abnormal_ClientReportOnce(EFuncType FuncType, float OriginValue, float ModifyValue, string ClientVersion, string ReportDesc)
	{
		if (zbCombatSystem.Instance != null)
		{
			if (lastBattleUUID != zbCombatSystem.Instance.battleInfo.BattleBaseInfo.UUID)
			{
				zbWsMsgHandler.Instance.Abnormal_ClientReport_Req(FuncType, OriginValue, ModifyValue, ClientVersion, ReportDesc);
			}
			lastBattleUUID = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.UUID;
		}
	}
}
