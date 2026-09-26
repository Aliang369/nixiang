using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaWnd : zbBaseWnd
{
	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	[NonSerialized]
	public zbArenaBottom m_bottomPanel;

	private Transform[] m_nodePlayers = (Transform[])(object)new Transform[5];

	private SpriteRenderer[] m_spriteFootholds = (SpriteRenderer[])(object)new SpriteRenderer[5];

	private SkeletonAnimation[] m_spineTaizis = (SkeletonAnimation[])(object)new SkeletonAnimation[5];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[5];

	[NonSerialized]
	public VisualElement[] m_nodes = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbRanks = (Label[])(object)new Label[5];

	private Label[] m_lbNames = (Label[])(object)new Label[5];

	private Label[] m_lbPowers = (Label[])(object)new Label[5];

	private Button[] m_btnTouchs = (Button[])(object)new Button[5];

	private Button[] m_btnDiamonds = (Button[])(object)new Button[5];

	private Label[] m_lbDiamonds = (Label[])(object)new Label[5];

	private Label m_lbTime;

	private Label m_lbSelfRank;

	private Label m_lbArenaCoin;

	private Button m_btnGift;

	private Label m_lbCount;

	private Button m_btnAdd;

	private Button m_btnRefresh;

	private Label m_lbRefresh;

	private Button m_btnSweep;

	private Button m_btnRank;

	private Button m_btnLog;

	private Button m_btnRule;

	private Button m_btnClose;

	public Button m_btnSettleAward;

	private VisualElement m_notifyGift;

	private VisualElement m_notifyRecord;

	private readonly int m_refreshInterval = 5;

	private long m_nextRefreshTm;

	private bool m_isCombatListReq;

	protected override void OnEnable()
	{
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg").Find("foothold")).GetComponent<SpriteRenderer>();
		m_spineAvatar = ((Component)((Component)m_spriteFoothold).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "arenaBottom", (string)null);
		m_bottomPanel.OnEnable(this, m_spriteFoothold, m_spineAvatar);
		for (int i = 0; i < 5; i++)
		{
			m_nodePlayers[i] = ((Component)m_GameObjectSceneRoot.transform.Find("node" + i)).GetComponent<Transform>();
			m_spriteFootholds[i] = ((Component)((Component)m_nodePlayers[i]).transform.Find("foothold")).GetComponent<SpriteRenderer>();
			m_spineTaizis[i] = ((Component)((Component)m_nodePlayers[i]).transform.Find("taizi")).GetComponent<SkeletonAnimation>();
			m_spineAvatars[i] = ((Component)((Component)m_spriteFootholds[i]).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
			m_nodes[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node" + i, (string)null);
			m_lbRanks[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_rank", (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_name", (string)null);
			m_lbPowers[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_power", (string)null);
			m_btnTouchs[i] = UQueryExtensions.Q<Button>(m_nodes[i], "btn_touch", (string)null);
			m_btnDiamonds[i] = UQueryExtensions.Q<Button>(m_nodes[i], "btn_diamond", (string)null);
			m_lbDiamonds[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_diamond", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchs[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnTouchs(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbSelfRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfRank", (string)null);
		m_lbArenaCoin = UQueryExtensions.Q<Label>(m_RootElement, "lb_arenaCoin", (string)null);
		m_btnGift = UQueryExtensions.Q<Button>(m_RootElement, "btn_gift", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(m_RootElement, "btn_rank", (string)null);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnSettleAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_settleAward", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGift, (Action)OnGift, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnLog, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSettleAward, (Action)OnSettleAward, "", (TrickleDown)0);
		m_notifyGift = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGift, (Vector2?)new Vector2(20f, 10f), 1f, true);
		m_notifyRecord = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLog, (Vector2?)new Vector2(1f, 7f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Honor
		});
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[4] { m_btnRank, m_btnLog, m_btnRule, m_btnSettleAward });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ArenaInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ArenaCombatList, RefreshCombatList);
		zbWsMsgHandler.Instance.Arena_CombatList_Req();
	}

	public override void Refresh()
	{
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_Arena_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Arena))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Arena_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_Arena_2);
		}
		zbSelf.Instance.arenaHandler.NeedArenaEffect();
		m_bottomPanel.Refresh();
		RefreshDetail();
		RefreshSelf();
		RefreshNotify();
		RefreshCombatList();
		RefreshTime();
	}

	private void RefreshDetail()
	{
		((TextElement)m_lbSelfRank).text = zbServerData.sGameData.ArenaInfo.Rank.ToString();
		((TextElement)m_lbArenaCoin).text = zbSelf.Instance.arenaHandler.GetDailyArenaCoin(zbServerData.sGameData.ArenaInfo.Rank).ToString();
		((TextElement)m_lbRefresh).text = "";
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10868") + ": " + (zbServerData.sGameData.ArenaInfo.DailyCount - zbServerData.sGameData.ArenaInfo.CombatCount) + "/" + (zbServerData.sGameData.ArenaInfo.DailyCount - zbServerData.sGameData.ArenaInfo.BuyCount);
	}

	private void RefreshSelf()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(zbServerData.sGameData.RoleBaseInfo.FootholdId);
		if (recordByKey != null)
		{
			m_spriteFoothold.sprite = zbAssetLoader.Instance.LoadSpriteAsset(recordByKey.icon);
		}
		m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.ArenaInfo.Rank <= 10));
		((VisualElement)m_btnSweep).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.ArenaInfo.Rank > 10));
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_notifyGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.arenaHandler.HasGiftNotify()));
		m_notifyRecord.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.ArenaRecord)));
	}

	private void RefreshCombatList(object data = null)
	{
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.arenaHandler.combatList != null)
		{
			for (int i = 0; i < 5; i++)
			{
				if (i < zbSelf.Instance.arenaHandler.combatList.Length)
				{
					ArenaCombatDetail arenaCombatDetail = zbSelf.Instance.arenaHandler.combatList[i];
					zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(arenaCombatDetail.RoleBaseInfo.FootholdId);
					if (recordByKey != null)
					{
						m_spriteFootholds[i].sprite = zbAssetLoader.Instance.LoadSpriteAsset(recordByKey.icon);
					}
					m_spineAvatars[i].RefreshCustom(arenaCombatDetail.RoleBaseInfo.AvatarId, arenaCombatDetail.RoleBaseInfo.GunId);
					((TextElement)m_lbNames[i]).text = arenaCombatDetail.RoleBaseInfo.RoleName;
					((TextElement)m_lbPowers[i]).text = zbUtil.ConvertToTBMK(arenaCombatDetail.RoleBaseInfo.TeamAttack);
					((TextElement)m_lbRanks[i]).text = arenaCombatDetail.ArenaRankInfo.Rank.ToString();
					((VisualElement)m_btnDiamonds[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(arenaCombatDetail.ArenaRankInfo.Diamond <= 0));
					((TextElement)m_lbDiamonds[i]).text = arenaCombatDetail.ArenaRankInfo.Diamond.ToString();
				}
			}
		}
		if (m_isCombatListReq && !zbGuide.Instance.IsGuiding())
		{
			for (int j = 0; j < 5; j++)
			{
				((Component)m_nodePlayers[j]).gameObject.SetActive(false);
				m_nodes[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			for (int k = 0; k < 5; k++)
			{
				((Component)m_spriteFootholds[k]).gameObject.SetActive(false);
			}
			((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
			return;
		}
		for (int l = 0; l < 5; l++)
		{
			if (zbSelf.Instance.arenaHandler.combatList != null && l < zbSelf.Instance.arenaHandler.combatList.Length)
			{
				((Component)m_nodePlayers[l]).gameObject.SetActive(true);
				((Component)m_spriteFootholds[l]).gameObject.SetActive(true);
				m_nodes[l].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
	}

	private IEnumerator ChuchangSequence()
	{
		zbUIMgr.Instance.ShowModel();
		for (int i = 4; i >= 0; i--)
		{
			if (zbSelf.Instance.arenaHandler.combatList != null && i < zbSelf.Instance.arenaHandler.combatList.Length)
			{
				((MonoBehaviour)this).StartCoroutine(FadeIn(i));
				yield return (object)new WaitForSecondsRealtime(0.1f);
			}
		}
		yield return (object)new WaitForSecondsRealtime(0.5f);
		m_isCombatListReq = false;
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator FadeIn(int index)
	{
		((Component)m_nodePlayers[index]).gameObject.SetActive(true);
		m_spineTaizis[index].AnimationState.ClearTrack(0);
		m_spineTaizis[index].AnimationState.SetAnimation(0, "enter", false);
		m_spineTaizis[index].AnimationState.AddAnimation(0, "loop", true, 0f);
		yield return (object)new WaitForSecondsRealtime(1f / 3f);
		((Component)m_spriteFootholds[index]).gameObject.SetActive(true);
		m_nodes[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void ShowGuide()
	{
		((VisualElement)m_btnTouchs[2]).schedule.Execute((Action)delegate
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnTouchs[2]);
		}).StartingIn(100L);
	}

	public void OnTouchs(ClickEvent evt, int index)
	{
		if (index < zbSelf.Instance.arenaHandler.combatList.Length)
		{
			zbUIMgr.Instance.ShowArenaDetail(EBattleFuncType.Arena, zbSelf.Instance.arenaHandler.combatList[index].RoleBaseInfo, zbSelf.Instance.arenaHandler.combatList[index].ArenaRankInfo);
		}
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Arena);
	}

	private void OnRefresh()
	{
		if (m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			m_isCombatListReq = true;
			zbWsMsgHandler.Instance.Arena_CombatList_Req();
		}
	}

	private void OnSweep()
	{
		if (zbSelf.Instance.arenaHandler.HasFightCountNotify())
		{
			zbWsMsgHandler.Instance.Arena_SaoDang_Req();
		}
		else if (zbSelf.Instance.arenaHandler.buyLimit > zbServerData.sGameData.ArenaInfo.BuyCount)
		{
			OnAdd();
		}
	}

	private void OnRank()
	{
		zbWsMsgHandler.Instance.Arena_RankList_Req(openWnd: true);
	}

	private void OnLog()
	{
		zbWsMsgHandler.Instance.Arena_RecordList_Req();
	}

	private void OnRule()
	{
		string[] langKeys = new string[4] { "lang.11075", "lang.11076", "lang.11077", "lang.11078" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnGift()
	{
		zbUIMgr.Instance.ShowArenaGift();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnSettleAward()
	{
		zbUIMgr.Instance.ShowArenaRankAward();
	}

	private void RefreshTime()
	{
		long num = m_nextRefreshTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num > 0)
		{
			((TextElement)m_lbRefresh).text = zbUtil.FormatTimeMMSS(num);
			((VisualElement)m_btnRefresh).SetEnabled(false);
		}
		else
		{
			((TextElement)m_lbRefresh).text = "";
			((VisualElement)m_btnRefresh).SetEnabled(true);
		}
		((TextElement)m_lbTime).text = string.Format(zbUtil.Language("lang.10490"), zbSelf.Instance.timeHandler.GetArenaRefreshTimeStr());
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		Refresh();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		Refresh();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
