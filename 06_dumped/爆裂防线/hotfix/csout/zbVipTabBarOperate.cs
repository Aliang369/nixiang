using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTabBarOperate
{
	private zbLotteryWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbBarOperateItem[] m_items = new zbBarOperateItem[6];

	private Label m_lbTime;

	private VisualElement m_spriteName;

	private Label m_lbName;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkillType;

	private Button m_btnHero;

	private Button m_btnRate;

	private Label m_lbFrag;

	private Label m_lbScore;

	private Button m_btnScore;

	private Button m_btnHeroDetail;

	private Button m_btnOne;

	private VisualElement m_spriteOne;

	private Label m_lbOne;

	private LimitTimeHeroDetail m_timeHeroDetail;

	private zbHeroCsvRecord m_heroRecord;

	private VisualElement m_notifyBar;

	private VisualElement m_notifyScore;

	public void OnEnable(zbLotteryWnd wndParent, VisualElement root)
	{
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		for (int i = 0; i < 6; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbBarOperateItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent, this);
		}
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_spriteName = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_name", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_spriteName, "lb_name", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_spriteName, "bg_rarity", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>(m_spriteName, "bg_skillType", (string)null);
		m_btnHero = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero", (string)null);
		m_btnRate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rate", (string)null);
		m_lbFrag = UQueryExtensions.Q<Label>(m_RootElement, "lb_frag", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_score", (string)null);
		m_btnScore = UQueryExtensions.Q<Button>(m_RootElement, "btn_score", (string)null);
		m_btnHeroDetail = UQueryExtensions.Q<Button>(m_RootElement, "btn_heroDetail", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_spriteOne = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>(m_RootElement, "lb_one", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHero, (Action)OnHero, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRate, (Action)OnRate, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnScore, (Action)OnScore, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeroDetail, (Action)OnHero, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action)OnBar, "", (TrickleDown)0);
		m_notifyBar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, -4f), 1f, true);
		m_notifyScore = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnScore, (Vector2?)new Vector2(20f, -2f), 1f, true);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_HeroFrag, RefreshHeroFrag);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateMoney);
	}

	public void Refresh()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
		{
			if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeHero.BaseInfo))
			{
				if (m_timeHeroDetail == null)
				{
					m_timeHeroDetail = limitTimeHero;
				}
				m_items[num].Refresh(limitTimeHero);
				if (zbSelf.Instance.operateHandler.GetTimeHeroCount() > 1)
				{
					((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				num++;
			}
		}
		if (m_timeHeroDetail != null)
		{
			RefreshSelectList(m_timeHeroDetail.BaseInfo.UUID);
			OnUpdateMoney(null);
		}
	}

	public void RefreshSelectList(string uuid)
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		m_timeHeroDetail = zbSelf.Instance.operateHandler.GetTimeHeroInfoByUUID(uuid);
		zbBarOperateItem[] items = m_items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].RefreshSelect(uuid);
		}
		m_heroRecord = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_timeHeroDetail.BaseData.HeroTid);
		if (m_heroRecord != null)
		{
			((TextElement)m_lbName).text = m_heroRecord.name;
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)m_heroRecord.rarity);
			m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
			zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_heroRecord.avatarId);
			if (recordByKey != null)
			{
				string spinePath = recordByKey.frontBone + "_SkeletonData.asset";
				zbUtil.RefreshSpine(m_wndParent.chouKaDiZuoHero, spinePath, "idle", loop: true);
			}
		}
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_timeHeroDetail.BaseData.HeroTid));
		m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		RefreshHeroFrag(null);
		((TextElement)m_lbScore).text = m_timeHeroDetail.BaseData.LeftScore.ToString();
		m_notifyBar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasTimeHeroBarGift(uuid)));
		m_notifyScore.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasTimeHeroScoreGift(uuid)));
		Update();
	}

	private void RefreshHeroFrag(object obj)
	{
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(m_timeHeroDetail.BaseData.HeroTid);
		long composeOrUpgradeCount = zbSelf.Instance.heroHandler.GetComposeOrUpgradeCount(m_timeHeroDetail.BaseData.HeroTid);
		if (composeOrUpgradeCount > 0)
		{
			((TextElement)m_lbFrag).text = zbUtil.Language("lang.10056") + ": " + heroFragCount + "/" + composeOrUpgradeCount;
		}
		else
		{
			((TextElement)m_lbFrag).text = zbUtil.Language("lang.10056") + ": " + heroFragCount;
		}
	}

	private void OnHero()
	{
		zbUIMgr.Instance.ShowHeroDetail(m_heroRecord, showleftRight: false);
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowBarRateOperate();
	}

	private void OnBar()
	{
		if (m_timeHeroDetail == null || !zbSelf.Instance.operateHandler.IsShowTime(m_timeHeroDetail.BaseInfo))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10301"));
			m_wndParent.OnClose();
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 31, 1))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 31L, 1L);
		}
		else
		{
			zbWsMsgHandler.Instance.Operate_LimitTimeHeroDraw_Req(m_timeHeroDetail.BaseInfo.UUID, 1);
		}
	}

	private void OnScore()
	{
		zbUIMgr.Instance.ShowBarScore(m_timeHeroDetail);
	}

	public void Update()
	{
		if (m_timeHeroDetail != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_timeHeroDetail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	private void OnUpdateMoney(object obj)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 31, 1);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
	}
}
