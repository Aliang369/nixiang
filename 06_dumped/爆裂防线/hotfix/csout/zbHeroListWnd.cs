using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbHeroListWnd : zbBaseWnd
{
	public VisualElement m_root;

	private ListView m_listView;

	private Button m_btnRarity;

	private Button m_btnBar;

	private Button m_btnDecompose;

	private Button m_btnClose;

	private VisualElement m_notifyBar;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_root = UQueryExtensions.Q<VisualElement>(m_RootElement, "root", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnRarity = UQueryExtensions.Q<Button>(m_RootElement, "btn_rarity", (string)null);
		m_btnBar = UQueryExtensions.Q<Button>(m_RootElement, "btn_bar", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRarity, (Action<ClickEvent>)OnRarity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBar, (Action<ClickEvent>)OnBar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action<ClickEvent>)OnDecompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_notifyBar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBar);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_HeroFrag, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		InitListView();
	}

	public override void Refresh()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		UpdateViewList();
		((VisualElement)m_btnRarity).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroRarity) ? 1 : 0));
		m_notifyBar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasBarRedPointByBarType(EBarType.Special)));
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbHeroListItem(this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (((BaseVerticalCollectionView)m_listView).itemsSource is List<zbSkillTypeCsvRecord> list && index >= 0 && index < list.Count && list[index] != null)
			{
				(element as zbHeroListItem)?.Refresh(list[index], index);
			}
		};
	}

	private void UpdateViewList()
	{
		zbSkillTypeCsvTableData records = zbDataConfig.Instance.skillTypeCsvTable.GetRecords();
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<zbSkillTypeCsvRecord>(records.records.Values);
	}

	private void OnRarity(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHeroRarity();
	}

	private void OnBar(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLottery(1);
	}

	private void OnDecompose(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHeroDecompose();
	}

	public void OnClose(ClickEvent evt)
	{
		if (!zbUIMgr.Instance.IsShow(EWndId.Formation))
		{
			zbUIMgr.Instance.ShowFormation();
		}
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_listView == null || ((VisualElement)m_listView).contentContainer == null)
		{
			return;
		}
		foreach (zbHeroListItem item in ((VisualElement)m_listView).contentContainer.Children())
		{
			item.Update();
		}
	}
}
