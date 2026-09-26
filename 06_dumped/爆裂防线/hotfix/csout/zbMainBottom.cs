using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMainBottom
{
	private VisualElement m_rootElement;

	private zbBaseWnd m_parentWnd;

	public Button m_btnLeader;

	public Button m_btnFormation;

	public Button m_btnHome;

	public Button m_btnMainQuest;

	public Button m_btnLegion;

	public Button m_btnArena;

	public Button m_btnChallenge;

	private VisualElement m_notifyLeader;

	private VisualElement m_notifyFormation;

	private VisualElement m_notifyHome;

	private VisualElement m_notifyMainQuest;

	private VisualElement m_notifyLegion;

	private VisualElement m_notifyArena;

	private VisualElement m_notifyChallenge;

	private VisualElement m_newFormation;

	private VisualElement m_newHome;

	private VisualElement m_legionNewTag;

	private VisualElement m_bubbleLegion;

	private VisualElement[] m_select = (VisualElement[])(object)new VisualElement[7];

	private bool m_isChallengeLock;

	private bool m_isLegionLock;

	private bool m_isHomeLock;

	private bool m_isArenaLock;

	private static Sprite s_challengeLock;

	private static Sprite s_challengeUnlock;

	private static Sprite s_legionLock;

	private static Sprite s_legionUnlock;

	private static Sprite s_defenseLock;

	private static Sprite s_defenseUnlock;

	private static Sprite s_arenaLock;

	private static Sprite s_arenaUnlock;

	private int currIndex;

	private static float m_lastDestroyTime = -10f;

	private const float m_destroyInterval = 10f;

	private ELegionWarState m_lastLegionWarState;

	private int m_delayFrames;

	private int m_delayFrameTarget = 5;

	public zbMainBottom(zbBaseWnd parentWnd, int index)
	{
		m_parentWnd = parentWnd;
		currIndex = index;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/main_bottom.uxml").Instantiate();
		m_rootElement = UQueryExtensions.Q<VisualElement>(val, "main_bottom", (string)null);
		parentWnd.GetRootElement().Add(m_rootElement);
		OnEnable();
	}

	public void OnEnable()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		m_btnLeader = UQueryExtensions.Q<Button>(m_rootElement, "btn_leader", (string)null);
		m_btnFormation = UQueryExtensions.Q<Button>(m_rootElement, "btn_formation", (string)null);
		m_btnHome = UQueryExtensions.Q<Button>(m_rootElement, "btn_defense", (string)null);
		m_btnMainQuest = UQueryExtensions.Q<Button>(m_rootElement, "btn_mainQuest", (string)null);
		m_btnLegion = UQueryExtensions.Q<Button>(m_rootElement, "btn_legion", (string)null);
		m_btnArena = UQueryExtensions.Q<Button>(m_rootElement, "btn_arena", (string)null);
		m_btnChallenge = UQueryExtensions.Q<Button>(m_rootElement, "btn_challenge", (string)null);
		m_legionNewTag = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegion, "icon_new", (string)null);
		m_bubbleLegion = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegion, "icon_bubble", (string)null);
		zbUIEffect.CreateSwingPauseAction(m_bubbleLegion);
		m_bubbleLegion.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_notifyLeader = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLeader);
		m_notifyFormation = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFormation);
		m_notifyHome = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHome);
		m_notifyMainQuest = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnMainQuest, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyLegion = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegion);
		m_notifyArena = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnArena);
		m_notifyChallenge = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnChallenge);
		m_newFormation = zbUtil.CreateNewTag((VisualElement)(object)m_btnFormation);
		m_newHome = zbUtil.CreateNewTag((VisualElement)(object)m_btnHome);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeader, (Action<ClickEvent>)OnLeader, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFormation, (Action<ClickEvent>)OnFormation, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHome, (Action<ClickEvent>)OnHome, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMainQuest, (Action<ClickEvent>)OnMainQuest, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegion, (Action<ClickEvent>)OnLegion, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnArena, (Action<ClickEvent>)OnArena, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChallenge, (Action)OnChallenge, "", (TrickleDown)0);
		for (int i = 0; i < 7; i++)
		{
			m_select[i] = UQueryExtensions.Q<VisualElement>(m_rootElement, "bg_select" + i, (string)null);
			m_select[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((currIndex != i) ? 1 : 0));
		}
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootElement);
		m_parentWnd.m_gameEevent.RegisterEvent(EGameEvent.Event_Peerage_Upgrade, OnEventPeerage);
		m_parentWnd.m_gameEevent.RegisterEvent(EGameEvent.Event_Peerage_Data, OnEventPeerage);
	}

	private void OnEventPeerage(object data)
	{
		Refresh();
	}

	public void SetVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	public void Refresh()
	{
		RefreshButtonState();
		UpdateRedPoint(forceUpdate: true);
	}

	private void RefreshButtonState()
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)s_challengeLock == (Object)null)
		{
			s_challengeLock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_challenge_lock.png");
		}
		if ((Object)(object)s_challengeUnlock == (Object)null)
		{
			s_challengeUnlock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_challenge.png");
		}
		if ((Object)(object)s_legionLock == (Object)null)
		{
			s_legionLock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_legion_lock.png");
		}
		if ((Object)(object)s_legionUnlock == (Object)null)
		{
			s_legionUnlock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_legion.png");
		}
		if ((Object)(object)s_defenseLock == (Object)null)
		{
			s_defenseLock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_defense_lock.png");
		}
		if ((Object)(object)s_defenseUnlock == (Object)null)
		{
			s_defenseUnlock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_defense.png");
		}
		if ((Object)(object)s_arenaLock == (Object)null)
		{
			s_arenaLock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_arena_lock.png");
		}
		if ((Object)(object)s_arenaUnlock == (Object)null)
		{
			s_arenaUnlock = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/main_b_arena.png");
		}
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnChallenge, "icon", (string)null).style.backgroundImage = new StyleBackground(IsChallengeUnLock() ? s_challengeUnlock : s_challengeLock);
		m_isLegionLock = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legion);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnLegion, "icon", (string)null).style.backgroundImage = new StyleBackground(m_isLegionLock ? s_legionLock : s_legionUnlock);
		m_isHomeLock = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Homeland);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnHome, "icon", (string)null).style.backgroundImage = new StyleBackground(m_isHomeLock ? s_defenseLock : s_defenseUnlock);
		m_isArenaLock = zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnArena, "icon", (string)null).style.backgroundImage = new StyleBackground(m_isArenaLock ? s_arenaLock : s_arenaUnlock);
	}

	private bool IsChallengeUnLock()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldLair) && zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldBoss) && zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EndlessHorde) && zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionBoss))
		{
			return !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EndlessHorde);
		}
		return true;
	}

	public void UpdateRedPoint(bool forceUpdate = false)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		if (!forceUpdate && m_delayFrames < m_delayFrameTarget)
		{
			m_delayFrames++;
			return;
		}
		m_delayFrames = 0;
		m_notifyLeader.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Leader)));
		m_notifyFormation.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Formation)));
		m_notifyHome.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_isHomeLock || !zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Home)) ? 1 : 0));
		m_notifyMainQuest.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.MainQuest) && !zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Challenge)) ? 1 : 0));
		m_notifyLegion.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_isLegionLock || !zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Legion)) ? 1 : 0));
		m_notifyArena.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_isArenaLock || !zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Arena)) ? 1 : 0));
		m_notifyChallenge.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_isChallengeLock || !zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Daily)) ? 1 : 0));
		m_legionNewTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.newTagHandler.isFirstUnLockLegion()));
		m_newFormation.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Formation) || (!zbSelf.Instance.heroHandler.HasNewPoint() && !zbSelf.Instance.newTagHandler.isNewFunc(EFuncType.GeneLock))) ? 1 : 0));
		m_newHome.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Home) || !zbSelf.Instance.wallHandler.HasNewPoint()) ? 1 : 0));
		RefreshLegionWarBubble();
	}

	public void RefreshLegionWarBubble()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		m_bubbleLegion.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.NeedShowLegionWarBubble()));
		ELegionWarState legionWarState = zbSelf.Instance.legionWarHandler.GetLegionWarState();
		if (legionWarState != m_lastLegionWarState)
		{
			m_lastLegionWarState = legionWarState;
			m_bubbleLegion.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>((legionWarState == ELegionWarState.Fighting) ? "ui/image/legionWar/legionWar_bubble1.png" : "ui/image/legionWar/legionWar_bubble.png"));
		}
	}

	public void OnLeader(ClickEvent evt)
	{
		UpdateRedPoint();
		if (currIndex != 0)
		{
			zbUIMgr.Instance.ShowLeader();
			DestroyLowUseRateWnd();
			DestroyBigMemoryWnd();
		}
	}

	public void OnFormation(ClickEvent evt)
	{
		UpdateRedPoint();
		if (currIndex != 1)
		{
			zbUIMgr.Instance.ShowFormation();
			DestroyLowUseRateWnd();
			DestroyBigMemoryWnd();
		}
	}

	public void OnHome(ClickEvent evt)
	{
		UpdateRedPoint();
		DestroyLowUseRateWnd2();
		if (currIndex != 2)
		{
			zbLinkHelper.OpenHome();
		}
	}

	public void OnMainQuest(ClickEvent evt)
	{
		UpdateRedPoint();
		DestroyLowUseRateWnd2();
		if (currIndex != 3)
		{
			zbUIMgr.Instance.ShowMainLevel();
		}
	}

	public void OnLegion(ClickEvent evt)
	{
		UpdateRedPoint();
		DestroyLowUseRateWnd2();
		if (currIndex != 4)
		{
			zbLinkHelper.OpenLegion();
		}
	}

	public void OnArena(ClickEvent evt)
	{
		UpdateRedPoint();
		if (currIndex != 5 && !zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Arena))
		{
			zbUIMgr.Instance.ShowArenaMain();
		}
	}

	public void OnChallenge()
	{
		UpdateRedPoint();
		if (currIndex != 6 && !zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.WorldLair))
		{
			zbWsMsgHandler.Instance.Feature_List_Req(open: true);
		}
	}

	private void DestroyBigMemoryWnd()
	{
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime - m_lastDestroyTime < 10f))
		{
			m_lastDestroyTime = unscaledTime;
			zbUIMgr.Instance.DestroyWnd(EWndId.Home);
		}
	}

	private void DestroyLowUseRateWnd2()
	{
		zbUIMgr.Instance.DestroyWnd(EWndId.Dungeon);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonLog);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonMod);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonResume);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonFormation);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonBalance);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonEventPopUp);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonFightPop);
		zbUIMgr.Instance.DestroyWnd(EWndId.DungeonExit);
		zbUIMgr.Instance.DestroyWnd(EWndId.WorldDungeon);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderHelpCard);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderMatchSuccess);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderSettleAward);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderRank);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderRecord);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossLadderSeasonInit);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossPeak);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossPeakRecord);
		zbUIMgr.Instance.DestroyWnd(EWndId.CrossPeakRank);
		zbUIMgr.Instance.DestroyWnd(EWndId.TruckHelpCard);
		zbUIMgr.Instance.DestroyWnd(EWndId.ArenaGift);
		zbUIMgr.Instance.DestroyWnd(EWndId.ArenaRecord);
		zbUIMgr.Instance.DestroyWnd(EWndId.ArenaRank);
		zbUIMgr.Instance.DestroyWnd(EWndId.ArenaEffect);
		zbUIMgr.Instance.DestroyWnd(EWndId.Treasure);
		zbUIMgr.Instance.DestroyWnd(EWndId.TreasurePrivilege);
		zbUIMgr.Instance.DestroyWnd(EWndId.TreasureUpgrade);
		zbUIMgr.Instance.DestroyWnd(EWndId.TreasureDetail);
		zbUIMgr.Instance.DestroyWnd(EWndId.ServantGet);
		zbUIMgr.Instance.DestroyWnd(EWndId.ServantPrivilege);
		zbUIMgr.Instance.DestroyWnd(EWndId.ServantSubSkillHandBook);
		zbUIMgr.Instance.DestroyWnd(EWndId.ServantUpgradeQuality);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectBoss);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectCard);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectHoliday);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectOperate);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectJourney);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectLegionWar);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectWall);
		zbUIMgr.Instance.DestroyWnd(EWndId.SubjectMecha);
		zbUIMgr.Instance.DestroyWnd(EWndId.RestaurantActive);
		zbUIMgr.Instance.DestroyWnd(EWndId.Hub);
	}

	private void DestroyLowUseRateWnd()
	{
		zbUIMgr.Instance.DestroyWnd(EWndId.Set);
		zbUIMgr.Instance.DestroyWnd(EWndId.Code);
		zbUIMgr.Instance.DestroyWnd(EWndId.ResetName);
		zbUIMgr.Instance.DestroyWnd(EWndId.Subject);
		zbUIMgr.Instance.DestroyWnd(EWndId.Mail);
		zbUIMgr.Instance.DestroyWnd(EWndId.MailDetail);
		zbUIMgr.Instance.DestroyWnd(EWndId.Hub);
		zbUIMgr.Instance.DestroyWnd(EWndId.NoticeDetail);
		zbUIMgr.Instance.DestroyWnd(EWndId.Support);
		zbUIMgr.Instance.DestroyWnd(EWndId.TiliBank);
		zbUIMgr.Instance.DestroyWnd(EWndId.DailySign);
		zbUIMgr.Instance.DestroyWnd(EWndId.DailyActive);
		zbUIMgr.Instance.DestroyWnd(EWndId.NovicePack);
		zbUIMgr.Instance.DestroyWnd(EWndId.InfiniteGift);
		zbUIMgr.Instance.DestroyWnd(EWndId.RankFirstReach);
		zbUIMgr.Instance.DestroyWnd(EWndId.HomeEdit);
		zbUIMgr.Instance.DestroyWnd(EWndId.HomeEditBag);
		zbUIMgr.Instance.DestroyWnd(EWndId.MonsterDetail);
		zbUIMgr.Instance.DestroyWnd(EWndId.Encyclopedia);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionDonate);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionDinner);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionMembers);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionList);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionRecommendList);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionBossAward);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionLimited);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionWarSettle);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionWarSettlePreview);
		zbUIMgr.Instance.DestroyWnd(EWndId.LegionWarRankAward);
		zbUIMgr.Instance.DestroyWnd(EWndId.Loading);
		zbUIMgr.Instance.DestroyWnd(EWndId.PremiumCardPreview);
		zbUIMgr.Instance.DestroyWnd(EWndId.WeekCard);
		zbUIMgr.Instance.DestroyWnd(EWndId.Guard);
		zbUIMgr.Instance.DestroyWnd(EWndId.GuardMonster);
		zbUIMgr.Instance.DestroyWnd(EWndId.GuardLvEdit);
		zbUIMgr.Instance.DestroyWnd(EWndId.GuardMonsterDetail);
		zbUIMgr.Instance.DestroyWnd(EWndId.Foothold);
		zbUIMgr.Instance.DestroyWnd(EWndId.JourneyMovie);
		zbUIMgr.Instance.DestroyWnd(EWndId.JourneyOneShop);
		zbUIMgr.Instance.DestroyWnd(EWndId.JourneyOneXunLuo);
	}
}
