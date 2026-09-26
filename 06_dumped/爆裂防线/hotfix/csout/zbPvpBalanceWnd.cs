using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpBalanceWnd : zbTabWnd
{
	private zbPvpBalanceAward m_awardPanel;

	private zbPvpBalanceDamage m_damagePanel;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_Detail;

	private VisualElement m_btns;

	private Label m_lbResult;

	private Label m_lbLoseResult;

	public Label m_lbTipResult;

	private Button m_btnClose;

	private VisualElement m_bg;

	private Label m_lbSuccessDesc;

	private Button m_btnShare;

	private Sprite m_winAndAwardPreBg;

	private Sprite m_windAndDmgBg;

	private Sprite m_loseBg;

	private VisualElement m_successDescDefense;

	private VisualElement m_killCount;

	private VisualElement m_selectTabBg;

	private SpineVisualElement m_winEffect;

	private SpineVisualElement m_leftEffect;

	private SpineVisualElement m_rightEffect;

	private float balanceTime;

	public zbPlayerAvatarTarget renderTargetLeft;

	public zbPlayerAvatarTarget renderTargetRight;

	protected override void Awake()
	{
		base.Awake();
		m_awardPanel = new zbPvpBalanceAward();
		m_damagePanel = new zbPvpBalanceDamage();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		renderTargetLeft = ((Component)((Component)this).transform.Find("pvpBalanceLeft")).GetComponent<zbPlayerAvatarTarget>();
		renderTargetRight = ((Component)((Component)this).transform.Find("pvpBalanceRight")).GetComponent<zbPlayerAvatarTarget>();
		m_Detail = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootDetail", (string)null);
		m_btns = UQueryExtensions.Q<VisualElement>(m_RootElement, "btns", (string)null);
		m_selectTabBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_result", (string)null);
		m_lbLoseResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_loseResult", (string)null);
		m_lbTipResult = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		m_lbSuccessDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_successDesc", (string)null);
		m_successDescDefense = UQueryExtensions.Q<VisualElement>(m_RootElement, "successDescDefense", (string)null);
		m_killCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "killCount", (string)null);
		m_winEffect = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "winEffect", (string)null);
		m_leftEffect = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "leftEffect", (string)null);
		m_rightEffect = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "rightEffect", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_winAndAwardPreBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_winAndAwardPreviewbg.png");
		m_windAndDmgBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/pvp/balance_winbg.png");
		m_loseBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_losebg.jpg");
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_awardPanel.OnEnable(this, m_tabLayers[0]);
		m_damagePanel.OnEnable(this, m_tabLayers[1]);
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
	}

	public void RefreshCustom()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.DestroyWnd(EWndId.ModWheel);
		zbUIMgr.Instance.DestroyWnd(EWndId.SelectMod);
		zbUIMgr.Instance.DestroyWnd(EWndId.ModSelectAction);
		balanceTime = 0f;
		InitLayOut();
		m_awardPanel.RefreshBalanceAward();
		m_damagePanel.Refresh();
		bool flag = zbCombatSystem.Instance.battleResultInfo.ResultScore == EBattleResultScore.Fail;
		((VisualElement)m_winEffect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (!flag)
		{
			((MonoBehaviour)this).StartCoroutine(PlayWinEffect());
			zbUtil.RefreshSpineUI(m_leftEffect, "animation/ui/ui_iconwin/ui_iconwin_SkeletonData.asset", "loop", loop: true);
			zbUtil.RefreshSpineUI(m_rightEffect, "animation/ui/ui_iconlose/ui_iconlose_SkeletonData.asset", "loop", loop: true);
		}
		else
		{
			zbUtil.RefreshSpineUI(m_leftEffect, "animation/ui/ui_iconlose/ui_iconlose_SkeletonData.asset", "loop", loop: true);
			zbUtil.RefreshSpineUI(m_rightEffect, "animation/ui/ui_iconwin/ui_iconwin_SkeletonData.asset", "loop", loop: true);
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
		((VisualElement)m_lbLoseResult).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbResult).visible = !flag;
		AutoFontSize();
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
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
		m_Detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btns.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void AutoFontSize()
	{
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

	private void SetButtonShareVisible()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
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
			zbSelf.Instance.crossLadderHandler.isUpgradeAck = false;
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
