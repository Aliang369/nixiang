using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatBalanceWnd : zbTabWnd
{
	private zbCombatBalanceAward m_awardPanel;

	private zbCombatBalanceDamage m_damagePanel;

	private zbCombatBalanceSuggest m_suggestDetail;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_Detail;

	private VisualElement m_btns;

	private VisualElement m_doubleAward;

	private Label m_lbResult;

	private Label m_lbLoseResult;

	private Button m_btnDouble;

	private Button m_btnClose;

	private VisualElement m_bg;

	private Label m_lbAwardText;

	private Label m_lbSuccessDesc;

	private Button m_btnShare;

	private Sprite m_winAndAwardPreBg;

	private Sprite m_windAndDmgBg;

	private Sprite m_loseBg;

	private VisualElement m_upgrade;

	private VisualElement m_successDescDefense;

	private VisualElement m_killCount;

	private VisualElement m_selectTabBg;

	private VisualElement m_suggest;

	private Button m_btnGem;

	private Button m_btnEquip;

	private Button m_btnStronger;

	private SpineVisualElement m_winEffect;

	private float balanceTime;

	protected override void Awake()
	{
		base.Awake();
		m_awardPanel = new zbCombatBalanceAward();
		m_damagePanel = new zbCombatBalanceDamage();
		m_suggestDetail = new zbCombatBalanceSuggest();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Detail = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootDetail", (string)null);
		m_btns = UQueryExtensions.Q<VisualElement>(m_RootElement, "btns", (string)null);
		m_selectTabBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_award", (string)null);
		m_lbResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_result", (string)null);
		m_lbLoseResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_loseResult", (string)null);
		m_lbAwardText = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardText", (string)null);
		m_btnDouble = UQueryExtensions.Q<Button>(m_RootElement, "btn_double", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		m_lbSuccessDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_successDesc", (string)null);
		m_successDescDefense = UQueryExtensions.Q<VisualElement>(m_RootElement, "successDescDefense", (string)null);
		m_killCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "killCount", (string)null);
		m_upgrade = UQueryExtensions.Q<VisualElement>(m_RootElement, "upgrade", (string)null);
		m_suggest = UQueryExtensions.Q<VisualElement>(m_RootElement, "suggest", (string)null);
		m_btnGem = UQueryExtensions.Q<Button>(m_RootElement, "btn_gem", (string)null);
		m_btnEquip = UQueryExtensions.Q<Button>(m_RootElement, "btn_equip", (string)null);
		m_btnStronger = UQueryExtensions.Q<Button>(m_RootElement, "btn_stronger", (string)null);
		m_winEffect = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "winEffect", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGem, (Action)OnLeader, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEquip, (Action)OnEquipUpdate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStronger, (Action)OnStronger, "", (TrickleDown)0);
		m_winAndAwardPreBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_winAndAwardPreviewbg.png");
		m_windAndDmgBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_winbg.png");
		m_loseBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_losebg.jpg");
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_awardPanel.OnEnable(this, m_tabLayers[0]);
		m_damagePanel.OnEnable(this, m_tabLayers[1]);
		m_suggestDetail.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "suggestDetail", (string)null));
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Award, OnUpdateEventAwardRefresh);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_PlayerSuggest, OnPlayerSuggest);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.DestroyWnd(EWndId.ModWheel);
		zbUIMgr.Instance.DestroyWnd(EWndId.SelectMod);
		zbUIMgr.Instance.DestroyWnd(EWndId.ModSelectAction);
		balanceTime = 0f;
		InitLayOut();
		m_awardPanel.RefreshBalanceAward();
		m_damagePanel.Refresh();
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(((zbCombatSystem.Instance.battleResultInfo.BattleType != EBattleFuncType.MainQuest || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.MainQuest) && (zbCombatSystem.Instance.battleResultInfo.BattleType != EBattleFuncType.EliteQuest || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest) && (zbCombatSystem.Instance.battleResultInfo.BattleType != EBattleFuncType.Legendary || zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Legendary)) ? 1 : 0));
		bool flag = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail;
		((VisualElement)m_winEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (!flag)
		{
			((MonoBehaviour)this).StartCoroutine(PlayWinEffect());
		}
		bool flag2 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.GuardVerify;
		bool flag3 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.MainQuest || zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.EliteQuest;
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType != EBattleFuncType.MainQuest)
		{
			_ = zbServerData.sGameData.EliteQuestInfo.DoubleAward;
		}
		else
		{
			_ = zbServerData.sGameData.MainQuestInfo.DoubleAward;
		}
		int num = 0;
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.MainQuest)
		{
			_ = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		}
		((VisualElement)m_btnDouble).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (flag3)
		{
			((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDouble, "lb_text", (string)null)).text = zbUtil.Language("lang.10466") + num;
		}
		SetButtonShareVisible();
		if (m_currTab == 0 && !flag)
		{
			m_awardPanel.SetBackgroundImage(m_winAndAwardPreBg);
		}
		else if (m_currTab == 1 && !flag)
		{
			m_damagePanel.SetBackgroundImage(m_windAndDmgBg);
		}
		else
		{
			m_awardPanel.SetBackgroundImage(m_loseBg);
			m_damagePanel.SetBackgroundImage(m_loseBg);
		}
		((VisualElement)m_lbAwardText).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbLoseResult).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag4 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.WorldLair || zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.WorldLairHelp;
		bool flag5 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.BossSubject;
		m_successDescDefense.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!(!flag & flag3) || flag4 || flag5) ? 1 : 0));
		((VisualElement)m_lbSuccessDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!((!flag && !flag3 && !flag4 && !flag2) | flag5)));
		m_killCount.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(!flag & flag4)));
		m_selectTabBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		if (flag2)
		{
			SelectTab(1);
		}
		if (flag5)
		{
			((TextElement)m_lbSuccessDesc).text = zbUtil.Language("lang.11345") + ": " + zbSelf.Instance.bossSubjectHandler.getScore;
		}
		else if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.EndlessHorde)
		{
			float num2 = zbCombatSystem.Instance.scrollDistance + (float)zbCombatSystem.Instance.endlessHordeCurrDistance;
			((TextElement)m_lbSuccessDesc).text = (num2 / 1000f).ToString("0.#") + "km";
		}
		else if ((!flag & flag3) && !flag4)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_successDescDefense, "lb_successDesc", (string)null)).text = "HP:" + Mathf.Floor((float)(long)zbCombatSystem.Instance.combatData.defense.curHp / (float)(long)zbCombatSystem.Instance.combatData.defense.maxHp * 100f) + "%";
		}
		else
		{
			float num3 = (float)(long)zbCombatSystem.Instance.combatData.defense.curHp / (float)(long)zbCombatSystem.Instance.combatData.defense.maxHp * 100f;
			((TextElement)m_lbSuccessDesc).text = "HP:" + Mathf.Max(1f, Mathf.Floor(num3)) + "%";
		}
		if (flag4)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_killCount, "lb_killCount", (string)null)).text = zbUtil.ConvertToTBMK((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount);
			if (zbSelf.Instance.worldLairHandler.m_worldLairInfo != null)
			{
				UQueryExtensions.Q<VisualElement>(m_killCount, "icon_newRecord", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount <= zbSelf.Instance.worldLairHandler.m_worldLairInfo.BestKill) ? 1 : 0));
			}
			else
			{
				UQueryExtensions.Q<VisualElement>(m_killCount, "icon_newRecord", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		((VisualElement)m_lbResult).visible = !flag;
		m_upgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
		if (zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.MainQuest)
		{
			return;
		}
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1)
		{
			if (flag)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(612);
			}
		}
		else if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 2)
		{
			if (flag)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(613);
			}
		}
		else if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 3 && flag)
		{
			zbProgressEventHelper.Instance.SetProgressTrack(614);
		}
		if (!flag)
		{
			zbProgressEventHelper.Instance.SetProgressEvent10(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId);
		}
	}

	private IEnumerator PlayWinEffect()
	{
		yield return null;
		yield return null;
		yield return (object)new WaitForSecondsRealtime(0.35f);
		((VisualElement)m_winEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return null;
		TrackEntry val = m_winEffect.AnimationState.SetAnimation(0, "in", false);
		if (val != null)
		{
			yield return (object)new WaitForSecondsRealtime(val.Animation.Duration);
		}
		m_winEffect.AnimationState.SetAnimation(0, "loop", true);
		((VisualElement)m_lbResult).visible = true;
		yield return null;
	}

	private void InitLayOut()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		m_Detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btns.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		bool flag = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail && zbCombatSystem.Instance.battleResultInfo.BattleType == EBattleFuncType.MainQuest;
		m_suggest.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			zbWsMsgHandler.Instance.Aux_PlayerSuggest_Req(EBattleFuncType.MainQuest, zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId, _needFirst: true);
		}
	}

	private void OnPlayerSuggest(object data)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.playerSuggestHandler.playerSuggestAck != null && zbSelf.Instance.playerSuggestHandler.playerSuggestAck.SuggestList.Count > 0;
		((VisualElement)UQueryExtensions.Q<Label>(m_suggest, "lb_desc", (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		m_suggestDetail.SetDisplay(flag);
		if (flag)
		{
			m_suggestDetail.Refresh();
		}
	}

	public void ShowNewRecord(EBoolState boolState)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.MainQuest || zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.EliteQuest)
		{
			((VisualElement)UQueryExtensions.Q<Label>(m_successDescDefense, "lb_successDesc", (string)null)).style.unityBackgroundImageTintColor = StyleColor.op_Implicit((boolState == EBoolState.True) ? Color.white : Color.clear);
		}
		else
		{
			((VisualElement)m_lbSuccessDesc).style.unityBackgroundImageTintColor = StyleColor.op_Implicit((boolState == EBoolState.True) ? Color.white : Color.clear);
		}
	}

	public override void DoSelectTab()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.battleResultInfo == null)
		{
			return;
		}
		bool flag = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail;
		switch (m_currTab)
		{
		case 0:
			m_awardPanel.RefreshBalanceAward();
			m_awardPanel.SetBackgroundImage((!flag) ? m_winAndAwardPreBg : m_loseBg);
			break;
		case 1:
			m_damagePanel.Refresh();
			m_damagePanel.SetBackgroundImage((!flag) ? m_windAndDmgBg : m_loseBg);
			break;
		}
		for (int i = 0; i < 2; i++)
		{
			if (m_currTab == i)
			{
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
				((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(val);
			}
			else
			{
				((VisualElement)m_tabButtons[i]).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			}
			((VisualElement)m_tabButtons[i]).style.opacity = StyleFloat.op_Implicit(1f);
		}
	}

	private void OnUpdateEventAwardRefresh(object data)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnDouble).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		SetButtonShareVisible();
		m_awardPanel.GenerateDataBalanceAward();
		m_awardPanel.RefreshBalanceAward();
	}

	private void SetButtonShareVisible()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(((int)((VisualElement)m_btnDouble).resolvedStyle.display != 1 || !zbChannelUtil.IsOpenShare()) ? 1 : 0));
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}

	private void OnLeader()
	{
		if (!CheckIsInDungeon())
		{
			zbUIMgr.Instance.ShowLeader();
			OnHideCombat();
		}
	}

	private void OnEquipUpdate()
	{
		if (!CheckIsInDungeon())
		{
			zbUIMgr.Instance.ShowEquipUpgrade();
			OnHideCombat();
		}
	}

	private void OnStronger()
	{
		if (!CheckIsInDungeon())
		{
			zbUIMgr.Instance.ShowStronger(delegate
			{
				OnHideCombat();
			});
		}
	}

	private bool CheckIsInDungeon()
	{
		EBattleFuncType battleType = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType;
		int num;
		if (battleType != EBattleFuncType.DungeonEasyFight && battleType != EBattleFuncType.DungeonRiotFight)
		{
			num = ((battleType == EBattleFuncType.DungeonBossFight) ? 1 : 0);
			if (num == 0)
			{
				goto IL_003b;
			}
		}
		else
		{
			num = 1;
		}
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10284"));
		goto IL_003b;
		IL_003b:
		return (byte)num != 0;
	}

	public void OnClose(ClickEvent evt)
	{
		OnHideCombat();
		EBattleFuncType battleType = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType;
		bool flag = zbSelf.Instance.elementTrialHandler.IsElementLairCombat(battleType);
		switch (battleType)
		{
		case EBattleFuncType.DungeonEasyFight:
		case EBattleFuncType.DungeonRiotFight:
		case EBattleFuncType.DungeonBossFight:
			if (zbSelf.Instance.worldDungeonHandler.IsWorldDungeon())
			{
				zbUIMgr.Instance.ShowDungeon(zbSelf.Instance.worldDungeonHandler);
				zbSelf.Instance.worldDungeonHandler.OnDungeonPassStateCheck();
				if (battleType == EBattleFuncType.DungeonRiotFight && (zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Win1 || zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Win2 || zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Win3))
				{
					zbUIMgr.Instance.ShowBossUp();
				}
			}
			return;
		case EBattleFuncType.MainQuest:
		{
			int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
			int level = (zbCombatSystem.Instance.IsGotoNextLevel() ? zbServerData.sGameData.MainQuestInfo.NextLevel : enemyId);
			zbUIMgr.Instance.ShowMainLevel(0, level);
			zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
			if (Object.op_Implicit((Object)(object)zbMainLevelWnd2))
			{
				zbMainLevelWnd2.AutoGetLevelAward(enemyId);
			}
			return;
		}
		case EBattleFuncType.EliteQuest:
		{
			int enemyId2 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
			int level2 = (zbCombatSystem.Instance.IsGotoNextLevel() ? zbServerData.sGameData.EliteQuestInfo.NextLevel : enemyId2);
			zbUIMgr.Instance.ShowEliteQuest(level2);
			zbEliteQuestWnd zbEliteQuestWnd2 = zbUIMgr.Instance.GetWndById(EWndId.EliteQuest) as zbEliteQuestWnd;
			if (Object.op_Implicit((Object)(object)zbEliteQuestWnd2))
			{
				zbEliteQuestWnd2.AutoGetLevelAward(enemyId2);
			}
			return;
		}
		case EBattleFuncType.LegionBoss:
			zbUIMgr.Instance.ShowLegionBoss();
			return;
		case EBattleFuncType.WorldBoss:
			zbUIMgr.Instance.ShowWorldBoss();
			return;
		case EBattleFuncType.WorldRescueDay:
		case EBattleFuncType.WorldRescueNight:
		case EBattleFuncType.WorldEventFightDay:
		case EBattleFuncType.WorldEventFightNight:
			zbUIMgr.Instance.ShowWorld();
			return;
		case EBattleFuncType.WorldLair:
			zbUIMgr.Instance.ShowWorldLairFight(zbServerData.sGameData.WorldLairInfo.SelfLairInfo);
			return;
		case EBattleFuncType.WorldLairHelp:
			(zbUIMgr.Instance.GetWndById(EWndId.WorldLairFight) as zbWorldLairFightWnd).Hide();
			zbUIMgr.Instance.ShowWorldLairFight(zbSelf.Instance.worldLairHandler.m_worldLairInfo, isHelp: true, zbSelf.Instance.worldLairHandler.m_worldLairHelpRoleBaseInfo);
			return;
		}
		if (flag)
		{
			zbUIMgr.Instance.ShowElementTrial();
			return;
		}
		switch (battleType)
		{
		case EBattleFuncType.EndlessHorde:
			zbUIMgr.Instance.ShowEndLessHorde();
			break;
		case EBattleFuncType.Legendary:
			zbUIMgr.Instance.ShowLegendary();
			break;
		case EBattleFuncType.Arena:
		{
			if (zbCombatSystem.Instance.battleResultInfo.ResultScore != EBattleResultScore.Fail)
			{
				zbWsMsgHandler.Instance.Arena_CombatList_Req();
			}
			zbUIMgr.Instance.ShowArena();
			if (zbServerData.sGameData.ArenaInfo.Rank >= zbSelf.Instance.arenaHandler.lastRank)
			{
				break;
			}
			long diamondCount = 0L;
			foreach (AwardDetail battleAward in zbCombatSystem.Instance.battleAwardList)
			{
				if (battleAward.Type == EResourceType.Money && battleAward.Tid == 2)
				{
					diamondCount = battleAward.Count;
					break;
				}
			}
			zbUIMgr.Instance.ShowArenaBestRank(diamondCount);
			break;
		}
		case EBattleFuncType.CrossLadderMatch:
		case EBattleFuncType.CrossLadderUpgrade:
			zbUIMgr.Instance.ShowCrossLadder();
			break;
		case EBattleFuncType.MineWar:
		case EBattleFuncType.CrossMine:
			if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
			{
				zbWsMsgHandler.Instance.MineWar_Data_Req(openWnd: true);
			}
			else
			{
				zbWsMsgHandler.Instance.CrossMine_Data_Req(zbSelf.Instance.mineWarHandler.currServerId, openWnd: true);
			}
			break;
		case EBattleFuncType.PeakArena:
			zbWsMsgHandler.Instance.CrossPeak_Data_Req(openWnd: true);
			break;
		case EBattleFuncType.Truck:
		case EBattleFuncType.Train:
			zbSelf.Instance.truckHandler.bFromBalance = true;
			if (battleType == EBattleFuncType.Train)
			{
				zbWsMsgHandler.Instance.Train_Data_Req();
			}
			zbLinkHelper.OpenTruck();
			break;
		case EBattleFuncType.LegionWar:
			zbUIMgr.Instance.ShowLegionWarFight();
			break;
		case EBattleFuncType.Facility:
			zbUIMgr.Instance.ShowFacility();
			break;
		case EBattleFuncType.BossSubject:
		{
			BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
			if (bossSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowBossSubjectMain(bossSubjectDetail);
			}
			break;
		}
		case EBattleFuncType.WallSubject:
		{
			WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
			if (wallSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowWallSubject(wallSubjectDetail);
			}
			break;
		}
		case EBattleFuncType.MechaSubject:
		{
			MechaSubjectDetail mechaSubjectDetail = zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail();
			if (mechaSubjectDetail != null)
			{
				zbUIMgr.Instance.ShowMechaSubject(mechaSubjectDetail);
			}
			break;
		}
		default:
			zbUIMgr.Instance.ShowMainLevel();
			break;
		}
	}

	private void OnHideCombat()
	{
		Hide();
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
	}

	protected override void UpdateAtShow()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		balanceTime += Time.unscaledDeltaTime;
		if ((double)balanceTime >= 0.5)
		{
			m_Detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_btns.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}
}
