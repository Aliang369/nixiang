using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbCombatTeam
{
	public Dictionary<int, List<stMainHitToSubSkill>> mainHitToSubSkillBySkill = new Dictionary<int, List<stMainHitToSubSkill>>();

	public Dictionary<int, List<stMainHitToSubBuff>> mainHitToSubBuffBySkill = new Dictionary<int, List<stMainHitToSubBuff>>();

	public Dictionary<int, List<stMainEndToSubSkill>> mainEndToSubSkillBySkill = new Dictionary<int, List<stMainEndToSubSkill>>();

	public List<stMainKillToSubSkill> mainKillToSubSkill = new List<stMainKillToSubSkill>();

	public Dictionary<int, List<stMainFirstHitToSubSkill>> mainFirstHitToSubSkillBySkill = new Dictionary<int, List<stMainFirstHitToSubSkill>>();

	public Dictionary<int, List<stMainHitBuffToSubSkill>> mainHitBuffToSubSkillBySkill = new Dictionary<int, List<stMainHitBuffToSubSkill>>();

	public List<stModIndex2022> modIndex2022 = new List<stModIndex2022>();

	public stModIndex2144 modIndex2144;

	public List<stModIndex1024> modIndex1024 = new List<stModIndex1024>();

	public List<stModIndex1025> modIndex1025 = new List<stModIndex1025>();

	public List<stModIndex1029> modIndex1029 = new List<stModIndex1029>();

	public List<stModIndex1030> modIndex1030 = new List<stModIndex1030>();

	public List<stModIndex1031> modIndex1031 = new List<stModIndex1031>();

	public List<stModIndex1032> modIndex1032 = new List<stModIndex1032>();

	public List<stModIndex1033> modIndex1033 = new List<stModIndex1033>();

	public List<stModIndex1034> modIndex1034 = new List<stModIndex1034>();

	public List<stModIndex1035> modIndex1035 = new List<stModIndex1035>();

	public List<stModIndex1036> modIndex1036 = new List<stModIndex1036>();

	public List<stModIndex1037> modIndex1037 = new List<stModIndex1037>();

	public List<stModIndex1039> modIndex1039 = new List<stModIndex1039>();

	public List<stModIndex1040> modIndex1040 = new List<stModIndex1040>();

	public List<stModIndex1041> modIndex1041 = new List<stModIndex1041>();

	public List<stModIndex1042> modIndex1042 = new List<stModIndex1042>();

	public List<stModIndex1043> modIndex1043 = new List<stModIndex1043>();

	public List<stModIndex1044> modIndex1044 = new List<stModIndex1044>();

	public List<stModIndex1048> modIndex1048 = new List<stModIndex1048>();

	public List<stModIndex1049> modIndex1049 = new List<stModIndex1049>();

	public List<stModIndex1051> modIndex1051 = new List<stModIndex1051>();

	public List<stModIndex1052> modIndex1052 = new List<stModIndex1052>();

	public List<stModIndex1053> modIndex1053 = new List<stModIndex1053>();

	public List<stModIndex1055> modIndex1055 = new List<stModIndex1055>();

	public List<stModIndex1059> modIndex1059 = new List<stModIndex1059>();

	public List<stModIndex1062> modIndex1062 = new List<stModIndex1062>();

	public List<stModIndex1063> modIndex1063 = new List<stModIndex1063>();

	public List<stModIndex1064> modIndex1064 = new List<stModIndex1064>();

	public List<stModIndex1065> modIndex1065 = new List<stModIndex1065>();

	public List<stModIndex1067> modIndex1067 = new List<stModIndex1067>();

	public List<int> modIndex1068 = new List<int>();

	public List<stModIndex1069> modIndex1069 = new List<stModIndex1069>();

	public List<stModIndex1076> modIndex1076 = new List<stModIndex1076>();

	public List<stModIndex1077> modIndex1077 = new List<stModIndex1077>();

	public List<stModIndex2014> modIndex2014 = new List<stModIndex2014>();

	public stModIndex2061 modIndex2061 = new stModIndex2061();

	public List<stModIndex2134> modIndex2134 = new List<stModIndex2134>();

	public List<stModIndex2142> modIndex2142 = new List<stModIndex2142>();

	public List<stModIndex1079> modIndex1079 = new List<stModIndex1079>();

	public List<stModIndex1080> modIndex1080 = new List<stModIndex1080>();

	public List<stModIndex1081> modIndex1081 = new List<stModIndex1081>();

	public stModIndex1082 modIndex1082 = new stModIndex1082();

	public int fireActionCount;

	public List<stModIndex2021> modIndex2021 = new List<stModIndex2021>();

	public List<stModIndex2093> modIndex2093 = new List<stModIndex2093>();

	public stModIndex2145 modIndex2145;

	public zbCombatData combatData;

	public BattleTeamInfo battleTeamInfo;

	public zbCombatTeamAttr teamAttr;

	public zbCombatViolent combatViolent;

	public zbCombatStat combatStat;

	private int dummyHeroIndex;

	public zbModMgr modMgr;

	public List<zbHero> heroList = new List<zbHero>();

	public zbMechaBase mecha;

	public zbFootholdBase foothold;

	public Dictionary<ETeamExtra, float> ExtraProperties = new Dictionary<ETeamExtra, float>();

	public Dictionary<int, float> subSkillAddDamage = new Dictionary<int, float>();

	public Dictionary<int, float> subSkillAddDuration = new Dictionary<int, float>();

	public Dictionary<int, int> subSkillAddPenetration = new Dictionary<int, int>();

	public Dictionary<int, float> subSkillAddDamageRange = new Dictionary<int, float>();

	public List<stSubHitToSubSkill> subHitToSubSkill = new List<stSubHitToSubSkill>();

	public List<stMainSkillBuffExtraDamage> mainSkillBuffExtraDamage = new List<stMainSkillBuffExtraDamage>();

	public List<stSubSkillHitToSubBuff> subSkillHitToSubBuff = new List<stSubSkillHitToSubBuff>();

	public List<stDesignSkillHitBuffToSubSkill> designSkillHitBuffToSubSkill = new List<stDesignSkillHitBuffToSubSkill>();

	public List<stMainSkillBuffExtraBuff> mainSkillBuffExtraBuff = new List<stMainSkillBuffExtraBuff>();

	public List<stMainHitDefaultToSubSkill> mainHitDefaultToSubSkill = new List<stMainHitDefaultToSubSkill>();

	public List<stMainIntervalToSubSkill> mainIntervalToSubSkill = new List<stMainIntervalToSubSkill>();

	public List<stSkillEndToSubSkill> skillEndToSubSkill = new List<stSkillEndToSubSkill>();

	public List<stSubSkillEndToSubSkill> subSkillEndToSubSkill = new List<stSubSkillEndToSubSkill>();

	public Dictionary<int, float> mainToDefaultAddDamage = new Dictionary<int, float>();

	public int trackIndex;

	private bool haveHelp;

	protected static readonly List<Vector3> heroVector3s;

	protected static readonly List<Vector3> heroVector3s1;

	protected static readonly List<Vector3> heroVector3s2;

	public zbSafeInt teamAttack = new zbSafeInt(0, "zbCombatTeam.teamAttack");

	private uint teamAttackSnapObf;

	private uint teamAttackSnapKey;

	private bool teamAttackSnapValid;

	public int mechaCallCount { get; set; }

	public int mechaMaxCount { get; protected set; } = 1;

	public void Trigger1081()
	{
		foreach (stModIndex1081 item in modIndex1081)
		{
			item.modIndex1081.OnSkillUpdate();
		}
	}

	public void Trigger1082(zbHero fromHero)
	{
		if (fromHero.skillRecord.attackType == 2)
		{
			fireActionCount++;
			if (modIndex1082.modIndex1082 != null && fireActionCount >= modIndex1082.fireTotleCount)
			{
				fireActionCount = 0;
				modIndex1082.modIndex1082.OnSkillUpdate();
			}
		}
	}

	public void Init(zbCombatData _combatData, BattleTeamInfo _battleTeamInfo, int _trackIndex, bool _haveHelp)
	{
		combatData = _combatData;
		battleTeamInfo = _battleTeamInfo;
		trackIndex = _trackIndex;
		haveHelp = _haveHelp;
		mechaCallCount = 0;
		mechaMaxCount = 1;
		foreach (BattleTeamAttrDetail attr in battleTeamInfo.AttrList)
		{
			if (attr.Type == EAttrTeamType.Attack)
			{
				teamAttack.Value = (int)attr.Value;
			}
		}
		RecordTeamAttackSnapshot(teamAttack.Value);
		for (int i = 0; i < battleTeamInfo.MechaInfo.SkillList.Count; i++)
		{
			BattleMechaSkillDetail battleMechaSkillDetail = battleTeamInfo.MechaInfo.SkillList[i];
			if (battleMechaSkillDetail.SkillId <= 10065 && battleMechaSkillDetail.SkillId >= 10011)
			{
				battleMechaSkillDetail.SkillId = ConvertNumber(battleMechaSkillDetail.SkillId);
				battleTeamInfo.MechaInfo.SkillList[i] = battleMechaSkillDetail;
			}
		}
		combatStat = new zbCombatStat();
		combatStat.Init();
		teamAttr = new zbCombatTeamAttr();
		teamAttr.Init(this);
		combatViolent = new zbCombatViolent();
		combatViolent.Init(battleTeamInfo);
		CreateMecha();
		CreateFoothold();
		ExtraProperties = Enum.GetValues(typeof(ETeamExtra)).Cast<ETeamExtra>().ToDictionary((ETeamExtra e) => e, (ETeamExtra e) => 0f);
		foreach (KeyValuePair<int, zbSkillSubCsvRecord> record in zbDataConfig.Instance.skillSubCsvTable.GetRecords().records)
		{
			subSkillAddDamage[record.Value.id] = 1f;
			subSkillAddDuration[record.Value.id] = 0f;
			subSkillAddPenetration[record.Value.id] = 0;
			subSkillAddDamageRange[record.Value.id] = 0f;
		}
		modMgr = new zbModMgr();
		modMgr.Init(this);
		foreach (int mod in battleTeamInfo.ModList)
		{
			modMgr.CreateMod(mod, 0);
		}
		foreach (BattleSkillDetail skill in battleTeamInfo.SkillList)
		{
			foreach (int mod2 in skill.ModList)
			{
				modMgr.CreateMod(mod2, 0);
			}
		}
	}

	public void Free()
	{
		modMgr.Free();
		modMgr = null;
		combatStat.Free();
		teamAttackSnapValid = false;
	}

	private void RecordTeamAttackSnapshot(int value)
	{
		teamAttackSnapKey = (uint)Rand.NextInt();
		teamAttackSnapObf = (uint)value ^ teamAttackSnapKey;
		teamAttackSnapValid = true;
	}

	public bool CheckTeamAttack(out int snapValue, out int liveValue)
	{
		snapValue = 0;
		liveValue = 0;
		if (!teamAttackSnapValid)
		{
			return false;
		}
		snapValue = (int)(teamAttackSnapObf ^ teamAttackSnapKey);
		liveValue = teamAttack.Value;
		return liveValue != snapValue;
	}

	public int ConvertNumber(int source)
	{
		int num = source / 10 % 10;
		int num2 = source % 10;
		return 10000 + num * 100 + num2;
	}

	public void Restore(zbCombatRecorder restoreData)
	{
		if (trackIndex == 1)
		{
			combatStat.Restore(restoreData.loadedData.heroDamageStats, restoreData.loadedData.damageBossSum);
			foreach (stRecordUseSelectMod userSelectMod in restoreData.loadedData.userSelectModList)
			{
				modMgr.CreateMod(userSelectMod.modId, userSelectMod.level, isUserSelect: true);
			}
			foreach (zbHero hero in heroList)
			{
				hero.Restore(restoreData.loadedData.heroList, restoreData.loadedData.heroQiang);
			}
			mechaCallCount = restoreData.loadedData.teamCommom.mechaCallCount;
			mechaMaxCount = restoreData.loadedData.teamCommom.mechaMaxCount;
			return;
		}
		combatStat.Restore(restoreData.loadedData.heroHelpDamageStats, 0L);
		foreach (stRecordUseSelectMod userHelpSelectMod in restoreData.loadedData.userHelpSelectModList)
		{
			modMgr.CreateMod(userHelpSelectMod.modId, userHelpSelectMod.level, isUserSelect: true);
		}
		foreach (zbHero hero2 in heroList)
		{
			hero2.Restore(restoreData.loadedData.heroHelpList, restoreData.loadedData.heroHelpQiang);
		}
		mechaCallCount = restoreData.loadedData.teamHelpCommom.mechaCallCount;
		mechaMaxCount = restoreData.loadedData.teamHelpCommom.mechaMaxCount;
	}

	public void UnRestoreData()
	{
		foreach (int item in new List<int>())
		{
			modMgr.CreateMod(item, 0, isUserSelect: true);
		}
	}

	public zbHero CreateHero(ESkillType skillType)
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (IsHeroCreated(skillType) || dummyHeroIndex >= 5)
		{
			return null;
		}
		BattleSkillDetail battleSkillDetail = null;
		foreach (BattleSkillDetail skill in battleTeamInfo.SkillList)
		{
			if (skill.SkillType == skillType)
			{
				battleSkillDetail = skill;
			}
		}
		if (battleSkillDetail == null)
		{
			return null;
		}
		zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey((int)battleSkillDetail.SkillType);
		GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/heroBase.prefab"));
		val.transform.SetParent(combatData.nodeRootHero);
		if (trackIndex == 1)
		{
			if (haveHelp)
			{
				val.transform.localPosition = heroVector3s1[dummyHeroIndex];
				val.transform.localScale = Vector3.one;
			}
			else
			{
				val.transform.localPosition = heroVector3s[dummyHeroIndex];
				val.transform.localScale = Vector3.one;
			}
		}
		else
		{
			val.transform.localPosition = heroVector3s2[dummyHeroIndex];
			val.transform.localScale = Vector3.one;
		}
		dummyHeroIndex++;
		return AddComponentAndReturn(val, battleSkillDetail, skillType);
	}

	public void CreateMecha()
	{
		if (!((Object)(object)mecha != (Object)null) && battleTeamInfo.MechaInfo != null && battleTeamInfo.MechaInfo.MechaId != 0)
		{
			mecha = combatData.mechaRegister.CreateMecha(battleTeamInfo.MechaInfo.MechaId);
			if (!((Object)(object)mecha == (Object)null))
			{
				((Component)mecha).transform.SetParent(combatData.nodeRootHero);
				mecha.InitData(combatData, this);
			}
		}
	}

	public void CreateFoothold()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp && !((Object)(object)foothold != (Object)null))
		{
			GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/extra_attacker/attacker_foothold.prefab"));
			val.transform.SetParent(combatData.nodeRootHero);
			val.transform.position = new Vector3(-20f, -20f, 0f);
			foothold = combatData.footholdRegister.CreateFoothold(battleTeamInfo.FootholdInfo, val);
			if ((Object)(object)foothold != (Object)null)
			{
				foothold.InitData(combatData, this, battleTeamInfo.FootholdInfo.FootholdId);
			}
		}
	}

	public void UseSelectMode(int modId)
	{
		zbDataConfig.Instance.modCsvTable.GetRecordByKey(modId);
		modMgr.CreateMod(modId, combatData.combatExp.curLevel, isUserSelect: true);
	}

	public bool HasSkillType(ESkillType skillType)
	{
		foreach (BattleSkillDetail skill in battleTeamInfo.SkillList)
		{
			if (skill.SkillType == skillType)
			{
				return true;
			}
		}
		return false;
	}

	public int GetHeroIdBySkillType(ESkillType skillType)
	{
		foreach (BattleSkillDetail skill in battleTeamInfo.SkillList)
		{
			if (skill.SkillType == skillType)
			{
				return skill.HeroTid;
			}
		}
		return 0;
	}

	public zbHero GetHeroBySkillType(ESkillType skillType)
	{
		foreach (zbHero hero in heroList)
		{
			if (hero.skillDetail.SkillType == skillType)
			{
				return hero;
			}
		}
		return null;
	}

	public bool IsHeroCreated(ESkillType skillType)
	{
		foreach (zbHero hero in heroList)
		{
			if (hero.skillDetail.SkillType == skillType)
			{
				return true;
			}
		}
		return false;
	}

	public zbHero AddComponentAndReturn(GameObject heroInstance, BattleSkillDetail skillDetail, ESkillType skillType)
	{
		zbHero zbHero2 = null;
		switch (skillType)
		{
		case ESkillType.Qiang:
			zbHero2 = heroInstance.AddComponent<zbHeroQiang>();
			break;
		case ESkillType.Pao:
			zbHero2 = heroInstance.AddComponent<zbHeroPao>();
			break;
		case ESkillType.Jiguang:
			zbHero2 = heroInstance.AddComponent<zbHeroJiguang>();
			break;
		case ESkillType.Bingdan:
			zbHero2 = heroInstance.AddComponent<zbHeroBingdan>();
			break;
		case ESkillType.Ranshaodan:
			zbHero2 = heroInstance.AddComponent<zbHeroRanshaodan>();
			break;
		case ESkillType.Hongzha:
			zbHero2 = heroInstance.AddComponent<zbHeroHongzha>();
			break;
		case ESkillType.Dianci:
			zbHero2 = heroInstance.AddComponent<zbHeroDianci>();
			break;
		case ESkillType.Wurenji:
			zbHero2 = heroInstance.AddComponent<zbHeroWurenji>();
			break;
		case ESkillType.Motuo:
			zbHero2 = heroInstance.AddComponent<zbHeroMotuo>();
			break;
		case ESkillType.Bingbao:
			zbHero2 = heroInstance.AddComponent<zbHeroBingbao>();
			break;
		case ESkillType.Shexian:
			zbHero2 = heroInstance.AddComponent<zbHeroShexian>();
			break;
		case ESkillType.Dianqiu:
			zbHero2 = heroInstance.AddComponent<zbHeroDianqiu>();
			break;
		case ESkillType.Longjuan:
			zbHero2 = heroInstance.AddComponent<zbHeroLongjuan>();
			break;
		case ESkillType.Fengren:
			zbHero2 = heroInstance.AddComponent<zbHeroFengren>();
			break;
		}
		zbHero2.InitData(combatData, this, skillDetail);
		heroList.Add(zbHero2);
		return zbHero2;
	}

	static zbCombatTeam()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		heroVector3s = new List<Vector3>
		{
			new Vector3(0f, -7f),
			new Vector3(-1.2f, -7f),
			new Vector3(1.2f, -7f),
			new Vector3(-2.4f, -7f),
			new Vector3(2.4f, -7f)
		};
		heroVector3s1 = new List<Vector3>
		{
			new Vector3(-1.6f, -7f),
			new Vector3(-2.6f, -7f),
			new Vector3(-0.6f, -7f),
			new Vector3(-2.1f, -7.5f),
			new Vector3(-1.1f, -7.5f)
		};
		heroVector3s2 = new List<Vector3>
		{
			new Vector3(1.6f, -7f),
			new Vector3(0.6f, -7f),
			new Vector3(2.6f, -7f),
			new Vector3(1.1f, -7.5f),
			new Vector3(2.1f, -7.5f)
		};
	}
}
