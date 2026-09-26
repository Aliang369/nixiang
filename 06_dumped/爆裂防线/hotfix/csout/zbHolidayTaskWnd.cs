using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHolidayTaskWnd : zbTabWnd
{
	private SkeletonAnimation m_spineAvatar;

	private Label m_lbDesc;

	private Label m_lbTime;

	private Label m_lbName;

	private VisualElement m_spriteRarity;

	private Label m_lbFrag;

	private Button m_btnHero;

	private Label m_lbLv;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnClose;

	private zbHolidayTaskChallenge m_panelChallenge;

	private zbHolidayTaskExchange m_panelExchange;

	private zbHolidayTaskAward m_panelAward;

	public HolidayTaskDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity", (string)null);
		m_lbFrag = UQueryExtensions.Q<Label>(m_RootElement, "lb_frag", (string)null);
		m_btnHero = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHero, (Action)OnHero, "", (TrickleDown)0);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(UQueryExtensions.Q<VisualElement>(m_RootElement, "progress_bg", (string)null), "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "layout" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
			m_notifyTabs[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i]);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_HolidayTask_Ack, OnEventUpdate);
	}

	public override void Refresh()
	{
		m_data = zbSelf.Instance.operateHandler.GetHolidayTaskInfoByUUID();
		if (m_data != null)
		{
			SelectTab(0);
			RefreshTime();
			RefreshHero();
			RefreshProgress();
			RefreshNotify();
		}
	}

	private void RefreshHero()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_data.HeroId);
		if (recordByKey != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			zbUtil.RefreshSpine(m_spineAvatar, recordByKey2.frontBone + "_SkeletonData.asset", "idle", loop: true);
			((TextElement)m_lbName).text = recordByKey.name;
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)recordByKey.rarity);
			m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		}
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(m_data.HeroId);
		long composeOrUpgradeCount = zbSelf.Instance.heroHandler.GetComposeOrUpgradeCount(m_data.HeroId);
		if (composeOrUpgradeCount > 0)
		{
			((TextElement)m_lbFrag).text = heroFragCount + "/" + composeOrUpgradeCount;
		}
		else
		{
			((TextElement)m_lbFrag).text = heroFragCount.ToString();
		}
	}

	private void RefreshProgress()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLv).text = "Lv." + m_data.Level;
		((TextElement)m_lbProgress).text = m_data.Exp + "/100";
		float num = Mathf.Clamp((float)m_data.Exp / 100f, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num * 544f, (LengthUnit)0));
	}

	private void RefreshNotify()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasTaskGift(m_data.BaseInfo.UUID)));
			m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasLevelGift(m_data.BaseInfo.UUID)));
			m_notifyTabs[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasScoreExchangeGift(m_data.BaseInfo.UUID)));
		}
	}

	public override void DoSelectTab()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if (m_panelChallenge == null)
			{
				m_panelChallenge = new zbHolidayTaskChallenge();
				m_panelChallenge.OnEnable(this, m_tabLayers[0]);
			}
			m_panelChallenge.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_panelAward == null)
			{
				m_panelAward = new zbHolidayTaskAward();
				m_panelAward.OnEnable(this, m_tabLayers[1]);
			}
			m_panelAward.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			break;
		case 2:
			if (m_panelExchange == null)
			{
				m_panelExchange = new zbHolidayTaskExchange();
				m_panelExchange.OnEnable(this, m_tabLayers[2]);
			}
			m_panelExchange.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			break;
		}
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			long seconds = m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_panelAward != null)
		{
			m_panelAward.Update();
		}
	}

	private void OnHero()
	{
		if (m_data != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Hero, m_data.HeroId, 1L);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		m_data = zbSelf.Instance.operateHandler.GetHolidayTaskInfoByUUID();
		DoSelectTab();
		RefreshProgress();
		RefreshNotify();
	}
}
