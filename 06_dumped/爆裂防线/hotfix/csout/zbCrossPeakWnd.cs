using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossPeakWnd : zbBaseWnd
{
	private class ActiveSpineBinding
	{
		public VisualElement placeholder;

		public zbCrossPeakPlayerAvatar spineGO;

		public int dataIndex;

		public zbCrossPeakItem crossPeakItem;
	}

	private VisualElement m_mine;

	private VisualElement m_firstShow;

	private Label m_lbLimitedTime;

	private Label m_lbFightNum;

	private Button m_btnBack;

	private Button m_btnRule;

	private Button m_btnRecord;

	private Button m_btnDetail;

	private Button m_btnAdd;

	private Button m_btnSettleAward;

	private Button m_btnPosition;

	private ScrollView m_scroll;

	private VisualElement m_container;

	private readonly List<ActiveSpineBinding> m_activeSpines = new List<ActiveSpineBinding>();

	private Label m_lbRank;

	private Label m_lbTodayTime;

	private VisualElement m_rankAward;

	[SerializeField]
	public GameObject m_spinePrefab;

	[SerializeField]
	private VisualTreeAsset m_rankItemTemplate;

	private CrossPeakRankDetail[] crossPeakCombatDetail;

	[SerializeField]
	private GameObject m_spineScene;

	[SerializeField]
	private GameObject m_spineAnimation_2;

	[SerializeField]
	private GameObject m_nodeFirst;

	[SerializeField]
	private SkeletonAnimation m_spineFirst1;

	[SerializeField]
	private SkeletonAnimation m_spineFirst2;

	[SerializeField]
	private SkeletonAnimation m_spineFirst3;

	private zbItem[] m_awards = new zbItem[5];

	private List<zbCrossPeakItem> crossPeakItemList = new List<zbCrossPeakItem>();

	private float viewHeight;

	private float contentHeight;

	private float scrollY;

	private float maxScroll;

	private float spineSceneMaxOffsetY = 2.4f;

	private bool isFirstShow = true;

	private float offsetY;

	private Button m_btnCard;

	private VisualElement m_cardTime;

	private Label m_lbCardTime;

	private SpineVisualElement m_spineCard;

	private bool m_isCardTimeRed;

	private VisualElement m_notifyCard;

	private VisualElement m_notifyRecord;

	private readonly float actionOffsetTime = 0.5f;

	private float scrollOffsetY;

	protected override void OnEnable()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		isFirstShow = true;
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_firstShow = UQueryExtensions.Q<VisualElement>(m_RootElement, "firstShow", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnDetail = UQueryExtensions.Q<Button>(m_RootElement, "btn_detail", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_lbLimitedTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_limitedTime", (string)null);
		m_lbFightNum = UQueryExtensions.Q<Label>(m_RootElement, "combat_count", (string)null);
		m_firstShow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_scroll = UQueryExtensions.Q<ScrollView>(m_RootElement, "listView", (string)null);
		m_scroll.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scroll.horizontalScrollerVisibility = (ScrollerVisibility)2;
		m_container = new VisualElement();
		m_container.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)0);
		((VisualElement)m_scroll).Add(m_container);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_scroll, "unity-content-viewport", (string)null).style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)0);
		m_lbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_rankAward = UQueryExtensions.Q<VisualElement>(m_mine, "awardPreview", (string)null);
		m_lbTodayTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_todayTime", (string)null);
		m_btnPosition = UQueryExtensions.Q<Button>(m_RootElement, "btn_position", (string)null);
		m_btnRecord = UQueryExtensions.Q<Button>(m_RootElement, "btn_record", (string)null);
		m_btnSettleAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_settleAward", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_card", (string)null);
		m_cardTime = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnCard, "time", (string)null);
		m_lbCardTime = UQueryExtensions.Q<Label>(m_cardTime, "lb_cardTime", (string)null);
		m_spineCard = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnCard, "spine_baofenka", (string)null);
		m_notifyCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCard, (Vector2?)new Vector2(-5f, 0f), 1f, true);
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		for (int i = 0; i < 5; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_awards[i].OnEnable(this);
		}
		m_container.Clear();
		crossPeakItemList.Clear();
		m_activeSpines.Clear();
		for (int j = 0; j < 50; j++)
		{
			zbCrossPeakItem zbCrossPeakItem2 = new zbCrossPeakItem(m_rankItemTemplate, this, j);
			if (j == 0)
			{
				((VisualElement)zbCrossPeakItem2).style.height = StyleLength.op_Implicit(500f);
			}
			else
			{
				((VisualElement)zbCrossPeakItem2).style.height = StyleLength.op_Implicit(280f);
			}
			bool flag = j == 0;
			zbCrossPeakItem2.m_spinePlaceholder.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			zbCrossPeakItem2.m_spinePlaceholderTop.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)zbCrossPeakItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_container.Add((VisualElement)(object)zbCrossPeakItem2);
			crossPeakItemList.Add(zbCrossPeakItem2);
			zbCrossPeakPlayerAvatar component = Object.Instantiate<GameObject>(m_spinePrefab).GetComponent<zbCrossPeakPlayerAvatar>();
			((Component)component).transform.SetParent(((Component)this).transform.Find("GameObjectSceneRoot"), false);
			((Component)component).transform.position = new Vector3(100f, 10f, -2f);
			m_activeSpines.Add(new ActiveSpineBinding
			{
				placeholder = (flag ? zbCrossPeakItem2.m_spinePlaceholderTop : zbCrossPeakItem2.m_spinePlaceholder),
				crossPeakItem = zbCrossPeakItem2,
				spineGO = component,
				dataIndex = j
			});
		}
		m_notifyRecord = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRecord, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPosition, (Action)OnPosition, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetail, (Action)OnDetail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecord, (Action)OnRecord, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSettleAward, (Action)OnSettleAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCard, (Action)OnCard, "", (TrickleDown)0);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[3] { m_btnRecord, m_btnRule, m_btnSettleAward });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_CrossPeakInfo, OnEventUpdate);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(byUIMgr: false);
	}

	public void RefreshCustom(bool byUIMgr)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.crossPeakHandler.settleRankInfo != null && zbSelf.Instance.crossPeakHandler.settleRankInfo.NewBestRank < zbSelf.Instance.crossPeakHandler.settleRankInfo.OldBestRank)
		{
			zbUIMgr.Instance.ShowCrossPeakBestRankWnd();
		}
		crossPeakCombatDetail = zbSelf.Instance.crossPeakHandler.combatList;
		UpdateListView();
		InitSelfInfo();
		RefreshCrossLadderCard();
		RefreshNotify();
		if (isFirstShow)
		{
			m_nodeFirst.SetActive(true);
			((MonoBehaviour)this).StartCoroutine(ActionScroll());
			return;
		}
		m_nodeFirst.SetActive(false);
		foreach (zbCrossPeakItem crossPeakItem in crossPeakItemList)
		{
			((VisualElement)crossPeakItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		m_firstShow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((MonoBehaviour)this).StartCoroutine(ActionScroll_3());
	}

	public IEnumerator ActionScroll()
	{
		((Component)m_spineFirst1).gameObject.SetActive(false);
		((Component)m_spineFirst2).gameObject.SetActive(false);
		((Component)m_spineFirst3).gameObject.SetActive(false);
		yield return null;
		((Component)m_spineFirst1).gameObject.SetActive(true);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		m_spineFirst1.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(actionOffsetTime);
		((VisualElement)crossPeakItemList[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		((Component)m_spineFirst2).gameObject.SetActive(true);
		m_spineFirst2.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(actionOffsetTime);
		((VisualElement)crossPeakItemList[1]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		((Component)m_spineFirst3).gameObject.SetActive(true);
		m_spineFirst3.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(actionOffsetTime);
		((VisualElement)crossPeakItemList[2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return (object)new WaitForSeconds(1f);
		foreach (zbCrossPeakItem crossPeakItem in crossPeakItemList)
		{
			((VisualElement)crossPeakItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		if (zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.Rank > 10 || zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.Rank == 0)
		{
			yield return null;
			yield return null;
			OnPosition();
			isFirstShow = false;
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(ActionScroll_2());
		}
	}

	public IEnumerator ActionScroll_2()
	{
		while (m_scroll.scrollOffset.y <= offsetY)
		{
			m_scroll.scrollOffset = new Vector2(m_scroll.scrollOffset.x, Time.deltaTime * 5000f + m_scroll.scrollOffset.y);
			yield return null;
		}
		isFirstShow = false;
		m_firstShow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public IEnumerator ActionScroll_3()
	{
		yield return null;
		yield return null;
		yield return null;
		m_scroll.scrollOffset = new Vector2(m_scroll.scrollOffset.x, scrollOffsetY);
	}

	private void OnPosition()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		int num = zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.Rank;
		if (num == 0)
		{
			num = 51;
		}
		m_scroll.scrollOffset = new Vector2(m_scroll.scrollOffset.x, (float)((num - 2) * 280));
		m_firstShow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void RefreshCrossLadderCard()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		m_cardTime.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard()));
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasFreePackRedDot()));
		RefreshCardTime();
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			m_spineCard.AnimationState.SetAnimation(0, "loop", true);
		}
		else
		{
			m_spineCard.AnimationState.SetAnimation(0, "normal", false);
		}
	}

	private void RefreshCardTime()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			long num = zbSelf.Instance.crossLadderHandler.GetCrossLadderCardDetail().ExpireTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbCardTime).text = zbUtil.FormatTimeSmart(num);
			bool flag = num < 86400;
			if (flag != m_isCardTimeRed)
			{
				m_isCardTimeRed = flag;
				((VisualElement)m_lbCardTime).style.color = StyleColor.op_Implicit(flag ? Color.red : Color.white);
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		m_notifyCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasFreePackRedDot()));
		m_notifyRecord.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.PeakArenaRecord)));
	}

	private void UpdateListView()
	{
		for (int i = 0; i < 50; i++)
		{
			CrossPeakRankDetail rankDetail = crossPeakCombatDetail[i];
			crossPeakItemList[i].Init(rankDetail);
			ActiveSpineBinding activeSpineBinding = m_activeSpines[i];
			SetupSpineAvatar(activeSpineBinding.spineGO, i + 1);
		}
	}

	private void SetupSpineAvatar(zbCrossPeakPlayerAvatar avatar, int rank)
	{
		int avatarId = 0;
		int gunAvatarId = 0;
		if (crossPeakCombatDetail[rank - 1].RoleBaseInfo != null)
		{
			avatarId = crossPeakCombatDetail[rank - 1].RoleBaseInfo.AvatarId;
			gunAvatarId = crossPeakCombatDetail[rank - 1].RoleBaseInfo.GunId;
		}
		avatar.RefreshCustom(rank, avatarId, gunAvatarId);
	}

	private void InitSelfInfo()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		RoleBaseInfo role = zbServerData.sGameData.RoleBaseInfo;
		CrossPeakRankDetail crossPeakRankDetail = crossPeakCombatDetail.FirstOrDefault((CrossPeakRankDetail r) => r.RoleBaseInfo?.RoleId == role.RoleId);
		if (crossPeakRankDetail != null)
		{
			((TextElement)m_lbRank).text = crossPeakRankDetail.Rank.ToString();
			int[][] awardList = zbDataConfig.Instance.peakRankAwardCsvTable.GetRecordByKey(crossPeakRankDetail.Rank).awardList;
			for (int num = 0; num < 5; num++)
			{
				bool flag = num < awardList.Length;
				((VisualElement)m_awards[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (flag)
				{
					m_awards[num].Refresh(new zbItemData((EResourceType)awardList[num][0], awardList[num][1], awardList[num][2]));
				}
			}
			offsetY = (crossPeakRankDetail.Rank - 2) * 280;
		}
		else
		{
			for (int num2 = 0; num2 < 5; num2++)
			{
				bool flag2 = num2 < 4;
				((VisualElement)m_awards[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
				if (flag2)
				{
					m_awards[num2].Refresh(new zbItemData(EResourceType.Box, 0L, 1L), isNameVis: false, isCountVis: true, isTouchVis: true, isFrameVis: true, isRareVis: true, string.Format(zbUtil.Language("lang.11119"), 50));
				}
			}
			((TextElement)m_lbRank).text = zbUtil.Language("lang.10512");
			offsetY = 13440f;
		}
		((TextElement)m_lbFightNum).text = zbUtil.Language("剩余次数:") + (zbServerData.sGameData.CrossPeakSelfInfo.DailyCount - zbServerData.sGameData.CrossPeakSelfInfo.CombatCount);
	}

	protected override void UpdateAtShow()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		foreach (ActiveSpineBinding activeSpine in m_activeSpines)
		{
			if (((VisualElement)activeSpine.crossPeakItem).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				Vector3 worldPositionFromVisualElement = zbUtil.GetWorldPositionFromVisualElement(activeSpine.placeholder, zbCameraHelper.Instance.mainCamera);
				worldPositionFromVisualElement.z = -2f;
				((Component)activeSpine.spineGO).transform.position = worldPositionFromVisualElement;
			}
		}
		UpdateLimitedTime();
		if (!isFirstShow)
		{
			if (!float.IsNaN(maxScroll) && maxScroll > 0f)
			{
				scrollY = m_scroll.scrollOffset.y;
				Transform transform = m_spineScene.transform;
				Transform transform2 = m_spineAnimation_2.transform;
				transform.position = new Vector3(transform.position.x, -1.2f + scrollY / maxScroll * spineSceneMaxOffsetY, transform.position.z);
				transform2.position = new Vector3(transform2.position.x, scrollY / 90f, transform2.position.z);
			}
			else
			{
				Rect layout = ((VisualElement)m_scroll).layout;
				viewHeight = ((Rect)(ref layout)).height;
				layout = ((VisualElement)m_scroll).contentContainer.layout;
				contentHeight = ((Rect)(ref layout)).height;
				maxScroll = contentHeight - viewHeight;
			}
		}
		if (m_scroll.scrollOffset.y > 0f)
		{
			scrollOffsetY = m_scroll.scrollOffset.y;
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshCardTime();
		RefreshNotify();
	}

	private void UpdateLimitedTime()
	{
		if (zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			((TextElement)m_lbLimitedTime).text = zbUtil.FormatTimeSmart(zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((TextElement)m_lbLimitedTime).text = "";
		}
		((TextElement)m_lbTodayTime).text = zbSelf.Instance.crossPeakHandler.GetArenaRefreshTimeStr();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		OnEventUpdate(null);
	}

	private void OnClose()
	{
		isFirstShow = false;
		Hide();
	}

	private void OnRecord()
	{
		zbWsMsgHandler.Instance.CrossPeak_RecordList_Req();
	}

	private void OnSettleAward()
	{
		zbUIMgr.Instance.ShowCrossLadderSettleAward();
	}

	public void OnRule()
	{
		string[] langKeys = new string[10] { "lang.11109", "lang.11110", "lang.11111", "lang.11112", "lang.11113", "lang.11114", "lang.11115", "lang.11116", "lang.11117", "lang.11118" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnDetail()
	{
		zbUIMgr.Instance.ShowCrossPeakRank();
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.PeakArena);
	}

	private void OnCard()
	{
		zbUIMgr.Instance.ShowCrossLadderHelpCardWnd();
	}
}
