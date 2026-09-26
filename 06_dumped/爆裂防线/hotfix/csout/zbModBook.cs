using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModBook
{
	private zbEncyclopediaWnd m_wndParent;

	private VisualElement m_RootElement;

	private MultiColumnListView m_listView;

	private const int ITEM_COUNT = 5;

	private const int COLUMN_COUNT = 3;

	private int rowCount;

	private Label m_lbSumCollect;

	private VisualElement m_spriteProgress;

	private zbItem[] m_items = new zbItem[5];

	private Label[] m_lbTargetCount = (Label[])(object)new Label[5];

	private Button[] m_btnGetAwards = (Button[])(object)new Button[5];

	public ModHandbookTarget m_modHandbookTargetInfo;

	private List<zbModHandbookTypeCsvRecord> m_listItemDatas = new List<zbModHandbookTypeCsvRecord>();

	private List<int> m_rowIndexes = new List<int>();

	public void OnEnable(zbEncyclopediaWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbSumCollect = UQueryExtensions.Q<Label>(m_RootElement, "lb_sumCollect", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		for (int i = 0; i < 5; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, $"itemNode{i}", (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(val, "item", (string)null);
			m_items[i].OnEnable(wndParent);
			m_lbTargetCount[i] = UQueryExtensions.Q<Label>(val, "lb_target", (string)null);
			int index = i;
			m_btnGetAwards[i] = UQueryExtensions.Q<Button>(val, "btn_touch", (string)null);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetAwards[i], (Action<ClickEvent>)delegate
			{
				OnGetAward(index);
			}, "", (TrickleDown)0);
		}
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 282f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Handbook, RefreshUI);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_ModActive_Ack, RefreshUI);
		m_wndParent.CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	private void RefreshUI(object obj)
	{
		Refresh();
	}

	public void Refresh()
	{
		RefreshProgress();
		RefreshTargetItem();
		RefreshSkillTypeList();
	}

	private void RefreshProgress()
	{
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		m_modHandbookTargetInfo = zbServerData.sGameData.HandbookInfo.ModHandbookInfo.TargetInfo;
		int totalCount = m_modHandbookTargetInfo.TotalCount;
		List<int> list = (from r in zbDataConfig.Instance.modHandbookTargetCsvTable.GetRecords().records.Values.ToList()
			select r.target).ToList();
		list.Insert(0, 0);
		List<float> list2 = new List<float> { 0f, 60f, 185f, 315f, 440f, 565f };
		int num = 0;
		for (int num2 = 0; num2 < list.Count - 1; num2++)
		{
			if (totalCount >= list[num2] && totalCount <= list[num2 + 1])
			{
				num = num2;
				break;
			}
		}
		float num3 = Mathf.InverseLerp((float)list[num], (float)list[num + 1], (float)totalCount);
		float num4 = Mathf.Lerp(list2[num], list2[num + 1], num3);
		num4 = Mathf.Min(num4, list2[list2.Count - 1]);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num4, (LengthUnit)0));
		((TextElement)m_lbSumCollect).text = totalCount.ToString();
	}

	private void RefreshTargetItem()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		List<zbModHandbookTargetCsvRecord> list = zbDataConfig.Instance.modHandbookTargetCsvTable.GetRecords().records.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			zbItemData data = new zbItemData((EResourceType)list[i].award[0], list[i].award[1], list[i].award[2]);
			m_items[i].Refresh(data);
			m_items[i].SetGettedVisible(m_modHandbookTargetInfo.TargetAward >= list[i].target);
			m_items[i].SetSpineVisible(m_modHandbookTargetInfo.TotalCount >= list[i].target && m_modHandbookTargetInfo.TargetAward < list[i].target);
			((VisualElement)m_btnGetAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_modHandbookTargetInfo.TotalCount < list[i].target || m_modHandbookTargetInfo.TargetAward >= list[i].target) ? 1 : 0));
			((TextElement)m_lbTargetCount[i]).text = list[i].target.ToString();
		}
	}

	private void RefreshSkillTypeList()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		m_listView.columns.Clear();
		m_listItemDatas.Clear();
		m_listItemDatas = zbDataConfig.Instance.modHandbookTypeCsvTable.GetRecords().records.Values.ToList();
		for (int i = 0; i < 3; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(220f),
				makeCell = () => (VisualElement)(object)new zbSkillTypeItem(m_wndParent),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0059: Unknown result type (might be due to invalid IL or missing references)
					//IL_0047: Unknown result type (might be due to invalid IL or missing references)
					int num2 = row * 3 + columnIndex;
					if (num2 < m_listItemDatas.Count)
					{
						(element as zbSkillTypeItem)?.Refresh(m_listItemDatas[num2]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		m_rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / 3f);
		for (int num = 0; num < rowCount; num++)
		{
			m_rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = m_rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void OnGetAward(int index)
	{
		zbWsMsgHandler.Instance.Handbook_ModTargetAward_Req();
	}
}
