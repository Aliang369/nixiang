using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarHandler
{
	public int currPlayerId;

	public int currActionPoints;

	public bool openLegionWar;

	public bool openLegionWarFight;

	public bool openLegionWarSettle;

	[CompilerGenerated]
	private Vector3 _003CtipVector3_003Ek__BackingField;

	public LegionWarZhanLingInfo PreZhanlingInfo;

	public LegionWarRecordDetail[] legionWarRecordDetails;

	public readonly string redProgressBg = "ui/image/legionWar/process_red_336x64_0.png";

	public readonly string redProgressPg = "ui/image/legionWar/process_red_336x64_1.png";

	public readonly string blueProgressBg = "ui/image/legionWar/process_blue_336x64_0.png";

	public readonly string blueProgressPg = "ui/image/legionWar/process_blue_336x64_1.png";

	public int searchAwardLimitNum;

	private bool showLegionWar = true;

	public bool showLegionWarBubble = true;

	public string saveSettleStr;

	public bool CanReq;

	public ELegionWarForce watchLegionWarForce = ELegionWarForce.Red;

	public int maxZone;

	public int currSettleZone;

	public ELegionWarState m_legionWarState { get; set; }

	public Vector3 tipVector3
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CtipVector3_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CtipVector3_003Ek__BackingField = value;
		}
	}

	public LegionWarMapInfo legionWarMapInfo { get; set; }

	public LegionWarSelfInfo legionWarSelfInfo { get; set; }

	public LegionWarSelfTeamInfo legionWarSelfTeamInfo { get; set; }

	public LegionWarPointStateDetail legionWarPointStateDetail { get; set; }

	public List<LegionBaseInfo> LegionWarSignUpList { get; set; } = new List<LegionBaseInfo>();

	public LegionWarLegionInfo legionWarLegionInfo { get; set; } = new LegionWarLegionInfo();

	public LegionWarPointInfo legionWarPointInfo { get; set; }

	public LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck { get; set; }

	public LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfoAck { get; set; }

	public LegionWarPlayerRankListAck legionWarPlayerRankListAck { get; set; }

	public LegionWarLegionRankListAck legionWarLegionRankListAck { get; set; }

	public List<LegionWarMoveDetail> HeroMoveList { get; set; } = new List<LegionWarMoveDetail>();

	public JObject legionWarBaseRecord { get; set; }

	public void Init()
	{
		legionWarLegionInfo.LegionForceId = ELegionWarForce.Blue;
		legionWarBaseRecord = zbDataConfig.Instance.legionWarBaseJsonTable.GetRecord();
		searchAwardLimitNum = Extensions.Value<int>((IEnumerable<JToken>)legionWarBaseRecord["searchAward"][(object)"limitNum"]);
		saveSettleStr = "settlePreview" + zbSelf.Instance.GetRoleId();
	}

	public bool IsLegionWarOpen()
	{
		return showLegionWar;
	}

	public void MergeToA(List<LegionBaseInfo> a, List<LegionBaseInfo> b)
	{
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < a.Count; i++)
		{
			hashSet.Add(a[i].LegionId);
		}
		for (int j = 0; j < b.Count; j++)
		{
			LegionBaseInfo legionBaseInfo = b[j];
			if (hashSet.Add(legionBaseInfo.LegionId))
			{
				a.Add(legionBaseInfo);
			}
		}
	}

	public void UpdateLegionWarBaseInfo(LegionWarBaseInfo legionWarBaseInfo)
	{
		zbServerData.sGameData.LegionWarBaseInfo = legionWarBaseInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarBaseInfo);
	}

	public ELegionWarState GetLegionWarState()
	{
		LegionWarOpenInfo legionWarOpenInfo = getLegionWarOpenInfo();
		if (legionWarOpenInfo != null)
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			if (serverTime >= legionWarOpenInfo.PreviewStartTm && serverTime <= legionWarOpenInfo.PreviewEndTm)
			{
				return ELegionWarState.Preview;
			}
			if (serverTime >= legionWarOpenInfo.SignUpStartTm && serverTime <= legionWarOpenInfo.SignUpEndTm)
			{
				return ELegionWarState.SignUp;
			}
			if (serverTime >= legionWarOpenInfo.PrepareStartTm && serverTime <= legionWarOpenInfo.PrepareEndTm)
			{
				return ELegionWarState.Prepare;
			}
			if (serverTime >= legionWarOpenInfo.FightingStartTm && serverTime <= legionWarOpenInfo.FightingEndTm)
			{
				if (zbSelf.Instance.legionWarHandler.m_legionWarState < ELegionWarState.Fighting)
				{
					return ELegionWarState.Fighting;
				}
				return zbSelf.Instance.legionWarHandler.m_legionWarState;
			}
			if (zbSelf.Instance.legionWarHandler.m_legionWarState < ELegionWarState.Settling)
			{
				return ELegionWarState.Settling;
			}
			return zbSelf.Instance.legionWarHandler.m_legionWarState;
		}
		return ELegionWarState.EmptyValue;
	}

	public ELegionWarState GetLegionWarStateByMapInfo()
	{
		LegionWarOpenInfo legionWarOpenInfo = getLegionWarOpenInfo();
		if (legionWarOpenInfo != null)
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			if (serverTime >= legionWarOpenInfo.FightingStartTm && serverTime <= legionWarOpenInfo.FightingEndTm)
			{
				return zbSelf.Instance.legionWarHandler.legionWarMapInfo.WarState;
			}
			return ELegionWarState.Finished;
		}
		return zbSelf.Instance.legionWarHandler.legionWarMapInfo.WarState;
	}

	public LegionWarOpenInfo getLegionWarOpenInfo()
	{
		return zbServerData.sGameData.LegionWarBaseInfo.LegionWarOpenInfo;
	}

	public long GetLeftTimeByZoneAndState(ELegionWarState state)
	{
		LegionWarOpenInfo legionWarOpenInfo = getLegionWarOpenInfo();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long result = 0L;
		switch (state)
		{
		case ELegionWarState.Preview:
			result = legionWarOpenInfo.PreviewEndTm - serverTime;
			break;
		case ELegionWarState.SignUp:
			result = legionWarOpenInfo.SignUpEndTm - serverTime;
			break;
		case ELegionWarState.Prepare:
			result = legionWarOpenInfo.PrepareEndTm - serverTime;
			break;
		case ELegionWarState.Fighting:
			result = legionWarOpenInfo.FightingEndTm - serverTime;
			break;
		case ELegionWarState.Settling:
		case ELegionWarState.Finished:
		case ELegionWarState.WaitNext:
			result = GetLeftTimeByWaitNextState(legionWarOpenInfo, serverTime);
			break;
		}
		return result;
	}

	public long GetLeftTimeByState()
	{
		LegionWarOpenInfo legionWarOpenInfo = getLegionWarOpenInfo();
		ELegionWarState legionWarState = GetLegionWarState();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long result = 0L;
		switch (legionWarState)
		{
		case ELegionWarState.Preview:
			result = legionWarOpenInfo.PreviewEndTm - serverTime;
			break;
		case ELegionWarState.SignUp:
			result = legionWarOpenInfo.SignUpEndTm - serverTime;
			break;
		case ELegionWarState.Prepare:
			result = legionWarOpenInfo.PrepareEndTm - serverTime;
			break;
		case ELegionWarState.Fighting:
			result = legionWarOpenInfo.FightingEndTm - serverTime;
			break;
		case ELegionWarState.Settling:
		case ELegionWarState.Finished:
		case ELegionWarState.WaitNext:
			result = GetLeftTimeByWaitNextState(legionWarOpenInfo, serverTime);
			break;
		}
		return result;
	}

	public long GetLeftTimeByWaitNextState(LegionWarOpenInfo legionWarOpenInfo, long serverTime)
	{
		if (legionWarOpenInfo.PreviewEndTm > serverTime)
		{
			return legionWarOpenInfo.PreviewEndTm - serverTime;
		}
		return legionWarOpenInfo.PreviewEndTm + 604800 - serverTime;
	}

	public bool HasTaskNotify()
	{
		foreach (TaskDetail legionWarTask in zbServerData.sGameData.LegionWarBaseInfo.LegionWarTaskList)
		{
			if (legionWarTask.State == EActiveState.Active)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanUpgradeZhanling()
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		LegionWarSelfInfo legionWarSelfInfo = zbSelf.Instance.legionWarHandler.legionWarSelfInfo;
		Dictionary<int, zbLegionWarLevelCsvRecord> records = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records;
		zbLegionWarLevelCsvRecord recordByKey = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecordByKey(legionWarZhanLingInfo.Level + 1);
		bool num = legionWarZhanLingInfo.Level == records.Count || recordByKey == null;
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		if ((num ? 1f : Mathf.Clamp01((float)((legionWarState == ELegionWarState.Fighting) ? legionWarSelfInfo.SelfScore : legionWarZhanLingInfo.Score) / (float)recordByKey.score)) * 100f >= 100f)
		{
			return !zbSelf.Instance.legionWarHandler.IsZhanlingMax();
		}
		return false;
	}

	public (int attackLimitNum, int actionLimitNum) GetAttackLimitNum()
	{
		int item = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["attack"][(object)"limitNum"]);
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.legionWarHandler.legionWarBaseRecord["action"][(object)"limitNum"]);
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		if (legionWarZhanLingInfo.HighActive == EBoolState.False)
		{
			return (attackLimitNum: item, actionLimitNum: num);
		}
		foreach (KeyValuePair<int, zbLegionWarLevelCsvRecord> record in zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records)
		{
			if (legionWarZhanLingInfo.Level >= record.Value.level && record.Value.awardHighType == 2)
			{
				num++;
			}
		}
		return (attackLimitNum: item, actionLimitNum: num);
	}

	public float GetActionRecoverPer()
	{
		float num = 0f;
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		if (legionWarZhanLingInfo.HighActive == EBoolState.False)
		{
			return 0f;
		}
		foreach (KeyValuePair<int, zbLegionWarLevelCsvRecord> record in zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records)
		{
			if (legionWarZhanLingInfo.Level >= record.Value.level && record.Value.awardHighType == 1)
			{
				num += record.Value.awardHigh;
			}
		}
		return num;
	}

	public bool IsInPoint(LegionWarPointPlayerDetail[] playerList)
	{
		long roleId = zbServerData.sGameData.RoleBaseInfo.RoleId;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].RoleBaseInfo.RoleId == roleId)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanReceiveZhanling()
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["legionWar"][(object)"mediumGoodsId"]);
		Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["legionWar"][(object)"highGoodsId"]);
		bool flag = legionWarZhanLingInfo.MediumActive == EBoolState.True;
		_ = legionWarZhanLingInfo.HighActive;
		if (legionWarZhanLingInfo.Level <= legionWarZhanLingInfo.LowAward)
		{
			if (flag)
			{
				return legionWarZhanLingInfo.Level > legionWarZhanLingInfo.MediumAward;
			}
			return false;
		}
		return true;
	}

	public void InitPlayerInfo(VisualElement element, RoleBaseInfo roleBaseInfo, LegionFlagInfo legionFlagInfo)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_name", (string)null)).text = roleBaseInfo.RoleName;
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId);
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			UQueryExtensions.Q<VisualElement>(element, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		}
		zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId);
		if (recordByKey2 != null)
		{
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			UQueryExtensions.Q<VisualElement>(element, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		}
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_level", (string)null)).text = roleBaseInfo.Level.ToString();
		UQueryExtensions.Q<VisualElement>(element, "icon_legionFlag", (string)null).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(legionFlagInfo?.Icon ?? 1));
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_legionName", (string)null)).text = ((legionFlagInfo == null) ? "测试啊" : legionFlagInfo.LegionName);
	}

	public void InitLegionInfo(VisualElement element, LegionFlagInfo legionFlagInfo)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		UQueryExtensions.Q<VisualElement>(element, "icon_legionFlag", (string)null).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(legionFlagInfo?.Icon ?? 1));
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_name", (string)null)).text = ((legionFlagInfo == null) ? "测试啊" : legionFlagInfo.LegionName);
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_lv", (string)null)).text = "Lv." + ((legionFlagInfo == null) ? "fff" : ((object)legionFlagInfo.Level));
	}

	public void SetSharpProgress(VisualElement root, int redScoreCount, int blueScoreCount, ELegionWarForce leftForceId, int maxWidth, int redScoreRate = -1, int blueScoreRate = -1)
	{
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		int num = Extensions.Value<int>((IEnumerable<JToken>)legionWarBaseRecord["warWinScore"]);
		VisualElement val = UQueryExtensions.Q<VisualElement>(root, "progress_left_bg", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(root, "progress_left_0", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(root, "progress_left_1", (string)null);
		VisualElement val4 = UQueryExtensions.Q<VisualElement>(root, "progress_right_bg", (string)null);
		VisualElement val5 = UQueryExtensions.Q<VisualElement>(root, "progress_right_0", (string)null);
		VisualElement val6 = UQueryExtensions.Q<VisualElement>(root, "progress_right_1", (string)null);
		Label val7 = UQueryExtensions.Q<Label>(root, "lb_redjifen", (string)null);
		Label val8 = UQueryExtensions.Q<Label>(root, "lb_bluejifen", (string)null);
		if (leftForceId == ELegionWarForce.Blue)
		{
			val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(blueProgressBg));
			val3.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(blueProgressPg));
			val4.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(redProgressBg));
			val6.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(redProgressPg));
			val2.style.width = StyleLength.op_Implicit(Math.Min((float)blueScoreCount / (float)num, 1f) * (float)maxWidth);
			val5.style.width = StyleLength.op_Implicit(Math.Min((float)redScoreCount / (float)num, 1f) * (float)maxWidth);
			if (redScoreRate == -1)
			{
				((TextElement)val7).text = blueScoreCount.ToString();
				((TextElement)val8).text = redScoreCount.ToString();
			}
			else
			{
				((TextElement)val7).text = blueScoreCount + "<color=green>(+" + blueScoreRate + "/m)</color>";
				((TextElement)val8).text = redScoreCount + "<color=green>(+" + redScoreRate + "/m)</color>";
			}
		}
		else
		{
			val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(redProgressBg));
			val3.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(redProgressPg));
			val4.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(blueProgressBg));
			val6.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(blueProgressPg));
			val5.style.width = StyleLength.op_Implicit(Math.Min((float)blueScoreCount / (float)num, 1f) * (float)maxWidth);
			val2.style.width = StyleLength.op_Implicit(Math.Min((float)redScoreCount / (float)num, 1f) * (float)maxWidth);
			if (redScoreRate == -1)
			{
				((TextElement)val7).text = redScoreCount.ToString();
				((TextElement)val8).text = blueScoreCount.ToString();
			}
			else
			{
				((TextElement)val7).text = redScoreCount + "<color=green>(+" + redScoreRate + "/m)</color>";
				((TextElement)val8).text = blueScoreCount + "<color=green>(+" + blueScoreRate + "/m)</color>";
			}
		}
	}

	public void SetBluntProgress(VisualElement root, float redOccupyPct, float blueOccupyPct, ELegionWarForce leftForceId, string progressName, int maxWidth, string sprType)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = UQueryExtensions.Q<VisualElement>(root, "progress_leftBg", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(root, "progressLeft", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(root, "progress_rightBg", (string)null);
		VisualElement val4 = UQueryExtensions.Q<VisualElement>(root, "progressRight", (string)null);
		StyleBackground backgroundImage = default(StyleBackground);
		((StyleBackground)(ref backgroundImage))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/" + progressName + "blue" + sprType));
		StyleBackground backgroundImage2 = default(StyleBackground);
		((StyleBackground)(ref backgroundImage2))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/" + progressName + "red" + sprType));
		if (leftForceId == ELegionWarForce.Blue)
		{
			val2.style.backgroundImage = backgroundImage;
			val4.style.backgroundImage = backgroundImage2;
			val.style.width = StyleLength.op_Implicit(blueOccupyPct * (float)maxWidth);
			val3.style.width = StyleLength.op_Implicit(redOccupyPct * (float)maxWidth);
		}
		else
		{
			val2.style.backgroundImage = backgroundImage2;
			val4.style.backgroundImage = backgroundImage;
			val3.style.width = StyleLength.op_Implicit(blueOccupyPct * (float)maxWidth);
			val.style.width = StyleLength.op_Implicit(redOccupyPct * (float)maxWidth);
		}
	}

	public void SetPlayerCount(VisualElement root, float redCount, float blueCount, ELegionWarForce leftForceId)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = UQueryExtensions.Q<VisualElement>(root, "red_people", (string)null);
		Label val2 = UQueryExtensions.Q<Label>(val, "lb_count", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(root, "blue_people", (string)null);
		Label val4 = UQueryExtensions.Q<Label>(val3, "lb_count", (string)null);
		StyleBackground backgroundImage = default(StyleBackground);
		((StyleBackground)(ref backgroundImage))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/people_blue.png"));
		StyleBackground backgroundImage2 = default(StyleBackground);
		((StyleBackground)(ref backgroundImage2))._002Ector(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legionWar/people_red.png"));
		if (leftForceId == ELegionWarForce.Blue)
		{
			val.style.backgroundImage = backgroundImage;
			val3.style.backgroundImage = backgroundImage2;
			((TextElement)val2).text = blueCount.ToString();
			((TextElement)val4).text = redCount.ToString();
		}
		else
		{
			val.style.backgroundImage = backgroundImage2;
			val3.style.backgroundImage = backgroundImage;
			((TextElement)val2).text = redCount.ToString();
			((TextElement)val4).text = blueCount.ToString();
		}
	}

	public LegionWarPointStateDetail GetLegionWarPointStateDetail(int point)
	{
		foreach (LegionWarPointStateDetail pointState in zbSelf.Instance.legionWarHandler.legionWarMapInfo.PointStateList)
		{
			if (pointState.PointId == point)
			{
				return pointState;
			}
		}
		return null;
	}

	public SkillDetail GetSkill(int eSkillType)
	{
		foreach (SkillDetail skill in zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.SkillList)
		{
			if (skill.SkillType == (ESkillType)eSkillType)
			{
				return skill;
			}
		}
		return null;
	}

	public void OnModList(int skillIndex = 0)
	{
		List<SkillDetail> list = new List<SkillDetail>();
		if ((zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.SkillList == null) | (zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModList == null))
		{
			return;
		}
		foreach (int mod in zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModList)
		{
			foreach (SkillDetail skill in zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)mod)
				{
					list.Add(skill);
				}
			}
		}
		zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, list.ToArray(), ((IEnumerable<int>)zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModList).ToList(), isResumeFight: false);
	}

	public int GetModNum(int eSkillType)
	{
		RepeatedField<int> modList = zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo.ModList;
		int num = 0;
		foreach (int item in modList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(item);
			if (recordByKey.skillLvUp == 1 && Enumerable.Contains(recordByKey.skillType, eSkillType))
			{
				num++;
			}
		}
		return num;
	}

	public bool CanBiaoji()
	{
		ELegionOfficial officialById = zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId);
		if (officialById == ELegionOfficial.Leader || officialById == ELegionOfficial.Adviser)
		{
			return true;
		}
		return false;
	}

	public void ShowLegionWarEffect(LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfo)
	{
		string key = "ShowLegionWarEffect" + zbSelf.Instance.GetRoleId();
		zbGameSave.Instance.GetInt(key);
		_ = zbServerData.sGameData.LegionWarBaseInfo.Season;
		zbGameSave.Instance.SetInt(key, zbServerData.sGameData.LegionWarBaseInfo.Season);
		foreach (LegionWarSettlePlayerInfo force in legionWarSettlePlayerInfo.ForceList)
		{
			if (legionWarSettlePlayerInfo.WinForceId == force.ForceId && ((IEnumerable<LegionWarPlayerRankDetail>)force.RankList).Count() >= 3)
			{
				zbUIMgr.Instance.ShowLegionWarEffect(((IEnumerable<LegionWarPlayerRankDetail>)force.RankList).ToArray());
			}
		}
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legion))
		{
			return false;
		}
		return true;
	}

	public void CallEnterPointReq()
	{
		zbLegionWarFightWnd zbLegionWarFightWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarFight) as zbLegionWarFightWnd;
		if ((Object)(object)zbLegionWarFightWnd2 != (Object)null && zbLegionWarFightWnd2.IsShow() && legionWarPointInfo != null)
		{
			zbWsMsgHandler.Instance.LegionWar_EnterPoint_Req(legionWarPointInfo.PointState.PointId);
		}
	}

	public bool IsZhanlingMax()
	{
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		int count = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records.Count;
		return legionWarZhanLingInfo.Level >= count;
	}

	public bool IsNearPoint(int PointId, int myPoint)
	{
		return Enumerable.Contains(zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(PointId).connection, myPoint);
	}

	public int GetMirrorId(ELegionWarForce forceId, int PointId)
	{
		if (forceId == ELegionWarForce.Red || forceId == ELegionWarForce.EmptyValue)
		{
			return PointId;
		}
		return zbDataConfig.Instance.legionWarMapCsvTable.GetRecordByKey(PointId).mirrorId;
	}

	public void CheckHighZhanling(LegionWarZhanLingInfo oldZhanling, LegionWarZhanLingInfo newZhanling)
	{
		bool flag = oldZhanling.HighActive == EBoolState.True;
		if (!flag && newZhanling.HighActive == EBoolState.False)
		{
			return;
		}
		Dictionary<int, zbLegionWarLevelCsvRecord>.ValueCollection values = zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records.Values;
		if (!flag && newZhanling.HighActive == EBoolState.True)
		{
			IEnumerable<zbLegionWarLevelCsvRecord> source = values.Where((zbLegionWarLevelCsvRecord r) => r.level <= newZhanling.Level);
			BubbleUpgradeInfo(source.ToArray());
		}
		if (flag && newZhanling.HighActive == EBoolState.True)
		{
			int oldLevel = oldZhanling.Level;
			int newLevel = newZhanling.Level;
			IEnumerable<zbLegionWarLevelCsvRecord> source2 = values.Where((zbLegionWarLevelCsvRecord r) => r.level > oldLevel && r.level <= newLevel);
			BubbleUpgradeInfo(source2.ToArray());
		}
	}

	public void BubbleUpgradeInfo(zbLegionWarLevelCsvRecord[] targetRecords)
	{
		if (targetRecords.Length == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (zbLegionWarLevelCsvRecord zbLegionWarLevelCsvRecord2 in targetRecords)
		{
			if (zbLegionWarLevelCsvRecord2.awardHighType == 1)
			{
				num += zbLegionWarLevelCsvRecord2.awardHigh;
			}
			if (zbLegionWarLevelCsvRecord2.awardHighType == 2)
			{
				num2 += zbLegionWarLevelCsvRecord2.awardHigh;
			}
		}
		if (num > 0f)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11255"), num * 100f + "%"));
		}
		if (num2 > 0f)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11256"), num2));
		}
	}

	public Vector2 GetRandomPoint(List<Vector2> points)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		int count = points.Count;
		if (count < 3)
		{
			return Vector2.zero;
		}
		float num = 0f;
		float[] array = new float[count - 2];
		for (int i = 0; i < count - 2; i++)
		{
			num += (array[i] = TriangleArea(points[0], points[i + 1], points[i + 2]));
		}
		float num2 = Rand.value * num;
		int num3 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (num2 < array[j])
			{
				num3 = j;
				break;
			}
			num2 -= array[j];
		}
		return RandomPointInTriangle(points[0], points[num3 + 1], points[num3 + 2]);
	}

	private float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) * 0.5f);
	}

	private Vector2 RandomPointInTriangle(Vector2 A, Vector2 B, Vector2 C)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float num = Rand.value;
		float num2 = Rand.value;
		if (num + num2 > 1f)
		{
			num = 1f - num;
			num2 = 1f - num2;
		}
		return A + num * (B - A) + num2 * (C - A);
	}

	public bool NeedShowLegionWarBubble()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionWar))
		{
			return false;
		}
		if (zbSelf.Instance.timeHandler.IsSubjectLegionWarTime())
		{
			if (showLegionWarBubble)
			{
				return true;
			}
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionWar) && GetLegionWarState() == ELegionWarState.Fighting)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshLegionRankInfo(VisualElement rankInfo, LegionWarLegionRankDetail detail)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (detail != null)
		{
			VisualElement obj = UQueryExtensions.Q<VisualElement>(rankInfo, "icon_rank", (string)null);
			Label val = UQueryExtensions.Q<Label>(rankInfo, "lb_rank", (string)null);
			VisualElement val2 = UQueryExtensions.Q<VisualElement>(rankInfo, "icon_topThree", (string)null);
			bool flag = detail.RankData.Rank <= 3;
			obj.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				val2.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/rank/icon_rank{detail.RankData.Rank}.png"));
			}
			else
			{
				((TextElement)val).text = detail.RankData.Rank.ToString();
			}
		}
	}

	public void RefreshPlayerRankInfo(VisualElement rankInfo, LegionWarPlayerRankDetail detail)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (detail != null)
		{
			VisualElement obj = UQueryExtensions.Q<VisualElement>(rankInfo, "icon_rank", (string)null);
			Label val = UQueryExtensions.Q<Label>(rankInfo, "lb_rank", (string)null);
			VisualElement val2 = UQueryExtensions.Q<VisualElement>(rankInfo, "icon_topThree", (string)null);
			bool flag = detail.RankData.Rank <= 3;
			obj.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				val2.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/rank/icon_rank{detail.RankData.Rank}.png"));
			}
			else
			{
				((TextElement)val).text = detail.RankData.Rank.ToString();
			}
		}
	}
}
