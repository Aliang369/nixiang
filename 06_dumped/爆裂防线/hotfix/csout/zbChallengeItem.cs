using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChallengeItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbChallengeItem>
	{
	}

	private zbChallengeWnd m_wndParent;

	private EFuncType m_funcType;

	private zbFunctionUnlockCsvRecord m_record;

	private bool m_lastIsLocked = true;

	private Label m_lbName;

	private VisualElement m_spriteIcon;

	private Button m_btnRank;

	private Label m_lbTime;

	private VisualElement m_awards;

	private ScrollView m_scrollView;

	private Label m_lbDesc;

	private Label m_lbElite;

	private Label m_lbCount;

	public Button m_btnFight;

	private VisualElement m_nodeLock;

	private Label m_lbLock;

	private VisualElement m_spriteLockTime;

	private Label m_lbLockTime;

	private VisualElement m_elementTrialPanel;

	private VisualElement m_worldlairPanel;

	private Label m_lbWorldLair;

	private VisualElement m_notify;

	public zbChallengeItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/challenge/tb_challenge.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbChallengeWnd wndParent, EFuncType funcType)
	{
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_funcType = funcType;
		m_record = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)funcType);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_rank", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbElite = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_elite", (string)null);
		m_awards = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "awards", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)this, "scrollView", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_fight", (string)null);
		m_nodeLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_lock", (string)null);
		m_lbLock = UQueryExtensions.Q<Label>(m_nodeLock, "lb_lock", (string)null);
		m_spriteLockTime = UQueryExtensions.Q<VisualElement>(m_nodeLock, "icon_time", (string)null);
		m_lbLockTime = UQueryExtensions.Q<Label>(m_nodeLock, "lb_lockTime", (string)null);
		m_worldlairPanel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "worldlairPanel", (string)null);
		m_lbWorldLair = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_worldlair", (string)null);
		m_elementTrialPanel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "elementTrialPanel", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFight, (Vector2?)new Vector2(-3f, 0f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(bool updateRefresh = false)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!updateRefresh)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((TextElement)m_lbName).text = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)m_funcType).name;
		((VisualElement)m_btnRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_worldlairPanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_btnFight).text = zbUtil.Language("lang.10694");
		m_elementTrialPanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbElite).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshSpriteIcon();
		if (zbSelf.Instance.funHandler.IsLockNoTip(m_funcType))
		{
			RefreshLockDetail();
			return;
		}
		RefreshTypeDetail();
		RefreshNotify();
	}

	private void RefreshSpriteIcon()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		switch (m_funcType)
		{
		case EFuncType.WorldLair:
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/challenge/challenge_4.png"));
			break;
		case EFuncType.WorldBoss:
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/challenge/challenge_1.png"));
			break;
		case EFuncType.LegionBoss:
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/challenge/challenge_2.png"));
			break;
		case EFuncType.ElementTrial:
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/challenge/challenge_5.png"));
			break;
		case EFuncType.EndlessHorde:
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/challenge/challenge_6.png"));
			break;
		}
	}

	private void RefreshLockDetail()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		RefreshTypeDetail();
		((VisualElement)m_btnRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)m_funcType);
		zbPeerageCsvRecord recordByKey2 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(recordByKey.unlockVal);
		((TextElement)m_lbDesc).text = "";
		((TextElement)m_lbCount).text = "";
		if (m_record != null && zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours) > 0)
		{
			m_spriteLockTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLock).text = zbUtil.Language("lang.10979");
			((TextElement)m_lbLockTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours));
		}
		else
		{
			m_spriteLockTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbLock).text = string.Format(zbUtil.Language("lang.10707"), recordByKey2.name);
			((TextElement)m_lbLockTime).text = "";
		}
	}

	private void RefreshTypeDetail()
	{
		switch (m_funcType)
		{
		case EFuncType.WorldLair:
			RefreshWorldLair();
			break;
		case EFuncType.WorldBoss:
			RefreshWorldBoss();
			break;
		case EFuncType.LegionBoss:
			RefreshLegionBoss();
			break;
		case EFuncType.ElementTrial:
			RefreshElementTrial();
			break;
		case EFuncType.EndlessHorde:
			RefreshEndlessHorde();
			break;
		}
	}

	public void ShowGuide()
	{
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnFight);
	}

	private void RefreshWorldLair()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_worldlairPanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((TextElement)m_lbWorldLair).text = ((zbSelf.Instance.worldLairHandler.helpCount > 0) ? string.Format(zbUtil.Language("lang.11224"), zbSelf.Instance.worldLairHandler.helpCount.ToString()) : zbUtil.Language("lang.11225"));
		((TextElement)m_lbCount).text = zbUtil.Language("lang.11223");
		bool num = zbServerData.sGameData.WorldLairInfo != null && zbServerData.sGameData.WorldLairInfo.SelfLairInfo != null;
		int num2 = zbGameSave.Instance.GetInt("WorldLairLevel" + zbSelf.Instance.GetRoleId());
		zbWorldLairLevelCsvRecord zbWorldLairLevelCsvRecord2 = ((!num) ? zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey((num2 <= 0) ? 1 : num2) : zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(zbServerData.sGameData.WorldLairInfo.SelfLairInfo.Level));
		RefreshAwardList(zbWorldLairLevelCsvRecord2.awardShow);
	}

	private void RefreshWorldBoss()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.WorldBossSelfInfo == null || zbSelf.Instance.worldBossHandler.worldBossInfo == null)
		{
			zbWorldBossCsvRecord recordByKey = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey(1);
			RefreshAwardList(recordByKey.awardShow);
			return;
		}
		bool flag = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldBoss);
		zbFunctionUnlockCsvRecord recordByKey2 = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)m_funcType);
		if (zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.StartTm && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.EndTm)
		{
			((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.WorldBossSelfInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
			((TextElement)m_lbDesc).text = "";
			WorldBossSelfInfo worldBossSelfInfo = zbServerData.sGameData.WorldBossSelfInfo;
			string text = $"{worldBossSelfInfo.DailyCount - worldBossSelfInfo.CombatCount}/{worldBossSelfInfo.DailyCount}";
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + text;
			SetButtonGray(m_btnFight, isGray: false);
		}
		else if (zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.EndTm)
		{
			((TextElement)m_lbDesc).text = (flag ? string.Format(zbUtil.Language("lang.10707"), recordByKey2.unlockVal) : (zbUtil.Language("lang.10699") + zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.WorldBossSelfInfo.StartTm + 86400 - zbSelf.Instance.timeHandler.GetServerTime()))));
			((TextElement)m_lbCount).text = "";
			SetButtonGray(m_btnFight, isGray: true);
		}
		else if (zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.StartTm)
		{
			((TextElement)m_lbDesc).text = (flag ? string.Format(zbUtil.Language("lang.10707"), recordByKey2.unlockVal) : (zbUtil.Language("lang.10699") + zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.WorldBossSelfInfo.StartTm - zbSelf.Instance.timeHandler.GetServerTime()))));
			((TextElement)m_lbCount).text = "";
			SetButtonGray(m_btnFight, isGray: true);
		}
		bool flag2 = flag && zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.StartTm && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.EndTm && zbSelf.Instance.worldBossHandler.worldBossInfo.BossId > 0;
		zbWorldBossCsvRecord recordByKey3 = zbDataConfig.Instance.worldBossCsvTable.GetRecordByKey((!flag2) ? 1 : zbSelf.Instance.worldBossHandler.worldBossInfo.BossId);
		RefreshAwardList(recordByKey3.awardShow);
		((VisualElement)m_btnRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (zbSelf.Instance.worldBossHandler.selfRank > 0)
		{
			((TextElement)m_btnRank).text = string.Format(zbUtil.Language("lang.10697"), zbSelf.Instance.worldBossHandler.selfRank);
		}
		else
		{
			((TextElement)m_btnRank).text = zbUtil.Language("lang.10512");
		}
	}

	private void RefreshLegionBoss()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.legionHandler.legionInfo != null && zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo != null)
		{
			((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(m_funcType) ? 1 : 0));
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
			((TextElement)m_lbDesc).text = zbUtil.Language("");
		}
		else if (!zbSelf.Instance.legionHandler.HasJoinLegion())
		{
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10715");
			((TextElement)m_btnFight).text = zbUtil.Language("lang.10714");
		}
		else if (zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level >= zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl)
		{
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10153");
			((TextElement)m_btnFight).text = zbUtil.Language("lang.10696");
		}
		if (zbServerData.sGameData.LegionSelfInfo != null && zbServerData.sGameData.LegionSelfInfo.BossInfo != null)
		{
			LegionSelfBoss bossInfo = zbServerData.sGameData.LegionSelfInfo.BossInfo;
			string text = $"{bossInfo.DailyCount - bossInfo.CombatCount + bossInfo.BuyCombat}/{bossInfo.DailyCount + bossInfo.BuyCombat}";
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + text;
		}
		else
		{
			((TextElement)m_lbCount).text = "";
		}
		((VisualElement)m_btnRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (zbSelf.Instance.legionHandler.selfRank > 0)
		{
			((TextElement)m_btnRank).text = string.Format(zbUtil.Language("lang.10697"), zbSelf.Instance.legionHandler.selfRank);
		}
		else
		{
			((TextElement)m_btnRank).text = zbUtil.Language("lang.10512");
		}
		bool flag = zbSelf.Instance.legionHandler.legionInfo != null && zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo != null && zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.BossId > 0;
		zbLegionBossChallengeCsvRecord recordByKey = zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecordByKey((!flag) ? 1 : zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.BossId);
		RefreshAwardList(recordByKey.awardShow);
	}

	private void RefreshElementTrial()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.ElementTrial);
		ElementTrialInfo elementTrialInfo = zbServerData.sGameData.ElementTrialInfo;
		int num = elementTrialInfo.DailyCount - elementTrialInfo.CombatCount;
		m_elementTrialPanel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + num;
		((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		_ = zbServerData.sGameData.RechargeActiveInfo.ZhanLingElementInfo;
		if (!flag)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_elementTrialPanel, "lb_desc", (string)null)).text = string.Format(zbUtil.Language("lang.10734"), zbServerData.sGameData.ElementTrialInfo.TotalLevel.ToString());
		}
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < elementTrialInfo.TrialList.Count; i++)
		{
			TrialDetail trialDetail = elementTrialInfo.TrialList[i];
			int[][] awardShow = zbDataConfig.Instance.elementTrialCsvTable.GetRecordByKey(trialDetail.TrialType).awardShow;
			foreach (int[] obj in awardShow)
			{
				int num2 = obj[3] * trialDetail.Level;
				int[] array = (int[])obj.Clone();
				array[2] += num2;
				list.Add(array);
			}
		}
		_ = list.Count;
		if (list.Count > 0)
		{
			RefreshAwardList(list.ToArray());
			return;
		}
		Dictionary<int, zbElementTrialCsvRecord> records = zbDataConfig.Instance.elementTrialCsvTable.GetRecords().records;
		List<int[]> list2 = new List<int[]>();
		foreach (KeyValuePair<int, zbElementTrialCsvRecord> item in records)
		{
			int[][] awardShow = item.Value.awardShow;
			foreach (int[] array2 in awardShow)
			{
				list2.Add(new int[3]
				{
					array2[0],
					array2[1],
					1
				});
			}
		}
		RefreshAwardList(list2.ToArray(), showCount: false);
	}

	private void RefreshEndlessHorde()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDesc).text = "";
		zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecordByKey(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		(zbEndlessHordeLevelCsvRecord curRecord, zbEndlessHordeLevelCsvRecord maxRecord, int loopNum) record = zbSelf.Instance.endlessHordeHandler.GetRecord(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		zbEndlessHordeLevelCsvRecord item = record.curRecord;
		zbEndlessHordeLevelCsvRecord item2 = record.maxRecord;
		zbEndlessHordeLevelCsvRecord zbEndlessHordeLevelCsvRecord2 = ((record.loopNum > 0) ? item2 : item);
		if (zbEndlessHordeLevelCsvRecord2 != null)
		{
			m_awards.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshAwardList(zbEndlessHordeLevelCsvRecord2.awardShow);
		}
		if (zbServerData.sGameData.EndlessHordeInfo != null)
		{
			string text = $"{zbServerData.sGameData.EndlessHordeInfo.DailyCount - zbServerData.sGameData.EndlessHordeInfo.CombatCount}/{zbServerData.sGameData.EndlessHordeInfo.DailyCount}";
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10159") + ":" + text;
		}
		else
		{
			((TextElement)m_lbCount).text = "";
		}
		((VisualElement)m_btnRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (zbSelf.Instance.endlessHordeHandler.selfRank > 0)
		{
			((TextElement)m_btnRank).text = string.Format(zbUtil.Language("lang.10697"), zbSelf.Instance.endlessHordeHandler.selfRank);
		}
		else
		{
			((TextElement)m_btnRank).text = zbUtil.Language("lang.10512");
		}
	}

	private void RefreshAwardList(int[][] awardList, bool showCount = true)
	{
		((VisualElement)m_scrollView).Clear();
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
		}).ExecuteLater(0L);
		for (int num = 0; num < awardList.Length; num++)
		{
			zbItem zbItem2 = CreateAward(awardList[num], showCount);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbItem2);
		}
	}

	private zbItem CreateAward(int[] awardData, bool showCount)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		zbItem obj = new zbItem();
		zbItemData data = new zbItemData((EResourceType)awardData[0], awardData[1], awardData[2]);
		((VisualElement)obj).style.flexGrow = StyleFloat.op_Implicit(1f);
		((VisualElement)obj).style.flexShrink = StyleFloat.op_Implicit(0f);
		((VisualElement)obj).style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)obj).style.alignSelf = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)obj).style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)0);
		((VisualElement)obj).style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)0);
		((VisualElement)obj).style.scale = new StyleScale(new Vector2(0.7f, 0.7f));
		((VisualElement)obj).style.width = StyleLength.op_Implicit(110f);
		obj.OnEnable(m_wndParent);
		obj.Refresh(data, isNameVis: false, showCount);
		return obj;
	}

	private void RefreshNotify()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		switch (m_funcType)
		{
		case EFuncType.WorldLair:
			flag = zbSelf.Instance.worldLairHandler.HasWorldLairRedPoint();
			break;
		case EFuncType.WorldBoss:
			flag = zbSelf.Instance.worldBossHandler.HasRedPoint();
			break;
		case EFuncType.LegionBoss:
			flag = zbSelf.Instance.legionHandler.HasCallengeCountRedPoint() || zbSelf.Instance.legionHandler.HasCallengeAwardRedPoint();
			break;
		case EFuncType.ElementTrial:
			flag = zbSelf.Instance.elementTrialHandler.HasRedPoint();
			break;
		case EFuncType.EndlessHorde:
			flag = zbSelf.Instance.endlessHordeHandler.HasRedPoint();
			break;
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnFight(ClickEvent evt)
	{
		m_wndParent.m_scrollToFuncType = m_funcType;
		switch (m_funcType)
		{
		case EFuncType.WorldLair:
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldLair))
			{
				zbGameSave.Instance.SetString("WorldLairFirstDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
				zbSelf.Instance.worldLairHandler.m_localDate = zbSelf.Instance.timeHandler.GetServerDateStr();
				if (zbServerData.sGameData.WorldLairInfo != null)
				{
					zbWsMsgHandler.Instance.WorldLair_Data_Req();
				}
				else
				{
					zbLinkHelper.OpenWorld();
				}
			}
			else
			{
				zbLinkHelper.OpenPeeerage();
			}
			break;
		case EFuncType.WorldBoss:
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldBoss))
			{
				if (zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.StartTm && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.EndTm)
				{
					zbLinkHelper.OpenWorldBoss();
				}
				else
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10701"));
				}
			}
			else
			{
				zbLinkHelper.OpenPeeerage();
			}
			break;
		case EFuncType.LegionBoss:
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.LegionBoss))
			{
				if (zbSelf.Instance.legionHandler.HasJoinLegion())
				{
					int level = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level;
					int legionLvl = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl;
					if (level >= legionLvl)
					{
						zbUIMgr.Instance.ShowLegionBoss();
					}
					else
					{
						zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10735"), legionLvl));
					}
				}
				else
				{
					zbLinkHelper.OpenLegion();
				}
			}
			else
			{
				zbLinkHelper.OpenPeeerage();
			}
			break;
		case EFuncType.ElementTrial:
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.ElementTrial))
			{
				zbLinkHelper.OpenElementTrial();
			}
			else
			{
				zbLinkHelper.OpenPeeerage();
			}
			break;
		case EFuncType.EndlessHorde:
			if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EndlessHorde))
			{
				zbLinkHelper.OpenEndlessHorde();
			}
			else
			{
				zbLinkHelper.OpenPeeerage();
			}
			break;
		}
	}

	private void SetButtonGray(Button btn, bool isGray)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (isGray)
		{
			((VisualElement)btn).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_180_80_green.png"));
		}
		else
		{
			((VisualElement)btn).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		}
	}

	private void OnRank()
	{
		switch (m_funcType)
		{
		case EFuncType.LegionBoss:
			if (zbSelf.Instance.legionHandler.HasJoinLegion() && zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level >= zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl)
			{
				zbWsMsgHandler.Instance.LegionBoss_RankList_Req();
			}
			break;
		case EFuncType.WorldBoss:
			if (zbServerData.sGameData.WorldBossSelfInfo != null && zbSelf.Instance.worldBossHandler.worldBossInfo != null && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldBoss) && zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.StartTm && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.EndTm && zbSelf.Instance.worldBossHandler.worldBossInfo.BossId > 0)
			{
				zbWsMsgHandler.Instance.WorldBoss_RankList_Req(zbSelf.Instance.worldBossHandler.worldBossInfo.UUID);
			}
			break;
		case EFuncType.EndlessHorde:
			if (zbServerData.sGameData.EndlessHordeInfo != null)
			{
				zbWsMsgHandler.Instance.Rank_List_Req(ERankType.EndlessHorde, openRank: true);
			}
			break;
		}
	}

	public void Update()
	{
		if (m_funcType == EFuncType.WorldBoss && zbServerData.sGameData.WorldBossSelfInfo != null)
		{
			if (zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.WorldBossSelfInfo.EndTm && !zbSelf.Instance.funHandler.IsLockNoTip(m_funcType))
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10699") + zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.WorldBossSelfInfo.StartTm + 86400 - zbSelf.Instance.timeHandler.GetServerTime()));
			}
			else if (zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldBossSelfInfo.StartTm && !zbSelf.Instance.funHandler.IsLockNoTip(m_funcType))
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10699") + zbUtil.FormatTimeHHMMSS((int)(zbServerData.sGameData.WorldBossSelfInfo.StartTm - zbSelf.Instance.timeHandler.GetServerTime()));
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbServerData.sGameData.WorldBossSelfInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
		}
		else if (m_funcType == EFuncType.LegionBoss && zbSelf.Instance.legionHandler.legionInfo != null && zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.legionHandler.legionInfo.LegionBossInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		if (m_record != null)
		{
			long funcOpenLeftTime = zbSelf.Instance.funHandler.GetFuncOpenLeftTime(m_record.serverHours);
			bool flag = funcOpenLeftTime > 0;
			if (flag)
			{
				((TextElement)m_lbLockTime).text = zbUtil.FormatTimeHHMMSS(funcOpenLeftTime);
			}
			bool lastIsLocked = m_lastIsLocked;
			m_lastIsLocked = flag;
			if (lastIsLocked && !flag)
			{
				Refresh(updateRefresh: true);
			}
			m_lastIsLocked = flag;
		}
	}
}
